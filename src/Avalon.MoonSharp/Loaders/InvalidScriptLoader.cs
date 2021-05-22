using System;

namespace MoonSharp.Interpreter.Loaders
{
    /// <summary>
    /// A script loader used for platforms we cannot initialize in any better way..
    /// </summary>
    internal class InvalidScriptLoader : IScriptLoader
    {
        private string _error;

        internal InvalidScriptLoader(string frameworkname)
        {
            _error = $@"Loading scripts from files is not automatically supported on {frameworkname}. 
Please implement your own IScriptLoader (possibly, extending ScriptLoaderBase for easier implementation),
use a preexisting loader like EmbeddedResourcesScriptLoader or UnityAssetsScriptLoader or load scripts from strings.";
        }

        public object LoadFile(string file, Table globalContext)
        {
            throw new PlatformNotSupportedException(_error);
        }

        public string ResolveFileName(string filename, Table globalContext)
        {
            return filename;
        }

        public string ResolveModuleName(string modname, Table globalContext)
        {
            throw new PlatformNotSupportedException(_error);
        }
    }
}