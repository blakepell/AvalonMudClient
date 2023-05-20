using System;
using System.Linq.Expressions;
using MoonSharp.Interpreter.Loaders;

namespace MoonSharp.Interpreter.Platforms
{
    /// <summary>
    /// A static class offering properties for auto-detection of system/platform details
    /// </summary>
    public static class PlatformAutoDetector
    {
        private static bool? _isRunningOnAOT;
        private static bool _autoDetectionsDone;

        /// <summary>
        /// Gets a value indicating whether this instance is running on mono.
        /// </summary>
        public static bool IsRunningOnMono { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running a system using Ahead-Of-Time compilation 
        /// and not supporting JIT.
        /// </summary>
        public static bool IsRunningOnAOT
        {
            // We do a lazy eval here, so we can wire out this code by not calling it, if necessary..
            get
            {
                if (!_isRunningOnAOT.HasValue)
                {
                    try
                    {
                        Expression e = Expression.Constant(5, typeof(int));
                        var lambda = Expression.Lambda<Func<int>>(e);
                        lambda.Compile();
                        _isRunningOnAOT = false;
                    }
                    catch (Exception)
                    {
                        _isRunningOnAOT = true;
                    }
                }

                return _isRunningOnAOT.Value;
            }
        }

        private static void AutoDetectPlatformFlags()
        {
            if (_autoDetectionsDone)
            {
                return;
            }

            //IsRunningOnUnity = AppDomain.CurrentDomain
            //    .GetAssemblies()
            //    .SelectMany(a => a.SafeGetTypes())
            //    .Any(t => t.FullName.StartsWith("UnityEngine."));

            IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);

            _autoDetectionsDone = true;
        }


        internal static IPlatformAccessor GetDefaultPlatform()
        {
            AutoDetectPlatformFlags();

            // How to implement functionality for other platforms.
            //if (IsRunningOnUnity)
            //{
            //    return new LimitedPlatformAccessor();
            //}

            return new StandardPlatformAccessor();
        }

        internal static IScriptLoader GetDefaultScriptLoader()
        {
            AutoDetectPlatformFlags();

            //if (IsRunningOnUnity)
            //{
            //    return new UnityAssetsScriptLoader();
            //}

            return new FileSystemScriptLoader();
        }
    }
}