using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using SabinoLabs.Domain.Entities;

namespace SabinoLabs.Domain.Services.Interfaces
{
    public interface IBeerService
    {
        Task<Beer> Save(Beer beer);

        Task<IPage<Beer>> FindAll(IPageable pageable);

        Task<Beer> FindOne(long id);

        Task Delete(long id);
    }
}
