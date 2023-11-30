using Microsoft.EntityFrameworkCore.Migrations;

namespace Samurai.Integration.EntityFramework.Migrations
{
    public partial class CopyDataShopifyOnTenanstForShopifyData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var query = @"INSERT INTO [Samurai.Integration].[dbo].[ShopifyData] ([Id],[CreationDate],[UpdateDate],[Status],[ProductIntegrationStatus],[SetProductsAsUnpublished],[BodyIntegrationType],[ProductGroupingEnabled],[ProductDescriptionIsHTML],[WriteCategoryNameTags],[ImageIntegrationEnabled],[OrderIntegrationStatus],[DisableCustomerDocument],[DisableAddressParse],[ParsePhoneDDD],[ShopifyStoreDomain],[ShopifyAppJson],[DaysToDelivery],[MinOrderId])
                        SELECT [Id],[CreationDate],[UpdateDate],[Status],[ProductIntegrationStatus],[SetProductsAsUnpublished],[BodyIntegrationType],[ProductGroupingEnabled],[ProductDescriptionIsHTML],[WriteCategoryNameTags],[ImageIntegrationEnabled],[OrderIntegrationStatus],[DisableCustomerDocument],[DisableAddressParse],[ParsePhoneDDD],[ShopifyStoreDomain],[ShopifyAppJson],[DaysToDelivery],[MinOrderId]
                        FROM [Samurai.Integration].[dbo].[Tenants]  
                        WHERE IntegrationType = 1 and not exists(SELECT TOP 1 1 from [Samurai.Integration].dbo.ShopifyData spdata where spdata.Id = Id);";

            migrationBuilder.Sql(query);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("TRUNCATE TABLE  [Samurai.Integration].dbo.ShopifyData ");

        }

    }
}
