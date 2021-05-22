using System;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors
{
    public abstract class HardwiredMemberDescriptor : IMemberDescriptor
    {
        protected HardwiredMemberDescriptor(Type memberType, string name, bool isStatic, MemberDescriptorAccess access)
        {
            this.IsStatic = isStatic;
            this.Name = name;
            this.MemberAccess = access;
            this.MemberType = memberType;
        }

        public Type MemberType { get; }

        public bool IsStatic { get; }

        public string Name { get; }

        public MemberDescriptorAccess MemberAccess { get; }


        public DynValue GetValue(Script script, object obj)
        {
            this.CheckAccess(MemberDescriptorAccess.CanRead, obj);
            var result = this.GetValueImpl(script, obj);
            return ClrToScriptConversions.ObjectToDynValue(script, result);
        }

        public void SetValue(Script script, object obj, DynValue value)
        {
            this.CheckAccess(MemberDescriptorAccess.CanWrite, obj);
            var v = ScriptToClrConversions.DynValueToObjectOfType(value, this.MemberType, null, false);
            this.SetValueImpl(script, obj, v);
        }


        protected virtual object GetValueImpl(Script script, object obj)
        {
            throw new InvalidOperationException("GetValue on write-only hardwired descriptor " + this.Name);
        }

        protected virtual void SetValueImpl(Script script, object obj, object value)
        {
            throw new InvalidOperationException("SetValue on read-only hardwired descriptor " + this.Name);
        }
    }
}