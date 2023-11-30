using Newtonsoft.Json;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.APIClient.SellerCenter.Models.Requests.Inputs
{
    public class AvailableValues
    {
        public AvailableValues()
        {
            Translations = new List<TranslationDto>();
        }
        public Guid? Id { get; set; }

        /// <summary>
        /// Valor |  Ex: 43, azul
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Url da Imagem
        /// </summary>
        public string ImageUrl { get; set; }

        public List<TranslationDto> Translations { get; set; }

    }
}
