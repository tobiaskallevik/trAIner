using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using OpenAI_API.Models;
using trainer.Data;
using trainer.Models;
using trainer.ViewModels;
using Pluralize.NET;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class GenerationController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;
    private readonly OpenAIAPI _openAiApi;

 
    public GenerationController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, OpenAIAPI openAiApi)
    {
        _db = db;
        _um = um;
        _rm = rm;
        _openAiApi = openAiApi;
    }
    
    public IActionResult Workout()
    {
        return View();
    }
    
    
    // Gets the generated workout from TempData and displays it
    public async Task<IActionResult> GeneratedWorkout()
    {
        // Gets the list of view models from TempData
        List<GeneratedWorkoutViewModel> generatedWorkoutViewModels = new List<GeneratedWorkoutViewModel>();

        var json = ""; 
        if (TempData["GeneratedWorkouts"] != null)
        {
            json = TempData["GeneratedWorkouts"].ToString();
            generatedWorkoutViewModels = JsonConvert.DeserializeObject<List<GeneratedWorkoutViewModel>>(json);
        }
        
        // Creates a new TempData object and stores the list of view models in it
        json = JsonConvert.SerializeObject(generatedWorkoutViewModels);
        TempData["MoreTempData"] = json;
        
        return View(generatedWorkoutViewModels);
    }
    

    // Generates a workout based on the user's input using the OpenAI API
    // Some of the code here might seem excessive (13 total if / else tests), but it's necessary to ensure that the workout is generated correctly. AI still big dumb dumb sometimes and db big dumb dumb too
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WorkoutGenerator(string generatorType, bool generatorGym, string generatorSkill)
    {

        string userInput;
        List<GeneratedWorkoutViewModel> generatedWorkoutViewModelsTemp = new List<GeneratedWorkoutViewModel>();
        
        // Creates a string based on the user's input
        if (generatorGym == true)
        {
            userInput = "Create a " + generatorType + " workout for a client who wants to train at a gym. They are a " + generatorSkill + " level athlete and wants 10 different exercises.";
        }
        else
        {
            userInput = "Create a " + generatorType + " workout for a client who wants to train at home. The client is a " + generatorSkill + " level athlete and wants 10 different exercises.";
        }
            

        // Sets parameters for the chatbot
        var chat = _openAiApi.Chat.CreateConversation();
        
        chat.AppendSystemMessage("Your role is as a workout generator. You are told to create a workout for a client. You are given the following information: workout type, client's skill level, " +
                                 "and whether the client wants to train at home or at a gym. Your provide an appropriate workout for the client based on the information given. The workout should be " +
                                 "formatted as JSON and only include the following attributes: exercise-name, sets. The root element should be exercises.");
        
        // Appends the user's input to the chatbot
        chat.AppendUserInput(userInput);
        
        // Gets the response from the chatbot
        string response = await chat.GetResponseFromChatbotAsync();

        // Tries to convert the response to a JSON object
        try
        {
            // Parse the response into a JSON object
            JsonDocument doc = JsonDocument.Parse(response); 
            JsonElement root = doc.RootElement;
            Console.WriteLine(response);
            // Tries to get the exercises and sets from the JSON object
        
            if (root.TryGetProperty("exercises", out JsonElement exercisesElement)) 
            { 
                foreach (JsonElement exerciseElement in exercisesElement.EnumerateArray()) 
                { 
                    if (exerciseElement.TryGetProperty("exercise-name", out JsonElement exerciseNameElement) && 
                        exerciseElement.TryGetProperty("sets", out JsonElement setsElement)) 
                    { 
                        // Gets the exercise name and sets from the JSON object
                        string exerciseName = exerciseNameElement.GetString();
                        int sets = setsElement.GetInt32();
                    
                        // Adds the exercise and sets to the list of view models
                        generatedWorkoutViewModelsTemp.Add(new GeneratedWorkoutViewModel
                        { 
                            ExerciseName = exerciseName,
                            Sets = sets
                        });
                
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Returns the view with an error message
            TempData["ErrorMessage"] = "An error occurred while generating the workout. Please try again.";
            return Redirect(nameof(Workout));
        }
 
        // Creates a new list of view models and a pluralizer. The pluralizer is used to singularize the exercise name
        List<GeneratedWorkoutViewModel> generatedWorkoutViewModels = new List<GeneratedWorkoutViewModel>();
        IPluralize pluralizer = new Pluralizer();
        
        
        // Goes through the list of view models and tries to find the exercise in the db
        foreach (var generatedExercise in generatedWorkoutViewModelsTemp)
        {
            // Replaces "-" with " " in the exercise name, sets the exercise name to lowercase and removes makes it single 
            generatedExercise.ExerciseName = generatedExercise.ExerciseName.Replace("-", " ");
            generatedExercise.ExerciseName = generatedExercise.ExerciseName.ToLower();
            string alteredExerciseName;
            
            // Singularizes / pluralize the exercise name
            if (pluralizer.IsSingular(generatedExercise.ExerciseName))
            {
                alteredExerciseName = pluralizer.Pluralize(generatedExercise.ExerciseName);
            }
            else
            {
                alteredExerciseName = pluralizer.Singularize(generatedExercise.ExerciseName);
            }
            
            // If the exercise name exists as in the db, than we get the exercise id and gif url
            if (await _db.Exercises.AnyAsync(e => e.Name.ToLower() == generatedExercise.ExerciseName && e.RequiresGym == generatorGym))
            {
                var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Name.ToLower() == generatedExercise.ExerciseName && e.RequiresGym == generatorGym);
                generatedExercise.ExerciseName = exercise.Name;
                generatedExercise.Id = exercise.Id;
                generatedExercise.GifUrl = exercise.GifUrl;
                generatedWorkoutViewModels.Add(generatedExercise);
            }
            
            // Tries to find based on alt form name
            else if (await _db.Exercises.AnyAsync(e => e.Name.ToLower() == alteredExerciseName && e.RequiresGym == generatorGym))
            {
                var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Name.ToLower() == alteredExerciseName && e.RequiresGym == generatorGym);
                generatedExercise.ExerciseName = exercise.Name;
                generatedExercise.Id = exercise.Id;
                generatedExercise.GifUrl = exercise.GifUrl;
                generatedWorkoutViewModels.Add(generatedExercise);
            }
            
            // Tries to find exercise that contains the name
            else if (await _db.Exercises.AnyAsync(e => e.Name.ToLower().Contains(generatedExercise.ExerciseName) && e.RequiresGym == generatorGym))
            {
                var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Name.ToLower().Contains(generatedExercise.ExerciseName) && e.RequiresGym == generatorGym);
                generatedExercise.ExerciseName = exercise.Name;
                generatedExercise.Id = exercise.Id;
                generatedExercise.GifUrl = exercise.GifUrl;
                generatedWorkoutViewModels.Add(generatedExercise);
            }
            
            // Tries to find exercise that contains the alt name
            else if (await _db.Exercises.AnyAsync(e => e.Name.ToLower().Contains(alteredExerciseName) && e.RequiresGym == generatorGym))
            {
                var exercise = await _db.Exercises.FirstOrDefaultAsync(e => e.Name.ToLower().Contains(alteredExerciseName) && e.RequiresGym == generatorGym);
                generatedExercise.ExerciseName = exercise.Name;
                generatedExercise.Id = exercise.Id;
                generatedExercise.GifUrl = exercise.GifUrl;
                generatedWorkoutViewModels.Add(generatedExercise);
            }
            
            // Breaks if the list has 7 elements
            if (generatedWorkoutViewModels.Count >= 7)
            {
                break;
            }
        }
        
        // If the list has at least 3 elements than we send it to the GET method 
        if (generatedWorkoutViewModels.Count >= 3)
        {
            // Serializes the list of view models and stores it in session
            var json = JsonConvert.SerializeObject(generatedWorkoutViewModels);
            TempData["GeneratedWorkouts"] = json;
            return RedirectToAction(nameof(GeneratedWorkout));
        }
        else
        {
            // Returns the view with an error message
            TempData["ErrorMessage"] = "An error occurred while generating the workout. Please try again.";
            return Redirect(nameof(Workout));
        }
    }
    
    
    // Adds the generated workout to the db
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGeneratedWorkout([FromForm] Microsoft.AspNetCore.Http.IFormCollection formData)
    {
        // Gets the user
        var user = await _um.GetUserAsync(User);
        var name = formData["Name"];

        // Creates a new workout
        Workout workout = new Workout
        {
            Name = name,
            UserId = user.Id,
            ApplicationUser = user
        };

        // Adds the workout to the db
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();

        // Gets the list of view models from TempData

        
        var i = 1;
        var id = 0;
        var sets = 0;
        // Goes through the list of view models and adds them to the db
        foreach (var key in formData.Keys)
        {
            if (key.StartsWith("Exercise["))
            {
                var idString = key.Substring("Exercise[".Length, key.IndexOf(']') - "Exercise[".Length);
                id = int.Parse(idString);
                if (key.EndsWith("].Sets"))
                {
                    sets = int.Parse(formData[key]);
                }
                
                WorkoutHasExercise workoutHasExercise = new WorkoutHasExercise
                {
                    WorkoutId = workout.Id,
                    ExerciseId = id,
                    IsOrder = i,
                    Sets = sets
                };
                _db.WorkoutHasExercises.Add(workoutHasExercise);
            }
            i++;
        }
        
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), "Trainer");
    }
}