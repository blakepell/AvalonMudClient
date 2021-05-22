using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MoonSharp.Interpreter.Loaders
{
    /// <summary>
    /// A script loader loading scripts from an assembly resources
    /// </summary>
    public class EmbeddedResourcesScriptLoader : ScriptLoaderBase
    {
        private string _namespace;
        private Assembly _resourceAssembly;
        private HashSet<string> _resourceNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourcesScriptLoader"/> class.
        /// </summary>
        /// <param name="resourceAssembly">The assembly containing the scripts as embedded resources or null to use the calling assembly.</param>
        public EmbeddedResourcesScriptLoader(Assembly resourceAssembly = null)
        {
            if (resourceAssembly == null)
            {
                resourceAssembly = Assembly.GetCallingAssembly();
            }

            _resourceAssembly = resourceAssembly;
            _namespace = _resourceAssembly.FullName.Split(',').First();
            _resourceNames = new HashSet<string>(_resourceAssembly.GetManifestResourceNames());
        }

        private string FileNameToResource(string file)
        {
            file = file.Replace('/', '.');
            file = file.Replace('\\', '.');
            return $"{_namespace}.{file}";
        }

        /// <summary>
        /// Checks if a script file exists.
        /// </summary>
        /// <param name="name">The script filename.</param>
        public override bool ScriptFileExists(string name)
        {
            name = this.FileNameToResource(name);
            return _resourceNames.Contains(name);
        }

        /// <summary>
        /// Opens a file for reading the script code.
        /// It can return either a string, a byte[] or a Stream.
        /// If a byte[] is returned, the content is assumed to be a serialized (dumped) bytecode. If it's a string, it's
        /// assumed to be either a script or the output of a string.dump call. If a Stream, autodetection takes place.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="globalContext">The global context.</param>
        /// <returns>
        /// A string, a byte[] or a Stream.
        /// </returns>
        public override object LoadFile(string file, Table globalContext)
        {
            file = this.FileNameToResource(file);
            return _resourceAssembly.GetManifestResourceStream(file);
        }
    }
}