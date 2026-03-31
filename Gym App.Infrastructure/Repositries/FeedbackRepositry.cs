using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Infastructure.Repositries
{
    public class FeedbackRepositry : BaseRepositry<Feedback>, IFeedbackRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Feedback> table;
        public FeedbackRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Feedback>();
        }

        public async Task<Feedback> GetFeedbackwithUser(Guid feedbackId)
        {
            return await table.Include(f => f.Workout).FirstOrDefaultAsync(f => f.Id == feedbackId);
        }

        public async Task<Feedback> GetFeedbackwithWorkout(Guid feedbackId)
        {
            return await table.Include(f => f.User).FirstOrDefaultAsync(f => f.Id == feedbackId);
        }
        public IQueryable<Feedback> GetFeedbacks()
        {
            return table.Include(f => f.User).Include(f => f.Workout).AsQueryable();
        }
        public async Task<IQueryable<Feedback>> GetFeedbacksOfUser(Guid userId)
        {
            return table.Include(f => f.User)
                  .Include(f => f.Workout)
                  .Where(f => f.User.Id == userId)
                  .AsQueryable();
        }
        public override IQueryable<Feedback> FilterSortColumn(string columnName, string sortOrder, IQueryable<Feedback> query)
        {
            Expression<Func<Feedback, object>> keySelector = columnName.ToLower() switch // throws error when sortColumn is null
            {
                "title" => Feedback => Feedback.Title, //order by Title
                "calories" => Feedback => Feedback.CaloriesBurned!, // order by CaloriesBurned
                "duration" => Feedback => Feedback.DurationMinutes!, // order by DurationMinutes
                _ => Feedback => Feedback.Id//failsafe: order by FeedbackID
            };
            //If no orderby was inputed, then we sort ascending
            if (!string.IsNullOrEmpty(sortOrder)) query = query.OrderBy(keySelector);//If any kind of value is in OrderBy then it is ascending

            //else if anything was inputted we sort descending
            else query = query.OrderByDescending(keySelector);

            return query;
        }
        public override IQueryable<Feedback> Search(string searchTerm, IQueryable<Feedback> query)
        {
            return query.Where(f => f.Title.Contains(searchTerm));
        }
    }
}
