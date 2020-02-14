using System.Collections.Generic;

namespace Avalon.Sqlite
{
    public class Table 
    {

        [Column("name")]
        public string Name { get; set; } = "";


        [Column("tbl_name")]
        public string TableName { get; set; } = "";

        [Column("rootpage")]
        public int RootPage { get; set; } = 0;

        [Column("sql")]
        public string Sql { get; set; } = "";

        public List<Field> Fields { get; set; } = new List<Field>();

    }
}
