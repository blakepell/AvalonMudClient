using Argus.ComponentModel;

namespace Avalon.Sqlite
{
    /// <summary>
    /// The metadata for a SQLite field.
    /// </summary>
    public class Field : Observable
    {
        private int _cid;

        /// <summary>
        /// The cid.
        /// </summary>
        [Column("cid")]
        public int CId
        {
            get => _cid;
            set => Set(ref _cid, value, nameof(CId));
        }

        private string _name;

        /// <summary>
        /// The name of the field.
        /// </summary>
        [Column("name")]
        public string Name
        {
            get => _name;
            set => Set(ref _name, value, nameof(Name));
        }

        private string _type;

        /// <summary>
        /// The data type of the field.
        /// </summary>
        [Column("type")]
        public string Type
        {
            get => _type;
            set => Set(ref _type, value, nameof(Type));
        }

        private int _notNull;

        /// <summary>
        /// If the field is defined as not null.
        /// </summary>
        [Column("notnull")]
        public int NotNull
        {
            get => _notNull;
            set => Set(ref _notNull, value, nameof(NotNull));
        }

        private string _defaultValue;

        /// <summary>
        /// The default value of the field if one exists.
        /// </summary>
        [Column("dflt_value")]
        public string DefaultValue
        {
            get => _defaultValue;
            set => Set(ref _defaultValue, value, nameof(DefaultValue));
        }
    }
}
