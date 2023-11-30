using Samurai.Integration.Application.Strategy.Interfaces;
using Samurai.Integration.Domain.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Samurai.Integration.Domain.Shopify.Models.Results.REST.OrderResult;

namespace Samurai.Integration.Application.Strategy.NoteAttributes
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
