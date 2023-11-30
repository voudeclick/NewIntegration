﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Samurai.Integration.EntityFramework.Database;

namespace Samurai.Integration.EntityFramework.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210914164338_AddMillenniumNewStockProcessMigration")]
    partial class AddMillenniumNewStockProcessMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.Integrations.Millenium.MillenniumProductStockIntegration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("IntegrationDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("MillenniumNewStockProcessId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Payload")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductSku")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<long>("TenantId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("MillenniumProductStockIntegrations");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.Integrations.Shopify.ShopifyProductStockIntegration", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("IntegrationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Payload")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("ProductShopifyId")
                        .HasColumnType("bigint");

                    b.Property<string>("ProductShopifySku")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ReferenceIntegrationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Result")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<long>("TenantId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("ShopifyProductStockIntegrations");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.MillenniumNewStockProcess", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("FinalTransId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("FinalUpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<long?>("InitialTransId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("InitialUpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ProcessDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("TenantId")
                        .HasColumnType("bigint");

                    b.Property<int?>("Top")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("MillenniumNewStockProcesses");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.Tenant", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BodyIntegrationType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("DaysToDelivery")
                        .HasColumnType("int");

                    b.Property<bool>("DisableAddressParse")
                        .HasColumnType("bit");

                    b.Property<bool>("DisableCustomerDocument")
                        .HasColumnType("bit");

                    b.Property<bool>("EnablePier8Integration")
                        .HasColumnType("bit");

                    b.Property<bool>("ImageIntegrationEnabled")
                        .HasColumnType("bit");

                    b.Property<int>("IntegrationType")
                        .HasColumnType("int");

                    b.Property<long>("MinOrderId")
                        .HasColumnType("bigint");

                    b.Property<bool>("MultiLocation")
                        .HasColumnType("bit");

                    b.Property<bool>("OrderIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<bool>("ParsePhoneDDD")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductDescriptionIsHTML")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductGroupingEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<bool>("SetProductsAsUnpublished")
                        .HasColumnType("bit");

                    b.Property<string>("ShopifyAppJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShopifyStoreDomain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("StoreHandle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StoreName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TenantLogging")
                        .HasColumnType("bit");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("WriteCategoryNameTags")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.BlingData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("APIKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ApiBaseUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoriaId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastProductUpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrderField")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("OrderIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<string>("OrderPrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("OrderStatusId")
                        .HasColumnType("int");

                    b.Property<string>("OrderStatusMapping")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ProductIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<string>("StoreHandle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("BlingData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.LocationMap", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("JsonMap")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("LocationMap");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<bool>("CapitalizeProductName")
                        .HasColumnType("bit");

                    b.Property<bool>("ControlStockByUpdateDate")
                        .HasColumnType("bit");

                    b.Property<string>("CorDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CorField")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DescriptionField")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EnableSaveIntegrationInformations")
                        .HasColumnType("bit");

                    b.Property<bool>("EnabledApprovedTransaction")
                        .HasColumnType("bit");

                    b.Property<bool>("EnabledStockMto")
                        .HasColumnType("bit");

                    b.Property<string>("EstampaDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EstampaField")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExcludedProductCharactersJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtraFieldConfigurationJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("HasZeroedPriceCase")
                        .HasColumnType("bit");

                    b.Property<string>("LoginJson")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NameField")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("NameSkuEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("NameSkuField")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfItensPerAPIQuery")
                        .HasColumnType("int");

                    b.Property<int>("OperatorType")
                        .HasColumnType("int");

                    b.Property<string>("OrderPrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SaleWithoutStockEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("SendDefaultCor")
                        .HasColumnType("bit");

                    b.Property<bool>("SendDefaultEstampa")
                        .HasColumnType("bit");

                    b.Property<bool>("SendDefaultTamanho")
                        .HasColumnType("bit");

                    b.Property<string>("SkuFieldType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SplitEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("TamanhoDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TamanhoField")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("VitrineId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("MillenniumData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumTransId", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("MillenniumDataId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("MillenniumLastUpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<long>("Value")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("MillenniumDataId");

                    b.ToTable("MillenniumTransId");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.NexaasData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeliveryTimeTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPickupPointEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("OrderPrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("OrganizationId")
                        .HasColumnType("bigint");

                    b.Property<long>("SaleChannelId")
                        .HasColumnType("bigint");

                    b.Property<string>("ServiceNameTemplate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("StockId")
                        .HasColumnType("bigint");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("NexaasData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.OmieData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("AppKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AppSecret")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("AppendSkuCode")
                        .HasColumnType("bit");

                    b.Property<string>("CategoryCaracteristicasJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CodigoCategoria")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CodigoCenarioImposto")
                        .HasColumnType("int");

                    b.Property<int?>("CodigoContaCorrente")
                        .HasColumnType("int");

                    b.Property<string>("CodigoEtapaPaymentConfirmed")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CodigoLocalEstoque")
                        .HasColumnType("int");

                    b.Property<bool>("CodigoParcelaFixa")
                        .HasColumnType("bit");

                    b.Property<string>("CodigoParcelaMappingJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("EscapeBodyPipe")
                        .HasColumnType("bit");

                    b.Property<string>("ExtraNotaFiscalEmailsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FreteMappingJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NameField")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("NaoGerarFinanceiro")
                        .HasColumnType("bit");

                    b.Property<string>("NormalizeProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderPrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ParcelaUnica")
                        .HasColumnType("bit");

                    b.Property<bool>("SendNotaFiscalEmailToClient")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("VariantCaracteristicasJson")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OmieData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.Pier8Data", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("ApiKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Pier8Data");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.PluggToData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("AccountSellerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AccountUserId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientSecret")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrderStatusMapping")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PluggToData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("OrderIntegrationStatus")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SellWithoutStock")
                        .HasColumnType("bit");

                    b.Property<string>("SellerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SellerWarehouseId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SellerCenterData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterTransId", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("LastProcessedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<long>("SellerCenterDataId")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SellerCenterDataId");

                    b.ToTable("SellerCenterTransId");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.ShopifyData", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<bool>("BlockFulfillmentNotificationPerShipmentService")
                        .HasColumnType("bit");

                    b.Property<int>("BodyIntegrationType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("DaysToDelivery")
                        .HasColumnType("int");

                    b.Property<int>("DelayProcessOfShopifyUpdateOrderMessages")
                        .HasColumnType("int");

                    b.Property<bool>("DisableAddressParse")
                        .HasColumnType("bit");

                    b.Property<bool>("DisableCustomerDocument")
                        .HasColumnType("bit");

                    b.Property<bool>("EnableSaveIntegrationInformations")
                        .HasColumnType("bit");

                    b.Property<bool>("EnableStockProductKit")
                        .HasColumnType("bit");

                    b.Property<bool>("HasProductKit")
                        .HasColumnType("bit");

                    b.Property<bool>("ImageIntegrationEnabled")
                        .HasColumnType("bit");

                    b.Property<long>("MinOrderId")
                        .HasColumnType("bigint");

                    b.Property<bool>("NotConsiderProductIfPriceIsZero")
                        .HasColumnType("bit");

                    b.Property<bool>("OrderIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<bool>("ParsePhoneDDD")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductDescriptionIsHTML")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductGroupingEnabled")
                        .HasColumnType("bit");

                    b.Property<bool>("ProductIntegrationStatus")
                        .HasColumnType("bit");

                    b.Property<bool>("SetProductsAsUnpublished")
                        .HasColumnType("bit");

                    b.Property<string>("ShipmentServicesForFulfillmentNotification")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShopifyAppJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShopifyStoreDomain")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("WriteCategoryNameTags")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("ShopifyData");
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.BlingData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("BlingData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.BlingData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.LocationMap", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("LocationMap")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.LocationMap", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("MillenniumData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumTransId", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.TenantData.MillenniumData", null)
                        .WithMany("TransIds")
                        .HasForeignKey("MillenniumDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.NexaasData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("NexaasData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.NexaasData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.OmieData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("OmieData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.OmieData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.Pier8Data", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("Pier8Data")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.Pier8Data", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.PluggToData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("PluggToData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.PluggToData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("SellerCenterData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterTransId", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.TenantData.SellerCenterData", null)
                        .WithMany("TransIds")
                        .HasForeignKey("SellerCenterDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Samurai.Integration.Domain.Entities.Database.TenantData.ShopifyData", b =>
                {
                    b.HasOne("Samurai.Integration.Domain.Entities.Database.Tenant", null)
                        .WithOne("ShopifyData")
                        .HasForeignKey("Samurai.Integration.Domain.Entities.Database.TenantData.ShopifyData", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
