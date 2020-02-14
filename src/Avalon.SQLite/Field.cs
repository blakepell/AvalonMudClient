namespace Avalon.Sqlite
{
    public class Field
    {
        [Column("cid")]
        public int CId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("notnull")]
        public int NotNull { get; set; }

        [Column("dflt_value")]
        public string DefaultValue { get; set; }

    }
}
