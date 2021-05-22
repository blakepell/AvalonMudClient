using MoonSharp.Interpreter.Compatibility.Frameworks;

namespace MoonSharp.Interpreter.Compatibility
{
    public static class Framework
    {
        private static FrameworkCurrent _frameworkCurrent = new FrameworkCurrent();

        public static FrameworkBase Do => _frameworkCurrent;
    }
}