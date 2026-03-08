using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Infastructure.Repositries
{
    public class UserRepositry : BaseRepositry<User>, IUserRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<User> table;
        public UserRepositry(DbBase db) : base(db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            table = _db.Set<User>();
        }

        public async Task<User> GetUserById(Guid userID)
        {
            return await table.FirstOrDefaultAsync(u=>u.Id == userID);
        }

        public async Task<bool> isUserEmailExist(string email)
        {
            return await table.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> isUserExist(Guid userID)
        {
            return await table.AnyAsync(u => u.Id == userID);
        }

        public async Task<bool> isUserNameExist(string name)
        {
            return await table.AnyAsync(u => u.Name == name);
        }

        public override IQueryable<User> FilterSortColumn(string columnName, string sortOrder, IQueryable<User> query)
        {
            if (string.IsNullOrEmpty(columnName)) return query;

            Expression<Func<User, object>> keySelector = columnName.ToLower() switch
            {
                "name" or "n" => u => u.Name,
                "email" or "e" => u => u.Email,
                "country" or "co" => u => u.Country,
                "state" or "s" => u => u.State,
                "city" or "ci" => u => u.City,
                "height" or "h" => u => u.HeightCm,
                "weight" or "w" => u => u.WeightKg,
                _ => u => u.Id,
            };

            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending";

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
        public override IQueryable<User> Search(string searchTerm, IQueryable<User> query)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;
            searchTerm = searchTerm.ToLower();
            return query.Where(u => u.Name.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm));
        }

    }
}
