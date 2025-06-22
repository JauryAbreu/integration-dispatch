namespace integration_dispatch.Model
{
    public class Config
    {
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }
        public string TransactionTable { get; set; }
        public string HeaderId { get; set; }
        public string HeaderTable { get; set; }
        public string LinesTable { get; set; }
        public string Quantity { get; set; }
        public string CanBeDispatched { get; set; }
        public List<TableMapping> TableMappings { get; set; }
    }
}
