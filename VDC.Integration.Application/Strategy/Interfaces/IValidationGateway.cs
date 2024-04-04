using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Domain.Dtos;

namespace VDC.Integration.Application.Strategy.Interfaces
{
    public interface IValidationGateway
    {
        bool IsGateway(string tenant);
        Task<List<NoteAttributeDto>> GetNoteAttributes(string checkoutId);
    }
}
