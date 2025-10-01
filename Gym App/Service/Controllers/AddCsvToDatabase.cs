using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Globalization;

namespace Gym_App.Service.Controllers
{
    public class AddCsvToDatabase : Controller // This controller is used to add the data from the csv file to the database. It is not applicable to the runtime of the app
                                               //connected to the WorkoutData class
                                               //Pls do make sure you don't run this when the data is already in the database
    {
        private readonly IWorkoutData _workoutData;
        public AddCsvToDatabase(IWorkoutData workoutData)
        {
            _workoutData = workoutData;
        }
        [HttpGet]
        [Route("/Adding Workout and Muscles Data")]
        public IActionResult AddingDataset()
        {
            if(_workoutData.AddingExerciseAndMuscles()) return View("Complete");
            else return View("Error");

        }
        [HttpGet]
        [Route("/Merging Exercises and Muscles")]
        public IActionResult MergingExercisesAndMuscles()
        {
            if(_workoutData.LinkingMusclesAndExercises()) return View("Complete");
            else return View("Error");
        }
    }
}