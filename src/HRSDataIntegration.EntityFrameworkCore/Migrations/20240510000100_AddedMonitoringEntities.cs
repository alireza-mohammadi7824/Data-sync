using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSDataIntegration.Migrations
{
    public partial class AddedMonitoringEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppServiceEndpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CheckIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    LastKnownStatus = table.Column<int>(type: "int", nullable: false),
                    LastCheckTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastResponseDurationMilliseconds = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServiceEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppServiceStatusSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceEndpointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMilliseconds = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ResultCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppServiceStatusSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppServiceStatusSnapshots_AppServiceEndpoints_ServiceEndpointId",
                        column: x => x.ServiceEndpointId,
                        principalTable: "AppServiceEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppServiceEndpoints_ServiceType_IsEnabled",
                table: "AppServiceEndpoints",
                columns: new[] { "ServiceType", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppServiceStatusSnapshots_ServiceEndpointId_CheckedAt",
                table: "AppServiceStatusSnapshots",
                columns: new[] { "ServiceEndpointId", "CheckedAt" },
                descending: new[] { false, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppServiceStatusSnapshots");

            migrationBuilder.DropTable(
                name: "AppServiceEndpoints");
        }
    }
}
