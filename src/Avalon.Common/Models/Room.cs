namespace Avalon.Common.Models
{
    public class Room
    {
        public int Vnum { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Exits { get; set; } = "";
        public string Area { get; set; } = "";
        public string Continent { get; set; } = "";
        public int North { get; set; } = -1;
        public int South { get; set; } = -1;
        public int East { get; set; } = -1;
        public int West { get; set; } = -1;
        public int Northwest { get; set; } = -1;
        public int Northeast { get; set; } = -1;
        public int Southeast { get; set; } = -1;
        public int Southwest { get; set; } = -1;
        public int Up { get; set; } = -1;
        public int Down { get; set; } = -1;
        public string Contents { get; set; } = "";
        public string Hash
        {
            get
            {
                return Argus.Cryptography.HashUtilities.Sha512Hash(Name + Description + Exits);
            }
        }

    }
}
