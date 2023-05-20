/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Plugins.DarkAndShatteredLands.Affects
{

    /// <summary>
    /// Represents a single affect.
    /// </summary>
    public class Affect
    {
        public string Name { get; set; } = "";

        public string Modifies { get; set; } = "none";

        public int Modifier { get; set; } = 0;

        public int Duration { get; set; } = -1;

        public override string ToString()
        {
            return $"{this.Name.PadRightToLength(18)} : modifies {this.Modifies} by {Modifier.ToString()} for {this.Duration.ToString()} ticks";
        }

        public string Display()
        {
            if (this.Duration == -1)
            {
                return $"{this.Name.PadRightToLength(18)} : Perm";
            }
            else if (this.Duration == -2)
            {
                return $"{this.Name.PadRightToLength(18)} : ?";
            }
            else
            {
                return $"{this.Name.PadRightToLength(18)} : {this.Duration.ToString()}";
            }
        }

    }
}
