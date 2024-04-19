using Microsoft.EntityFrameworkCore;
using Serilog.Context;

namespace SyncData.PublishServer
{
	public class ServerContext : DbContext
	{
		public ServerContext(DbContextOptions<ServerContext> options)
			: base(options)
		{
		}

		public required DbSet<ServerModel> serverPublishConfig { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) =>
			modelBuilder.Entity<ServerModel>(entity =>
			{
				entity.ToTable("serverModel"); //change the table name if required
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).HasColumnName("id");
				entity.Property(e => e.Key).HasColumnName("key");
				entity.Property(e => e.Name).HasColumnName("name");
				entity.Property(e => e.Url).HasColumnName("url");
				entity.Property(e => e.Pull).HasColumnName("pull");
				entity.Property(e => e.Push).HasColumnName("push");
				entity.Property(e => e.Alias).HasColumnName("alias");
				entity.Property(e => e.Message).HasColumnName("message");
			});
	}
}
