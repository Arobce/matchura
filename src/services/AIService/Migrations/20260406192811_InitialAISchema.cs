using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIService.Migrations
{
    /// <inheritdoc />
    public partial class InitialAISchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateSkills",
                columns: table => new
                {
                    CandidateSkillId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CandidateId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SkillName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SkillCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProficiencyLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    YearsUsed = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateSkills", x => x.CandidateSkillId);
                });

            migrationBuilder.CreateTable(
                name: "MatchScores",
                columns: table => new
                {
                    MatchScoreId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CandidateId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SkillScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ExperienceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EducationScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Strengths = table.Column<string>(type: "jsonb", nullable: true),
                    Gaps = table.Column<string>(type: "jsonb", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchScores", x => x.MatchScoreId);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    ResumeId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CandidateId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: true),
                    ParsedData = table.Column<string>(type: "jsonb", nullable: true),
                    ParseStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.ResumeId);
                });

            migrationBuilder.CreateTable(
                name: "SkillGapReports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CandidateId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OverallReadiness = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EstimatedTimeToReady = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MissingSkills = table.Column<string>(type: "jsonb", nullable: true),
                    RecommendedActions = table.Column<string>(type: "jsonb", nullable: true),
                    StrengthAreas = table.Column<string>(type: "jsonb", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGapReports", x => x.ReportId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateSkills_CandidateId",
                table: "CandidateSkills",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_CandidateId_JobId",
                table: "MatchScores",
                columns: new[] { "CandidateId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_JobId",
                table: "MatchScores",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_CandidateId",
                table: "Resumes",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapReports_CandidateId_JobId",
                table: "SkillGapReports",
                columns: new[] { "CandidateId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillGapReports_JobId",
                table: "SkillGapReports",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateSkills");

            migrationBuilder.DropTable(
                name: "MatchScores");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "SkillGapReports");
        }
    }
}
