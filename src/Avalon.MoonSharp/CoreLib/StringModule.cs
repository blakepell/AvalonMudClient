// Disable warnings about XML documentation

#pragma warning disable 1591

using Cysharp.Text;
using MoonSharp.Interpreter.CoreLib.StringLib;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing string Lua functions 
    /// </summary>
    [MoonSharpModule(Namespace = "string")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class StringModule
    {
        public static void MoonSharpInit(Table globalTable, Table stringTable)
        {
            var stringMetatable = new Table(globalTable.OwnerScript);
            stringMetatable.Set("__index", DynValue.NewTable(stringTable));
            globalTable.OwnerScript.SetTypeMetatable(DataType.String, stringMetatable);
        }

        [MoonSharpModuleMethod]
        public static DynValue @char(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                for (int i = 0; i < args.Count; i++)
                {
                    var v = args[i];
                    double d = 0d;

                    if (v.Type == DataType.String)
                    {
                        var nd = v.CastToNumber();

                        if (nd == null)
                        {
                            args.AsType(i, "char", DataType.Number);
                        }
                        else
                        {
                            d = nd.Value;
                        }
                    }
                    else
                    {
                        args.AsType(i, "char", DataType.Number);
                        d = v.Number;
                    }

                    sb.Append((char)(d));
                }

                return DynValue.NewString(sb.ToString());
            }
        }


        [MoonSharpModuleMethod]
        public static DynValue @byte(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vs = args.AsType(0, "byte", DataType.String);
            var vi = args.AsType(1, "byte", DataType.Number, true);
            var vj = args.AsType(2, "byte", DataType.Number, true);

            return PerformByteLike(vs, vi, vj, i => Unicode2Ascii(i));
        }

        [MoonSharpModuleMethod]
        public static DynValue unicode(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vs = args.AsType(0, "unicode", DataType.String);
            var vi = args.AsType(1, "unicode", DataType.Number, true);
            var vj = args.AsType(2, "unicode", DataType.Number, true);

            return PerformByteLike(vs, vi, vj, i => i);
        }

        private static int Unicode2Ascii(int i)
        {
            if (i >= 0 && i <= 255)
            {
                return i;
            }

            return '?';
        }

        private static DynValue PerformByteLike(DynValue vs, DynValue vi, DynValue vj, Func<int, int> filter)
        {
            var range = StringRange.FromLuaRange(vi, vj);
            string s = range.ApplyToString(vs.String);

            int length = s.Length;
            var rets = new DynValue[length];

            for (int i = 0; i < length; ++i)
            {
                rets[i] = DynValue.NewNumber(filter(s[i]));
            }

            return DynValue.NewTuple(rets);
        }


        private static int? AdjustIndex(string s, DynValue vi, int defval)
        {
            if (vi.IsNil())
            {
                return defval;
            }

            int i = (int)Math.Round(vi.Number, 0);

            if (i == 0)
            {
                return null;
            }

            if (i > 0)
            {
                return i - 1;
            }

            return s.Length - i;
        }

        [MoonSharpModuleMethod]
        public static DynValue len(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vs = args.AsType(0, "len", DataType.String);
            return DynValue.NewNumber(vs.String.Length);
        }


        [MoonSharpModuleMethod]
        public static DynValue match(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return executionContext.EmulateClassicCall(args, "match", KopiLua_StringLib.str_match);
        }


        [MoonSharpModuleMethod]
        public static DynValue gmatch(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return executionContext.EmulateClassicCall(args, "gmatch", KopiLua_StringLib.str_gmatch);
        }

        [MoonSharpModuleMethod]
        public static DynValue gsub(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return executionContext.EmulateClassicCall(args, "gsub", KopiLua_StringLib.str_gsub);
        }

        [MoonSharpModuleMethod]
        public static DynValue find(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return executionContext.EmulateClassicCall(args, "find",
                KopiLua_StringLib.str_find);
        }


        [MoonSharpModuleMethod]
        public static DynValue lower(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s = args.AsType(0, "lower", DataType.String);
            return DynValue.NewString(arg_s.String.ToLower());
        }

        [MoonSharpModuleMethod]
        public static DynValue upper(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s = args.AsType(0, "upper", DataType.String);
            return DynValue.NewString(arg_s.String.ToUpper());
        }

        [MoonSharpModuleMethod]
        public static DynValue rep(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s = args.AsType(0, "rep", DataType.String);
            var arg_n = args.AsType(1, "rep", DataType.Number);
            var arg_sep = args.AsType(2, "rep", DataType.String, true);

            if (string.IsNullOrEmpty(arg_s.String) || (arg_n.Number < 1))
            {
                return DynValue.NewString("");
            }

            string sep = (arg_sep.IsNotNil()) ? arg_sep.String : null;

            int count = (int)arg_n.Number;

            using (var sb = ZString.CreateStringBuilder())
            {
                for (int i = 0; i < count; ++i)
                {
                    if (i != 0 && sep != null)
                    {
                        sb.Append(sep);
                    }

                    sb.Append(arg_s.String);
                }

                return DynValue.NewString(sb.ToString());
            }
        }

        [MoonSharpModuleMethod]
        public static DynValue format(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return executionContext.EmulateClassicCall(args, "format", KopiLua_StringLib.str_format);
        }


        [MoonSharpModuleMethod]
        public static DynValue reverse(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s = args.AsType(0, "reverse", DataType.String);

            if (string.IsNullOrEmpty(arg_s.String))
            {
                return DynValue.NewString("");
            }

            var elements = arg_s.String.ToCharArray();
            Array.Reverse(elements);

            return DynValue.NewString(new string(elements));
        }

        [MoonSharpModuleMethod]
        public static DynValue sub(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s = args.AsType(0, "sub", DataType.String);
            var arg_i = args.AsType(1, "sub", DataType.Number, true);
            var arg_j = args.AsType(2, "sub", DataType.Number, true);

            var range = StringRange.FromLuaRange(arg_i, arg_j, -1);
            string s = range.ApplyToString(arg_s.String);

            return DynValue.NewString(s);
        }

        [MoonSharpModuleMethod]
        public static DynValue startsWith(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s1 = args.AsType(0, "startsWith", DataType.String, true);
            var arg_s2 = args.AsType(1, "startsWith", DataType.String, true);

            if (arg_s1.IsNil() || arg_s2.IsNil())
            {
                return DynValue.False;
            }

            return DynValue.NewBoolean(arg_s1.String.StartsWith(arg_s2.String));
        }

        [MoonSharpModuleMethod]
        public static DynValue endsWith(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s1 = args.AsType(0, "endsWith", DataType.String, true);
            var arg_s2 = args.AsType(1, "endsWith", DataType.String, true);

            if (arg_s1.IsNil() || arg_s2.IsNil())
            {
                return DynValue.False;
            }

            return DynValue.NewBoolean(arg_s1.String.EndsWith(arg_s2.String));
        }

        [MoonSharpModuleMethod]
        public static DynValue contains(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg_s1 = args.AsType(0, "contains", DataType.String, true);
            var arg_s2 = args.AsType(1, "contains", DataType.String, true);

            if (arg_s1.IsNil() || arg_s2.IsNil())
            {
                return DynValue.False;
            }

            return DynValue.NewBoolean(arg_s1.String.Contains(arg_s2.String));
        }

        [MoonSharpModuleMethod]
        public static DynValue guid(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewString(Guid.NewGuid().ToString());
        }

        [MoonSharpModuleMethod]
        public static DynValue doink(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewString("hellllo");
        }

        [MoonSharpModuleMethod]
        public static DynValue left(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg1 = args.AsType(0, "left", DataType.String, true);
            var arg2 = args.AsType(1, "left", DataType.Number, true);

            if (arg1.IsNil() || arg2.IsNil())
            {
                return DynValue.NewString("");
            }

            int length = (int)arg2.Number;

            if (length >= arg1.String.Length)
            {
                return DynValue.NewString(arg1.String);
            }

            return DynValue.NewString(arg1.String.Substring(0, length));
        }

        [MoonSharpModuleMethod]
        public static DynValue right(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg1 = args.AsType(0, "right", DataType.String, true);
            var arg2 = args.AsType(1, "right", DataType.Number, true);
            
            if (arg1.IsNil() || arg2.IsNil() || arg2.Number <= 0)
            {
                return DynValue.NewString("");
            }
            
            int length = (int)arg2.Number;

            if (length >= arg1.String.Length)
            {
                return DynValue.NewString(arg1.String);
            }

            return DynValue.NewString(arg1.String.Substring(arg1.String.Length - length, length));
        }

        [MoonSharpModuleMethod]
        public static DynValue mid(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg1 = args.AsType(0, "mid", DataType.String, true);
            var arg2 = args.AsType(1, "mid", DataType.Number, true);
            var arg3 = args.AsType(2, "mid", DataType.Number, true);

            if (arg1.IsNil() || arg2.IsNil() || arg3.IsNil())
            {
                return DynValue.NewString("");
            }

            int startIndex = (int)arg2.Number - 1;
            int length = (int)arg3.Number;

            // We don't have a zero index in Lua.. so if someone asks for it convert it to 1.
            if (startIndex == 0)
            {
                startIndex = 1;
            }

            if (startIndex + length > arg1.String.Length + 1)
            {
                return DynValue.NewString(arg1.String.Substring(startIndex));
            }

            return DynValue.NewString(arg1.String.Substring(startIndex, length));
        }
    }
}