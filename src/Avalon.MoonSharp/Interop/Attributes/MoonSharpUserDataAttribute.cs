using System;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Marks a type of automatic registration as userdata (which happens only if UserData.RegisterAssembly is called).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class MoonSharpUserDataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoonSharpUserDataAttribute"/> class.
        /// </summary>
        public MoonSharpUserDataAttribute()
        {
            this.AccessMode = InteropAccessMode.Default;
        }

        /// <summary>
        /// The interop access mode
        /// </summary>
        public InteropAccessMode AccessMode { get; set; }
    }
}