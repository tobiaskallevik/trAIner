// Filters what to show on the shared page
$(document).ready(function() {
    // Shows workout list or routine list based on saved preference
    function showCorrectTab() {
        var selectedTab = sessionStorage.getItem('selectedTab') || 'workoutsButton';
        $("#" + selectedTab).trigger("click");
    }

    // Shows workout list
    $("#workoutsButton").click(function() {
        sessionStorage.setItem('selectedTab', 'workoutsButton'); // Save preference
        $("#workoutsList").show();
        $("#routinesList").hide();
        $("#workoutsTitle").show();
        $("#routinesTitle").hide();
        $("#workoutsButton").addClass("active");
        $("#routinesButton").removeClass("active");
    });

    // Shows routine list
    $("#routinesButton").click(function() {
        sessionStorage.setItem('selectedTab', 'routinesButton'); // Save preference
        $("#workoutsList").hide();
        $("#routinesList").show();
        $("#workoutsTitle").hide();
        $("#routinesTitle").show();
        $("#workoutsButton").removeClass("active");
        $("#routinesButton").addClass("active");
    });

    // Set up the back button
    $("#backButton").click(function() {
        // Navigate back and show the correct tab
        window.history.back();
        showCorrectTab();
    });

    // Shows the correct list on page load based on saved preference
    showCorrectTab();
});
