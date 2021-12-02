/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.ComponentModel;

namespace Avalon.Sqlite.Common
{
    /// <summary>
    /// The metadata for a SQLite table.
    /// </summary>
    /// <remarks>
    /// The duplicate properties for Name and TableName are from SQLite itself.
    /// </remarks>
    public class Table : Observable
    {

        private string _name;

        /// <summary>
        /// The table name.
        /// </summary>
        [Column("name")]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value, nameof(Name));
        }

        private string _tableName;

        /// <summary>
        /// The table name.
        /// </summary>
        [Column("tbl_name")]
        public string TableName
        {
            get => _tableName;
            set => Set(ref _tableName, value, nameof(TableName));
        }

        private int _rootPage;

        /// <summary>
        /// The root page.
        /// </summary>
        [Column("rootpage")]
        public int RootPage
        {
            get => _rootPage;
            set => Set(ref _rootPage, value, nameof(RootPage));
        }

        private string _sql;

        /// <summary>
        /// The SQL provided by SQLite to create the table.
        /// </summary>
        [Column("sql")]
        public string Sql
        {
            get => _sql;
            set => Set(ref _sql, value, nameof(Sql));
        }

        /// <summary>
        /// The fields contained in the table.
        /// </summary>
        public ObservableCollection<Field> Fields { get; set; } = new ObservableCollection<Field>();

    }
}
