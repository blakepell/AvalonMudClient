﻿// Disable warnings about XML documentation

#pragma warning disable 1591

using System;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing system related Lua functions from the 'os' module.
    /// Proper support requires a compatible IPlatformAccessor
    /// </summary>
    [MoonSharpModule(Namespace = "os")]
    public class OsSystemModule
    {
        [MoonSharpModuleMethod(Description = "Shells a process.",
            AutoCompleteHint = "os.execute(string path)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple")]
        public static DynValue execute(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args.AsType(0, "execute", DataType.String, true);

            if (v.IsNil())
            {
                return DynValue.NewBoolean(true);
            }

            try
            {
                int exitCode = Script.GlobalOptions.Platform.OS_Execute(v.String);

                return DynValue.NewTuple(
                    DynValue.Nil,
                    DynValue.NewString("exit"),
                    DynValue.NewNumber(exitCode));
            }
            catch (Exception)
            {
                // +++ bad to swallow.. 
                return DynValue.Nil;
            }
        }

        [MoonSharpModuleMethod(Description = "Exits the current process/program execution.",
            AutoCompleteHint = "os.exit()\r\nos.exit(int exitCode)",
            ParameterCount = 1,
            ReturnTypeHint = "void")]
        public static DynValue exit(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v_exitCode = args.AsType(0, "exit", DataType.Number, true);
            int exitCode = 0;

            if (v_exitCode.IsNotNil())
            {
                exitCode = (int) v_exitCode.Number;
            }

            Script.GlobalOptions.Platform.OS_ExitFast(exitCode);

            throw new InvalidOperationException("Unreachable code.. reached.");
        }

        [MoonSharpModuleMethod(Description = "Returns an environment variable.",
            AutoCompleteHint = "os.getenv(string variableName)",
            ParameterCount = 1,
            ReturnTypeHint = "string")]
        public static DynValue getenv(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var varName = args.AsType(0, "getenv", DataType.String);

            string val = Script.GlobalOptions.Platform.GetEnvironmentVariable(varName.String);

            if (val == null)
            {
                return DynValue.Nil;
            }

            return DynValue.NewString(val);
        }

        [MoonSharpModuleMethod(Description = "Removes a file.",
            AutoCompleteHint = "os.remove(string path)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple")]
        public static DynValue remove(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string fileName = args.AsType(0, "remove", DataType.String).String;

            try
            {
                if (Script.GlobalOptions.Platform.OS_FileExists(fileName))
                {
                    Script.GlobalOptions.Platform.OS_FileDelete(fileName);
                    return DynValue.True;
                }

                return DynValue.NewTuple(
                    DynValue.Nil,
                    DynValue.NewString("{0}: No such file or directory.", fileName),
                    DynValue.NewNumber(-1));
            }
            catch (Exception ex)
            {
                return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.Message), DynValue.NewNumber(-1));
            }
        }

        [MoonSharpModuleMethod(Description = "Renames a file.",
            AutoCompleteHint = "os.rename(string oldName, string newName)",
            ParameterCount = 2,
            ReturnTypeHint = "bool")]
        public static DynValue rename(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string fileNameOld = args.AsType(0, "rename", DataType.String).String;
            string fileNameNew = args.AsType(1, "rename", DataType.String).String;

            try
            {
                if (!Script.GlobalOptions.Platform.OS_FileExists(fileNameOld))
                {
                    return DynValue.NewTuple(DynValue.Nil,
                        DynValue.NewString("{0}: No such file or directory.", fileNameOld),
                        DynValue.NewNumber(-1));
                }

                Script.GlobalOptions.Platform.OS_FileMove(fileNameOld, fileNameNew);
                return DynValue.True;
            }
            catch (Exception ex)
            {
                return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.Message), DynValue.NewNumber(-1));
            }
        }

        [MoonSharpModuleMethod(Description = "Not Implemented",
            AutoCompleteHint = "os.ssetlocale()",
            ParameterCount = 0,
            ReturnTypeHint = "void")]
        public static DynValue setlocale(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewString("n/a");
        }

        [MoonSharpModuleMethod(Description = "Gets a temporary filename.",
            AutoCompleteHint = "os.tmpname",
            ParameterCount = 0,
            ReturnTypeHint = "string")]
        public static DynValue tmpname(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewString(Script.GlobalOptions.Platform.IO_OS_GetTempFilename());
        }
    }
}