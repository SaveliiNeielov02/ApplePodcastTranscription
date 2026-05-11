using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplePodcastTranscription.Migrations
{
    /// <inheritdoc />
    public partial class SessionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PodcastsRecords",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "TranscriptionError",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "TranscriptionStatus",
                table: "PodcastsRecords");

            migrationBuilder.AddColumn<string>(
                name: "ArtistName",
                table: "PodcastsRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "PodcastsRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PodcastsRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PodcastsRecords",
                table: "PodcastsRecords",
                column: "ExternalId");

            migrationBuilder.CreateTable(
                name: "TranscriptSessions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    PodcastRecordId = table.Column<string>(type: "TEXT", nullable: false),
                    TranscriptionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptionError = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscriptSessions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_TranscriptSessions_PodcastsRecords_PodcastRecordId",
                        column: x => x.PodcastRecordId,
                        principalTable: "PodcastsRecords",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranscriptSessions_PodcastRecordId",
                table: "TranscriptSessions",
                column: "PodcastRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranscriptSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PodcastsRecords",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "ArtistName",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "PodcastsRecords");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "PodcastsRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "PodcastsRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TranscriptionError",
                table: "PodcastsRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TranscriptionStatus",
                table: "PodcastsRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PodcastsRecords",
                table: "PodcastsRecords",
                column: "Guid");
        }
    }
}
