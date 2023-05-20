using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors
{
    public abstract class HardwiredMethodMemberDescriptor : FunctionMemberDescriptorBase
    {
        public override DynValue Execute(Script script, object obj, ScriptExecutionContext context,
            CallbackArguments args)
        {
            this.CheckAccess(MemberDescriptorAccess.CanExecute, obj);

            var pars = base.BuildArgumentList(script, obj, context, args, out _);
            var retv = this.Invoke(script, obj, pars, this.CalcArgsCount(pars));

            return DynValue.FromObject(script, retv);
        }

        private int CalcArgsCount(object[] pars)
        {
            int count = pars.Length;

            for (int i = 0; i < pars.Length; i++)
            {
                if (this.Parameters[i].HasDefaultValue && (pars[i] is DefaultValue))
                {
                    count -= 1;
                }
            }

            return count;
        }

        protected abstract object Invoke(Script script, object obj, object[] pars, int argscount);
    }
}