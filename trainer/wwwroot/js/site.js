// Function to change the color of the sidebar element when a user visits a specific page
$(document).ready(function() {
    var path = window.location.pathname;
    var changeColor = function(id) {
        $(id).css({
            'background-color': '#0A84FF',
            'box-shadow': '0 0.1rem 0.2rem 0 rgba(0,0,0,0.59)',
            'transition': 'background-color 2s ease, box-shadow 0.2s ease;',
            'border-radius': '0.5rem'
        }) 
    };
    if (path == '/Trainer/Index' || path.startsWith('/Log') || path.startsWith('/Generation') || path.startsWith('/Routines') || path.startsWith('/Workouts') || path.startsWith('/PendingShares')) {
        
        changeColor('#mainMenuDesktopIcon');
        changeColor('#mainMenuMobileIcon');
        
    } else if (path == '/ExerciseStats/Index') {
        
        changeColor('#statsDesktopIcon');
        changeColor('#statsMobileIcon');
        
    } else if (path == '/Social/Friends') {
        
        changeColor('#socialDesktopIcon');
        
    } else if (path == '/Admin/Index' || path == '/Exercise/Index' || path == '/Account/Index' || path == '/Stats/Index') {
        
        changeColor('#adminDesktopIcon');
        
    } else if (path.startsWith('/Identity/Account/Manage')) {
        changeColor('#accountDesktopIcon');
    }
});

// Function to validate the input of the search bar to protect against injection
// Vanilla JS as in this instance it is easier to use than jQuery
function validateInput(input) {
    var regex = /^[a-zA-Z0-9æøåÆØÅ@ ]*$/;
    if (!regex.test(input.value)) {
        alert("Invalid characters detected. Please only use alphanumeric characters.");
        input.value = "";
    }
}


// Displays the loading modal when the user presses the button
$("#workoutGeneratorForm").on("submit", function(event) {
    $(".loadingModal").css("display", "flex");
});

// Loading dots
var dots = '';
var element = document.getElementsByClassName('loading-text h3');

setInterval(function() {
    dots = dots + '.';
    element.innerText = dots;
}, 1000); 


