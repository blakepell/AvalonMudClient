// Disable warnings about XML documentation

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using Cysharp.Text;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing time related Lua functions from the 'os' module.
    /// </summary>
    [MoonSharpModule(Namespace = "os")]
    public class OsTimeModule
    {
        private static DateTime Time0 = DateTime.UtcNow;
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static DynValue GetUnixTime(DateTime dateTime, DateTime? epoch = null)
        {
            double time = (dateTime - (epoch ?? Epoch)).TotalSeconds;

            if (time < 0.0)
            {
                return DynValue.Nil;
            }

            return DynValue.NewNumber(time);
        }

        private static DateTime FromUnixTime(double unixtime)
        {
            var ts = TimeSpan.FromSeconds(unixtime);
            return Epoch + ts;
        }

        [MoonSharpModuleMethod(Description = "Returns the number of seconds of CPU time a program has used.",
            AutoCompleteHint = "os.clock.()",
            ParameterCount = 0,
            ReturnTypeHint = "int")]
        public static DynValue clock(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var t = GetUnixTime(DateTime.UtcNow, Time0);

            if (t.IsNil())
            {
                return DynValue.NewNumber(0.0);
            }

            return t;
        }

        [MoonSharpModuleMethod(Description = "Returns the difference in time between two values.",
            AutoCompleteHint = "os.difftime(int value1, int value2)",
            ParameterCount = 2,
            ReturnTypeHint = "int")]
        public static DynValue difftime(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var t2 = args.AsType(0, "difftime", DataType.Number);
            var t1 = args.AsType(1, "difftime", DataType.Number, true);

            if (t1.IsNil())
            {
                return DynValue.NewNumber(t2.Number);
            }

            return DynValue.NewNumber(t2.Number - t1.Number);
        }

        [MoonSharpModuleMethod(Description = "Returns the current date and time.",
            AutoCompleteHint = "os.time()\r\nos.time{year=1970, month=1, day=1, hour=0, sec=1}",
            ParameterCount = 1,
            ReturnTypeHint = "int")]
        public static DynValue time(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var date = DateTime.UtcNow;

            if (args.Count > 0)
            {
                var vt = args.AsType(0, "time", DataType.Table, true);
                if (vt.Type == DataType.Table)
                {
                    date = ParseTimeTable(vt.Table);
                }
            }

            return GetUnixTime(date);
        }

        private static DateTime ParseTimeTable(Table t)
        {
            int sec = GetTimeTableField(t, "sec") ?? 0;
            int min = GetTimeTableField(t, "min") ?? 0;
            int hour = GetTimeTableField(t, "hour") ?? 12;
            var day = GetTimeTableField(t, "day");
            var month = GetTimeTableField(t, "month");
            var year = GetTimeTableField(t, "year");

            if (day == null)
            {
                throw new ScriptRuntimeException("field 'day' missing in date table");
            }

            if (month == null)
            {
                throw new ScriptRuntimeException("field 'month' missing in date table");
            }

            if (year == null)
            {
                throw new ScriptRuntimeException("field 'year' missing in date table");
            }

            return new DateTime(year.Value, month.Value, day.Value, hour, min, sec);
        }


        private static int? GetTimeTableField(Table t, string key)
        {
            var v = t.Get(key);
            var d = v.CastToNumber();

            if (d.HasValue)
            {
                return (int)d.Value;
            }

            return null;
        }

        [MoonSharpModuleMethod(Description = "Returns the date as a table or a string.",
            AutoCompleteHint = "os.date()\r\nos.date(string format, int datetime)",
            ParameterCount = 2,
            ReturnTypeHint = "string or table")]
        public static DynValue date(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var reference = DateTime.UtcNow;

            var vformat = args.AsType(0, "date", DataType.String, true);
            var vtime = args.AsType(1, "date", DataType.Number, true);

            string format = (vformat.IsNil()) ? "%c" : vformat.String;

            if (vtime.IsNotNil())
            {
                reference = FromUnixTime(vtime.Number);
            }

            bool isDst = false;

            if (format.StartsWith("!"))
            {
                format = format.Substring(1);
            }
            else
            {
                try
                {
                    reference = TimeZoneInfo.ConvertTimeFromUtc(reference, TimeZoneInfo.Local);
                    isDst = reference.IsDaylightSavingTime();
                }
                catch (TimeZoneNotFoundException)
                {
                    // this catches a weird mono bug: https://bugzilla.xamarin.com/show_bug.cgi?id=11817
                    // however the behavior is definitely not correct. damn.
                }
            }

            if (format == "*t")
            {
                var t = new Table(executionContext.GetScript());

                t.Set("year", DynValue.NewNumber(reference.Year));
                t.Set("month", DynValue.NewNumber(reference.Month));
                t.Set("day", DynValue.NewNumber(reference.Day));
                t.Set("hour", DynValue.NewNumber(reference.Hour));
                t.Set("min", DynValue.NewNumber(reference.Minute));
                t.Set("sec", DynValue.NewNumber(reference.Second));
                t.Set("wday", DynValue.NewNumber(((int)reference.DayOfWeek) + 1));
                t.Set("yday", DynValue.NewNumber(reference.DayOfYear));
                t.Set("isdst", DynValue.NewBoolean(isDst));

                return DynValue.NewTable(t);
            }

            return DynValue.NewString(StrFTime(format, reference));
        }

        private static string StrFTime(string format, DateTime d)
        {
            // ref: http://www.cplusplus.com/reference/ctime/strftime/

            var STANDARD_PATTERNS = new Dictionary<char, string>
            {
                {'a', "ddd"},
                {'A', "dddd"},
                {'b', "MMM"},
                {'B', "MMMM"},
                {'c', "f"},
                {'d', "dd"},
                {'D', "MM/dd/yy"},
                {'F', "yyyy-MM-dd"},
                {'g', "yy"},
                {'G', "yyyy"},
                {'h', "MMM"},
                {'H', "HH"},
                {'I', "hh"},
                {'m', "MM"},
                {'M', "mm"},
                {'p', "tt"},
                {'r', "h:mm:ss tt"},
                {'R', "HH:mm"},
                {'S', "ss"},
                {'T', "HH:mm:ss"},
                {'y', "yy"},
                {'Y', "yyyy"},
                {'x', "d"},
                {'X', "T"},
                {'z', "zzz"},
                {'Z', "zzz"}
            };


            using (var sb = ZString.CreateStringBuilder())
            {
                bool isEscapeSequence = false;

                for (int i = 0; i < format.Length; i++)
                {
                    char c = format[i];

                    if (c == '%')
                    {
                        if (isEscapeSequence)
                        {
                            sb.Append('%');
                            isEscapeSequence = false;
                        }
                        else
                        {
                            isEscapeSequence = true;
                        }

                        continue;
                    }

                    if (!isEscapeSequence)
                    {
                        sb.Append(c);
                        continue;
                    }

                    if (c == 'O' || c == 'E')
                    {
                        continue; // no modifiers
                    }

                    isEscapeSequence = false;

                    if (STANDARD_PATTERNS.TryGetValue(c, out string value))
                    {
                        sb.Append(d.ToString(value));
                    }
                    else if (c == 'e')
                    {
                        string s = d.ToString("%d");

                        if (s.Length < 2)
                        {
                            sb.Append(' ');
                            sb.Append(s);
                        }
                        else
                        {
                            sb.Append(s);
                        }
                    }
                    else if (c == 'n')
                    {
                        sb.Append('\n');
                    }
                    else if (c == 't')
                    {
                        sb.Append('\t');
                    }
                    else if (c == 'C')
                    {
                        sb.Append(d.Year / 100);
                    }
                    else if (c == 'j')
                    {
                        sb.Append(d.DayOfYear.ToString("000"));
                    }
                    else if (c == 'u')
                    {
                        int weekDay = (int)d.DayOfWeek;
                        if (weekDay == 0)
                        {
                            weekDay = 7;
                        }

                        sb.Append(weekDay);
                    }
                    else if (c == 'w')
                    {
                        int weekDay = (int)d.DayOfWeek;
                        sb.Append(weekDay);
                    }
                    else if (c == 'U')
                    {
                        // Week number with the first Sunday as the first day of week one (00-53)
                        sb.Append("??");
                    }
                    else if (c == 'V')
                    {
                        // ISO 8601 week number (00-53)
                        sb.Append("??");
                    }
                    else if (c == 'W')
                    {
                        // Week number with the first Monday as the first day of week one (00-53)
                        sb.Append("??");
                    }
                    else
                    {
                        throw new ScriptRuntimeException("bad argument #1 to 'date' (invalid conversion specifier '{0}')", format);
                    }
                }

                return sb.ToString();
            }
        }
    }
}