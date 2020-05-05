using Argus.Extensions;

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
            return $"{this.Name.PadRightToLength(25)} : modifies {this.Modifies} by {Modifier} for {this.Duration} ticks";
        }

        public string Display()
        {
            if (this.Duration == -1)
            {
                return $"{this.Name.PadRightToLength(25)} : Permanently";
            }
            else
            {
                return $"{this.Name.PadRightToLength(25)} : {this.Duration}";
            }
        }

    }
}
