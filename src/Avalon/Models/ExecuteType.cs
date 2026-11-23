namespace Avalon.Models;

/// <summary>
/// Specifies how the interpreter runs an automation action.
/// </summary>
public enum ExecuteType
{
    /// <summary>
    /// Runs the action through the game command interpreter.
    /// </summary>
    Command = 0,

    /// <summary>
    /// Runs the action as JavaScript.
    /// </summary>
    JavaScript = 1
}
