using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalFormsSystem.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "AssetTypes",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         AssetTypeName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //         IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__AssetTyp__3214EC27F8985D91", x => x.ID);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "Employees",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         EmployeeNo = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
            //         Name = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
            //         DateHired = table.Column<DateOnly>(type: "date", nullable: true),
            //         Company = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
            //         Location = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
            //         Department = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         Section = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         Category = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
            //         Status = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
            //         IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
            //         PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         IsFirstLogin = table.Column<bool>(type: "bit", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__Employee__3214EC270C6F6526", x => x.ID);
            //     });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Employees_UserId",
                        column: x => x.UserId,
                        principalTable: "Employees",
                        principalColumn: "ID");
                });

            // migrationBuilder.CreateTable(
            //     name: "DamagedReports",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         ControlNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         Item = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         FixedAssetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         DatePurchased = table.Column<DateOnly>(type: "date", nullable: true),
            //         BrandSize = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         LocationUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         IncidentDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //         CauseOfDamage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         ImmediateAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         RecommendedAction = table.Column<int>(type: "int", nullable: true),
            //         ReportedByEmployeeId = table.Column<int>(type: "int", nullable: false),
            //         ReceivedByEmployeeId = table.Column<int>(type: "int", nullable: true),
            //         ReceivedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //         Findings = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         NegligenceFlag = table.Column<int>(type: "int", nullable: true),
            //         NegligenceDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         AdministrativeDiscipline = table.Column<bool>(type: "bit", nullable: true),
            //         InvestigatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
            //         VerifiedByEmployeeId = table.Column<int>(type: "int", nullable: true),
            //         NotedByEmployeeId = table.Column<int>(type: "int", nullable: true),
            //         RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            //         UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_DamagedReports", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_DamagedReports_Employees_InvestigatedByEmployeeId",
            //             column: x => x.InvestigatedByEmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK_DamagedReports_Employees_NotedByEmployeeId",
            //             column: x => x.NotedByEmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK_DamagedReports_Employees_ReceivedByEmployeeId",
            //             column: x => x.ReceivedByEmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK_DamagedReports_Employees_ReportedByEmployeeId",
            //             column: x => x.ReportedByEmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "ID",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_DamagedReports_Employees_VerifiedByEmployeeId",
            //             column: x => x.VerifiedByEmployeeId,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "FixedAssetRequests",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         ControlNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //         DateRequested = table.Column<DateOnly>(type: "date", nullable: false),
            //         Department = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //         TargetDateNeeded = table.Column<DateOnly>(type: "date", nullable: false),
            //         Section = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
            //         Quantity = table.Column<int>(type: "int", nullable: false),
            //         AssetTypeID = table.Column<int>(type: "int", nullable: true),
            //         AssetType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         DetailedDescription = table.Column<string>(type: "text", nullable: false),
            //         ReasonPurpose = table.Column<string>(type: "text", nullable: false),
            //         ProposedLocation = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         EstimatedLifeSpan = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         RequestType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
            //         ExistingUnitCount = table.Column<int>(type: "int", nullable: true),
            //         ExistingUser = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         DamagedReportNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //         RequestedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         RequestedByName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         RequestedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         EvaluatedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         EvaluatedByName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         EvaluatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         RequestStatus = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true, defaultValue: "Draft"),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
            //         UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__FixedAss__3214EC27F65086B2", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Asset__4316F928",
            //             column: x => x.AssetTypeID,
            //             principalTable: "AssetTypes",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Evalu__45F365D3",
            //             column: x => x.EvaluatedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Reque__44FF419A",
            //             column: x => x.RequestedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "FixedAssetTransferHistory",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
            //         FromDepartment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         FromSection = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         FromEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         ToDepartment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ToSection = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ToEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         TransferDate = table.Column<DateOnly>(type: "date", nullable: false),
            //         TransferReason = table.Column<string>(type: "text", nullable: true),
            //         ProcessedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__FixedAss__3214EC27B06C25E6", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__FromE__6D0D32F4",
            //             column: x => x.FromEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Proce__6EF57B66",
            //             column: x => x.ProcessedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__ToEmp__6E01572D",
            //             column: x => x.ToEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "DamagedReportImages",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         DamagedReportId = table.Column<int>(type: "int", nullable: false),
            //         Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //         FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //         FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            //         ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //         DisplayOrder = table.Column<int>(type: "int", nullable: false),
            //         UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_DamagedReportImages", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_DamagedReportImages_DamagedReports_DamagedReportId",
            //             column: x => x.DamagedReportId,
            //             principalTable: "DamagedReports",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "ExistingUnitDetails",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetRequestID = table.Column<int>(type: "int", nullable: false),
            //         ItemNo = table.Column<int>(type: "int", nullable: false),
            //         Description = table.Column<string>(type: "text", nullable: false),
            //         Location = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         UserName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         Remarks = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__Existing__3214EC27CE005FA4", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__ExistingU__Fixed__5535A963",
            //             column: x => x.FixedAssetRequestID,
            //             principalTable: "FixedAssetRequests",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "FixedAssetPrintLogs",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetRequestID = table.Column<int>(type: "int", nullable: false),
            //         PrintedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         PrintDateTime = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
            //         PrintFormat = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "Full Form"),
            //         IPAddress = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //         UserAgent = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
            //         Remarks = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__FixedAss__3214EC27840E0AAA", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Fixed__628FA481",
            //             column: x => x.FixedAssetRequestID,
            //             principalTable: "FixedAssetRequests",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Print__6383C8BA",
            //             column: x => x.PrintedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "FixedAssetRequestApprovals",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetRequestID = table.Column<int>(type: "int", nullable: false),
            //         ReceivedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         ReceivedDate = table.Column<DateOnly>(type: "date", nullable: true),
            //         Quotation1Reference = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         Quotation1Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //         Quotation2Reference = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         Quotation2Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //         ExecutiveRemarks = table.Column<string>(type: "text", nullable: true),
            //         ExecutiveRecommendingApproval = table.Column<bool>(type: "bit", nullable: true),
            //         ExecutiveEvaluatedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         ExecutiveEvaluatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         VPApproved = table.Column<bool>(type: "bit", nullable: true),
            //         VPRemarks = table.Column<string>(type: "text", nullable: true),
            //         VPApprovedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         VPApprovedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         PresidentApproved = table.Column<bool>(type: "bit", nullable: true),
            //         PresidentRemarks = table.Column<string>(type: "text", nullable: true),
            //         PresidentApprovedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         PresidentApprovedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         FixedAssetCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //         IsCapitalized = table.Column<bool>(type: "bit", nullable: true),
            //         AmortizationMonths = table.Column<int>(type: "int", nullable: true),
            //         FinanceRemarks = table.Column<string>(type: "text", nullable: true),
            //         FinanceProcessedByEmployeeID = table.Column<int>(type: "int", nullable: true),
            //         FinanceProcessedAt = table.Column<DateTime>(type: "datetime", nullable: true),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
            //         UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__FixedAss__3214EC2767ABA27B", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Execu__4D94879B",
            //             column: x => x.ExecutiveEvaluatedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Finan__5070F446",
            //             column: x => x.FinanceProcessedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Fixed__4BAC3F29",
            //             column: x => x.FixedAssetRequestID,
            //             principalTable: "FixedAssetRequests",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Presi__4F7CD00D",
            //             column: x => x.PresidentApprovedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__Recei__4CA06362",
            //             column: x => x.ReceivedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__FixedAsse__VPApp__4E88ABD4",
            //             column: x => x.VPApprovedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "MemorandumReceipt",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetRequestID = table.Column<int>(type: "int", nullable: false),
            //         FixedAssetCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
            //         Department = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         Section = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         PONumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //         PODate = table.Column<DateOnly>(type: "date", nullable: true),
            //         TransactionType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
            //         ItemDescription = table.Column<string>(type: "text", nullable: false),
            //         Manufacturer = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         SerialNumber = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ModelNumber = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         Brand = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ReceivedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         ReceivedByName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         ReceivedSignature = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ReceivedDate = table.Column<DateOnly>(type: "date", nullable: false),
            //         ReleasedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         ReleasedByName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
            //         ReleasedSignature = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //         ReleasedDate = table.Column<DateOnly>(type: "date", nullable: false),
            //         CCPurchasing = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
            //         CCFinance = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
            //         CCRequestingDept = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
            //         CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__Memorand__3214EC277729DE94", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__Memorandu__Fixed__59063A47",
            //             column: x => x.FixedAssetRequestID,
            //             principalTable: "FixedAssetRequests",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__Memorandu__Recei__5AEE82B9",
            //             column: x => x.ReceivedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__Memorandu__Relea__5BE2A6F2",
            //             column: x => x.ReleasedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "RequestStatusHistory",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         FixedAssetRequestID = table.Column<int>(type: "int", nullable: false),
            //         OldStatus = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
            //         NewStatus = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
            //         ChangedByEmployeeID = table.Column<int>(type: "int", nullable: false),
            //         Remarks = table.Column<string>(type: "text", nullable: true),
            //         ChangedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK__RequestS__3214EC27B134BA8C", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK__RequestSt__Chang__693CA210",
            //             column: x => x.ChangedByEmployeeID,
            //             principalTable: "Employees",
            //             principalColumn: "ID");
            //         table.ForeignKey(
            //             name: "FK__RequestSt__Fixed__68487DD7",
            //             column: x => x.FixedAssetRequestID,
            //             principalTable: "FixedAssetRequests",
            //             principalColumn: "ID");
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "UQ__AssetTyp__6824772C5E2EF8A2",
            //     table: "AssetTypes",
            //     column: "AssetTypeName",
            //     unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReportImages_DamagedReportId",
            //     table: "DamagedReportImages",
            //     column: "DamagedReportId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReports_InvestigatedByEmployeeId",
            //     table: "DamagedReports",
            //     column: "InvestigatedByEmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReports_NotedByEmployeeId",
            //     table: "DamagedReports",
            //     column: "NotedByEmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReports_ReceivedByEmployeeId",
            //     table: "DamagedReports",
            //     column: "ReceivedByEmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReports_ReportedByEmployeeId",
            //     table: "DamagedReports",
            //     column: "ReportedByEmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_DamagedReports_VerifiedByEmployeeId",
            //     table: "DamagedReports",
            //     column: "VerifiedByEmployeeId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Employees_Department",
            //     table: "Employees",
            //     column: "Department");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Employees_EmployeeNo",
            //     table: "Employees",
            //     column: "EmployeeNo");

            // migrationBuilder.CreateIndex(
            //     name: "UQ__Employee__7AD0F1B749DF82FF",
            //     table: "Employees",
            //     column: "EmployeeNo",
            //     unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_ExistingUnitDetails_FixedAssetRequestID",
            //     table: "ExistingUnitDetails",
            //     column: "FixedAssetRequestID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetPrintLogs_FixedAssetRequestID",
            //     table: "FixedAssetPrintLogs",
            //     column: "FixedAssetRequestID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetPrintLogs_PrintedByEmployeeID",
            //     table: "FixedAssetPrintLogs",
            //     column: "PrintedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_ExecutiveEvaluatedByEmployeeID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "ExecutiveEvaluatedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_FinanceProcessedByEmployeeID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "FinanceProcessedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_FixedAssetRequestID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "FixedAssetRequestID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_PresidentApprovedByEmployeeID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "PresidentApprovedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_ReceivedByEmployeeID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "ReceivedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequestApprovals_VPApprovedByEmployeeID",
            //     table: "FixedAssetRequestApprovals",
            //     column: "VPApprovedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_AssetTypeID",
            //     table: "FixedAssetRequests",
            //     column: "AssetTypeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_ControlNo",
            //     table: "FixedAssetRequests",
            //     column: "ControlNo");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_DateRequested",
            //     table: "FixedAssetRequests",
            //     column: "DateRequested");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_Department",
            //     table: "FixedAssetRequests",
            //     column: "Department");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_EvaluatedByEmployeeID",
            //     table: "FixedAssetRequests",
            //     column: "EvaluatedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_RequestedByEmployeeID",
            //     table: "FixedAssetRequests",
            //     column: "RequestedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetRequests_RequestStatus",
            //     table: "FixedAssetRequests",
            //     column: "RequestStatus");

            // migrationBuilder.CreateIndex(
            //     name: "UQ__FixedAss__091DC38FA5A637C2",
            //     table: "FixedAssetRequests",
            //     column: "ControlNo",
            //     unique: true,
            //     filter: "[ControlNo] IS NOT NULL");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetTransferHistory_FromEmployeeID",
            //     table: "FixedAssetTransferHistory",
            //     column: "FromEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetTransferHistory_ProcessedByEmployeeID",
            //     table: "FixedAssetTransferHistory",
            //     column: "ProcessedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_FixedAssetTransferHistory_ToEmployeeID",
            //     table: "FixedAssetTransferHistory",
            //     column: "ToEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_MemorandumReceipt_FixedAssetRequestID",
            //     table: "MemorandumReceipt",
            //     column: "FixedAssetRequestID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_MemorandumReceipt_ReceivedByEmployeeID",
            //     table: "MemorandumReceipt",
            //     column: "ReceivedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_MemorandumReceipt_ReleasedByEmployeeID",
            //     table: "MemorandumReceipt",
            //     column: "ReleasedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_RequestStatusHistory_ChangedByEmployeeID",
            //     table: "RequestStatusHistory",
            //     column: "ChangedByEmployeeID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_RequestStatusHistory_FixedAssetRequestID",
            //     table: "RequestStatusHistory",
            //     column: "FixedAssetRequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            // migrationBuilder.DropTable(
            //     name: "DamagedReportImages");

            // migrationBuilder.DropTable(
            //     name: "ExistingUnitDetails");

            // migrationBuilder.DropTable(
            //     name: "FixedAssetPrintLogs");

            // migrationBuilder.DropTable(
            //     name: "FixedAssetRequestApprovals");

            // migrationBuilder.DropTable(
            //     name: "FixedAssetTransferHistory");

            // migrationBuilder.DropTable(
            //     name: "MemorandumReceipt");

            // migrationBuilder.DropTable(
            //     name: "RequestStatusHistory");

            // migrationBuilder.DropTable(
            //     name: "DamagedReports");

            // migrationBuilder.DropTable(
            //     name: "FixedAssetRequests");

            // migrationBuilder.DropTable(
            //     name: "AssetTypes");

            // migrationBuilder.DropTable(
            //     name: "Employees");
        }
    }
}
