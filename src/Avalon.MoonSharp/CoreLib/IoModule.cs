// Disable warnings about XML documentation

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.CoreLib.IO;
using MoonSharp.Interpreter.Platforms;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing io Lua functions. Proper support requires a compatible IPlatformAccessor
    /// </summary>
    [MoonSharpModule(Namespace = "io")]
    public class IoModule
    {
        public static void MoonSharpInit(Table globalTable, Table ioTable)
        {
            UserData.RegisterType<FileUserDataBase>(InteropAccessMode.Default, "file");

            var meta = new Table(ioTable.OwnerScript);
            var __index = DynValue.NewCallback(new CallbackFunction(__index_callback, "__index_callback"));
            meta.Set("__index", __index);
            ioTable.MetaTable = meta;

            SetStandardFile(globalTable.OwnerScript, StandardFileType.StdIn, globalTable.OwnerScript.Options.Stdin);
            SetStandardFile(globalTable.OwnerScript, StandardFileType.StdOut, globalTable.OwnerScript.Options.Stdout);
            SetStandardFile(globalTable.OwnerScript, StandardFileType.StdErr, globalTable.OwnerScript.Options.Stderr);
        }

        private static DynValue __index_callback(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string name = args[1].CastToString();

            if (name == "stdin")
            {
                return GetStandardFile(executionContext.GetScript(), StandardFileType.StdIn);
            }

            if (name == "stdout")
            {
                return GetStandardFile(executionContext.GetScript(), StandardFileType.StdOut);
            }

            if (name == "stderr")
            {
                return GetStandardFile(executionContext.GetScript(), StandardFileType.StdErr);
            }

            return DynValue.Nil;
        }

        private static DynValue GetStandardFile(Script S, StandardFileType file)
        {
            var R = S.Registry;

            var ff = R.Get("853BEAAF298648839E2C99D005E1DF94_STD_" + file);
            return ff;
        }

        private static void SetStandardFile(Script S, StandardFileType file, Stream optionsStream)
        {
            var R = S.Registry;

            optionsStream = optionsStream ?? Script.GlobalOptions.Platform.IO_GetStandardStream(file);

            FileUserDataBase udb;

            if (file == StandardFileType.StdIn)
            {
                udb = StandardIOFileUserDataBase.CreateInputStream(optionsStream);
            }
            else
            {
                udb = StandardIOFileUserDataBase.CreateOutputStream(optionsStream);
            }

            R.Set("853BEAAF298648839E2C99D005E1DF94_STD_" + file, UserData.Create(udb));
        }


        private static FileUserDataBase GetDefaultFile(ScriptExecutionContext executionContext, StandardFileType file)
        {
            var R = executionContext.GetScript().Registry;

            var ff = R.Get("853BEAAF298648839E2C99D005E1DF94_" + file);

            if (ff.IsNil())
            {
                ff = GetStandardFile(executionContext.GetScript(), file);
            }

            return ff.CheckUserDataType<FileUserDataBase>("getdefaultfile(" + file + ")");
        }


        private static void SetDefaultFile(ScriptExecutionContext executionContext, StandardFileType file,
            FileUserDataBase fileHandle)
        {
            SetDefaultFile(executionContext.GetScript(), file, fileHandle);
        }

        internal static void SetDefaultFile(Script script, StandardFileType file, FileUserDataBase fileHandle)
        {
            var R = script.Registry;
            R.Set("853BEAAF298648839E2C99D005E1DF94_" + file, UserData.Create(fileHandle));
        }

        public static void SetDefaultFile(Script script, StandardFileType file, Stream stream)
        {
            if (file == StandardFileType.StdIn)
            {
                SetDefaultFile(script, file, StandardIOFileUserDataBase.CreateInputStream(stream));
            }
            else
            {
                SetDefaultFile(script, file, StandardIOFileUserDataBase.CreateOutputStream(stream));
            }
        }


        [MoonSharpModuleMethod(Description = "Closes the current io stream.",
            AutoCompleteHint = "io.close()",
            ParameterCount = 0,
            ReturnTypeHint = "tuple<nil, string>")]
        public static DynValue close(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var outp = args.AsUserData<FileUserDataBase>(0, "close", true) ??
                       GetDefaultFile(executionContext, StandardFileType.StdOut);
            return outp.close(executionContext, args);
        }

        [MoonSharpModuleMethod(Description = "Flushes the current io stream.",
            AutoCompleteHint = "io.flush()",
            ParameterCount = 0,
            ReturnTypeHint = "bool")]
        public static DynValue flush(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var outp = args.AsUserData<FileUserDataBase>(0, "close", true) ??
                       GetDefaultFile(executionContext, StandardFileType.StdOut);
            outp.flush();
            return DynValue.True;
        }


        [MoonSharpModuleMethod(Description = "When called with a file name, it opens the named file (in text mode), and sets its handle as the default input file. When called with a file handle, it simply sets this file handle as the default input file. When called without parameters, it returns the current default input file. o In case of errors this function raises the error, instead of returning an error code.",
            AutoCompleteHint = "io.input(string file)",
            ParameterCount = 1,
            ReturnTypeHint = "object")]
        public static DynValue input(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return HandleDefaultStreamSetter(executionContext, args, StandardFileType.StdIn);
        }

        [MoonSharpModuleMethod(Description = "Sets the output to a file.",
            AutoCompleteHint = "io.output(string filenme)",
            ParameterCount = 0,
            ReturnTypeHint = "object")]
        public static DynValue output(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return HandleDefaultStreamSetter(executionContext, args, StandardFileType.StdOut);
        }

        private static DynValue HandleDefaultStreamSetter(ScriptExecutionContext executionContext,
            CallbackArguments args, StandardFileType defaultFiles)
        {
            if (args.Count == 0 || args[0].IsNil())
            {
                var file = GetDefaultFile(executionContext, defaultFiles);
                return UserData.Create(file);
            }

            FileUserDataBase inp;

            if (args[0].Type == DataType.String || args[0].Type == DataType.Number)
            {
                string fileName = args[0].CastToString();
                inp = Open(executionContext, fileName, GetUTF8Encoding(),
                    defaultFiles == StandardFileType.StdIn ? "r" : "w");
            }
            else
            {
                inp = args.AsUserData<FileUserDataBase>(0, defaultFiles == StandardFileType.StdIn ? "input" : "output");
            }

            SetDefaultFile(executionContext, defaultFiles, inp);

            return UserData.Create(inp);
        }

        private static Encoding GetUTF8Encoding()
        {
            return new UTF8Encoding(false);
        }

        [MoonSharpModuleMethod(Description = "Returns an iterator function that each time called returns a new line from the file.",
            AutoCompleteHint = "io.lines()",
            ParameterCount = 0,
            ReturnTypeHint = "enumerable<string>")]
        public static DynValue lines(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string filename = args.AsType(0, "lines", DataType.String).String;

            try
            {
                var readLines = new List<DynValue>();

                using (var stream =
                    Script.GlobalOptions.Platform.IO_OpenFile(executionContext.GetScript(), filename, null, "r"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            readLines.Add(DynValue.NewString(line));
                        }
                    }
                }

                readLines.Add(DynValue.Nil);

                return DynValue.FromObject(executionContext.GetScript(), readLines.Select(s => s));
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(IoExceptionToLuaMessage(ex, filename));
            }
        }

        [MoonSharpModuleMethod(Description = "Opens an output file.",
            AutoCompleteHint = "io.open(string filename, string mode, string encoding)",
            ParameterCount = 0,
            ReturnTypeHint = "object")]
        public static DynValue open(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string filename = args.AsType(0, "open", DataType.String).String;
            var vmode = args.AsType(1, "open", DataType.String, true);
            var vencoding = args.AsType(2, "open", DataType.String, true);

            string mode = vmode.IsNil() ? "r" : vmode.String;

            string invalidChars = mode.Replace("+", "")
                .Replace("r", "")
                .Replace("a", "")
                .Replace("w", "")
                .Replace("b", "")
                .Replace("t", "");

            if (invalidChars.Length > 0)
            {
                throw ScriptRuntimeException.BadArgument(1, "open", "invalid mode");
            }


            try
            {
                string encoding = vencoding.IsNil() ? null : vencoding.String;

                // list of codes: http://msdn.microsoft.com/en-us/library/vstudio/system.text.encoding%28v=vs.90%29.aspx.
                // In addition, "binary" is available.
                Encoding e;
                bool isBinary = Framework.Do.StringContainsChar(mode, 'b');

                if (encoding == "binary")
                {
                    e = new BinaryEncoding();
                }
                else if (encoding == null)
                {
                    if (!isBinary)
                    {
                        e = GetUTF8Encoding();
                    }
                    else
                    {
                        e = new BinaryEncoding();
                    }
                }
                else
                {
                    if (isBinary)
                    {
                        throw new ScriptRuntimeException(
                            "Can't specify encodings other than nil or 'binary' for binary streams.");
                    }

                    e = Encoding.GetEncoding(encoding);
                }

                return UserData.Create(Open(executionContext, filename, e, mode));
            }
            catch (Exception ex)
            {
                return DynValue.NewTuple(DynValue.Nil,
                    DynValue.NewString(IoExceptionToLuaMessage(ex, filename)));
            }
        }

        public static string IoExceptionToLuaMessage(Exception ex, string filename)
        {
            if (ex is FileNotFoundException)
            {
                return string.Format("{0}: No such file or directory", filename);
            }

            return ex.Message;
        }

        [MoonSharpModuleMethod(Description = "Checks whether an object is a valid file handle.",
            AutoCompleteHint = "io.type(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "string")]
        public static DynValue type(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            if (args[0].Type != DataType.UserData)
            {
                return DynValue.Nil;
            }

            var file = args[0].UserData.Object as FileUserDataBase;

            if (file == null)
            {
                return DynValue.Nil;
            }

            if (file.isopen())
            {
                return DynValue.NewString("file");
            }

            return DynValue.NewString("closed file");
        }

        [MoonSharpModuleMethod(Description = "Checks whether an object is a valid file handle.",
            AutoCompleteHint = "io.typebool(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "bool")]
        public static DynValue typebool(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            if (args[0].Type != DataType.UserData)
            {
                return DynValue.Nil;
            }

            var file = args[0].UserData.Object as FileUserDataBase;

            if (file == null)
            {
                return DynValue.Nil;
            }

            if (file.isopen())
            {
                return DynValue.NewBoolean(true);
            }

            return DynValue.NewBoolean(false);
        }

        [MoonSharpModuleMethod(Description = "Reads from a file.",
            AutoCompleteHint = "io.read(file f)",
            ParameterCount = 1,
            ReturnTypeHint = "string")]
        public static DynValue read(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var file = GetDefaultFile(executionContext, StandardFileType.StdIn);
            return file.read(executionContext, args);
        }

        [MoonSharpModuleMethod(Description = "Writes to a file.",
            AutoCompleteHint = "io.write(string value)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple<nil, string>")]
        public static DynValue write(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var file = GetDefaultFile(executionContext, StandardFileType.StdOut);
            return file.write(executionContext, args);
        }

        [MoonSharpModuleMethod(Description = "Returns an open temporary file.",
            AutoCompleteHint = "io.tmpfile()",
            ParameterCount = 0,
            ReturnTypeHint = "file")]
        public static DynValue tmpfile(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            string tmpfilename = Script.GlobalOptions.Platform.IO_OS_GetTempFilename();
            var file = Open(executionContext, tmpfilename, GetUTF8Encoding(), "w");
            return UserData.Create(file);
        }

        private static FileUserDataBase Open(ScriptExecutionContext executionContext, string filename,
            Encoding encoding, string mode)
        {
            return new FileUserData(executionContext.GetScript(), filename, encoding, mode);
        }
    }
}