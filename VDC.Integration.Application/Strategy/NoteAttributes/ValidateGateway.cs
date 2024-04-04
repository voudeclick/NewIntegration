using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Domain.Dtos;
using static VDC.Integration.Domain.Shopify.Models.Results.REST.OrderResult;

namespace VDC.Integration.Application.Strategy.NoteAttributes
{
    public class ValidateGateway
    {
        readonly List<IValidationGateway> _gateway;

        public ValidateGateway(List<IValidationGateway> gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<NoteAttribute>> ReturnNoteAttributes(string gatewayName, string checkoutId)
        {
            var gatewaysValid = _gateway.Where(x => x.IsGateway(gatewayName));

            List<NoteAttributeDto> orderNoteAttributes = null;
            foreach (var gateway in gatewaysValid)
            {
                orderNoteAttributes = await gateway.GetNoteAttributes(checkoutId);
            }

            return orderNoteAttributes?.Select(s => new NoteAttribute
            {
                name = s.name,
                value = s.value,
            })?.ToList();
        }
    }
}
