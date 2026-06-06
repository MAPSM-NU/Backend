using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class WorkoutSetRepository : BaseRepositry<WorkoutSet>, IWorkoutSetRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<WorkoutSet> table;

        public WorkoutSetRepository(DbContext context) : base(context)
        {
            _context = context;
            table = _context.Set<WorkoutSet>();
        }

        public async Task<IEnumerable<WorkoutSet>> GetSetsByExerciseInstanceAsync(Guid exerciseInstanceId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(s => s.ExerciseInstanceId == exerciseInstanceId)
                .OrderBy(s => s.SetNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCompletedSetsCountAsync(Guid exerciseInstanceId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(s => s.ExerciseInstanceId == exerciseInstanceId && s.IsCompleted)
                .CountAsync(cancellationToken);
        }
    }
}
