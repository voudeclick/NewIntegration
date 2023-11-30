using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Bling.Models.Results
{
    public class BlingUpdateStatusOrderResult
    {
        public Retorno retorno { get; set; }

        public class Retorno
        {

            public List<Item> pedidos { get; set; }
        }

        public class Item
        {

            public Pedido pedido { get; set; }

            public class Pedido
            {
                public string numero { get; set; }
                public string mensagem { get; set; }
             
            }
        }
    }
    //    {
    //    "retorno": {
    //        "pedidos":[
    //            {
    //                "pedido": {
    //                    "numero": "289258",
    //                    "mensagem": "No Bling, o status foi atualizado para atendido."
    //                }
    //            }
    //        ]
    //    }
    //}
}
