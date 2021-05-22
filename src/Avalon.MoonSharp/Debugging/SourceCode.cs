using System.Collections.Generic;

namespace MoonSharp.Interpreter.Debugging
{
    /// <summary>
    /// Class representing the source code of a given script
    /// </summary>
    public class SourceCode : IScriptPrivateResource
    {
        internal SourceCode(string name, string code, Script ownerScript)
        {
            this.Refs = new List<SourceRef>();
            this.Name = name;
            this.Code = code;
            this.OwnerScript = ownerScript;
        }

        /// <summary>
        /// Gets the name of the source code
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the source code as a string
        /// </summary>
        public string Code { get; }

        internal List<SourceRef> Refs { get; }

        /// <summary>
        /// Gets the script owning this resource.
        /// </summary>
        public Script OwnerScript { get; }

    }
}