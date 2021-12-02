/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using Avalon.Common.Triggers;

namespace Avalon.Common.Models
{
    public class Package : IPackage, INotifyPropertyChanged
    {

        private string _id = "";

        /// <inheritdoc />
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _name = "";

        /// <inheritdoc />
        public string Name 
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _description = "";

        /// <inheritdoc />
        public string Description 
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _author = "";

        /// <inheritdoc />
        public string Author 
        {
            get => _author;
            set
            {
                _author = value;
                OnPropertyChanged(nameof(Author));
            }
        }

        private string _gameAddress = "";

        /// <inheritdoc />
        public string GameAddress
        {
            get => _gameAddress;
            set
            {
                _gameAddress = value;
                OnPropertyChanged(nameof(GameAddress));
            }
        }

        private string _category = "";
        /// <inheritdoc />        
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        private int _version = 0;
        /// <inheritdoc />
        public int Version 
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }

        private string _minimumClientVersion = "0.0.0.0";

        /// <inheritdoc/>
        public string MinimumClientVersion 
        {
            get => _minimumClientVersion;
            set
            {
                _minimumClientVersion = value;
                OnPropertyChanged(nameof(MinimumClientVersion));
            }
        }

        private bool _isInstalled = false;
        
        /// <summary>
        /// Property to display on the UI if a package (or part of it) was found installed into the
        /// current profile.  This is calculated at the time the Package Manager loads.
        /// </summary>
        [JsonIgnore]
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                _isInstalled = value;
                OnPropertyChanged(nameof(IsInstalled));
            }
        }

        private bool _updateAvailable = false;

        /// <summary>
        /// Whether an update is available to this package from the package manager API site.
        /// </summary>
        [JsonIgnore]
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;
                OnPropertyChanged(nameof(UpdateAvailable));
            }
        }

        private string _setupCommand = "";

        /// <inheritdoc />
        public string SetupCommand
        {
            get => _setupCommand;
            set
            {
                _setupCommand = value;
                OnPropertyChanged(nameof(SetupCommand));
            }
        }

        private string _setupLuaScript = "";

        /// <inheritdoc />
        public string SetupLuaScript
        {
            get => _setupLuaScript;
            set
            {
                _setupLuaScript = value;
                OnPropertyChanged(nameof(SetupLuaScript));
            }
        }

        private string _uninstallCommand = "";

        /// <inheritdoc />
        public string UninstallCommand
        {
            get => _uninstallCommand;
            set
            {
                _uninstallCommand = value;
                OnPropertyChanged(nameof(UninstallCommand));
            }
        }

        private string _uninstallLuaScript = "";

        /// <inheritdoc />
        public string UninstallLuaScript
        {
            get => _uninstallLuaScript;
            set
            {
                _uninstallLuaScript = value;
                OnPropertyChanged(nameof(UninstallLuaScript));
            }
        }

        /// <inheritdoc />
        public List<Alias> AliasList { get; set; } = new();

        /// <inheritdoc />
        public List<Trigger> TriggerList { get; set; } = new();

        /// <inheritdoc />
        public List<Direction> DirectionList { get; set; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
