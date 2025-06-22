
namespace integration_dispatch.Model
{
    public class JoinTable
    {
        public string SourceTable { get; set; }
        public List<ColumnMapping> ColumnMappings { get; set; }
        public JoinCondition JoinCondition { get; set; }
    }
}
