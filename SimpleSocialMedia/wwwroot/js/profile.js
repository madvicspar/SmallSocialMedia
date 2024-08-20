var modal = $('#editProfileModal');
var btn = $('.edit-profile-button');
var span = $('.close');

$('.subscribe-button').on('click', function () {
    var $button = $(this);
    var isSubscribed = $button.hasClass('unsubscribe');
    var userId = $button.data("user-id");

    var subscribeUrl = '/Users/Subscribe';
    var unsubscribeUrl = '/Users/Unsubscribe';

    var requestUrl = isSubscribed ? unsubscribeUrl : subscribeUrl;

    $.ajax({
        url: requestUrl,
        type: 'POST',
        data: { userId: userId },
        success: function (response) {
            if (isSubscribed) {
                $button.removeClass('unsubscribe').addClass('subscribe');
                $button.text('Подписаться');
            } else {
                $button.removeClass('subscribe').addClass('unsubscribe');
                $button.text('Отписаться');
            }
        },
        error: function () {
            alert('Произошла ошибка при выполнении запроса. Попробуйте еще раз.');
        }
    });
});

$('.tab-link').on('click', function () {
    $('.tab-link').removeClass('active');
    $(this).addClass('active');

    var type = $(this).data('type');
    var contentContainer = type === "my-posts" ? ".post-feed" : ".comments";

    $('.tab-content > div').hide();
    $(contentContainer).show();

    $.ajax({
        url: `/Users/${type === "my-posts" ? "GetMyPostsTab" : "GetMyCommentsTab"}`,
        method: 'GET',
        data: { userId: $(this).closest('.tabs').data('user-id') },
        success: function (data) {
            $(contentContainer).html(data);
        },
        error: function () {
            alert('Произошла ошибка при загрузке данных.');
        }
    });
});


$(document).on('click', '.followers-link, .following-link', function (e) {
    e.preventDefault();

    var userId = $(this).data('user-id');
    var linkType = $(this).hasClass('followers-link') ? 'followers' : 'following';
    window.location.href = `/Users/Subscriptions?userId=${userId}&type=${linkType}`;
});

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
    $('.edit-header-photo').css('background-image', 'url(' + defaultHeaderPhotoUrl + ')');
});

var isMenuVisible = false;

$('.edit-avatar').on('mouseover', function () {
    var $menuContent = $(this).siblings('.avatar-menu-content');
    $('.avatar-menu-content').not($menuContent).hide();
    $menuContent.show();
    isMenuVisible = true;
});

$('.avatar-menu-content').on('mouseenter', function () {
    isMenuVisible = true;
});

$('.edit-avatar, .edit-avatar-container, .avatar-menu-content').on('mouseleave', function () {
    isMenuVisible = false;
    setTimeout(function () {
        if (!isMenuVisible) {
            $('.avatar-menu-content').hide();
        }
    }, 100);
});

$('.update-avatar').on('click', function () {
    $('#add-avatar-input').click();
});

$('#add-avatar-input').on('change', function () {
    var input = this;
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('.edit-avatar').attr('src', e.target.result);
        };
        reader.readAsDataURL(input.files[0]);
    }
});

$('.delete-avatar').on('click', function () {
    $('.edit-avatar').attr('src', defaultAvatarUrl);
});

$('.save-profile').on('click', function () {
    var avatarUrl = $('.edit-avatar').attr('src');

    if (avatarUrl !== defaultAvatarUrl) {
        $('#avatar-photo-url').val(avatarUrl);
    } else {
        $('#avatar-photo-url').val(null);
    }

    var backgroundImage = $('.edit-header-photo').css('background-image');
    var urls = backgroundImage.match(/url\(["']?(.*?)["']?\)/gi);
    var headerUrl = urls ? urls[0].replace(/url\(["']?(.*?)["']?\)/i, '$1') : null;

    if (headerUrl !== defaultHeaderPhotoUrl) {
        $('#header-photo-url').val(headerUrl);
    } else {
        $('#header-photo-url').val(null);
    }

    $('#edit-profile-form').submit();
});