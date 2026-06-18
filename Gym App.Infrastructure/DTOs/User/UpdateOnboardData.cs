using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.User
{
    public class UpdateOnboardData
    {
        public required string requestType {  get; set; }
        public required string type {  get; set; }
        public required string name { get; set; }
    }
    public class UpdateOnboardDataList
    {
        public List<UpdateOnboardData> Data { get; set; } = new List<UpdateOnboardData>();
    }
    public enum OnboardDataType
    {
        FitnessGoals,
        f,
        MeidcalCondition,
        m,
        Injury,
        i,
        ExerciseRestrictions,
        e,
    }
    public enum requestType
    {
        create,
        c,
        remove,
        r
    }
}
