using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using JHipsterNet.Core.Pagination.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SabinoLabs.Domain.Repositories.Interfaces;

namespace SabinoLabs.Infrastructure.Data.Repositories
{
    public class FluentRepository<TEntity> : IFluentRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly IGenericRepository<TEntity> _repository;
        private bool _disableTracking;
        private Expression<Func<TEntity, bool>> _filter;
        private readonly List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> _include;
        private readonly List<Expression<Func<TEntity, object>>> _includeProperties;
        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderBy;

        public FluentRepository(IGenericRepository<TEntity> repository, DbSet<TEntity> dbset)
        {
            _repository = repository;
            _dbSet = dbset;
            _include = new List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>>();
            _includeProperties = new List<Expression<Func<TEntity, object>>>();
        }

        public IFluentRepository<TEntity> Filter(Expression<Func<TEntity, bool>> filter)
        {
            _filter = filter;
            return this;
        }

        public IFluentRepository<TEntity> OrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        public IFluentRepository<TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            _includeProperties.Add(expression);
            return this;
        }

        public IFluentRepository<TEntity> Include(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include)
        {
            _include.Add(include);
            return this;
        }

        public IFluentRepository<TEntity> AsNoTracking()
        {
            _disableTracking = true;
            return this;
        }

        public async Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> filter)
        {
            _filter = filter;
            IQueryable<TEntity> query = BuildQuery();
            return await query.SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = BuildQuery();
            return await query.ToListAsync();
        }

        public async Task<IPage<TEntity>> GetPageAsync(IPageable pageable)
        {
            IQueryable<TEntity> query = BuildQuery();
            return await query.UsePageableAsync(pageable);
        }

        private IQueryable<TEntity> BuildQuery()
        {
            IQueryable<TEntity> query = _dbSet;

            if (_disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (_includeProperties != null)
            {
                _includeProperties.ForEach(i => { query = query.Include(i); });
            }

            if (_include != null)
            {
                _include.ForEach(i => { query = i(query); });
            }

            if (_filter != null)
            {
                query = query.Where(_filter);
            }

            if (_orderBy != null)
            {
                query = _orderBy(query);
            }

            return query;
        }
    }
}
