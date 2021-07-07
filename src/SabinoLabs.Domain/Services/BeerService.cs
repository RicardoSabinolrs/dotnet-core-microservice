using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using SabinoLabs.Domain.Entities;
using SabinoLabs.Domain.Repositories.Interfaces;
using SabinoLabs.Domain.Services.Interfaces;

namespace SabinoLabs.Domain.Services
{
    public class BeerService : IBeerService
    {
        protected readonly IBeerRepository _beerRepository;

        public BeerService(IBeerRepository beerRepository) => _beerRepository = beerRepository;

        public virtual async Task<Beer> Save(Beer beer)
        {
            await _beerRepository.CreateOrUpdateAsync(beer);
            await _beerRepository.SaveChangesAsync();
            return beer;
        }

        public virtual async Task<IPage<Beer>> FindAll(IPageable pageable)
        {
            IPage<Beer> page = await _beerRepository.QueryHelper()
                .GetPageAsync(pageable);
            return page;
        }

        public virtual async Task<Beer> FindOne(long id)
        {
            Beer result = await _beerRepository.QueryHelper()
                .GetOneAsync(beer => beer.Id == id);
            return result;
        }

        public virtual async Task Delete(long id)
        {
            await _beerRepository.DeleteByIdAsync(id);
            await _beerRepository.SaveChangesAsync();
        }
    }
}
