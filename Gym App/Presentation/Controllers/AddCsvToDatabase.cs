using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using Gym_App.Domain;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Globalization;

namespace Gym_App.Api.Controllers
{
    [Route("api/v1/exercise-data")]
    public class AddCsvToDatabase : Controller // This controller is used to add the data from the csv file to the database. It is not applicable to the runtime of the app
                                               //connected to the WorkoutData class
                                               //Pls do make sure you don't run this when the data is already in the database
    {
        private readonly IExerciseData _workoutData;
        public AddCsvToDatabase(IExerciseData workoutData)
        {
            _workoutData = workoutData;
        }
        [HttpGet]
        [Route("add-exercises-data")]
        public IActionResult AddingDataset()
        {
            if(_workoutData.AddingExerciseAndMuscles()) return View("Complete");
            else return View("Error");

        }
        [HttpGet]
        [Route("merging-exercises-and-muscles")]
        public IActionResult MergingExercisesAndMuscles()
        {
            if(_workoutData.LinkingMusclesAndExercises()) return View("Complete");
            else return View("Error");
        }
    }
}