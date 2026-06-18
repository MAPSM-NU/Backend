using Gym_App.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Core
{
    public class FitnessGoals : BaseEntity
    {
        [Column(TypeName = "varchar(30)")]
        public required string Name {  get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
    public class Injury : BaseEntity
    {
        [Column(TypeName = "varchar(30)")]
        public required string Name { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
    public class ExerciseRestrictions : BaseEntity
    {
        [Column(TypeName = "varchar(30)")]
        public required string Name { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
    public class MedicalCondition : BaseEntity
    {
        [Column(TypeName = "varchar(30)")]
        public required string Name { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }

    public enum FitnessGoalType
    {
        WeightLoss,
        MuscleGain,
        StrengthTraining,
        Endurance,
        Flexibility,
        GeneralFitness
    }
    public enum InjuryType
    {
        Knee,
        Back,
        Shoulder,
        Ankle,
        Wrist,
        Hip
    }
    public enum MedicalConditionsType
    {
        Diabetes,
        HighBloodPressure,
        HeartCondition,
        Asthma,
        Arthritis
    }
    public enum ExerciseRestrictionsType
    {
        NoRunning,
        NoHeavyLifting,
        LowImpactOnly,
        UpperBodyOnly,
        LowerBodyOnly
    }
}
