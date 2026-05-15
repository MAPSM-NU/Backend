using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<ExerciseInstance>> GetExercisesByWorkoutAsync(Guid workoutId)
        {
            return await table
                .Where(e => e.WorkoutId == workoutId)
                .OrderBy(e => e.ExerciseOrder)
                .ToListAsync();
        }

        public async Task<ExerciseInstance?> GetWithSetsAsync(Guid exerciseInstanceId)
        {
            return await table
                .Include(e => e.Sets)
                .Include(e => e.Exercise)
                .FirstOrDefaultAsync(e => e.Id == exerciseInstanceId);
        }

        public async Task<int> GetCompletedExercisesCountAsync(Guid workoutId)
        {
            return await table
                .Where(e => e.WorkoutId == workoutId && e.IsCompleted)
                .CountAsync();
        }
    }
}
