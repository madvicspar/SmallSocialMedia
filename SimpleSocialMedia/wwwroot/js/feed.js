$('.tab-link').on('click', function () {
    $('.tab-link').removeClass('active');
    $(this).addClass('active');

    var type = $(this).data('type');
    var method = type == "my-news" ? "GetMyNewsTab" : "GetPopularPostsTab";

    $.ajax({
        url: `/Feed/${method}`,
        method: 'GET',
        success: function (data) {
            $('.post-feed').html(data);
        },
        error: function () {
            alert('Произошла ошибка при загрузке постов.');
        }
    });
});

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}