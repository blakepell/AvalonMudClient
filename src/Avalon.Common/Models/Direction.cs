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
    /// <summary>
    /// Represents a path from one room to another.
    /// </summary>
    public class Direction : ICloneable, INotifyPropertyChanged
    {
        public Direction()
        {

        }

        public Direction(string name, string speedwalk)
        {
            this.Name = name;
            this.Speedwalk = speedwalk;
        }

        public Direction(string name, string speedwalk, string startingRoom)
        {
            this.Name = name;
            this.Speedwalk = speedwalk;
            this.StartingRoom = startingRoom;
        }

        /// <summary>
        /// The friendly name of the direction.
        /// </summary>
        public string Name { get; set; } = "";


        private string _speedwalk = "";

        /// <summary>
        /// The directions to speed walk from one point to another.
        /// </summary>
        public string Speedwalk
        {
            get => _speedwalk;
            set
            {
                _speedwalk = value;
                OnPropertyChanged(nameof(Speedwalk));
            }
        }

        /// <summary>
        /// The name of the starting room.  This is important to be correct as the StartingRoom and EndingRoom
        /// are used in tandem to combine sets of directions.
        /// </summary>
        public string StartingRoom { get; set; } = "";

        /// <summary>
        /// The name of the ending room.  This is important to be correct as the StartingRoom and EndingRoom
        /// are used in tandem to combine sets of directions.
        /// </summary>
        public string EndingRoom { get; set; } = "";

        /// <summary>
        /// Whether or not the direction can be auto-updated.
        /// </summary>
        public bool Lock { get; set; } = false;

        [JsonIgnore]
        public int DegreeOfSeparation { get; set; } = 0;

        /// <summary>
        /// The package that imported this <see cref="Direction"/>.
        /// </summary>
        public string PackageId { get; set; } = "";

        /// <summary>
        /// Clones the direction.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
