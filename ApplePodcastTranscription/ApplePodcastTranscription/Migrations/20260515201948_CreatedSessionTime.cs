using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplePodcastTranscription.Migrations
{
    /// <inheritdoc />
    public partial class CreatedSessionTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedAtInUnixTimeSeconds",
                table: "TranscriptSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtInUnixTimeSeconds",
                table: "TranscriptSessions");
        }
    }
}
