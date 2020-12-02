using Argus.ComponentModel;
using System.Collections.ObjectModel;

namespace Avalon.Sqlite
{
    /// <summary>
    /// The metadata for a SQLite view.
    /// </summary>
    public class View : Observable
    {
        private string _name;

        /// <summary>
        /// The name of the view.
        /// </summary>
        [Column("name")]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value, nameof(Name));
        }

        private string _tableName;

        /// <summary>
        /// The name of the view.
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
        /// The SQL provided by SQLite to create this view.
        /// </summary>
        [Column("sql")]
        public string Sql
        {
            get => _sql;
            set => Set(ref _sql, value, nameof(Sql));
        }

        /// <summary>
        /// The fields contained in this view.
        /// </summary>
        public ObservableCollection<Field> Fields { get; set; } = new ObservableCollection<Field>();
    }
}