namespace NeoTemplate.Models.Migrations.Migrations
{
  using System;
  using Microsoft.EntityFrameworkCore.Migrations;

  public partial class InitialMigration : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "OneTimeTokens",
          columns: table => new
          {
            Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
            Resource = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            Info = table.Column<string>(type: "nvarchar(max)", nullable: false),
            CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
            Expiry = table.Column<DateTime>(type: "datetime", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_OneTimeTokens", columns => columns.Token);
          });

      migrationBuilder.CreateTable(
          name: "ReportDataEntries",
          columns: table => new
          {
            ReportDataEntryId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            ReportRequestId = table.Column<int>(type: "int", nullable: false),
            ReportData = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ValidUntil = table.Column<DateTime>(type: "datetime", nullable: true),
            TenantId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReportDataEntries", columns => columns.ReportDataEntryId);
          });

      migrationBuilder.CreateTable(
          name: "ReportRequestStatuses",
          columns: table => new
          {
            ReportRequestStatusId = table.Column<int>(type: "int", nullable: false),
            ReportRequestStatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReportRequestStatuses", columns => columns.ReportRequestStatusId);
          });

      migrationBuilder.CreateTable(
          name: "ReportRequestTypes",
          columns: table => new
          {
            ReportRequestTypeId = table.Column<int>(type: "int", nullable: false),
            ReportRequestTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReportRequestTypes", columns => columns.ReportRequestTypeId);
          });

      migrationBuilder.CreateTable(
          name: "Users",
          columns: table => new
          {
            UserId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            IdentityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ClientId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            UserName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
            ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Users", columns => columns.UserId);
          });

      migrationBuilder.CreateTable(
          name: "ReportRequests",
          columns: table => new
          {
            ReportRequestId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            RequestType = table.Column<int>(type: "int", maxLength: 50, nullable: false),
            Criteria = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Status = table.Column<int>(type: "int", nullable: false),
            Error = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
            CreatedBy = table.Column<int>(type: "int", nullable: false),
            CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
            DataLoadStartedOn = table.Column<DateTime>(type: "datetime", nullable: true),
            DataLoadCompletedOn = table.Column<DateTime>(type: "datetime", nullable: true),
            DataRequestedOn = table.Column<DateTime>(type: "datetime", nullable: true),
            RequestedByUserGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            DataLoadReportRequestId = table.Column<int>(type: "int", nullable: true),
            TenantId = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReportRequests", columns => columns.ReportRequestId);
            table.ForeignKey(
                      name: "ReportRequest_ReportRequestStatus",
                      column: columns => columns.Status,
                      principalTable: "ReportRequestStatuses",
                      principalColumn: "ReportRequestStatusId",
                      onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                      name: "ReportRequest_ReportRequestType",
                      column: columns => columns.RequestType,
                      principalTable: "ReportRequestTypes",
                      principalColumn: "ReportRequestTypeId",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateTable(
          name: "ReportRequestStatusHistory",
          columns: table => new
          {
            ReportRequestStatusHistoryId = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            ReportRequestId = table.Column<int>(type: "int", nullable: false),
            Status = table.Column<int>(type: "int", nullable: false),
            CreatedBy = table.Column<int>(type: "int", nullable: false),
            CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ReportRequestStatusHistory", columns => columns.ReportRequestStatusHistoryId);
            table.ForeignKey(
                      name: "FK_ReportRequestStatusHistory_ReportRequests_ReportRequestId",
                      column: columns => columns.ReportRequestId,
                      principalTable: "ReportRequests",
                      principalColumn: "ReportRequestId",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "ReportRequestStatusHistory_ReportRequestStatus",
                      column: columns => columns.Status,
                      principalTable: "ReportRequestStatuses",
                      principalColumn: "ReportRequestStatusId",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.InsertData(
          table: "ReportRequestStatuses",
          columns: new[] { "ReportRequestStatusId", "ReportRequestStatusName" },
          values: new object[,]
          {
                    { 1, "Created" },
                    { -3, "Report Ready Event Failed" },
                    { -2, "Loading Data Failed" },
                    { 32, "Data Expired" },
                    { 31, "Data Removed" },
                    { -1, "Build Report Failed" },
                    { 20, "Data Requested" },
                    { 11, "Loading Data Completed" },
                    { 10, "Loading Data" },
                    { 2, "Request For Data Created" },
                    { 30, "Data No Longer Available" }
          });

      migrationBuilder.InsertData(
          table: "ReportRequestTypes",
          columns: new[] { "ReportRequestTypeId", "ReportRequestTypeName" },
          values: new object[,]
          {
                    { 101, "Download Pdf" },
                    { -1, "View Denied" },
                    { 1, "View" },
                    { 2, "View Data" },
                    { 3, "Load Data" },
                    { 102, "Download Excel" }
          });

      migrationBuilder.CreateIndex(
          name: "IX_ReportDataEntries_ReportRequestId",
          table: "ReportDataEntries",
          column: "ReportRequestId",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_ReportDataEntries_TenantId",
          table: "ReportDataEntries",
          column: "TenantId");

      migrationBuilder.CreateIndex(
          name: "IX_ReportRequests_RequestType",
          table: "ReportRequests",
          column: "RequestType");

      migrationBuilder.CreateIndex(
          name: "IX_ReportRequests_Status",
          table: "ReportRequests",
          column: "Status");

      migrationBuilder.CreateIndex(
          name: "IX_ReportRequests_TenantId",
          table: "ReportRequests",
          column: "TenantId");

      migrationBuilder.CreateIndex(
          name: "IX_ReportRequestStatusHistory_ReportRequestId",
          table: "ReportRequestStatusHistory",
          column: "ReportRequestId");

      migrationBuilder.CreateIndex(
          name: "IX_ReportRequestStatusHistory_Status",
          table: "ReportRequestStatusHistory",
          column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "OneTimeTokens");

      migrationBuilder.DropTable(
          name: "ReportDataEntries");

      migrationBuilder.DropTable(
          name: "ReportRequestStatusHistory");

      migrationBuilder.DropTable(
          name: "Users");

      migrationBuilder.DropTable(
          name: "ReportRequests");

      migrationBuilder.DropTable(
          name: "ReportRequestStatuses");

      migrationBuilder.DropTable(
          name: "ReportRequestTypes");
    }
  }
}
