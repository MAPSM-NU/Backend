using CsvHelper.Configuration;

namespace Gym_App.Domain.DTOs
{
    public class ExerciseCSVDTOMap : ClassMap<ExerciseCSVDTO>
    {
        public ExerciseCSVDTOMap()
        {
            Map(m => m.Name).Name("exercise_name");
            Map(m => m.Description).Name("details");
            Map(m => m.Difficulty).Name("Difficulty");
            Map(m => m.VideoUrl).Name("videoURL");
            Map(m => m.Category).Name("Category");
            Map(m => m.Grip).Name("Grips");
            Map(m => m.MuscleGroup).Name("target");
        }
    }
}