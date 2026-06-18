using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infrastructure.Repositries
{
    public class FitnessGoalsRepositry : BaseRepositry<FitnessGoals>, IFitnessGoalsRepositry
    {
        private readonly DbSet<FitnessGoals> table;
        public FitnessGoalsRepositry(DbBase context) : base(context)
        {
            this.table = context.Set<FitnessGoals>();
        }

        public async Task<FitnessGoals> GetFitnessGoalUsingName(string name)
        {
            return await table.FirstOrDefaultAsync(fg => fg.Name.ToLower() == name.ToLower());
        }
    }
    public class InjuryRepositry : BaseRepositry<Injury>, IInjuryRepositry
    {
        private readonly DbSet<Injury> table;
        public InjuryRepositry(DbBase context) : base(context)
        {
            this.table = context.Set<Injury>();
        }
        public async Task<Injury> GetInjuryUsingName(string name)
        {
            return await table.FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
        }
    }
    public class ExerciseRestrictionsRepositry : BaseRepositry<ExerciseRestrictions>, IExerciseRestrictionsRepositry
    {
        private readonly DbSet<ExerciseRestrictions> table;
        public ExerciseRestrictionsRepositry(DbBase context) : base(context)
        {
            this.table = context.Set<ExerciseRestrictions>();
        }
        public async Task<ExerciseRestrictions> GetExerciseRestrictionUsingName(string name)
        {
            return await table.FirstOrDefaultAsync(er => er.Name.ToLower() == name.ToLower());
        }
    }
    public class MedicalConditionsRepositry : BaseRepositry<MedicalCondition>, IMedicalConditionsRepositry
    {
        private readonly DbSet<MedicalCondition> table;
        public MedicalConditionsRepositry(DbBase context) : base(context)
        {
            this.table = context.Set<MedicalCondition>();
        }
        public async Task<MedicalCondition> GetMedicalConditionUsingName(string name)
        {
            return await table.FirstOrDefaultAsync(mc => mc.Name.ToLower() == name.ToLower());
        }
    }
}
