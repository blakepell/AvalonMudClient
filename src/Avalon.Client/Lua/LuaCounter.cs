namespace Avalon.Lua
{
    /// <summary>
    /// Represents a statistical counter that the LuaCaller will track.
    /// </summary>
    public enum LuaCounter
    {
        /// <summary>
        /// The number of Lua scripts that are actively running in the instance of the LuaCaller.
        /// </summary>
        ActiveScripts,
        /// <summary>
        /// The number of scripts that have run to completion in the instance of the LuaCaller.
        /// </summary>
        ScriptsRun,
        /// <summary>
        /// The number of scripts that have encountered errors in the instance of the LuaCaller.
        /// </summary>
        ErrorCount,
        /// <summary>
        /// The number of SQL statements that have been run in the instance of the LuaCaller.
        /// </summary>
        SqlCount
    }
}
