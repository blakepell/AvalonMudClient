using System;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// In a module type, mark methods or fields with this attribute to have them exposed as module functions.
    /// Methods must have the signature "public static DynValue ...(ScriptExecutionContextCallbackArguments)".
    /// Fields must be static or const strings, with an anonymous Lua function inside.
    /// 
    /// See <see cref="MoonSharpModuleAttribute"/> for more information about modules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, Inherited = false)]
    public sealed class MoonSharpModuleMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the function in the module (defaults to member name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An optional description that can be used for intellisense.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An options auto complete verbiage hint for IDE's that implement it.
        /// </summary>
        public string AutoCompleteHint { get; set; }

        /// <summary>
        /// A return hint so that autocomplete can infer what the DynValue is of the method.
        /// </summary>
        public string ReturnTypeHint { get; set; }

        /// <summary>
        /// The number of parameters a function has.
        /// </summary>
        public int ParameterCount { get; set; } = 0;
    }
}