using System.Collections.Generic;

namespace Avalon.Sqlite
{

    public class Schema
    {
        public string DatabaseName { get; set; } = "";

        public List<Table> Tables { get; set; } = new List<Table>();

        public List<View> Views { get; set; } = new List<View>();

    }

}