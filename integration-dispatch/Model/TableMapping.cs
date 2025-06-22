namespace integration_dispatch.Model
{
    public class TableMapping
    {
        public string SourceTable { get; set; }
        public string DestinationTable { get; set; }
        public List<ColumnMapping> ColumnMappings { get; set; }
        public List<WhereMapping> WhereMappings { get; set; }
        public List<JoinTable> JoinTables { get; set; }
    }
}
