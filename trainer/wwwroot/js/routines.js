
// Gets the anti-forgery token
var token = $('input[name="__RequestVerificationToken"]').val();

// Deletes a workout from the routine
$('.delete-from-routine').click(function() {
    var rWId = $(this).data('routine-workout-id');
    var workoutId = $(this).data('workout-id');
    var routineId = $(this).data('routine-id');
    
    $.ajax({
        url: '/Routines/DeleteFromRoutine',
        type: 'POST',
        data: { rWId: rWId, workoutId: workoutId, routineId: routineId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Moves a workout up in the routine
$('.move-workout-up').click(function() {
    var rWId = $(this).data('routine-workout-id');
    var workoutId = $(this).data('workout-id');
    var routineId = $(this).data('routine-id');

    $.ajax({
        url: '/Routines/MoveUp',
        type: 'POST',
        data: { rWId: rWId, workoutId: workoutId, routineId: routineId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});

// Moves a workout down in the routine
$('.move-workout-down').click(function() {
    var rWId = $(this).data('routine-workout-id');
    var workoutId = $(this).data('workout-id');
    var routineId = $(this).data('routine-id');

    $.ajax({
        url: '/Routines/MoveDown',
        type: 'POST',
        data: { rWId: rWId, workoutId: workoutId, routineId: routineId },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});


// Toggles the exercise dropdown
$(".exerciseDropdownArrow").click(function(event){
    event.stopPropagation();  
    // Shows or hides the dropdown
    var dropdown = $(this).closest('.exerciseListItem').find('.exerciseDropdown');
    dropdown.toggleClass('d-none d-flex');

    // Checks if the dropdown is visible and rotates the arrow
    if (dropdown.hasClass('d-flex')) {
        $(this).css('transform', 'rotate(180deg)');
    } else {
        $(this).css('transform', 'rotate(0deg)');
    }
});


// Changes the name of the routine
$('#saveRoutineName').click(function() {
    var routineId = $(this).data('routine-id');
    var updateRoutineName = $('#updateRoutineName').val();
    $.ajax({
        url: '/Routines/UpdateName',
        type: 'POST',
        data: { routineId: routineId, name: updateRoutineName },
        headers: { 'RequestVerificationToken': token },
        success: function() {
            history.pushState({}, '');
            location.reload();
        }
    });
});
