using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// A class representing a script coroutine
    /// </summary>
    public class Coroutine : RefIdObject, IScriptPrivateResource
    {
        /// <summary>
        /// Possible types of coroutine
        /// </summary>
        public enum CoroutineType
        {
            /// <summary>
            /// A valid coroutine
            /// </summary>
            Coroutine,

            /// <summary>
            /// A CLR callback assigned to a coroutine. 
            /// </summary>
            ClrCallback,

            /// <summary>
            /// A CLR callback assigned to a coroutine and already executed.
            /// </summary>
            ClrCallbackDead
        }

        private CallbackFunction _clrCallback;
        private Processor _processor;


        internal Coroutine(CallbackFunction function)
        {
            this.Type = CoroutineType.ClrCallback;
            _clrCallback = function;
            this.OwnerScript = null;
        }

        internal Coroutine(Processor proc)
        {
            this.Type = CoroutineType.Coroutine;
            _processor = proc;
            _processor.AssociatedCoroutine = this;
            this.OwnerScript = proc.GetScript();
        }

        /// <summary>
        /// Gets the type of coroutine
        /// </summary>
        public CoroutineType Type { get; private set; }

        /// <summary>
        /// Gets the coroutine state.
        /// </summary>
        public CoroutineState State
        {
            get
            {
                if (this.Type == CoroutineType.ClrCallback)
                {
                    return CoroutineState.NotStarted;
                }

                if (this.Type == CoroutineType.ClrCallbackDead)
                {
                    return CoroutineState.Dead;
                }

                return _processor.State;
            }
        }

        /// <summary>
        /// Gets or sets the automatic yield counter.
        /// </summary>
        /// <value>
        /// The automatic yield counter.
        /// </value>
        public long AutoYieldCounter
        {
            get => _processor.AutoYieldCounter;
            set => _processor.AutoYieldCounter = value;
        }

        /// <summary>
        /// Gets the script owning this resource.
        /// </summary>
        /// <value>
        /// The script owning this resource.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public Script OwnerScript { get; }

        internal void MarkClrCallbackAsDead()
        {
            if (this.Type != CoroutineType.ClrCallback)
            {
                throw new InvalidOperationException("State must be CoroutineType.ClrCallback");
            }

            this.Type = CoroutineType.ClrCallbackDead;
        }


        /// <summary>
        /// Gets this coroutine as a typed enumerable which can be looped over for resuming.
        /// Returns its result as DynValue(s)
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public IEnumerable<DynValue> AsTypedEnumerable()
        {
            if (this.Type != CoroutineType.Coroutine)
            {
                throw new InvalidOperationException(
                    "Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead");
            }

            while (this.State == CoroutineState.NotStarted || this.State == CoroutineState.Suspended ||
                   this.State == CoroutineState.ForceSuspended)
            {
                yield return this.Resume();
            }
        }


        /// <summary>
        /// Gets this coroutine as a typed enumerable which can be looped over for resuming.
        /// Returns its result as System.Object. Only the first element of tuples is returned.
        /// Only non-CLR coroutines can be resumed with this method. Use an overload of the Resume method accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public IEnumerable<object> AsEnumerable()
        {
            foreach (var v in this.AsTypedEnumerable())
            {
                yield return v.ToScalar().ToObject();
            }
        }

        /// <summary>
        /// Gets this coroutine as a typed enumerable which can be looped over for resuming.
        /// Returns its result as the specified type. Only the first element of tuples is returned.
        /// Only non-CLR coroutines can be resumed with this method. Use an overload of the Resume method accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public IEnumerable<T> AsEnumerable<T>()
        {
            foreach (var v in this.AsTypedEnumerable())
            {
                yield return v.ToScalar().ToObject<T>();
            }
        }

        /// <summary>
        /// The purpose of this method is to convert a MoonSharp/Lua coroutine to a Unity3D coroutine.
        /// This loops over the coroutine, discarding returned values, and returning null for each invocation.
        /// This means however that the coroutine will be invoked each frame.
        /// Only non-CLR coroutines can be resumed with this method. Use an overload of the Resume method accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public IEnumerator AsUnityCoroutine()
        {
#pragma warning disable 0219
            foreach (var v in this.AsTypedEnumerable())
            {
                yield return null;
            }
#pragma warning restore 0219
        }

        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public DynValue Resume(params DynValue[] args)
        {
            this.CheckScriptOwnership(args);

            if (this.Type == CoroutineType.Coroutine)
            {
                return _processor.Coroutine_Resume(args);
            }

            throw new InvalidOperationException(
                "Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead");
        }


        /// <summary>
        /// Resumes the coroutine.
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        /// <param name="args">The arguments.</param>
        public DynValue Resume(ScriptExecutionContext context, params DynValue[] args)
        {
            this.CheckScriptOwnership(context);
            this.CheckScriptOwnership(args);

            if (this.Type == CoroutineType.Coroutine)
            {
                return _processor.Coroutine_Resume(args);
            }

            if (this.Type == CoroutineType.ClrCallback)
            {
                var ret = _clrCallback.Invoke(context, args);
                this.MarkClrCallbackAsDead();
                return ret;
            }

            throw ScriptRuntimeException.CannotResumeNotSuspended(CoroutineState.Dead);
        }

        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public DynValue Resume()
        {
            return this.Resume(new DynValue[0]);
        }


        /// <summary>
        /// Resumes the coroutine.
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        public DynValue Resume(ScriptExecutionContext context)
        {
            return this.Resume(context, new DynValue[0]);
        }

        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.</exception>
        public DynValue Resume(params object[] args)
        {
            if (this.Type != CoroutineType.Coroutine)
            {
                throw new InvalidOperationException(
                    "Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead");
            }

            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.FromObject(this.OwnerScript, args[i]);
            }

            return this.Resume(dargs);
        }


        /// <summary>
        /// Resumes the coroutine
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        /// <param name="args">The arguments.</param>
        public DynValue Resume(ScriptExecutionContext context, params object[] args)
        {
            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.FromObject(context.GetScript(), args[i]);
            }

            return this.Resume(context, dargs);
        }


        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public Task<DynValue> ResumeAsync(params DynValue[] args)
        {
            return Task.Factory.StartNew(() => this.Resume(args));
        }


        /// <summary>
        /// Resumes the coroutine.
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        /// <param name="args">The arguments.</param>
        public Task<DynValue> ResumeAsync(ScriptExecutionContext context, params DynValue[] args)
        {
            return Task.Factory.StartNew(() => this.Resume(context, args));
        }

        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
        public Task<DynValue> ResumeAsync()
        {
            return Task.Factory.StartNew(() => this.Resume());
        }


        /// <summary>
        /// Resumes the coroutine.
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        public Task<DynValue> ResumeAsync(ScriptExecutionContext context)
        {
            return Task.Factory.StartNew(() => this.Resume(context));
        }

        /// <summary>
        /// Resumes the coroutine.
        /// Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead.</exception>
        public Task<DynValue> ResumeAsync(params object[] args)
        {
            return Task.Factory.StartNew(() => this.Resume(args));
        }


        /// <summary>
        /// Resumes the coroutine
        /// </summary>
        /// <param name="context">The ScriptExecutionContext.</param>
        /// <param name="args">The arguments.</param>
        public Task<DynValue> ResumeAsync(ScriptExecutionContext context, params object[] args)
        {
            return Task.Factory.StartNew(() => this.Resume(context, args));
        }
    }
}