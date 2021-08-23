/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Collections.ObjectModel;
using Argus.ComponentModel;

namespace Avalon.Sqlite.Common
{

    /// <summary>
    /// The schema of the SQLite database.
    /// </summary>
    public class Schema : Observable
    {
        private string _connectionString;

        /// <summary>
        /// The connection string of the schema, used to display a tooltip for the user.
        /// </summary>
        public string ConnectionString
        {
            get => _connectionString;
            set => Set(ref _connectionString, value, nameof(ConnectionString));
        }


        private string _databaseName;

        /// <summary>
        /// The database name.
        /// </summary>
        public string DatabaseName
        {
            get => _databaseName;
            set => Set(ref _databaseName, value, nameof(DatabaseName));
        }

        private ObservableCollection<Table> _tables;

        /// <summary>
        /// The tables inside the SQLite database.
        /// </summary>
        public ObservableCollection<Table> Tables
        {
            get => _tables;
            set => Set(ref _tables, value, nameof(Tables));
        }

        private ObservableCollection<View> _views;

        /// <summary>
        /// The views inside the SQLite database.
        /// </summary>
        public ObservableCollection<View> Views
        {
            get => _views;
            set => Set(ref _views, value, nameof(Views));
        }
    }
}