using System.Collections.Generic;
using System.Linq;
using static Samurai.Integration.Domain.Models.Product;

namespace Samurai.Integration.Domain.Extensions
{
    public static class ProductExtensions
    {
        public static List<ListVariations> DistinctVariations(this List<SkuSellerCenter> obj) 
        {
            var itens = new List<ListVariations>();
            var variantions = obj.SelectMany(x => x.InfoVariations).GroupBy(x => x.NomeVariacao).ToList();

            foreach (var variation in variantions)
            {
                itens.Add(new ListVariations {
                    NomeVariacao = variation.Key,
                    Values = variation.GroupBy(y => y.ValorVariacao).Select(x => x.Key).Distinct().ToList()
                });
            }

            return itens;
        }
    }
}
