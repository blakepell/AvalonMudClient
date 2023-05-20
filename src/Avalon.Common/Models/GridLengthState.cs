/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Models
{
    public class GridLengthState
    {
        public enum GridUnitType
        {
            Auto = 0,
            Pixel = 1,
            Star = 2
        }

        public enum GridElementType
        {
            Row = 0,
            Column = 1,
        }

        /// <summary>
        /// The length of the grid element.
        /// </summary>
        public double Length { get; set; } = 0;

        /// <summary>
        /// The index of where the grid length is.
        /// </summary>
        public int Index = 0;

        /// <summary>
        /// The unit type that corresponds to 
        /// </summary>
        public GridUnitType UnitType { get; set; }

        /// <summary>
        /// Whether this is a row or a column.
        /// </summary>
        public GridElementType ElementType { get; set; }

    }
}
