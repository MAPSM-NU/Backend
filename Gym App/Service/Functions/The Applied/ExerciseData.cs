using CsvHelper;
using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using System.Globalization;

namespace Gym_App.Service.Functions.The_Applied
{
    public class ExerciseData : IExerciseData // This class is used for the adding and merging of the alr made dataset we have. Not applicable to the runtime of the app
                                            //connected to the controller AddCsvToDatabase
                                            //Pls do make sure you don't run this when the data is already in the database (plan to add checks later)
    {
        private readonly DbBase _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ExerciseData(DbBase db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public bool AddingExerciseAndMuscles()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Domain", "Entities", "workout-data.csv");

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            csv.Context.RegisterClassMap<ExerciseCSVDTOMap>();

            var records = csv.GetRecords<ExerciseCSVDTO>().ToList();
            var Exercises = records.Select(r => new
            {
                Name = r.Name,
                Description = r.Description,
                Difficulty = r.Difficulty,
                VideoUrl = r.VideoUrl,
                Category = r.Category,
                Grip = r.Grip
            }).ToList();
            var Muscles = records.Select(r => new
            {
                MuscleGroup = r.MuscleGroup,
            }).ToList();
            List<string> muscles = new List<string>();
            foreach (var record in Muscles)
            {
                var newRecord = record.MuscleGroup.Trim();
                newRecord = newRecord.Replace("[", "").Replace("]", "").Replace("'", "").Replace("Primary: ", "").Replace("Secondary: ", "").Replace("Tertiary: ", "")
                    .Replace("{", "").Replace("}", "").Replace(" ", "");
                var muscleNames = newRecord.Split(',').ToList();
                foreach (string muscleName in muscleNames)
                {
                    bool add = true;
                    foreach (string m in muscles)
                    {
                        if (m == muscleName)
                        {
                            add = false;
                            break;
                        }
                    }

                    if (add)
                    {
                        muscles.Add(muscleName);
                        var muscle = new Domain.Entities.Muscles
                        {
                            MusclesID = Guid.NewGuid(),
                            Name = muscleName
                        };
                        _db.Muscles.Add(muscle);
                    }
                }
            }
            _db.Exercises.AddRange(Exercises.Select(e => new Domain.Entities.Exercise
            {
                ExerciseID = Guid.NewGuid(),
                Name = e.Name,
                Description = e.Description,
                Difficulty = e.Difficulty,
                VideoUrl = e.VideoUrl,
                Category = e.Category,
                Grip = e.Grip
            }));
            _db.SaveChanges();
            return true;
        }

        public bool LinkingMusclesAndExercises()
        {
            var exercises = _db.Exercises.ToList();
            var muscles = _db.Muscles.ToList();
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, "Domain", "Entities", "workout-data.csv");
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            csv.Context.RegisterClassMap<ExerciseCSVDTOMap>();
            var records = csv.GetRecords<ExerciseCSVDTO>().ToList();
            var MusclesData = records.Select(r => new
            {
                Name = r.Name,
                MuscleGroup = r.MuscleGroup,
            }).ToList();
            foreach (var exercise in records)
            {
                var currentExercise = exercises.FirstOrDefault(e => e.Name == exercise.Name);
                foreach (var muscleName in exercise.MuscleGroup.Replace("[", "").Replace("]", "").Replace("'", "").Replace("Primary: ", "").Replace("Secondary: ", "").Replace("Tertiary: ", "")
                    .Replace("{", "").Replace("}", "").Replace(" ", "").Split(',').ToList())
                {
                    var currentMuscle = muscles.FirstOrDefault(m => m.Name == muscleName);
                    if (currentExercise is not null && currentMuscle is not null)
                    {
                        currentExercise.Muscles?.Add(currentMuscle);
                    }
                }
            }
            _db.SaveChanges();
            return true;
        }
    }
}
