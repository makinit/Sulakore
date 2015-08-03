/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Security.Cryptography;

using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class Contractor
    {
        private static readonly IDictionary<Assembly, ExtensionInfo> _extensionInfoByAssembly;

        private readonly IDictionary<string, ExtensionForm> _extensionByHash;
        private readonly IDictionary<Assembly, string> _initialExtensionPaths;

        public event EventHandler<ExtensionActionEventArgs> ExtensionAction;
        protected virtual void OnExtensionAction(ExtensionActionEventArgs e)
        {
            ExtensionAction?.Invoke(this, e);
        }

        public HHotel Hotel { get; set; }
        public HConnection Connection { get; }

        private readonly List<ExtensionForm> _extensions;
        public IReadOnlyList<ExtensionForm> Extensions => _extensions;

        static Contractor()
        {
            _extensionInfoByAssembly = new Dictionary<Assembly, ExtensionInfo>();
        }
        public Contractor(HConnection connection)
        {
            Connection = connection;

            _extensions = new List<ExtensionForm>();
            _extensionByHash = new Dictionary<string, ExtensionForm>();
            _initialExtensionPaths = new Dictionary<Assembly, string>();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void HandleIncoming(InterceptedEventArgs e)
        {
            if (Extensions.Count < 1) return;

            ExtensionForm[] extensions = _extensions.ToArray();
            foreach (ExtensionForm extension in extensions)
            {
                if (extension.IsRunning)
                    extension.Triggers?.HandleIncoming(e);
            }
        }
        public void HandleOutgoing(InterceptedEventArgs e)
        {
            if (Extensions.Count < 1) return;

            ExtensionForm[] extensions = _extensions.ToArray();
            foreach (ExtensionForm extension in extensions)
            {
                if (extension.IsRunning)
                    extension.Triggers?.HandleOutgoing(e);
            }
        }

        public void LoadInstalledExtensions()
        {
            if (Directory.Exists("Extensions"))
            {
                var extensionsDirectory = new DirectoryInfo(Path.GetFullPath("Extensions"));
                IEnumerable<DirectoryInfo> creatorDirectories = extensionsDirectory
                    .EnumerateDirectories().Where(d => d.Name != "$Dependencies");

                foreach (DirectoryInfo creatorDirectory in creatorDirectories)
                {
                    IEnumerable<FileInfo> creatorFiles = creatorDirectory.EnumerateFiles()
                        .Where(t => t.Extension == ".dll" || t.Extension == ".exe");

                    foreach (FileInfo creatorFile in creatorFiles)
                        Install(creatorFile.FullName);
                }
            }
        }
        public ExtensionForm Install(string path)
        {
            if (!File.Exists(path)) return null;

            path = Path.GetFullPath(path);
            string fileHash = GetFileMD5Hash(path);

            if (_extensionByHash.ContainsKey(fileHash))
            {
                ExtensionForm extensionForm = _extensionByHash[fileHash];
                if (extensionForm.IsDisposed)
                {
                    extensionForm = ReininitializeExtension(extensionForm);
                }
                else
                {
                    OnExtensionAction(new ExtensionActionEventArgs(
                        extensionForm, ExtensionActionType.Reinstalled));
                }
                return extensionForm;
            }

            byte[] rawFile = File.ReadAllBytes(path);
            Assembly fileAssembly = Assembly.Load(rawFile);
            Type extensionFormType = GetExtensionFormType(fileAssembly);

            if (extensionFormType != null)
            {
                var extensionInfo = new ExtensionInfo(
                    fileAssembly, Hotel, Connection);

                string extensionInstallPath = CreateExtensionDirectory(
                    extensionInfo.Creator, fileHash, path);

                extensionInfo.FileLocation = extensionInstallPath;
                extensionInfo.Version = new Version(FileVersionInfo
                    .GetVersionInfo(extensionInstallPath).FileVersion);

                _initialExtensionPaths.Add(fileAssembly, Path.GetDirectoryName(path));

                _extensionInfoByAssembly.Add(fileAssembly, extensionInfo);
                return InitializeExtension(extensionFormType, fileHash);
            }
            else
            {
                throw new Exception(
                    "Application is not a valid extension, unable to locate type inheriting from ExtensionForm.");
            }
        }
        public void Uninstall(ExtensionForm extension)
        {
            string extensionHash = GetFileMD5Hash(extension.FileLocation);
            string creatorDirectory = Path.GetDirectoryName(extension.FileLocation);
            if (File.Exists(extension.FileLocation))
            {
                File.Delete(extension.FileLocation);
                var creatorDirectoryInfo = new DirectoryInfo(creatorDirectory);

                if (creatorDirectoryInfo.GetFiles().Length < 1)
                    creatorDirectoryInfo.Delete();
            }

            if (_extensions.Contains(extension))
                _extensions.Remove(extension);

            if (extension.IsRunning)
                extension.Close();

            if (!extension.IsDisposed)
                extension.Dispose();

            if (_extensionByHash.ContainsKey(extensionHash))
                _extensionByHash.Remove(extensionHash);

            OnExtensionAction(new ExtensionActionEventArgs(
                extension, ExtensionActionType.Uninstalled));
        }
        public ExtensionForm GetExtensionForm(string extensionHash)
        {
            return _extensionByHash.ContainsKey(extensionHash) ?
                _extensionByHash[extensionHash] : null;
        }
        public ExtensionForm ReininitializeExtension(ExtensionForm extension)
        {
            if (!extension.IsDisposed) return extension;
            string extensionHash = GetFileMD5Hash(extension.FileLocation);

            ExtensionForm cachedExtensions = _extensionByHash[extensionHash];
            if (!cachedExtensions.IsDisposed) return cachedExtensions;

            if (_extensions.Contains(extension))
                _extensions.Remove(extension);

            var extensionForm = (ExtensionForm)Activator.CreateInstance(extension.GetType());

            _extensions.Add(extensionForm);
            _extensionByHash[extensionHash] = extensionForm;

            extensionForm.Shown += ExtensionForm_Shown;
            extensionForm.FormClosed += ExtensionForm_FormClosed;

            return extensionForm;
        }

        private string GetFileMD5Hash(string path)
        {
            using (var md5 = MD5.Create())
            using (var fileStream = File.OpenRead(path))
            {
                return BitConverter.ToString(
                    md5.ComputeHash(fileStream)).Replace("-", string.Empty).ToLower();
            }
        }
        private Type GetExtensionFormType(Assembly fileAssembly)
        {
            Type extensionFormType = null;
            Type[] assemblyTypes = fileAssembly.GetTypes();
            foreach (Type assemblyType in assemblyTypes)
            {
                if (assemblyType.IsInterface || assemblyType.IsAbstract) continue;

                if (extensionFormType == null &&
                    assemblyType.BaseType == typeof(ExtensionForm))
                {
                    extensionFormType = assemblyType;
                    break;
                }
            }
            return extensionFormType;
        }
        private ExtensionForm InitializeExtension(Type extensionFormType, string extensionHash)
        {
            var extensionForm = (ExtensionForm)Activator.CreateInstance(extensionFormType);

            extensionForm.Shown += ExtensionForm_Shown;
            extensionForm.FormClosed += ExtensionForm_FormClosed;

            _extensions.Add(extensionForm);
            _extensionByHash[extensionHash] = extensionForm;
            OnExtensionAction(new ExtensionActionEventArgs(extensionForm, ExtensionActionType.Installed));

            if (extensionForm.IsRunning)
                OnExtensionAction(new ExtensionActionEventArgs(extensionForm, ExtensionActionType.Opened));

            return extensionForm;
        }
        private string CreateExtensionDirectory(string creator, string fileHash, string initialPath)
        {
            initialPath = Path.GetFullPath(initialPath);
            string extensionExt = Path.GetExtension(initialPath);
            string extensionFileName = Path.GetFileNameWithoutExtension(initialPath);
            string installDirectory = Path.GetFullPath(Path.Combine("Extensions", creator));

            if (!Directory.Exists(installDirectory))
                Directory.CreateDirectory(installDirectory);

            string extensionInstallPath = initialPath;
            string extensionSuffix = $"({fileHash}){extensionExt}";
            if (!initialPath.EndsWith(extensionSuffix))
            {
                extensionInstallPath = Path.Combine(installDirectory,
                    extensionFileName + extensionSuffix);

                bool isAlreadyInstalled = false;
                if (File.Exists(extensionInstallPath))
                {
                    string possibleCopyMD5 = GetFileMD5Hash(extensionInstallPath);
                    isAlreadyInstalled = (possibleCopyMD5 == fileHash);
                }

                if (!isAlreadyInstalled)
                    File.Copy(initialPath, extensionInstallPath, !isAlreadyInstalled);
            }
            return extensionInstallPath;
        }

        private void ExtensionForm_Shown(object sender, EventArgs e)
        {
            var extension = (ExtensionForm)sender;

            OnExtensionAction(new ExtensionActionEventArgs(
                extension, ExtensionActionType.Opened));
        }
        private void ExtensionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var extension = (ExtensionForm)sender;
            try
            {
                OnExtensionAction(new ExtensionActionEventArgs(
                    extension, ExtensionActionType.Closed));
            }
            finally
            {
                extension.Shown -= ExtensionForm_Shown;
                extension.FormClosed -= ExtensionForm_FormClosed;
            }
        }

        private FileSystemInfo LookForDependency(string path, string dependencyName)
        {
            path = Path.GetFullPath(path);
            var directoryInfo = new DirectoryInfo(path);

            FileSystemInfo[] possibleDependencies = directoryInfo.GetFileSystemInfos("*.dll");
            foreach (FileSystemInfo possibleDependency in possibleDependencies)
            {
                string assemblyName = AssemblyName.GetAssemblyName(
                    possibleDependency.FullName).FullName;

                if (assemblyName == dependencyName)
                    return possibleDependency;
            }
            return null;
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!Directory.Exists("Extensions\\$Dependencies"))
            {
                Directory.CreateDirectory("Extensions\\$Dependencies")
                    .Attributes = (FileAttributes.Directory | FileAttributes.Hidden);
            }

            FileSystemInfo dependency = LookForDependency(
                "Extensions\\$Dependencies", args.Name);

            if (dependency == null)
            {
                if (args.RequestingAssembly == null)
                    return null;

                string initialExtensionPath =
                    _initialExtensionPaths[args.RequestingAssembly];

                dependency = LookForDependency(initialExtensionPath, args.Name);
                if (dependency == null) return null;

                File.Copy(dependency.FullName,
                    Path.Combine("Extensions\\$Dependencies", dependency.Name));
            }

            byte[] rawDependency = File.ReadAllBytes(dependency.FullName);
            return Assembly.Load(rawDependency);
        }

        internal static ExtensionInfo GetExtensionInfo(Assembly extensionAssembly)
        {
            return _extensionInfoByAssembly.ContainsKey(extensionAssembly) ?
                    _extensionInfoByAssembly[extensionAssembly] : null;
        }
    }
}