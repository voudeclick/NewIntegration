using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VDC.Integration.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class vdcinitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "IntegrationCache",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
                    Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ExpiresAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SlidingExpirationInSeconds = table.Column<long>(type: "bigint", nullable: true),
                    AbsoluteExpiration = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationErrors",
                columns: table => new
                {
                    Tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    MessagePattern = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationErrors", x => x.Tag);
                });

            migrationBuilder.CreateTable(
                name: "LogsAbandonMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAbandonMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumImageIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    IdProduto = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumIntegrationProductReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MillenniumResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumImageIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumListProductManualProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumListProductManualProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumNewPricesProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialTransId = table.Column<long>(type: "bigint", nullable: true),
                    FinalTransId = table.Column<long>(type: "bigint", nullable: true),
                    InitialUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Top = table.Column<int>(type: "int", nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    MillenniumResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastPrice = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumNewPricesProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumNewProductProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialTransId = table.Column<long>(type: "bigint", nullable: true),
                    FinalTransId = table.Column<long>(type: "bigint", nullable: true),
                    InitialUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Top = table.Column<int>(type: "int", nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    MillenniumResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastFirstAndLastIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumNewProductProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumNewStockProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialTransId = table.Column<long>(type: "bigint", nullable: true),
                    FinalTransId = table.Column<long>(type: "bigint", nullable: true),
                    InitialUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Top = table.Column<int>(type: "int", nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    MillenniumResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumNewStockProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumOrderStatusUpdate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    CodPedidov = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumOrderStatusUpdate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductImageIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    IdProduto = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumListProductProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MillenniumIntegrationProductReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Routine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductImageIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumNewProductProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductPriceIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductSku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumNewPriceProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductPriceIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumProductStockIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductSku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumNewStockProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumProductStockIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumUpdateOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MillenniumResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumUpdateOrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OmieUpdateOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OmieResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmieUpdateOrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Params",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Params", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyListOrderIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShopifyListOrderProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyListOrderIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyListOrderProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShopifyResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyListOrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductImageIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductShopifyId = table.Column<long>(type: "bigint", nullable: true),
                    ExternalProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductImageIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductShopifyId = table.Column<long>(type: "bigint", nullable: true),
                    ProductShopifySku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductPriceIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductShopifyId = table.Column<long>(type: "bigint", nullable: true),
                    ProductShopifySku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductPriceIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyProductStockIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ProductShopifyId = table.Column<long>(type: "bigint", nullable: true),
                    ProductShopifySku = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReferenceIntegrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyProductStockIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyUpdateOrderTagNumberProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderUpdateMutationInput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopifyResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopifyListOrderProcessReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyUpdateOrderTagNumberProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ProductIntegrationStatus = table.Column<bool>(type: "bit", nullable: false),
                    SetProductsAsUnpublished = table.Column<bool>(type: "bit", nullable: false),
                    BodyIntegrationType = table.Column<int>(type: "int", nullable: false),
                    ProductGroupingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProductDescriptionIsHTML = table.Column<bool>(type: "bit", nullable: false),
                    WriteCategoryNameTags = table.Column<bool>(type: "bit", nullable: false),
                    ImageIntegrationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OrderIntegrationStatus = table.Column<bool>(type: "bit", nullable: false),
                    DisableCustomerDocument = table.Column<bool>(type: "bit", nullable: false),
                    DisableAddressParse = table.Column<bool>(type: "bit", nullable: false),
                    ParsePhoneDDD = table.Column<bool>(type: "bit", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreHandle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShopifyStoreDomain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShopifyAppJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationType = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DaysToDelivery = table.Column<int>(type: "int", nullable: false),
                    MinOrderId = table.Column<long>(type: "bigint", nullable: false),
                    MultiLocation = table.Column<bool>(type: "bit", nullable: false),
                    TenantLogging = table.Column<bool>(type: "bit", nullable: false),
                    DeactivatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Search = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersProfile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationMap",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    JsonMap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMap_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MethodPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTypeShopify = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentTypeMillenniun = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodPayment_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LoginJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExtraFieldConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlExtraPaymentInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExcludedProductCharactersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VitrineId = table.Column<long>(type: "bigint", nullable: false),
                    SplitEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SaleWithoutStockEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProductIntegrationPrice = table.Column<bool>(type: "bit", nullable: false),
                    NameField = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionField = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderPrefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendDefaultCor = table.Column<bool>(type: "bit", nullable: false),
                    TamanhoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TamanhoField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendDefaultTamanho = table.Column<bool>(type: "bit", nullable: false),
                    EstampaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstampaField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendDefaultEstampa = table.Column<bool>(type: "bit", nullable: false),
                    CapitalizeProductName = table.Column<bool>(type: "bit", nullable: false),
                    NameSkuEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NameSkuField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnabledStockMto = table.Column<bool>(type: "bit", nullable: false),
                    EnabledApprovedTransaction = table.Column<bool>(type: "bit", nullable: false),
                    SkuFieldType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlStockByUpdateDate = table.Column<bool>(type: "bit", nullable: false),
                    ControlPriceByUpdateDate = table.Column<bool>(type: "bit", nullable: false),
                    ControlProductByUpdateDate = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfItensPerAPIQuery = table.Column<int>(type: "int", nullable: false),
                    EnableSaveProcessIntegrations = table.Column<bool>(type: "bit", nullable: false),
                    EnableSaveIntegrationInformations = table.Column<bool>(type: "bit", nullable: false),
                    EnableExtraPaymentInformation = table.Column<bool>(type: "bit", nullable: false),
                    HasZeroedPriceCase = table.Column<bool>(type: "bit", nullable: false),
                    OperatorType = table.Column<int>(type: "int", nullable: false),
                    StoreDomainByBrasPag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableProductKit = table.Column<bool>(type: "bit", nullable: false),
                    EnableMaskedNSU = table.Column<bool>(type: "bit", nullable: false),
                    SendPaymentMethod = table.Column<bool>(type: "bit", nullable: false),
                    EnableProductDiscount = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MillenniumData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OmieData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    AppKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderPrefix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EscapeBodyPipe = table.Column<bool>(type: "bit", nullable: false),
                    AppendSkuCode = table.Column<bool>(type: "bit", nullable: false),
                    CodigoLocalEstoque = table.Column<long>(type: "bigint", nullable: true),
                    CodigoCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoContaCorrente = table.Column<long>(type: "bigint", nullable: true),
                    CodigoCenarioImposto = table.Column<long>(type: "bigint", nullable: true),
                    CodigoEtapaPaymentConfirmed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SendNotaFiscalEmailToClient = table.Column<bool>(type: "bit", nullable: false),
                    ExtraNotaFiscalEmailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoParcelaMappingJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParcelaUnica = table.Column<bool>(type: "bit", nullable: false),
                    NaoGerarFinanceiro = table.Column<bool>(type: "bit", nullable: false),
                    NameField = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoParcelaFixa = table.Column<bool>(type: "bit", nullable: false),
                    NormalizeProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VariantCaracteristicasJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryCaracteristicasJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreteMappingJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmieData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OmieData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopifyData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ProductIntegrationStatus = table.Column<bool>(type: "bit", nullable: false),
                    SetProductsAsUnpublished = table.Column<bool>(type: "bit", nullable: false),
                    BodyIntegrationType = table.Column<int>(type: "int", nullable: false),
                    ProductGroupingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProductDescriptionIsHTML = table.Column<bool>(type: "bit", nullable: false),
                    WriteCategoryNameTags = table.Column<bool>(type: "bit", nullable: false),
                    ImageIntegrationEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OrderIntegrationStatus = table.Column<bool>(type: "bit", nullable: false),
                    DisableCustomerDocument = table.Column<bool>(type: "bit", nullable: false),
                    DisableAddressParse = table.Column<bool>(type: "bit", nullable: false),
                    ParsePhoneDDD = table.Column<bool>(type: "bit", nullable: false),
                    ShopifyStoreDomain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShopifyAppJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysToDelivery = table.Column<int>(type: "int", nullable: false),
                    MinOrderId = table.Column<long>(type: "bigint", nullable: false),
                    HasProductKit = table.Column<bool>(type: "bit", nullable: false),
                    EnableStockProductKit = table.Column<bool>(type: "bit", nullable: false),
                    BlockFulfillmentNotificationPerShipmentService = table.Column<bool>(type: "bit", nullable: false),
                    ShipmentServicesForFulfillmentNotification = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DelayProcessOfShopifyUpdateOrderMessages = table.Column<int>(type: "int", nullable: false),
                    EnableSaveIntegrationInformations = table.Column<bool>(type: "bit", nullable: false),
                    NotConsiderProductIfPriceIsZero = table.Column<bool>(type: "bit", nullable: false),
                    SkuFieldType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnableMaxVariantsQueryGraphQL = table.Column<bool>(type: "bit", nullable: false),
                    MaxVariantsQueryGraphQL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableScheduledNextHour = table.Column<bool>(type: "bit", nullable: false),
                    EnableAuxiliaryCountry = table.Column<bool>(type: "bit", nullable: false),
                    DisableUpdateProduct = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopifyData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopifyData_Tenants_Id",
                        column: x => x.Id,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MercadoPago",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MillenniumDataId = table.Column<long>(type: "bigint", nullable: false),
                    Authorization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MercadoPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MercadoPago_MillenniumData_MillenniumDataId",
                        column: x => x.MillenniumDataId,
                        principalTable: "MillenniumData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MillenniumTransId",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MillenniumDataId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    MillenniumLastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MillenniumTransId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MillenniumTransId_MillenniumData_MillenniumDataId",
                        column: x => x.MillenniumDataId,
                        principalTable: "MillenniumData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationCache_ExpiresAtTime",
                schema: "dbo",
                table: "IntegrationCache",
                column: "ExpiresAtTime");

            migrationBuilder.CreateIndex(
                name: "IX_MercadoPago_MillenniumDataId",
                table: "MercadoPago",
                column: "MillenniumDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MethodPayment_TenantId",
                table: "MethodPayment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MillenniumTransId_MillenniumDataId",
                table: "MillenniumTransId",
                column: "MillenniumDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationCache",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "IntegrationErrors");

            migrationBuilder.DropTable(
                name: "LocationMap");

            migrationBuilder.DropTable(
                name: "LogsAbandonMessages");

            migrationBuilder.DropTable(
                name: "MercadoPago");

            migrationBuilder.DropTable(
                name: "MethodPayment");

            migrationBuilder.DropTable(
                name: "MillenniumImageIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumListProductManualProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumNewPricesProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumNewProductProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumNewStockProcesses");

            migrationBuilder.DropTable(
                name: "MillenniumOrderStatusUpdate");

            migrationBuilder.DropTable(
                name: "MillenniumProductImageIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumProductIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumProductPriceIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumProductStockIntegrations");

            migrationBuilder.DropTable(
                name: "MillenniumTransId");

            migrationBuilder.DropTable(
                name: "MillenniumUpdateOrderProcesses");

            migrationBuilder.DropTable(
                name: "OmieData");

            migrationBuilder.DropTable(
                name: "OmieUpdateOrderProcesses");

            migrationBuilder.DropTable(
                name: "Params");

            migrationBuilder.DropTable(
                name: "ShopifyData");

            migrationBuilder.DropTable(
                name: "ShopifyListOrderIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyListOrderProcesses");

            migrationBuilder.DropTable(
                name: "ShopifyProductImageIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductPriceIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyProductStockIntegrations");

            migrationBuilder.DropTable(
                name: "ShopifyUpdateOrderTagNumberProcesses");

            migrationBuilder.DropTable(
                name: "UsersProfile");

            migrationBuilder.DropTable(
                name: "MillenniumData");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
