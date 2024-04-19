using System.ComponentModel.DataAnnotations.Schema;

namespace SyncData.PublishServer
{
    public class ServerModel
    {
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
        public Guid Key { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public bool Pull { get; set; }
        public bool Push { get; set; }
        public string Alias { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
