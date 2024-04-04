using System;

namespace VDC.Integration.APIClient.API.Models.Result
{
    public class PaymentExtraInfoBrasPagResult
    {
        public string x_gateway_nsu { get; set; }
        public string x_gateway_authorization_code { get; set; }
        public string x_gateway_approvement_date { get; set; }
        public string x_gateway_boleto_number { get; set; }
    }

    public class PaymentExtraInfoMoipResult
    {
        public string gatewayToken { get; set; }
        public string gatewayOrderToken { get; set; }
    }

    public class PaymentExtraInfoMercadoPagoResult
    {
        public long? id { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_approved { get; set; }
        public DateTime? date_last_updated { get; set; }
        public DateTime? money_release_date { get; set; }
        public string payment_method_id { get; set; }
        public string payment_type_id { get; set; }
        public string status { get; set; }
        public string status_detail { get; set; }
        public string currency_id { get; set; }
        public string description { get; set; }
        public long? collector_id { get; set; }
        public Payer? payer { get; set; }
        public PaymentMethod? payment_method { get; set; }
        public Metadata? metadata { get; set; }
        public AdditionalInfo? additional_info { get; set; }
        public Barcode? barcode { get; set; }
        public long? transaction_amount { get; set; }
        public long? transaction_amount_refunded { get; set; }
        public long? coupon_amount { get; set; }
        public TransactionDetails transaction_details { get; set; }
        public long? installments { get; set; }
        public Card card { get; set; }
    }

    public class PaymentMethod
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class AdditionalInfo
    {

    }

    public class Card
    {
    }
    public class Metadata
    {
    }

    public class Identification
    {
        public string type { get; set; }
        public long? number { get; set; }
    }

    public class Payer
    {
        public long? id { get; set; }
        public string email { get; set; }
        public Identification? identification { get; set; }
        public string type { get; set; }
    }

    public class TransactionDetails
    {
        public long? net_received_amount { get; set; }
        public long? total_paid_amount { get; set; }
        public long? overpaid_amount { get; set; }
        public long? installment_amount { get; set; }
    }

    public class Barcode
    {
        public string content { get; set; }
    }
}
