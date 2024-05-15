using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncData.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "syncMasterPublisherServerConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: true),
                    PullEnabled = table.Column<bool>(type: "bit", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValue: "icon-planet color-blue-grey"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValue: "realtime"),
                    AllowedServers = table.Column<string>(type: "ntext", nullable: true),
                    SendSettings = table.Column<string>(type: "ntext", nullable: true),
                    PublisherSettings = table.Column<string>(type: "ntext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_syncMasterPublisherServerConfig", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "syncMasterPublisherServerConfig");
        }
    }
}
