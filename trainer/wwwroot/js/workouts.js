// Gets the anti-forgery token
// This is needed since we are using ajax to post data to the server
// Using ajax to post data to the server can be a security risk if not done properly
var token = $('input[name="__RequestVerificationToken"]').val();

// Changes the value of the isOrdered when the arrow buttons are clicked
$('.move-exercise-up').click(function() {
    var exerciseId = $(this).data('exercise-id');
    var workoutId = $(this).data('workout-id');

    $.ajax({
        url: '/Workouts/MoveUp',
        type: 'POST',
        data: { exerciseId: exerciseId, workoutId: workoutId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

$('.move-exercise-down').click(function() {
    var exerciseId = $(this).data('exercise-id');
    var workoutId = $(this).data('workout-id');

    $.ajax({
        url: '/Workouts/MoveDown',
        type: 'POST',
        data: { exerciseId: exerciseId, workoutId: workoutId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Changes the value of the the sets when the arrow buttons are clicked
$('.sets-up').click(function() {
    var exerciseId = $(this).data('exercise-id');
    var workoutId = $(this).data('workout-id');
    $.ajax({
        url: '/Workouts/SetsUp',
        type: 'POST',
        data: { exerciseId: exerciseId, workoutId: workoutId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

$('.sets-down').click(function() {
    var exerciseId = $(this).data('exercise-id');
    var workoutId = $(this).data('workout-id');
    $.ajax({
        url: '/Workouts/SetsDown',
        type: 'POST',
        data: { exerciseId: exerciseId, workoutId: workoutId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Deletes an exercise from the workout
$('.delete-from-workout').click(function() {
    var exerciseId = $(this).data('exercise-id');
    var workoutId = $(this).data('workout-id');
    $.ajax({
        url: '/Workouts/DeleteFromWorkout',
        type: 'POST',
        data: { exerciseId: exerciseId, workoutId: workoutId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Filter and search exercises
$(document).ready(function() {
    function filterAndSearchExercises() {
        var muscleGroup = $('#muscleGroupFilter').val();
        var search = $('#exerciseSearch').val().toLowerCase();

        $('.exerciseContainer').hide();

        $('.exerciseContainer').filter(function() {
            var matchesMuscleGroup = (muscleGroup === "Show All") ||
                ($(this).data('muscle-group') === muscleGroup);
            var matchesSearch = search === "" ||
                $(this).data('exercise-name').toLowerCase().indexOf(search) > -1;

            return matchesMuscleGroup && matchesSearch;
        }).show();
    }

    // Event handlers
    $('#muscleGroupFilter').change(filterAndSearchExercises);
    $('#exerciseSearch').on('input', filterAndSearchExercises);
});

// Modal for showing the image of the exercise
$(document).ready(function() {
    // Get the modal
    var modal = document.getElementById("modal1");
    var modalImg = document.getElementById("img01");

    // When the user clicks on the image, open the modal and show the gif of the exercise
    $(".exerciseInfo").click(function(event){
        modal.style.display = "block";
        var gifUrl = $(event.target).data('gif-url'); 
        modalImg.src = "/img/gifs/" + gifUrl; 
    });
    
    // When the user clicks anywhere outside of the image, close it
    window.onclick = function(event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
});

// Changes the name of the workout
$('#saveWorkoutName').click(function() {
    var workoutId = $(this).data('workout-id');
    var updateWorkoutName = $('#updateWorkoutName').val();
    $.ajax({
        url: '/Workouts/UpdateName',
        type: 'POST',
        data: { workoutId: workoutId, name: updateWorkoutName },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

$(".workoutDropdownArrow").click(function(event){
    event.stopPropagation();
    // Shows or hides the dropdown
    var dropdown = $(this).closest('.workoutContent').find('.exerciseDropdown');
    dropdown.toggleClass('d-none d-flex');

    // Checks if the dropdown is visible and rotates the arrow
    if (dropdown.hasClass('d-flex')) {
        $(this).css('transform', 'rotate(180deg)');
    } else {
        $(this).css('transform', 'rotate(0deg)');
    }
});




