using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Omie.Enum.Webhook
{
    public enum WebhookType
    {
        ProductCreate = 1,
        ProductEdit = 2,
        ProductRemove = 3,
        Stock = 4,
        Order = 5
    }

    public static class WebhookTypeExtension
    {
        public static Dictionary<WebhookType, List<string>> mappings = new Dictionary<WebhookType, List<string>>
        {
            {
                WebhookType.ProductCreate, new List<string>{
                    "produto.incluido"
                }
            },{
                WebhookType.ProductEdit, new List<string>{
                    "produto.alterado"
                }
            },{
                WebhookType.ProductRemove, new List<string>{
                    "produto.excluido"
                }
            },{
                WebhookType.Stock, new List<string>{
                    "produto.movimentacaoestoque"
                }
            },{
                WebhookType.Order, new List<string>{
                    "vendaproduto.alterada",
                    "vendaproduto.cancelada",
                    "vendaproduto.devolvida",
                    "vendaproduto.etapaalterada",
                    "vendaproduto.faturada",
                    "vendaproduto.incluida"
                }
            }
        };

        public static List<string> GetTopics(this WebhookType type)
        {
            return mappings[type];
        }
    }
}
