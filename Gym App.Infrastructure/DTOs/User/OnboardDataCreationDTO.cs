using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.User
{
    public class OnboardDataCreationDTO
    {
        public required Guid userId {  get; set; }
        public List<string> FitnessGoals {  get; set; } = new List<string>();
        public List<string> Injuries {  get; set; } = new List<string>();
        public List<string> ExerciseRestrictions {  get; set; } = new List<string>();
        public List<string> MedicalConditions { get; set; } = new List<string>();
    }
}
