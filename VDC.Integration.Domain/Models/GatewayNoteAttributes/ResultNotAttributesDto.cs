using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VDC.Integration.Domain.Dtos;

namespace VDC.Integration.Domain.Models.GatewayNoteAttributes
{
    public class ResultNotAttributesDto
    {
        public string CheckoutId { get; set; }
        public string NoteAttributes { get; set; }
        public int Id { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public List<NoteAttributeDto> BuildNoteAttribute()
        {
            if (string.IsNullOrWhiteSpace(NoteAttributes)) return null;

            var dictionaryNotesAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(NoteAttributes);

            return dictionaryNotesAttributes.Select(s => new NoteAttributeDto
            {
                name = s.Key,
                value = s.Value,
            }).ToList();
        }
    }
}
