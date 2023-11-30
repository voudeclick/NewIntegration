using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Samurai.Integration.APIClient.Converters;

namespace Samurai.Integration.APIClient.Bling.Models.Results
{
    public class BlingGetAllSituationResult
    {
        public Retorno retorno { get; set; }

        public class Retorno {

            public List<Item> situacoes { get; set; }
        }

        public class Item {

            public Situacao situacao { get; set; }

            public class Situacao
            {
                [JsonConverter(typeof(StringToIntConverter))]
                public int id { get; set; }

                [JsonConverter(typeof(StringToIntConverter))]
                public int idHerdado { get; set; }
                public string nome { get; set; }
                public string cor { get; set; }
            }
        }

        public Item.Situacao GetSituationByName(string name)
        {
           
            var status =  retorno.situacoes.Where(x => string.Equals(x.situacao.nome, name, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault()?.situacao ?? new Item.Situacao();

            if (status.idHerdado != 0)
                status = GetSituationById(status.idHerdado);

            return status;
        }
        public Item.Situacao GetSituationById(int idStatus)
            => retorno.situacoes.Where(x => x.situacao.id== idStatus)
                                .FirstOrDefault()?.situacao ?? new Item.Situacao();


    }


}
