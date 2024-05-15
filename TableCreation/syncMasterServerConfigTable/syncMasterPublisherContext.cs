using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncData.TableCreation.syncMasterServerConfigTable
{
    public class syncMasterPublisherContext : DbContext
    {
        public syncMasterPublisherContext(DbContextOptions<syncMasterPublisherContext> options) : base(options)
        {
        }

        public DbSet<syncMasterPublisherModel> syncMasterPublisherModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<syncMasterPublisherModel>(entity =>
        {
            entity.ToTable("syncMasterPublisherServerConfig");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Key).HasColumnName("Key");
            entity.Property(e => e.Alias).HasColumnName("Alias");
            entity.Property(e => e.PushEnabled).HasColumnName("PushEnabled");
            entity.Property(e => e.PullEnabled).HasColumnName("PullEnabled");
            entity.Property(e => e.SortOrder).HasColumnName("SortOrder");
            entity.Property(e => e.Url).HasColumnName("Url");
            entity.Property(e => e.BaseUrl).HasColumnName("BaseUrl");
            entity.Property(e => e.Name).HasColumnName("Name");
            entity.Property(e => e.Icon).HasColumnName("Icon").HasDefaultValue("icon-planet color-blue-grey");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.Message).HasColumnName("Message");
            entity.Property(e => e.Publisher).HasColumnName("Publisher").HasDefaultValue("realtime");
            entity.Property(e => e.AllowedServers).HasColumnName("AllowedServers").HasColumnType("ntext").HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<IEnumerable<usyncAllowedServerModel>>(v));
            entity.Property(e => e.SendSettings).HasColumnName("SendSettings").HasColumnType("ntext").HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<usyncSendModel>(v));
            entity.Property(e => e.PublisherSettings).HasColumnName("PublisherSettings").HasColumnType("ntext").HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<IDictionary<string, bool>>(v));

        });


    }
}
