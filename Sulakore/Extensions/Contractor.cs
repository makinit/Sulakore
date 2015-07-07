﻿using System;
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
            EventHandler<ExtensionActionEventArgs> handler = ExtensionAction;
            if (handler != null) handler(this, e);
        }

        public HHotel Hotel { get; private set; }
        public HConnection Connection { get; private set; }

        private readonly List<ExtensionForm> _extensions;
        public IReadOnlyList<ExtensionForm> Extensions
        {
            get { return _extensions; }
        }

        static Contractor()
        {
            _extensionInfoByAssembly = new Dictionary<Assembly, ExtensionInfo>();
        }
        public Contractor(HConnection connection)
        {
            Connection = connection;
            Hotel = SKore.ToHotel(Connection.GameHostName);

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
                if (!extension.IsRunning) continue;

                if (extension.Triggers != null)
                    extension.Triggers.HandleIncoming(e);
            }
        }
        public void HandleOutgoing(InterceptedEventArgs e)
        {
            if (Extensions.Count < 1) return;

            ExtensionForm[] extensions = _extensions.ToArray();
            foreach (ExtensionForm extension in extensions)
            {
                if (!extension.IsRunning) continue;

                if (extension.Triggers != null)
                    extension.Triggers.HandleOutgoing(e);
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
                    if (_extensions.Contains(extensionForm))
                        _extensions.Remove(extensionForm);
                    else
                        CreateExtensionDirectory(extensionForm.Creator, fileHash, path);

                    extensionForm = InitializeExtension(extensionForm.GetType(), fileHash);
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
        }
        public ExtensionForm GetExtensionForm(string extensionHash)
        {
            return _extensionByHash.ContainsKey(extensionHash) ?
                _extensionByHash[extensionHash] : null;
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
            string extensionSuffix = string.Format("({0}){1}", fileHash, extensionExt);
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
            OnExtensionAction(new ExtensionActionEventArgs(extension, ExtensionActionType.Opened));
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
                string initialExtensionPath =
                    _initialExtensionPaths[args.RequestingAssembly];

                dependency = LookForDependency(initialExtensionPath, args.Name);
                if (dependency == null)
                {
                    // TODO: Create event handler to notify that a dependency of an extension was not found, possibly even 'ask' for one?
                    throw new Exception(
                        "Failed to resolve dependency for the assembly: " + args.RequestingAssembly.FullName);
                }

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