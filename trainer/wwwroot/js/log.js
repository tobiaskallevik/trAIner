var token = $('input[name="__RequestVerificationToken"]').val();

// Deletes a logged workout
$('.delete-logged-workout').click(function() {
    var logId = $(this).data('log-id');
    $.ajax({
        url: '/Log/DeleteLoggedWorkout',
        type: 'POST',
        data: { logId: logId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Only shows the workouts that are in the selected routine
$(document).ready(function() {
    $('#filterByRoutine').change(function() {
        var routineId = $(this).val();  
        console.log("Routine ID: ", routineId);
        if (routineId == "0") {  
            $('.chooseWorkoutToLog').show();
        } else {
            $('.chooseWorkoutToLog').hide();
            $('.chooseWorkoutToLog[data-routine-ids*="' + routineId + '"]').show();
        }
    });
});


