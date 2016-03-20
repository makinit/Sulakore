using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography;

using Sulakore.Habbo.Web;
using Sulakore.Communication;

namespace Sulakore.Modules
{
    public abstract class Contractor : IContractor
    {
        private readonly Dictionary<string, Type> _moduleTypes;
        private readonly Dictionary<Type, string> _modulePaths;
        private readonly Dictionary<Type, IModule> _initializedModules;
        private readonly Dictionary<Type, ModuleAttribute> _moduleAttributes;
        private readonly Dictionary<Type, IEnumerable<AuthorAttribute>> _authorAttributes;

        private static readonly Type _iModuleType, _iComponentType;
        private static readonly Dictionary<string, Assembly> _cachedFileAsms;
        private static readonly Dictionary<Assembly, IContractor> _contractors;

        public DirectoryInfo ModulesDirectory { get; }
        public DirectoryInfo DependenciesDirectory { get; }

        public abstract HHotel Hotel { get; }
        public abstract HGameData GameData { get; }
        public abstract IHConnection Connection { get; }

        static Contractor()
        {
            _iModuleType = typeof(IModule);
            _iComponentType = typeof(IComponent);
            _cachedFileAsms = new Dictionary<string, Assembly>();
            _contractors = new Dictionary<Assembly, IContractor>();
        }
        public Contractor(string installDirectory)
        {
            _moduleTypes = new Dictionary<string, Type>();
            _modulePaths = new Dictionary<Type, string>();
            _initializedModules = new Dictionary<Type, IModule>();
            _moduleAttributes = new Dictionary<Type, ModuleAttribute>();
            _authorAttributes = new Dictionary<Type, IEnumerable<AuthorAttribute>>();

            ModulesDirectory = Directory.CreateDirectory(installDirectory);
            DependenciesDirectory = ModulesDirectory.CreateSubdirectory("Dependencies");
        }

        public bool InstallModule(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) return false;

            string copiedFilePath = CopyFile(path);
            Type moduleType = GetModuleType(copiedFilePath);

            Assembly fileAsm = null;
            if (!_cachedFileAsms.ContainsKey(copiedFilePath))
            {
                byte[] fileData = File.ReadAllBytes(path);

                fileAsm = Assembly.Load(fileData);
                _cachedFileAsms.Add(copiedFilePath, fileAsm);
            }
            else
            {
                OnModuleReinstalled(
                    GetModule(moduleType), moduleType);

                return true;
            }

            foreach (Type type in fileAsm.ExportedTypes)
            {
                var moduleAtt = type.GetCustomAttribute<ModuleAttribute>();
                _moduleAttributes[type] = moduleAtt;

                var authorAtts = type.GetCustomAttributes<AuthorAttribute>();
                _authorAttributes[type] = authorAtts;

                if (moduleAtt != null &&
                    _iModuleType.IsAssignableFrom(type))
                {
                    moduleType = type;
                    _contractors[fileAsm] = this;
                    _moduleTypes[copiedFilePath] = type;
                    _modulePaths[type] = copiedFilePath;
                    break;
                }
            }
            if (moduleType == null)
            {
                // Do not remove from '_cachedFileAsm', otherwise assemblies will continue
                // to be added to the GAC, even though it's the same file/assembly.

                if (File.Exists(copiedFilePath))
                    File.Delete(copiedFilePath);

                return false;
            }
            else OnModuleInstalled(moduleType);
            return true;
        }
        protected abstract void OnModuleInstalled(Type type);
        protected abstract void OnModuleReinstalled(IModule module, Type type);

        protected void UninstallModule(Type type)
        {
            string filePath = GetModuleFilePath(type);
            if (File.Exists(filePath)) File.Delete(filePath);

            if (_authorAttributes.ContainsKey(type))
                _authorAttributes.Remove(type);

            if (_moduleAttributes.ContainsKey(type))
                _moduleAttributes.Remove(type);

            if (_modulePaths.ContainsKey(type))
                _modulePaths.Remove(type);

            if (_moduleTypes.ContainsKey(filePath))
                _moduleTypes.Remove(filePath);

            IModule module = GetModule(type);
            DisposeModule(type);

            OnModuleUninstalled(module, type);
        }
        protected abstract void OnModuleUninstalled(IModule module, Type type);

        protected IModule InitializeModule(Type type)
        {
            // Multiple instances not supported.
            if (_initializedModules.ContainsKey(type))
                return _initializedModules[type];

            IModule module = null;
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
                module = (IModule)Activator.CreateInstance(type);

                _initializedModules[type] = module;
                OnModuleInitialized(module, type);
            }
            catch
            {
                if (module != null)
                {
                    _initializedModules.Remove(type);
                    DisposeModule(type);
                    module = null;
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;

                if (module != null &&
                    _iComponentType.IsAssignableFrom(type))
                {
                    ((IComponent)module).Disposed += ModuleComponent_Disposed;
                }
            }
            return module;
        }
        protected abstract void OnModuleInitialized(IModule module, Type type);

        protected void DisposeModule(Type type)
        {
            IModule module = GetModule(type);
            if (module == null) return;

            module.Dispose();

            if (_initializedModules.ContainsKey(type))
                _initializedModules.Remove(type);

            OnModuleDisposed(module, type);
        }
        protected abstract void OnModuleDisposed(IModule module, Type type);

        protected int GetInitializedCount()
        {
            return _initializedModules.Count;
        }
        protected IModule GetModule(Type type)
        {
            IModule module = null;
            _initializedModules.TryGetValue(type, out module);
            return module;
        }
        protected Type GetModuleType(string path)
        {
            Type moduleType = null;
            _moduleTypes.TryGetValue(path, out moduleType);
            return moduleType;
        }
        protected string GetModuleFilePath(Type type)
        {
            string modulePath = string.Empty;
            _modulePaths.TryGetValue(type, out modulePath);
            return modulePath;
        }
        protected ModuleAttribute GetModuleAttribute(Type type)
        {
            ModuleAttribute moduleAtt = null;
            _moduleAttributes.TryGetValue(type, out moduleAtt);
            return moduleAtt;
        }
        protected IEnumerable<AuthorAttribute> GetAuthorAttributes(Type type)
        {
            IEnumerable<AuthorAttribute> authorAtts = null;
            _authorAttributes.TryGetValue(type, out authorAtts);
            return authorAtts;
        }

        protected string CopyFile(string path)
        {
            path = Path.GetFullPath(path);
            string fileHash = GetFileHash(path);
            string fileExt = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            string copiedFilePath = path;
            string fileNameSuffix = $"({fileHash}){fileExt}";
            if (!path.EndsWith(fileNameSuffix))
            {
                copiedFilePath = Path.Combine(
                    ModulesDirectory.FullName, fileName + fileNameSuffix);

                File.Copy(path, copiedFilePath, true);
            }
            return copiedFilePath;
        }
        protected string GetFileHash(string path)
        {
            using (var md5 = MD5.Create())
            using (var fileStream = File.OpenRead(path))
            {
                return BitConverter.ToString(
                    md5.ComputeHash(fileStream))
                    .Replace("-", string.Empty).ToLower();
            }
        }

        public static IContractor GetInstaller(Assembly moduleAssembly)
        {
            IContractor installer = null;
            _contractors.TryGetValue(moduleAssembly, out installer);
            return installer;
        }

        private void ModuleComponent_Disposed(object sender, EventArgs e)
        {
            ((IComponent)sender).Disposed -= ModuleComponent_Disposed;
            DisposeModule(sender.GetType()); // It won't hurt to re-dispose, it's better than making another "special" method for it.
        }
        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }
    }
}