using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiUpdateOrderStatusRequest
    {
        public long vitrine { get; set; }
        public List<Status_Pedidos> status_pedidos { get; set; }
        public bool somente_inclui_pedido { get { return false; } }

        public class Status_Pedidos
        {
            public string cod_pedidov { get; set; }
            public int status { get; set; }
        }
    }
}
