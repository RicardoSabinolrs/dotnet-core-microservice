using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SabinoLabs.Domain;
using SabinoLabs.Domain.Entities;
using SabinoLabs.Domain.Entities.Interfaces;

namespace SabinoLabs.Infrastructure.Data
{
    public class ApplicationDatabaseContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options) => _httpContextAccessor = httpContextAccessor;

        public DbSet<Beer> Beers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) => base.OnModelCreating(builder);

        /// <summary>
        ///     SaveChangesAsync with entities audit
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<EntityEntry> entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditedEntityBase && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            string modifiedOrCreatedBy = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

            foreach (EntityEntry entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((IAuditedEntityBase)entityEntry.Entity).CreatedDate = DateTime.Now;
                    ((IAuditedEntityBase)entityEntry.Entity).CreatedBy = modifiedOrCreatedBy;
                }
                else
                {
                    Entry((IAuditedEntityBase)entityEntry.Entity).Property(p => p.CreatedDate).IsModified = false;
                    Entry((IAuditedEntityBase)entityEntry.Entity).Property(p => p.CreatedBy).IsModified = false;
                }

                ((IAuditedEntityBase)entityEntry.Entity).LastModifiedDate = DateTime.Now;
                ((IAuditedEntityBase)entityEntry.Entity).LastModifiedBy = modifiedOrCreatedBy;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
