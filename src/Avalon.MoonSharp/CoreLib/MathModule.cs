// Disable warnings about XML documentation

#pragma warning disable 1591

using System;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing math Lua functions 
    /// </summary>
    [MoonSharpModule(Namespace = "math")]
    public class MathModule
    {
        [MoonSharpModuleMethod(Description = "Returns Pi",
            AutoCompleteHint = "math.pi()",
            ParameterCount = 0,
            ReturnTypeHint = "double")]
        public const double pi = Math.PI;

        [MoonSharpModuleMethod(Description = "Returns the max numeric value.",
            AutoCompleteHint = "math.huge()",
            ParameterCount = 0,
            ReturnTypeHint = "double")]
        public const double huge = double.MaxValue;

        private static Random GetRandom(Script s)
        {
            var rr = s.Registry.Get("F61E3AA7247D4D1EB7A45430B0C8C9BB_MATH_RANDOM");
            return (rr.UserData.Object as AnonWrapper<Random>).Value;
        }

        private static void SetRandom(Script s, Random random)
        {
            var rr = UserData.Create(new AnonWrapper<Random>(random));
            s.Registry.Set("F61E3AA7247D4D1EB7A45430B0C8C9BB_MATH_RANDOM", rr);
        }

        public static void MoonSharpInit(Table globalTable, Table ioTable)
        {
            SetRandom(globalTable.OwnerScript, new Random());
        }

        private static DynValue exec1(CallbackArguments args, string funcName, Func<double, double> func)
        {
            var arg = args.AsType(0, funcName, DataType.Number);
            return DynValue.NewNumber(func(arg.Number));
        }

        private static DynValue exec2(CallbackArguments args, string funcName, Func<double, double, double> func)
        {
            var arg = args.AsType(0, funcName, DataType.Number);
            var arg2 = args.AsType(1, funcName, DataType.Number);
            return DynValue.NewNumber(func(arg.Number, arg2.Number));
        }

        private static DynValue exec2n(CallbackArguments args, string funcName, double defVal,
            Func<double, double, double> func)
        {
            var arg = args.AsType(0, funcName, DataType.Number);
            var arg2 = args.AsType(1, funcName, DataType.Number, true);

            return DynValue.NewNumber(func(arg.Number, arg2.IsNil() ? defVal : arg2.Number));
        }

        private static DynValue execaccum(CallbackArguments args, string funcName, Func<double, double, double> func)
        {
            double accum = double.NaN;

            if (args.Count == 0)
            {
                throw new ScriptRuntimeException("bad argument #1 to '{0}' (number expected, got no value)", funcName);
            }

            for (int i = 0; i < args.Count; i++)
            {
                var arg = args.AsType(i, funcName, DataType.Number);

                if (i == 0)
                {
                    accum = arg.Number;
                }
                else
                {
                    accum = func(accum, arg.Number);
                }
            }

            return DynValue.NewNumber(accum);
        }


        [MoonSharpModuleMethod(Description = "Returns the absolute value of a number.",
            AutoCompleteHint = "math.abs(int value)",
            ParameterCount = 1,
            ReturnTypeHint = "int")]
        public static DynValue abs(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "abs", d => Math.Abs(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the arc cosine of X (in radians)",
            AutoCompleteHint = "math.acos(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue acos(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "acos", d => Math.Acos(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the arc sine of X (in radians)",
            AutoCompleteHint = "math.asin(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue asin(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "asin", d => Math.Asin(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the arc tangent of X (in radians)",
            AutoCompleteHint = "math.atan(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue atan(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "atan", d => Math.Atan(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the arc tangent of y/x (in radians), but uses the signs of both parameters to find the quadrant of the result. (It also handles correctly the case of x being zero.)",
            AutoCompleteHint = "math.atan2(double value, double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue atan2(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "atan2", (d1, d2) => Math.Atan2(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns the smallest integer larger than or equal to x.",
            AutoCompleteHint = "math.ceiling(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue ceil(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "ceil", d => Math.Ceiling(d));
        }

        [MoonSharpModuleMethod(Description = "Returns cosine of x.",
            AutoCompleteHint = "math.cos(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue cos(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "cos", d => Math.Cos(d));
        }

        [MoonSharpModuleMethod(Description = "Returns hyperbolic cosine of x.",
            AutoCompleteHint = "math.cos(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue cosh(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "cosh", d => Math.Cosh(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the angle x (given in radians) in degrees.",
            AutoCompleteHint = "math.deg(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue deg(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "deg", d => d * 180.0 / Math.PI);
        }

        [MoonSharpModuleMethod(Description = "Returns the value of e^x.",
            AutoCompleteHint = "math.exp(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue exp(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "exp", d => Math.Exp(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the largest interger smaller than or equal to x.",
            AutoCompleteHint = "math.floor(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue floor(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "floor", d => Math.Floor(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the remainder of the division of x by y that rounds the quotient towards zero.",
            AutoCompleteHint = "math.fmod(double value1, double value2)",
            ParameterCount = 2,
            ReturnTypeHint = "double")]
        public static DynValue fmod(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "fmod", (d1, d2) => Math.IEEERemainder(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns m and e such that x = m2e, e is an integer and the absolute value of m is in the range [0.5, 1) (or zero when x is zero).",
            AutoCompleteHint = "math.exp(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple<double, double>")]
        public static DynValue frexp(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            // http://stackoverflow.com/questions/389993/extracting-mantissa-and-exponent-from-double-in-c-sharp

            var arg = args.AsType(0, "frexp", DataType.Number);

            double d = arg.Number;

            // Translate the double into sign, exponent and mantissa.
            long bits = BitConverter.DoubleToInt64Bits(d);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            bool negative = (bits < 0);
            int exponent = (int) ((bits >> 52) & 0x7ffL);
            long mantissa = bits & 0xfffffffffffffL;

            // Subnormal numbers; exponent is effectively one higher,
            // but there's no extra normalisation bit in the mantissa
            if (exponent == 0)
            {
                exponent++;
            }
            // Normal numbers; leave exponent as it is but add extra
            // bit to the front of the mantissa
            else
            {
                mantissa = mantissa | (1L << 52);
            }

            // Bias the exponent. It's actually biased by 1023, but we're
            // treating the mantissa as m.0 rather than 0.m, so we need
            // to subtract another 52 from it.
            exponent -= 1075;

            if (mantissa == 0)
            {
                return DynValue.NewTuple(DynValue.NewNumber(0), DynValue.NewNumber(0));
            }

            /* Normalize */
            while ((mantissa & 1) == 0)
            {
                /*  i.e., Mantissa is even */
                mantissa >>= 1;
                exponent++;
            }

            double m = mantissa;
            double e = exponent;
            while (m >= 1)
            {
                m /= 2.0;
                e += 1.0;
            }

            if (negative)
            {
                m = -m;
            }

            return DynValue.NewTuple(DynValue.NewNumber(m), DynValue.NewNumber(e));
        }

        [MoonSharpModuleMethod(Description = "Returns m2^e",
            AutoCompleteHint = "math.ldexp(double value1, double value2)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue ldexp(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "ldexp", (d1, d2) => d1 * Math.Pow(2, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns the natural logarithm of x.",
            AutoCompleteHint = "math.log(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue log(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2n(args, "log", Math.E, (d1, d2) => Math.Log(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns the maximum value among its arguments.",
            AutoCompleteHint = "math.max(double value1, double value2)",
            ParameterCount = 2,
            ReturnTypeHint = "double")]
        public static DynValue max(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return execaccum(args, "max", (d1, d2) => Math.Max(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns the minimum value among its arguments.",
            AutoCompleteHint = "math.min(double value1, double value2)",
            ParameterCount = 2,
            ReturnTypeHint = "double")]
        public static DynValue min(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return execaccum(args, "min", (d1, d2) => Math.Min(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns two numbers, the integral part of x and the fractional part of x.",
            AutoCompleteHint = "math.modf(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple<int, int>")]
        public static DynValue modf(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg = args.AsType(0, "modf", DataType.Number);
            return DynValue.NewTuple(DynValue.NewNumber(Math.Floor(arg.Number)),
                DynValue.NewNumber(arg.Number - Math.Floor(arg.Number)));
        }


        [MoonSharpModuleMethod(Description = "Returns x^y.",
            AutoCompleteHint = "math.pow(double value1, double value2)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue pow(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec2(args, "pow", (d1, d2) => Math.Pow(d1, d2));
        }

        [MoonSharpModuleMethod(Description = "Returns the angle x (given in degrees) in radians.",
            AutoCompleteHint = "math.rad(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue rad(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "rad", d => d * Math.PI / 180.0);
        }

        [MoonSharpModuleMethod(Description = "Returns a pseudo-random number between the two values.",
            AutoCompleteHint = "math.random(int lowValue, int highValue)",
            ParameterCount = 2,
            ReturnTypeHint = "int")]
        public static DynValue random(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var m = args.AsType(0, "random", DataType.Number, true);
            var n = args.AsType(1, "random", DataType.Number, true);
            var R = GetRandom(executionContext.GetScript());
            double d;

            if (m.IsNil() && n.IsNil())
            {
                d = R.NextDouble();
            }
            else
            {
                int a = n.IsNil() ? 1 : (int) n.Number;
                int b = (int) m.Number;

                if (a < b)
                {
                    d = R.Next(a, b + 1);
                }
                else
                {
                    d = R.Next(b, a + 1);
                }
            }

            return DynValue.NewNumber(d);
        }

        [MoonSharpModuleMethod(Description = "Sets x as the seed for the pseudo-random generator.",
            AutoCompleteHint = "math.randomseed(int seedValue)",
            ParameterCount = 1,
            ReturnTypeHint = "void")]
        public static DynValue randomseed(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var arg = args.AsType(0, "randomseed", DataType.Number);
            var script = executionContext.GetScript();
            SetRandom(script, new Random((int) arg.Number));
            return DynValue.Nil;
        }

        [MoonSharpModuleMethod(Description = "Returns the sine of x (assumed to be in radians)",
            AutoCompleteHint = "math.sin(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue sin(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "sin", d => Math.Sin(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the hyperbolic sine of x.",
            AutoCompleteHint = "math.sinh(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue sinh(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "sinh", d => Math.Sinh(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the square root of a value.",
            AutoCompleteHint = "math.sqrt(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue sqrt(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "sqrt", d => Math.Sqrt(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the tangent of x (assumed to be in radians)",
            AutoCompleteHint = "math.tan(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue tan(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "tan", d => Math.Tan(d));
        }

        [MoonSharpModuleMethod(Description = "Returns the hyperbolic tangent of a x.",
            AutoCompleteHint = "math.tanh(double value)",
            ParameterCount = 1,
            ReturnTypeHint = "double")]
        public static DynValue tanh(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return exec1(args, "tanh", d => Math.Tanh(d));
        }
    }
}