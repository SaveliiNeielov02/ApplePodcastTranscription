using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplePodcastTranscription.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PodcastsRecords",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: false),
                    TranscriptionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptionError = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptionText = table.Column<string>(type: "TEXT", nullable: true),
                    DownloadedAtInUnixTimeSeconds = table.Column<long>(type: "INTEGER", nullable: false),
                    TranscribedAtInUnixTimeSeconds = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastsRecords", x => x.Guid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PodcastsRecords");
        }
    }
}
