// Filters what to show on the friends page
$(document).ready(function() {
    // Shows friends list
    $("#friendsButton").click(function() {
        $("#friendsList").show();
        $("#requestsList").hide();
        $("#friendsButton").addClass("active");
        $("#requestsButton").removeClass("active");
    });

    // Shows requests list
    $("#requestsButton").click(function() {
        $("#requestsList").show();
        $("#friendsList").hide();
        $("#requestsButton").addClass("active");
        $("#friendsButton").removeClass("active");
    });

    // Shows friends list by default
    $("#friendsButton").trigger("click");
});

// Asks the user to confirm if they want to delete a friend
$(document).ready(function() {
    $(".deleteFriend").click(function(e) {
        if (!confirm("Are you sure you want to delete this friend?")) {
            e.preventDefault();
        }
    });
});
