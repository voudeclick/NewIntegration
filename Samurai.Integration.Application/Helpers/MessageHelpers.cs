using Samurai.Integration.APIClient.Millennium.Models.Results;
using Samurai.Integration.Domain.Messages.SellerCenter;
using Samurai.Integration.Domain.Messages.SellerCenter.ProductActor;
using System.Collections.Generic;
using System.Linq;
using Product = Samurai.Integration.Domain.Models.Product;

namespace Samurai.Integration.Application.Helpers
{
    public class MessageHelpers
    {
        public static Tout TransformToMessage<Tout>(List<MillenniumApiListPricesResult.Value> source, bool hasZeroedPrice = false) 
            where Tout: SellerCenterUpdatePriceAndStockMessage
        {
            var priceMessageList = new SellerCenterUpdatePriceAndStockMessage();

            priceMessageList.ProductId = source.FirstOrDefault().produto.ToString();
            if (hasZeroedPrice)
            {
                priceMessageList.Values
                .AddRange(source.Select(x => new Product.SkuPrice
                {
                    Sku = x.sku,
                    Price = x.preco1.Value > 0 ? x.preco1.Value : x.preco2 ?? 0,
                    CompareAtPrice = x.preco2 ?? x.preco1.Value,
                }).ToList());
                return (Tout)priceMessageList;
            }

            priceMessageList.Values
           .AddRange(source.Select(x => new Product.SkuPrice
           {
               Sku = x.sku,
               Price = x.preco1.Value,
               CompareAtPrice = x.preco2 ?? x.preco1.Value
           }).ToList());

            return (Tout)priceMessageList;
        }

    }
}
