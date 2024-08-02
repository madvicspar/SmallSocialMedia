function loadTab(tabType) {
    $.ajax({
        url: '/Users/LoadUsers',
        type: 'GET',
        data: { type: tabType, userId: currentUserId },
        success: function (data) {
            $('#users-list').html(data);
        },
        error: function (xhr) {
            alert('Произошла ошибка при загрузке данных. Попробуйте еще раз.');
        }
    });
}

$(document).ready(function () {
    var initialTab = tab;
    loadTab(initialTab);

    $('.tab-link').click(function () {
        var type = $(this).data('type');
        $('.tab-link').removeClass('active');
        $(this).addClass('active');
        loadTab(type);
    });

    $(document).on('click', '.unsubscribe-button', function () {
        var userId = $(this).data('user-id');
        alert('Unsubscribed from user with ID: ' + userId);
    });
});

$(document).on('click', '.back-button', function () {
    window.location.href = `/Users/Profile?userId=${currentUserId}`;
});

$(document).on('click', '.user-card', function () {
    var userId = $(this).data('user-id');
    window.location.href = `/Users/Profile?userId=${userId}`;
});

$(document).on('mouseover', '.subscribe-button', function () {
    this.closest('.user-card').classList.add('no-hover');
});

$(document).on('mouseleave', '.subscribe-button', function () {
    this.closest('.user-card').classList.remove('no-hover');
});

$(document).on('click', '.subscribe-button', function (event) {
    event.stopPropagation();
    var button = $(this);
    var userId = button.data('user-id');
    var isSubscribed = button.hasClass('unsubscribe');

    $.ajax({
        url: isSubscribed ? '/Users/Unsubscribe' : '/Users/Subscribe',
        type: 'POST',
        data: { userId: userId },
        success: function (response) {
            button.toggleClass('unsubscribe subscribe');
            button.text(isSubscribed ? 'Подписаться' : 'Отписаться');
        },
        error: function (xhr) {
            alert('Произошла ошибка при выполнении операции. Попробуйте еще раз.');
        }
    });
});