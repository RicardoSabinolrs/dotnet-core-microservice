using System.Threading.Tasks;
using SabinoLabs.Domain;
using SabinoLabs.Domain.Entities;
using SabinoLabs.Domain.Repositories.Interfaces;

namespace SabinoLabs.Infrastructure.Data.Repositories
{
    public class BeerRepository : GenericRepository<Beer>, IBeerRepository
    {
        public BeerRepository(IUnitOfWork context) : base(context)
        {
        }

        public override async Task<Beer> CreateOrUpdateAsync(Beer beer)
        {
            bool exists = await Exists(x => x.Id == beer.Id);

            if (beer.Id != 0 && exists)
            {
                Update(beer);
            }
            else
            {
                _context.AddOrUpdateGraph(beer);
            }

            return beer;
        }
    }
}
