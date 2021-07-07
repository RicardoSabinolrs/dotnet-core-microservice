using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using JHipsterNet.Core.Pagination.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SabinoLabs.Domain.Repositories.Interfaces;

namespace SabinoLabs.Infrastructure.Data.Repositories
{
    public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity>, IDisposable where TEntity : class
    {
        protected internal readonly IUnitOfWork _context;
        protected internal readonly DbSet<TEntity> _dbSet;

        public GenericRepository(IUnitOfWork context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public void Dispose() => _context?.Dispose();

        public virtual async Task<TEntity> GetOneAsync(object id) => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<IPage<TEntity>> GetPageAsync(IPageable pageable) =>
            await _dbSet.UsePageableAsync(pageable);

        public virtual async Task<bool> Exists(Expression<Func<TEntity, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public virtual TEntity Add(TEntity entity)
        {
            _dbSet.Add(entity);
            return entity;
        }

        public virtual bool AddRange(params TEntity[] entities)
        {
            _dbSet.AddRange(entities);
            return true;
        }

        public virtual TEntity Attach(TEntity entity)
        {
            EntityEntry<TEntity> entry = _dbSet.Attach(entity);
            entry.State = EntityState.Added;
            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            _dbSet.Update(entity);
            return entity;
        }

        public virtual bool UpdateRange(params TEntity[] entities)
        {
            _dbSet.UpdateRange(entities);
            return true;
        }

        public abstract Task<TEntity> CreateOrUpdateAsync(TEntity entity);

        public virtual async Task Clear()
        {
            List<TEntity> allEntities = await _dbSet.ToListAsync();
            _dbSet.RemoveRange(allEntities);
        }

        public virtual async Task DeleteByIdAsync(object id)
        {
            TEntity entity = await GetOneAsync(id);
            _dbSet.Remove(entity);
        }

        public virtual async Task DeleteAsync(TEntity entity) => await Task.FromResult(_dbSet.Remove(entity));

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);

        public virtual async Task<int> CountAsync()
        {
            int countTask = await _dbSet.CountAsync();
            return countTask;
        }

        public virtual IFluentRepository<TEntity> QueryHelper()
        {
            FluentRepository<TEntity> fluentRepository = new FluentRepository<TEntity>(this, _dbSet);
            return fluentRepository;
        }

        protected async Task RemoveManyToManyRelationship(string joinEntityName, string ownerIdKey, string ownedIdKey,
            long ownerEntityId, List<long> idsToIgnore)
        {
            DbSet<Dictionary<string, object>> dbset = _context.Set<Dictionary<string, object>>(joinEntityName);

            List<Dictionary<string, object>> manyToManyData = await dbset
                .Where(joinPropertyBag => joinPropertyBag[ownerIdKey].Equals(ownerEntityId))
                .ToListAsync();

            List<Dictionary<string, object>> filteredManyToManyData = manyToManyData
                .Where(joinPropertyBag =>
                    !idsToIgnore.Any(idToIgnore => joinPropertyBag[ownedIdKey].Equals(idToIgnore)))
                .ToList();

            dbset.RemoveRange(filteredManyToManyData);
        }
    }
}
