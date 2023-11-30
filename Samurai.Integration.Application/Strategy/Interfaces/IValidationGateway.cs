using Samurai.Integration.Domain.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Strategy.Interfaces
{
    public interface IValidationGateway
    {
        bool IsGateway(string tenant);
        Task<List<NoteAttributeDto>> GetNoteAttributes(string checkoutId);
    }
}
