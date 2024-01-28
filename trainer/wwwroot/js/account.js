var token = $('input[name="__RequestVerificationToken"]').val();

function updateStars() {
    $('.adminStar').hide();

    $('.adminStar').filter(function() {
        var isAdmin = false;
        
        if ($(this).data('admin') === "True") {
            isAdmin = true;
        }

        return isAdmin;
    }).show();

    $('.superAdminStar').hide();

    $('.superAdminStar').filter(function() {
        var isSuperAdmin = false;
        
        if ($(this).data('super-admin') === "True") {
            isSuperAdmin = true;
        }

        return isSuperAdmin;
    }).show();
}

function updateBannedSymbol() {
    $('.banned-symbol').hide();

    $('.banned-symbol').filter(function () {
        var isBanned = false;

        if ($(this).data('is-banned') === "True") {
            isBanned = true;
        }
        
        console.log($(this).data('is-banned'));

        return isBanned;
    }).show();
}

$(document).ready(function() {
    updateStars();
    updateBannedSymbol();
});

// Makes user an admin or removes admin role
$('.admin-action-button').click(function(e) {
    e.preventDefault();
    var $button = $(this);
    var userId = $button.data('user-id');
    var isAdmin = $button.data('is-admin');
    var $parentContainer = $button.closest('.workoutListItem');
    var $adminStar = $parentContainer.find('.adminStar');
    
    if (isAdmin === "True") {
        var action = "RemoveAdmin";
    } else {
        var action = "MakeAdmin";
    }

    // AJAX request to server to update user role
    $.ajax({
        url: '/Account/UpdateUserRole',
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: { userId: userId, action: action},
        success: function(data) {
            console.log(data.success);
            if(data.success) {
                // Toggle button text and action
                if(action === "MakeAdmin") {
                    $button.text("Remove Admin");
                    $button.data('is-admin', 'True');
                    $adminStar.data('admin', 'True');
                } else {
                    $button.text("Make Admin");
                    $button.data('is-admin', 'False');
                    $adminStar.data('admin', 'False');
                }
                updateStars();
            }
            else {
                alert(data.message);
            }
        },
        error: function(error) {
            console.error('Error:', error);
        }
    });
});

// Ban users
$('.ban-action-button').click(function(e) {
    e.preventDefault();
    var $button = $(this);
    var userId = $button.data('user-id');
    var isBanned = $button.data('is-banned');
    var $parentContainer = $button.closest('.workoutListItem');
    var $banSymbol = $parentContainer.find('.banned-symbol');

    if (isBanned === "True") {
        var action = "Unban";
    } else {
        var action = "Ban";
    }

    // AJAX request to server to update user role
    $.ajax({
        url: '/Account/UpdateUserRole',
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: { userId: userId, action: action},
        success: function(data) {
            if(data.success) {
                // Toggle button text and action
                if(action === "Ban") {
                    $button.text("Unban");
                    $button.data('is-banned', 'True');
                    $banSymbol.data('is-banned', 'True');
                } else {
                    $button.text("Ban");
                    $button.data('is-banned', 'False');
                    $banSymbol.data('is-banned', 'False');
                }
                updateBannedSymbol();
            }
            else {
                alert(data.message);
            }
        },
        error: function(error) {
            console.error('Error:', error);
        }
    });
});
