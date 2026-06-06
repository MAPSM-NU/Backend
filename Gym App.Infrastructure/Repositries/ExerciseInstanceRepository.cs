using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class ExerciseInstanceRepository : BaseRepositry<ExerciseInstance>, IExerciseInstanceRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<ExerciseInstance> table;
        public ExerciseInstanceRepository(DbContext context) : base(context)
        {
            _context = context;
            table = _context.Set<ExerciseInstance>();
        }

        public async Task<IEnumerable<ExerciseInstance>> GetExercisesByWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(e => e.WorkoutId == workoutId)
                .OrderBy(e => e.ExerciseOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<ExerciseInstance?> GetWithSetsAsync(Guid exerciseInstanceId, CancellationToken cancellationToken = default)
        {
            return await table
                .Include(e => e.Sets)
                .Include(e => e.Exercise)
                .FirstOrDefaultAsync(e => e.Id == exerciseInstanceId, cancellationToken);
        }

        public async Task<int> GetCompletedExercisesCountAsync(Guid workoutId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(e => e.WorkoutId == workoutId && e.IsCompleted)
                .CountAsync(cancellationToken);
        }
    }
}
