using System;
using System.Threading.Tasks;
using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    /// <summary>
    /// Base class for a hash command.
    /// </summary>
    public abstract class HashCommand : IHashCommand
    {
        protected HashCommand(IInterpreter interp)
        {
            this.Interpreter = interp;
        }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public string Parameters { get; set; }

        public virtual void Execute()
        {
        }

        public virtual Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public IInterpreter Interpreter { get; set; }

        public bool IsAsync { get; set; }

    }
}
