using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class UserRepositry : BaseRepositry<User>, IUserRepositry
    {
        private readonly DbContext _db;
        private readonly DbSet<User> table;
        public UserRepositry(DbContext db) : base(db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            table = _db.Set<User>();
        }

        public async Task<User> GetUserById(Guid userID, bool includeRole, CancellationToken cancellationToken = default)
        {
            if (includeRole)
                return await table.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userID, cancellationToken);
            else
                return await table.FirstOrDefaultAsync(u => u.Id == userID, cancellationToken);
        }
        public async Task<User> GetUserByEmail(string email, bool includeRole, CancellationToken cancellationToken = default)
        {
            if (includeRole)
                return await table.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            else
                return await table.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
        public async Task<User> GetUserByName(string name, bool includeRole, CancellationToken cancellationToken = default)
        {
            if (includeRole)
                return await table.Include(u => u.Role).FirstOrDefaultAsync(u => u.Name == name, cancellationToken);
            else
                return await table.FirstOrDefaultAsync(u => u.Name == name, cancellationToken);
        }
        public async Task<ICollection<User>> GetUsersByRole(Guid roleID, CancellationToken cancellationToken = default)
        {
            return await table.Where(u => u.RoleID == roleID).ToListAsync(cancellationToken);
        }
        public async Task<IQueryable<User>> GetUsersByRoleAsQueryable(Guid roleID, CancellationToken cancellationToken = default)
        {
            return table.Where(u => u.RoleID == roleID);
        }

        public async Task<bool> isUserEmailExist(string email, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> isUserExist(Guid userID, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(u => u.Id == userID, cancellationToken);
        }

        public async Task<bool> isUserNameExist(string name, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(u => u.Name == name, cancellationToken);
        }

        public override IQueryable<User> FilterSortColumn(string columnName, string sortOrder, IQueryable<User> query)
        {
            if (string.IsNullOrEmpty(columnName)) return query;

            Expression<Func<User, object>> keySelector = columnName.ToLower() switch
            {
                "name" or "n" => u => u.Name,
                "email" or "e" => u => u.Email,
                "country" or "co" => u => u.Country!,
                "state" or "s" => u => u.State!,
                "city" or "ci" => u => u.City!,
                "height" or "h" => u => u.HeightCm!,
                "weight" or "w" => u => u.WeightKg!,
                "createdat" or "ca" => u => u.CreatedAt,
                _ => u => u.Id,
            };

            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
        public override IQueryable<User> Search(string searchTerm, IQueryable<User> query)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;
            searchTerm = searchTerm.ToLower();
            return query.Where(u => u.Name.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm) || u.City.ToLower().Contains(searchTerm) || u.State.ToLower().Contains(searchTerm) || u.Country.ToLower().Contains(searchTerm));
        }
    }
}
