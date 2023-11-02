/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Cysharp.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;
using MahApps.Metro.IconPacks;
using MoonSharp.Interpreter;

namespace MoonSharp.Interpreter.Wpf.Editor
{
    public static class LuaCompletion
    {
        /// <summary>
        /// Static list of the auto complete data.
        /// </summary>
        private static Dictionary<string, List<LuaCompletionData>> CompletionData { get; set; }

        /// <summary>
        /// A list of all namespaces that have been loaded.
        /// </summary>
        public static List<string> Namespaces { get; set; } = new();

        /// <summary>
        /// A list of all <see cref="Type"/> classes that have been loaded.
        /// </summary>
        public static List<Type> Types { get; set; }

        public static SolidColorBrush MethodBrush { get; }

        public static SolidColorBrush PropertyBrush { get; }

        public static SolidColorBrush SnippetBrush { get; }

        /// <summary>
        /// Constructs the static list of completion data based off of any class that has
        /// a <see cref="MoonSharpModuleAttribute"/> on it.
        /// </summary>
        static LuaCompletion()
        {
            MethodBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#B180D3")!;
            MethodBrush.Freeze();

            PropertyBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#75BCFF")!;
            PropertyBrush.Freeze();

            SnippetBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#57A64A")!;
            SnippetBrush.Freeze();

            CompletionData = new();

            // Construct the list of types we want to autocomplete for.
            Types = new List<Type>();

            // Get the MoonSharp types from the MoonSharp assembly.  This is basically the Lua
            // common library calls.
            foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                if (assemblyName?.Name == null || !assemblyName.Name.Contains("moonsharp", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var assembly = Assembly.Load(assemblyName);
                Types.AddRange(assembly.GetTypes().Where(x => x.IsDefined(typeof(MoonSharpModuleAttribute))));
            }

            // Get the custom types from this project
            Types.AddRange(Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsDefined(typeof(MoonSharpModuleAttribute))));

            // Go through the types
            foreach (var t in Types)
            {
                var attr = t.GetCustomAttributes(false)
                    .OfType<MoonSharpModuleAttribute>()
                    .SingleOrDefault();

                if (attr == null || string.IsNullOrWhiteSpace(attr.Namespace))
                {
                    continue;
                }

                List<LuaCompletionData>? entries;

                if (CompletionData.ContainsKey(attr.Namespace))
                {
                    entries = CompletionData[attr.Namespace];
                }
                else
                {
                    entries = new List<LuaCompletionData>();
                    CompletionData.Add(attr.Namespace, entries);
                    Namespaces.Add(attr.Namespace);
                }

                // Methods
                foreach (var method in t.GetMethods()
                             .Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object) && m.IsDefined(typeof(MoonSharpModuleMethodAttribute)))
                             .OrderBy(m => m.Name))
                {
                    var entry = entries.FirstOrDefault(x => x.Text == method.Name);

                    if (entry != null)
                    {
                        // It exists, therefore this is an overload.
                        ConstructMembers(method, entry);
                    }
                    else
                    {
                        var lcd = new LuaCompletionData(method.Name, "")
                        {
                            Icon = PackIconCodiconsKind.SymbolMethod
                        };

                        ConstructMembers(method, lcd);
                        entries.Add(lcd);
                    }
                }

                // Properties
                foreach (var prop in t.GetProperties().Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object)).OrderBy(m => m.Name))
                {
                    var entry = entries.FirstOrDefault(x => x.Text == prop.Name);

                    if (entry != null)
                    {
                        // It exists, therefore this is an overload.
                        ConstructProperties(prop, entry);
                    }
                    else
                    {
                        var lcd = new LuaCompletionData(prop.Name, "")
                        {
                            Icon = PackIconCodiconsKind.SymbolProperty
                        };

                        ConstructProperties(prop, lcd);
                        entries.Add(lcd);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the completion data based on the pattern.
        /// </summary>
        public static void LoadCompletionData(IList<ICompletionData> data, string pattern)
        {
            if (!CompletionData.ContainsKey(pattern))
            {
                return;
            }

            var entries = CompletionData[pattern];

            foreach (var entry in entries)
            {
                data.Add(entry);
            }
        }

        /// <summary>
        /// Constructs the data about a method and it's overloads.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="lcd"></param>
        private static void ConstructMembers(MethodInfo method, LuaCompletionData lcd)
        {
            // Whether to start with just the method name (for the first one) or append
            // the parameters as an overload.
            if (string.IsNullOrWhiteSpace((string)lcd.Description))
            {
                lcd.IconForeground = MethodBrush;
                lcd.IsMethod = true;

                if (method.CustomAttributes.Any())
                {
                    var attr = method.GetCustomAttributes<MoonSharpModuleMethodAttribute>().FirstOrDefault();

                    if (attr != null)
                    {
                        lcd.CodeMetadataControl.Description = attr.Description;
                        lcd.CodeMetadataControl.ReturnType = CleanupType(method.ReturnType.ToString());
                        lcd.CodeMetadataControl.MemberType = "snippet";

                        if (!string.IsNullOrWhiteSpace(attr.ReturnTypeHint))
                        {
                            lcd.CodeMetadataControl.ReturnType = attr.ReturnTypeHint;
                        }

                        lcd.Arguments = attr.ParameterCount;

                        // If it has an autocomplete hint then use it and get out, don't need to reflect.
                        if (!string.IsNullOrWhiteSpace(attr.AutoCompleteHint))
                        {
                            lcd.CodeMetadataControl.AutoCompleteHint += $"{attr.AutoCompleteHint}\r\n";
                            return;
                        }
                    }
                }
            }
            else
            {
                lcd.CodeMetadataControl.Description = (string)lcd.Description;
            }

            // There was no auto complete hint, try to construct it (this works great for .NET functions
            // but sub par for MoonSharp functions since Moonsharp infers it's arguments.
            var parameterDescriptions = string.Join
                (", ", method.GetParameters()
                     .Select(x => $"{x.ParameterType} {x.Name}")
                     .ToArray());

            lcd.CodeMetadataControl.ReturnType = LuaCompletion.CleanupType(method.ReturnType.ToString());
            lcd.CodeMetadataControl.AutoCompleteHint += CleanupType($"{method.Name}({parameterDescriptions})\r\n");
        }

        /// <summary>
        /// Constructs the data about a property.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="lcd"></param>
        private static void ConstructProperties(PropertyInfo prop, LuaCompletionData lcd)
        {
            // Whether to start with just the method name (for the first one) or append
            // the parameters as an overload.
            if (string.IsNullOrWhiteSpace((string)lcd.Description))
            {
                lcd.CodeMetadataControl.Description = (string)lcd.Description;
                lcd.CodeMetadataControl.ReturnType = CleanupType(prop.PropertyType.ToString());
                lcd.CodeMetadataControl.MemberType = "Property";
                lcd.IconForeground = PropertyBrush;
                lcd.IsProperty = true;

                if (prop.CustomAttributes.Any())
                {
                    var attr = prop.GetCustomAttributes<MoonSharpModuleMethodAttribute>().FirstOrDefault();

                    if (attr != null)
                    {
                        lcd.CodeMetadataControl.Description = attr.Description;
                    }

                    var attrDesc = prop.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();

                    if (attrDesc != null)
                    {
                        lcd.Description = attrDesc.Description;
                        lcd.CodeMetadataControl.Description = attrDesc.Description;
                    }

                    lcd.CodeMetadataControl.AutoCompleteHint = $"Gets or sets {lcd.CodeMetadataControl.ReturnType}";
                }
            }
            else
            {
                lcd.CodeMetadataControl.Description = (string)lcd.Description;
            }
        }

        /// <summary>
        /// Auto completion for snippets.
        /// </summary>
        /// <param name="data"></param>
        public static void LoadCompletionDataSnippets(IList<ICompletionData> data)
        {
            var lcd = new LuaCompletionData("For Loop", "A snippet to show how to do a basic for loop.", "");
            lcd.Icon = PackIconCodiconsKind.Code;
            lcd.IconForeground = SnippetBrush;
            lcd.CodeMetadataControl.MemberType = "For Loop";
            lcd.CodeMetadataControl.ReturnType = "snippet";
            data.Add(lcd);

            lcd = new LuaCompletionData("For Loop Pairs", "A snippet to show iterate over pairs.", "");
            lcd.Icon = PackIconCodiconsKind.Code;
            lcd.IconForeground = SnippetBrush;
            lcd.CodeMetadataControl.MemberType = "For Loop Pairs";
            lcd.CodeMetadataControl.ReturnType = "snippet";
            data.Add(lcd);

            lcd = new LuaCompletionData("Multiline String", "A snippet that creates a string variable with a multiline string syntax.", "");
            lcd.Icon = PackIconCodiconsKind.Code;
            lcd.IconForeground = SnippetBrush;
            lcd.CodeMetadataControl.MemberType = "Multiline String";
            lcd.CodeMetadataControl.ReturnType = "snippet";
            data.Add(lcd);

            lcd = new LuaCompletionData("While Loop", "A snippet to to show how to do a while loop with pausing.", "");
            lcd.Icon = PackIconCodiconsKind.Code;
            lcd.IconForeground = SnippetBrush;
            lcd.CodeMetadataControl.MemberType = "While Pairs";
            lcd.CodeMetadataControl.ReturnType = "snippet";
            data.Add(lcd);
        }

        /// <summary>
        /// Auto completion for all registered Lua namespaces.
        /// </summary>
        /// <param name="data"></param>
        public static void LoadCompletionDataNamespaces(IList<ICompletionData> data)
        {
            foreach (string ns in Namespaces.OrderBy(x => x))
            {
                var lcd = new LuaCompletionData(ns, "Lua namespace", "");
                lcd.Icon = PackIconCodiconsKind.SymbolNamespace;
                lcd.IconForeground = SnippetBrush;
                lcd.CodeMetadataControl.MemberType = ns;
                lcd.CodeMetadataControl.ReturnType = "";
                data.Add(lcd);
            }
        }

        /// <summary>
        /// Cleans up the type for autocomplete display.
        /// </summary>
        /// <param name="t"></param>
        public static string CleanupType(string t)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                sb.Append(t);
                sb.Replace("System.Void", "void");
                sb.Replace("System.String", "string");
                sb.Replace("System.Int32", "int");
                sb.Replace("System.UInt32", "int");
                sb.Replace("System.DataTime", "datetime");
                sb.Replace("System.Boolean", "bool");
                sb.Replace("System.Collections.Generic", "");
                sb.Replace("System.Object", "object");
                sb.Replace("System.Diagnostics.", "");
                sb.Replace("Avalon.Common.Models.", "");
                sb.Replace("MoonSharp.Interpreter.", "");
                sb.Replace("MahApps.Metro.IconPacks.", "");
                sb.Replace("LuaAutomation.Common.Windows.", "");
                sb.Replace("LuaAutomation.Common", "");
                sb.Replace("System.", "");
                sb.Replace("LuaAutomation.Lua.", "");
                sb.Replace("LuaAutomation.", "");
                sb.Replace(".IEnumerable`1[.Dictionary`2[string,object]]", "dictionary<string, object>");
                sb.Replace(".List`1[object]", "list<object>");
                sb.Replace(".List`1[string]", "list<string>");
                sb.Replace(".List`1[Double]", "list<double>");

                return sb.ToString();
            }
        }
    }
}
