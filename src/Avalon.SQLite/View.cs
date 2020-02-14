using System.Collections.Generic;

namespace Avalon.Sqlite
{
    public class View
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("tbl_name")]
        public string TableName { get; set; }

        [Column("rootpage")]
        public int RootPage { get; set; }

        [Column("sql")]
        public string Sql { get; set; }

        public List<Field> Fields { get; set; } = new List<Field>();

    }
}
