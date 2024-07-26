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

$('.add-header-photo-container').on('click', function () {
    $('#add-header-photo-input').click();
});

$('#add-header-photo-input').on('change', function () {
    var input = this;
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('.edit-header-photo').css('background-image', 'url(' + e.target.result + ')');
        };
        reader.readAsDataURL(input.files[0]);
    }
});

$('.delete-header-photo-container').on('click', function () {
    var backgroundImage = 'url(' + initialHeaderPhotoUrl + '), url(' + defaultHeaderPhotoUrl + ')';
    $('.edit-header-photo').css('background-image', backgroundImage);
});