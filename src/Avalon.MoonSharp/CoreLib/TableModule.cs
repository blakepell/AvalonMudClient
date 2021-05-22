// Disable warnings about XML documentation

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Text;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing table Lua functions 
    /// </summary>
    [MoonSharpModule(Namespace = "table")]
    public class TableModule
    {
        [MoonSharpModuleMethod]
        public static DynValue unpack(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var s = args.AsType(0, "unpack", DataType.Table);
            var vi = args.AsType(1, "unpack", DataType.Number, true);
            var vj = args.AsType(2, "unpack", DataType.Number, true);

            int ii = vi.IsNil() ? 1 : (int) vi.Number;
            int ij = vj.IsNil() ? GetTableLength(executionContext, s) : (int) vj.Number;

            var t = s.Table;

            var v = new DynValue[ij - ii + 1];

            int tidx = 0;
            for (int i = ii; i <= ij; i++)
            {
                v[tidx++] = t.Get(i);
            }

            return DynValue.NewTuple(v);
        }

        [MoonSharpModuleMethod]
        public static DynValue pack(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var t = new Table(executionContext.GetScript());
            var v = DynValue.NewTable(t);

            for (int i = 0; i < args.Count; i++)
            {
                t.Set(i + 1, args[i]);
            }

            t.Set("n", DynValue.NewNumber(args.Count));

            return v;
        }

        [MoonSharpModuleMethod]
        public static DynValue sort(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vlist = args.AsType(0, "sort", DataType.Table);
            var lt = args[1];

            if (lt.Type != DataType.Function && lt.Type != DataType.ClrFunction && lt.IsNotNil())
            {
                args.AsType(1, "sort", DataType.Function, true); // this throws
            }

            int end = GetTableLength(executionContext, vlist);

            var values = new List<DynValue>();

            for (int i = 1; i <= end; i++)
            {
                values.Add(vlist.Table.Get(i));
            }

            try
            {
                values.Sort((a, b) => SortComparer(executionContext, a, b, lt));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is ScriptRuntimeException)
                {
                    throw ex.InnerException;
                }
            }

            for (int i = 0; i < values.Count; i++)
            {
                vlist.Table.Set(i + 1, values[i]);
            }

            return vlist;
        }

        private static int SortComparer(ScriptExecutionContext executionContext, DynValue a, DynValue b, DynValue lt)
        {
            if (lt == null || lt.IsNil())
            {
                lt = executionContext.GetBinaryMetamethod(a, b, "__lt");

                if (lt == null || lt.IsNil())
                {
                    if (a.Type == DataType.Number && b.Type == DataType.Number)
                    {
                        return a.Number.CompareTo(b.Number);
                    }

                    if (a.Type == DataType.String && b.Type == DataType.String)
                    {
                        return a.String.CompareTo(b.String);
                    }

                    throw ScriptRuntimeException.CompareInvalidType(a, b);
                }

                return LuaComparerToClrComparer(
                    executionContext.GetScript().Call(lt, a, b),
                    executionContext.GetScript().Call(lt, b, a));
            }

            return LuaComparerToClrComparer(
                executionContext.GetScript().Call(lt, a, b),
                executionContext.GetScript().Call(lt, b, a));
        }

        private static int LuaComparerToClrComparer(DynValue dynValue1, DynValue dynValue2)
        {
            bool v1 = dynValue1.CastToBool();
            bool v2 = dynValue2.CastToBool();

            if (v1 && !v2)
            {
                return -1;
            }

            if (v2 && !v1)
            {
                return 1;
            }

            if (v1 || v2)
            {
                throw new ScriptRuntimeException("invalid order function for sorting");
            }

            return 0;
        }

        [MoonSharpModuleMethod]
        public static DynValue insert(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vlist = args.AsType(0, "table.insert", DataType.Table);
            var vpos = args[1];
            var vvalue = args[2];

            if (args.Count > 3)
            {
                throw new ScriptRuntimeException("wrong number of arguments to 'insert'");
            }

            int len = GetTableLength(executionContext, vlist);
            var list = vlist.Table;

            if (args.Count == 2)
            {
                vvalue = vpos;
                vpos = DynValue.NewNumber(len + 1);
            }

            if (vpos.Type != DataType.Number)
            {
                throw ScriptRuntimeException.BadArgument(1, "table.insert", DataType.Number, vpos.Type, false);
            }

            int pos = (int) vpos.Number;

            if (pos > len + 1 || pos < 1)
            {
                throw new ScriptRuntimeException("bad argument #2 to 'insert' (position out of bounds)");
            }

            for (int i = len; i >= pos; i--)
            {
                list.Set(i + 1, list.Get(i));
            }

            list.Set(pos, vvalue);

            return vlist;
        }


        [MoonSharpModuleMethod]
        public static DynValue remove(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vlist = args.AsType(0, "table.remove", DataType.Table);
            var vpos = args.AsType(1, "table.remove", DataType.Number, true);
            var ret = DynValue.Nil;

            if (args.Count > 2)
            {
                throw new ScriptRuntimeException("wrong number of arguments to 'remove'");
            }

            int len = GetTableLength(executionContext, vlist);
            var list = vlist.Table;

            int pos = vpos.IsNil() ? len : (int) vpos.Number;

            if (pos >= len + 1 || (pos < 1 && len > 0))
            {
                throw new ScriptRuntimeException("bad argument #1 to 'remove' (position out of bounds)");
            }

            for (int i = pos; i <= len; i++)
            {
                if (i == pos)
                {
                    ret = list.Get(i);
                }

                list.Set(i, list.Get(i + 1));
            }

            return ret;
        }


        //table.concat (list [, sep [, i [, j]]])
        //Given a list where all elements are strings or numbers, returns the string list[i]..sep..list[i+1] (...) sep..list[j]. 
        //The default value for sep is the empty string, the default for i is 1, and the default for j is #list. If i is greater 
        //than j, returns the empty string. 
        [MoonSharpModuleMethod]
        public static DynValue concat(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vlist = args.AsType(0, "concat", DataType.Table);
            var vsep = args.AsType(1, "concat", DataType.String, true);
            var vstart = args.AsType(2, "concat", DataType.Number, true);
            var vend = args.AsType(3, "concat", DataType.Number, true);

            var list = vlist.Table;
            string sep = vsep.IsNil() ? "" : vsep.String;
            int start = vstart.IsNilOrNan() ? 1 : (int) vstart.Number;
            int end;

            if (vend.IsNilOrNan())
            {
                end = GetTableLength(executionContext, vlist);
            }
            else
            {
                end = (int) vend.Number;
            }

            if (end < start)
            {
                return DynValue.NewString(string.Empty);
            }

            using (var sb = ZString.CreateStringBuilder())
            {
                for (int i = start; i <= end; i++)
                {
                    var v = list.Get(i);

                    if (v.Type != DataType.Number && v.Type != DataType.String)
                    {
                        throw new ScriptRuntimeException("invalid value ({1}) at index {0} in table for 'concat'", i, v.Type.ToLuaTypeString());
                    }

                    string s = v.ToPrintString();

                    if (i != start)
                    {
                        sb.Append(sep);
                    }

                    sb.Append(s);
                }

                return DynValue.NewString(sb.ToString());
            }
        }

        private static int GetTableLength(ScriptExecutionContext executionContext, DynValue vlist)
        {
            var __len = executionContext.GetMetamethod(vlist, "__len");

            if (__len != null)
            {
                var lenv = executionContext.GetScript().Call(__len, vlist);

                var len = lenv.CastToNumber();

                if (len == null)
                {
                    throw new ScriptRuntimeException("object length is not a number");
                }

                return (int) len;
            }

            return vlist.Table.Length;
        }
    }


    /// <summary>
    /// Class exposing table.unpack and table.pack in the global namespace (to work around the most common Lua 5.1 compatibility issue).
    /// </summary>
    [MoonSharpModule]
    public class TableModule_Globals
    {
        [MoonSharpModuleMethod]
        public static DynValue unpack(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return TableModule.unpack(executionContext, args);
        }

        [MoonSharpModuleMethod]
        public static DynValue pack(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return TableModule.pack(executionContext, args);
        }
    }
}