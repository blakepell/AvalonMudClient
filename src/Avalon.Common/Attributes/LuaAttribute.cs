/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Attributes
{
    /// <summary>
    /// A Lua attribute that can be put onto a string property to denote it as Lua code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LuaAttribute : Attribute
    {
    }
}
