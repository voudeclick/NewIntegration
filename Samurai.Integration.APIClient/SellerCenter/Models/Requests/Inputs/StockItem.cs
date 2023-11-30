namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class StockItem
    {
        /// <summary>
        /// Id do Armazem
        /// </summary>
        public string SellerWarehouseId { get; set; }
        public string CurrencyCode { get => "BRL"; }
        public decimal? FromPrice { get; set; }
        public decimal? ByPrice { get; set; }
        public int Quantity { get; set; }
        public bool SellWithoutStock { get; set; }
    }
}
