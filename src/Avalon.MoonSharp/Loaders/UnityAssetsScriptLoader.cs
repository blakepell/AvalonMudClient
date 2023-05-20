using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Loaders
{
    /// <summary>
    /// A script loader which can load scripts from assets in Unity3D.
    /// Scripts should be saved as .txt files in a subdirectory of Assets/Resources.
    /// 
    /// When MoonSharp is activated on Unity3D and the default script loader is used,
    /// scripts should be saved as .txt files in Assets/Resources/MoonSharp/Scripts.
    /// </summary>
    public class UnityAssetsScriptLoader : ScriptLoaderBase
    {
        private Dictionary<string, string> _resources = new Dictionary<string, string>();

        /// <summary>
        /// The default path where scripts are meant to be stored (if not changed)
        /// </summary>
        public const string DEFAULT_PATH = "MoonSharp/Scripts";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="assetsPath">The path, relative to Assets/Resources. For example
        /// if your scripts are stored under Assets/Resources/Scripts, you should
        /// pass the value "Scripts". If null, "MoonSharp/Scripts" is used. </param>
        public UnityAssetsScriptLoader(string assetsPath = null)
        {
            assetsPath = assetsPath ?? DEFAULT_PATH;
#if UNITY_5
            LoadResourcesUnityNative(assetsPath);
#else
            this.LoadResourcesWithReflection(assetsPath);
#endif
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnityAssetsScriptLoader"/> class.
        /// </summary>
        /// <param name="scriptToCodeMap">A dictionary mapping filenames to the proper Lua script code.</param>
        public UnityAssetsScriptLoader(Dictionary<string, string> scriptToCodeMap)
        {
            _resources = scriptToCodeMap;
        }

#if UNITY_5
        void LoadResourcesUnityNative(string assetsPath)
        {
            try
            {
                UnityEngine.Object[] array = UnityEngine.Resources.LoadAll(assetsPath, typeof(UnityEngine.TextAsset));

                for (int i = 0; i < array.Length; i++)
                {
                    UnityEngine.TextAsset o = (UnityEngine.TextAsset)array[i];

                    string name = o.name;
                    string text = o.text;

                    m_Resources.Add(name, text);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogErrorFormat("Error initializing UnityScriptLoader : {0}", ex);
            }
        }

#else

        private void LoadResourcesWithReflection(string assetsPath)
        {
            try
            {
                var resourcesType = Type.GetType("UnityEngine.Resources, UnityEngine");
                var textAssetType = Type.GetType("UnityEngine.TextAsset, UnityEngine");

                var textAssetNameGet = Framework.Do.GetGetMethod(Framework.Do.GetProperty(textAssetType, "name"));
                var textAssetTextGet = Framework.Do.GetGetMethod(Framework.Do.GetProperty(textAssetType, "text"));

                var loadAll = Framework.Do.GetMethod(resourcesType, "LoadAll",
                    new[] {typeof(string), typeof(Type)});

                var array = (Array) loadAll.Invoke(null, new object[] {assetsPath, textAssetType});

                for (int i = 0; i < array.Length; i++)
                {
                    var o = array.GetValue(i);

                    string name = textAssetNameGet.Invoke(o, null) as string;
                    string text = textAssetTextGet.Invoke(o, null) as string;

                    _resources.Add(name, text);
                }
            }
            catch (Exception ex)
            {
#if !(ENABLE_DOTNET || NETFX_CORE)
                Console.WriteLine("Error initializing UnityScriptLoader : {0}", ex);
#endif
                Debug.WriteLine("Error initializing UnityScriptLoader : {0}", ex);
            }
        }
#endif

        private string GetFileName(string filename)
        {
            int b = Math.Max(filename.LastIndexOf('\\'), filename.LastIndexOf('/'));

            if (b > 0)
            {
                filename = filename.Substring(b + 1);
            }

            return filename;
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
        /// <exception cref="System.Exception">UnityAssetsScriptLoader.LoadFile : Cannot load  + file</exception>
        public override object LoadFile(string file, Table globalContext)
        {
            file = this.GetFileName(file);

            if (_resources.TryGetValue(file, out string loadFile))
            {
                return loadFile;
            }

            string error =
                $@"Cannot load script '{file}'. By default, scripts should be .txt files placed under a Assets/Resources/{DEFAULT_PATH} directory.
If you want scripts to be put in another directory or another way, use a custom instance of UnityAssetsScriptLoader or implement
your own IScriptLoader (possibly extending ScriptLoaderBase).";

            throw new Exception(error);
        }

        /// <summary>
        /// Checks if a given file exists
        /// </summary>
        /// <param name="file">The file.</param>
        public override bool ScriptFileExists(string file)
        {
            file = this.GetFileName(file);
            return _resources.ContainsKey(file);
        }


        /// <summary>
        /// Gets the list of loaded scripts filenames (useful for debugging purposes).
        /// </summary>
        public string[] GetLoadedScripts()
        {
            return _resources.Keys.ToArray();
        }
    }
}