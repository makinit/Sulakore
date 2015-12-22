using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using Sulakore.Habbo.Web;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class Contractor : IReadOnlyList<ExtensionForm>, IList
    {
        private readonly List<ExtensionForm> _extensions;
        private readonly IDictionary<string, ExtensionForm> _extensionHash;
        private readonly IDictionary<Assembly, string> _initialExtensionPaths;

        private static readonly IDictionary<Assembly, ExtensionInfo> _extensionInfo;
        private static readonly DirectoryInfo _dependenciesDirectory, _extensionsDirectory;

        public event EventHandler<ExtensionActionEventArgs> ExtensionAction;
        protected virtual void OnExtensionAction(ExtensionActionEventArgs e)
        {
            ExtensionAction?.Invoke(this, e);
        }

        public HHotel Hotel { get; set; }
        public HGameData GameData { get; set; }
        public IHConnection Connection { get; set; }

        public int Count => _extensions.Count;
        public ExtensionForm this[int index] => _extensions[index];

        static Contractor()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

            _extensionsDirectory = new DirectoryInfo("Extensions");
            _dependenciesDirectory = new DirectoryInfo("Extensions\\Dependencies");

            _extensionInfo =
                new Dictionary<Assembly, ExtensionInfo>();
        }
        public Contractor()
        {
            _extensions = new List<ExtensionForm>();
            _extensionHash = new Dictionary<string, ExtensionForm>();
            _initialExtensionPaths = new Dictionary<Assembly, string>();
        }

        public void HandleIncoming(InterceptedEventArgs e)
        {
            HandleInterception(false, e);
        }
        public void HandleOutgoing(InterceptedEventArgs e)
        {
            HandleInterception(true, e);
        }
        protected virtual void HandleInterception(bool isOutgoing, InterceptedEventArgs e)
        {
            if (Count < 1) return;

            ExtensionForm[] extensions = _extensions.ToArray();
            foreach (ExtensionForm extension in extensions)
            {
                if (!extension.IsRunning) continue;

                if (isOutgoing) extension.Triggers.HandleOutgoing(e);
                else extension.Triggers.HandleIncoming(e);
            }
        }

        public void LoadExtensions()
        {
            if (!_extensionsDirectory.Exists)
                _extensionsDirectory.Create();

            IEnumerable<FileSystemInfo> extensions =
                _extensionsDirectory.EnumerateFileSystemInfos()
                .Where(f => f.Extension == ".dll" || f.Extension == ".exe");

            LoadExtensions(extensions);
        }
        protected virtual void LoadExtensions(IEnumerable<FileSystemInfo> extensions)
        {
            foreach (FileSystemInfo extension in extensions)
                Install(extension.FullName);
        }

        public ExtensionForm Install(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) return null;

            string hash = GetHash(path);
            if (_extensionHash.ContainsKey(hash))
            {
                ExtensionForm extensionForm = _extensionHash[hash];
                if (!extensionForm.IsDisposed)
                {
                    OnExtensionAction(new ExtensionActionEventArgs(
                        extensionForm, ExtensionActionType.Reinstalled));
                }
                else extensionForm = Initialize(extensionForm);
                return extensionForm;
            }
            return Install(path, hash);
        }
        public virtual void Uninstall(ExtensionForm extension)
        {
            if (File.Exists(extension.FileLocation))
                File.Delete(extension.FileLocation);

            if (_extensions.Contains(extension))
                _extensions.Remove(extension);

            if (extension.IsRunning)
                extension.Close();

            if (!extension.IsDisposed)
                extension.Dispose();

            if (_extensionHash.ContainsKey(extension.Hash))
                _extensionHash.Remove(extension.Hash);

            OnExtensionAction(new ExtensionActionEventArgs(
                extension, ExtensionActionType.Uninstalled));
        }
        protected virtual ExtensionForm Install(string path, string hash)
        {
            // Create a copy of the assembly, and load it into memory(byte[]).
            string installedExtensionPath = CopyExtension(hash, path);
            byte[] rawFile = File.ReadAllBytes(installedExtensionPath);

            // Read the assembly from memory (byte[]), so we can delete the file when uninstalled.
            Assembly fileAssembly = Assembly.Load(rawFile);
            InstallDependenciesFrom(path, fileAssembly);

            ExtensionForm extension = null;
            Type extensionFormType = GetExtensionFormType(fileAssembly);
            if (extensionFormType != null)
            {
                var extensionInfo = new ExtensionInfo(
                    installedExtensionPath, hash, this);

                _extensionInfo.Add(fileAssembly, extensionInfo);
                _initialExtensionPaths.Add(fileAssembly, Path.GetDirectoryName(path));

                extension = Initialize(extensionFormType);
                if (extension != null)
                {
                    var extensionAction = extension.IsRunning ?
                        ExtensionActionType.Opened : ExtensionActionType.Installed;

                    OnExtensionAction(new ExtensionActionEventArgs(
                        extension, extensionAction));
                }
                else
                {
                    _extensionInfo.Remove(fileAssembly);
                    _initialExtensionPaths.Remove(fileAssembly);
                }
            }

            if (extension == null &&
                File.Exists(installedExtensionPath))
            {
                File.Delete(installedExtensionPath);
            }
            return extension;
        }

        private ExtensionForm Initialize(Type extensionType)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
                var extension = (ExtensionForm)Activator.CreateInstance(extensionType);
                extension.Disposed += ExtensionDisposed;
                extension.Shown += ExtensionShown;

                _extensions.Add(extension);
                _extensionHash[extension.Hash] = extension;

                return extension;
            }
            catch { /* Failed to create extension instance. */ return null; }
            finally { AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve; }
        }
        public virtual ExtensionForm Initialize(ExtensionForm extension)
        {
            if (extension.IsDisposed)
            {
                if (_extensions.Contains(extension))
                    _extensions.Remove(extension);

                return Initialize(extension.GetType());
            }
            else return extension;
        }

        private void ExtensionShown(object sender, EventArgs e)
        {
            var extension = (ExtensionForm)sender;

            OnExtensionAction(new ExtensionActionEventArgs(
                extension, ExtensionActionType.Opened));
        }
        private void ExtensionDisposed(object sender, EventArgs e)
        {
            var extension = (ExtensionForm)sender;
            try
            {
                OnExtensionAction(new ExtensionActionEventArgs(
                    extension, ExtensionActionType.Closed));
            }
            finally
            {
                extension.Shown -= ExtensionShown;
                extension.Disposed -= ExtensionDisposed;
            }
        }

        private string GetHash(string path)
        {
            using (var md5 = MD5.Create())
            using (var fileStream = File.OpenRead(path))
            {
                return BitConverter.ToString(
                    md5.ComputeHash(fileStream))
                    .Replace("-", string.Empty).ToLower();
            }
        }
        private string CopyExtension(string hash, string path)
        {
            if (!_extensionsDirectory.Exists)
                _extensionsDirectory.Create();

            path = Path.GetFullPath(path);
            string extensionExt = Path.GetExtension(path);
            string extensionFileName = Path.GetFileNameWithoutExtension(path);

            string extensionInstallPath = path;
            string extensionSuffix = $"({hash}){extensionExt}";
            if (!path.EndsWith(extensionSuffix))
            {
                extensionInstallPath = Path.Combine(
                    _extensionsDirectory.FullName, extensionFileName + extensionSuffix);

                bool isAlreadyInstalled = false;
                if (File.Exists(extensionInstallPath))
                {
                    isAlreadyInstalled =
                        (GetHash(extensionInstallPath) == hash);
                }

                if (!isAlreadyInstalled)
                    File.Copy(path, extensionInstallPath, !isAlreadyInstalled);
            }
            return extensionInstallPath;
        }
        private Type GetExtensionFormType(Assembly fileAssembly)
        {
            try
            {
                Type[] assemblyTypes = fileAssembly.GetTypes();
                foreach (Type assemblyType in assemblyTypes)
                {
                    if (assemblyType.IsInterface || assemblyType.IsAbstract) continue;
                    if (assemblyType.BaseType == typeof(ExtensionForm)) return assemblyType;
                }
                return null;
            }
            catch (ReflectionTypeLoadException) { return null; }
        }

        private static FileSystemInfo GetDependency(string path, string dependencyName)
        {
            return GetDependency(new DirectoryInfo(path), dependencyName);
        }
        protected virtual void InstallDependenciesFrom(string path, Assembly fileAssembly)
        {
            if (!_dependenciesDirectory.Exists)
                _dependenciesDirectory.Create();

            AssemblyName[] references = fileAssembly.GetReferencedAssemblies();
            var fileReferences = new Dictionary<string, AssemblyName>(references.Length);

            foreach (AssemblyName reference in references)
                fileReferences[reference.Name] = reference;

            string[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetName().Name).ToArray();

            IEnumerable<string> missingAssemblies = fileReferences
                .Keys.Except(loadedAssemblies);

            var sourceDirectory = new DirectoryInfo(Path.GetDirectoryName(path));
            foreach (string missingAssembly in missingAssemblies)
            {
                string assemblyName = fileReferences[missingAssembly].FullName;
                FileSystemInfo dependency = GetDependency(_dependenciesDirectory, assemblyName);
                if (dependency == null)
                {
                    dependency = GetDependency(sourceDirectory, assemblyName);
                    if (dependency != null)
                    {
                        string installDependencyPath = Path.Combine(
                           _dependenciesDirectory.FullName, dependency.Name);

                        File.Copy(dependency.FullName, installDependencyPath);
                    }
                }
            }
        }
        private static FileSystemInfo GetDependency(DirectoryInfo directory, string dependencyName)
        {
            FileSystemInfo[] libraries = directory.GetFileSystemInfos("*.dll");
            foreach (FileSystemInfo library in libraries)
            {
                string libraryName = AssemblyName.GetAssemblyName(
                    library.FullName).FullName;

                if (libraryName == dependencyName)
                    return library;
            }
            return null;
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!_dependenciesDirectory.Exists)
                _dependenciesDirectory.Create();

            FileSystemInfo dependency = GetDependency(_dependenciesDirectory, args.Name);
            if (dependency != null)
            {
                byte[] rawDependency = File.ReadAllBytes(dependency.FullName);
                return Assembly.Load(rawDependency);
            }
            return null;
        }
        internal static ExtensionInfo GetExtensionInfo(Assembly extensionAssembly)
        {
            return _extensionInfo.ContainsKey(extensionAssembly) ?
                    _extensionInfo[extensionAssembly] : null;
        }

        #region IList Implementation
        object IList.this[int index]
        {
            get { return ((IList)_extensions)[index]; }
            set { ((IList)_extensions)[index] = value; }
        }
        public object SyncRoot => ((IList)_extensions).SyncRoot;
        public bool IsReadOnly => ((IList)_extensions).IsReadOnly;
        public bool IsFixedSize => ((IList)_extensions).IsFixedSize;
        public bool IsSynchronized => ((IList)_extensions).IsSynchronized;

        public void Clear() =>
            _extensions.Clear();

        public void RemoveAt(int index) =>
            _extensions.RemoveAt(index);

        public int Add(object value) =>
            ((IList)_extensions).Add(value);

        public void Remove(object value) =>
            ((IList)_extensions).Remove(value);

        public bool Contains(object value) =>
            ((IList)_extensions).Contains(value);

        public int IndexOf(object value) =>
            ((IList)_extensions).IndexOf(value);

        public void CopyTo(Array array, int index) =>
            ((IList)_extensions).CopyTo(array, index);

        public void Insert(int index, object value) =>
            ((IList)_extensions).Insert(index, value);

        public IEnumerator<ExtensionForm> GetEnumerator() =>
            ((IReadOnlyList<ExtensionForm>)_extensions).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
             ((IReadOnlyList<ExtensionForm>)_extensions).GetEnumerator();
        #endregion
    }
}