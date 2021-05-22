/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Argus.Extensions;

namespace Avalon.Lua
{
    public static class LuaCompletion
    {
        /// <summary>
        /// A static list of completion data we can construct and format once.  We are going
        /// to consolidate overloads.
        /// </summary>
        private static Dictionary<string, ICompletionData> _luaCompletionData;

        /// <summary>
        /// Loads the completion data based on the pattern.
        /// </summary>
        public static void LoadCompletionData(IList<ICompletionData> data, string pattern)
        {
            if (pattern == "lua")
            {
                if (_luaCompletionData == null)
                {
                    // Initialize the Lua completion data once.
                    _luaCompletionData = new Dictionary<string, ICompletionData>();

                    var t = typeof(ScriptCommands);

                    // This should get all of our methods but exclude ones that are defined on
                    // object like ToString, GetHashCode, Equals, etc.
                    foreach (var method in t.GetMethods().Where(m => m.DeclaringType != typeof(object)))
                    {
                        if (_luaCompletionData.ContainsKey(method.Name))
                        {
                            // It exists, therefore this is an overload.
                            ConstructMembers(method, (LuaCompletionData) _luaCompletionData[method.Name]);
                        }
                        else
                        {
                            var lcd = new LuaCompletionData(method.Name, "");
                            _luaCompletionData.Add(method.Name, lcd);
                            ConstructMembers(method, lcd);
                            data.Add(lcd);
                        }
                    }
                }
                else
                {
                    foreach (var item in _luaCompletionData)
                    {
                        data.Add(item.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Constructs the data about a method and it's overloads.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="lcd"></param>
        public static void ConstructMembers(MethodInfo method, LuaCompletionData lcd)
        {
            var sb = Argus.Memory.StringBuilderPool.Take();

            // Whether to start with just the method name (for the first one) or append
            // the parameters as an overload.
            if (string.IsNullOrWhiteSpace((string)lcd.Description))
            {
                sb.AppendLine($"{method.Name} => Returns {method.ReturnType}");
                sb.Replace("System.Void", "nothing");

                if (method.CustomAttributes.Any())
                {
                    var attr = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name.Contains("DescriptionAttribute"));

                    if (attr?.ConstructorArguments.Count > 0)
                    {
                        sb.AppendLine(attr.ConstructorArguments[0].ToString().Trim('"'));
                    }
                }

            }
            else
            {
                sb.AppendLine((string)lcd.Description);
            }

            var parameterDescriptions = string.Join
                (", ", method.GetParameters()
                     .Select(x => $"{x.ParameterType} {x.Name}")
                     .ToArray());

            // The call and parameters
            sb.AppendLine($"{method.Name}({parameterDescriptions})");

            // Cleanup the parameters to make them easier to read.
            sb.Replace("System.String", "string")
                .Replace("System.Int32", "number")
                .Replace("System.DataTime", "datetime")
                .Replace("System.Boolean", "boolean")
                .Replace("System.Collections.Generic", "")
                .Replace("System.Object", "object")
                .Replace("Avalon.Common.Models.", "")
                .Replace("MahApps.Metro.IconPacks.", "");

            // Remove any double line breaks if they exist.
            sb.Replace("\r\n\r\n", "\r\n");

            // Trim the final line break off.
            sb.TrimEnd('\r', '\n');

            lcd.Description = sb.ToString();
            
            Argus.Memory.StringBuilderPool.Return(sb);
        }

        public static void LoadCompletionDatasnippets(IList<ICompletionData> data)
        {
            data.Add(new LuaCompletionData("Scheduled Tasks", "A snippet to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop", "A snippet to show how to use scheduled tasks", ""));
            data.Add(new LuaCompletionData("For Loop Pairs", "A snippet to show how to use scheduled tasks", ""));
        }

    }
}
