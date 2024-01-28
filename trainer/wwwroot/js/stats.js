// Gets the anti-forgery token
// This is needed since we are using ajax to post data to the server
// Using ajax to post data to the server can be a security risk if not done properly
var token = $('input[name="__RequestVerificationToken"]').val();

$(document).ready(function() {
    function updateCounts() {
        // Get the number of users
        $.ajax({
            url: '/Stats/GetUserCount',
            type: 'GET',
            headers: { 'RequestVerificationToken': token },
            success: function(count) {
                $('#userCount').text(count);
            }
        });
        
        // Get the number of workouts
        $.ajax({
            url: '/Stats/GetWorkoutCount',
            type: 'GET',
            headers: { 'RequestVerificationToken': token },
            success: function(count) {
                $('#workoutCount').text(count);
            }
        });

        // Get the number of exercises
        $.ajax({
            url: '/Stats/GetExerciseCount',
            type: 'GET',
            headers: { 'RequestVerificationToken': token },
            success: function(count) {
                $('#exerciseCount').text(count);
            }
        });
        
        // Get the number of routines
        $.ajax({
            url: '/Stats/GetRoutineCount',
            type: 'GET',
            headers: { 'RequestVerificationToken': token },
            success: function(count) {
                $('#routineCount').text(count);
            }
        });
        
        // Get the number of logged workouts
        $.ajax({
            url: '/Stats/GetLoggedWorkoutCount',
            type: 'GET',
            headers: { 'RequestVerificationToken': token },
            success: function(count) {
                $('#loggedWorkoutCount').text(count);
            }
        });
    }
    
    // Updates each minute
    if ($('#statPageContainer').length > 0) {
        updateCounts();
        setInterval(updateCounts, 60000);
    }
});