using System.Threading.Tasks;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Represents a hash command.
    /// </summary>
    public interface IHashCommand
    {

        string Name { get; }

        string Description { get; }

        string Parameters { get; set; }

        void Execute();

        Task ExecuteAsync();

        IInterpreter Interpreter { get; set; }

        bool IsAsync { get; set; }

    }
}
