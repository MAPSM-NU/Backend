using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task<User> GetUserById(Guid userID, bool includeRole)
        {
            if (includeRole)
                return await table.Include(u => u.Role)
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Id == userID);
            else
                return await table
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Id == userID);
        }
        public async Task<User> GetUserByEmail(string email, bool includeRole)
        {
            if (includeRole)
                return await table.Include(u => u.Role)
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Email == email);
            else
                return await table
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User> GetUserByName(string name, bool includeRole)
        {
            if (includeRole)
                return await table.Include(u => u.Role)
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Name == name);
            else
                return await table
                    .Include(u => u.UserStats)
                    .Include(u => u.FitnessGoals)
                    .Include(u => u.Injuries)
                    .Include(u => u.MedicalConditions)
                    .Include(u => u.ExerciseRestrictions)
                    .FirstOrDefaultAsync(u => u.Name == name);
        }
        public async Task<ICollection<User>> GetUsersByRole(Guid roleID)
        {
            return await table.Where(u => u.RoleID == roleID).ToListAsync();
        }
        public async Task<IQueryable<User>> GetUsersByRoleAsQueryable(Guid roleID)
        {
            return table.Where(u => u.RoleID == roleID);
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
