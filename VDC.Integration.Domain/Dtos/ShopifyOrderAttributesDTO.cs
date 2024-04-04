using System.Collections.Generic;

namespace VDC.Integration.Domain.Dtos
{
    public class ShopifyOrderAttributesDTO
    {
        public long tenantId { get; set; }
        public long id { get; set; }
        public List<ShopifyNoteAttributesDTO> note_attributes { get; set; }

        public ShopifyOrderAttributesDTO()
        {
            note_attributes = new List<ShopifyNoteAttributesDTO>();
        }
    }

    public class ShopifyNoteAttributesDTO
    {
        public string name { get; set; }
        public string value { get; set; }

        public ShopifyNoteAttributesDTO(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
