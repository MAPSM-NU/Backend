using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IFitnessGoalsRepositry : IBaseRepositry<FitnessGoals>
    {
        public Task<FitnessGoals> GetFitnessGoalUsingName(string name);
    }
    public interface IMedicalConditionsRepositry : IBaseRepositry<MedicalCondition>
    {
        public Task<MedicalCondition> GetMedicalConditionUsingName(string name);
    }
    public interface IInjuryRepositry : IBaseRepositry<Injury>
    {
        public Task<Injury> GetInjuryUsingName(string name);
    }
    public interface IExerciseRestrictionsRepositry : IBaseRepositry<ExerciseRestrictions>
    {
        public Task<ExerciseRestrictions> GetExerciseRestrictionUsingName(string name);
    }
}
