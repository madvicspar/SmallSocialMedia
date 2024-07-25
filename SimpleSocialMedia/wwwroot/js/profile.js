var modal = $('#editProfileModal');
var btn = $('.edit-profile-button');
var span = $('.close');

btn.on('click', function () {
    modal.show();
});

span.on('click', function () {
    modal.hide();
});

$(window).on('click', function (event) {
    if ($(event.target).is(modal)) {
        modal.hide();
    }
});