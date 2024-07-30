$(document).on('input', '#new-post-content', function (e) {
    this.style.height = 'auto';
    if (this.scrollHeight > window.innerHeight * 0.6) {
        this.style.height = window.innerHeight * 0.6 + 'px';
        this.style.overflowY = 'scroll';
    } else {
        this.style.height = (this.scrollHeight) + 'px';
        this.style.overflowY = 'hidden';
    }
}).trigger('input');

$(document).on('click', '.new-post-button', function (e) {
    e.preventDefault();
    var content = $('#new-post-content').val();

    $.ajax({
        url: `/Posts/Add`,
        type: 'POST',
        data: { content: content },
        success: function (data) {
            $('#new-post-content').val('');
            loadPosts();
        },
        error: function (data) {
            alert('Ошибка при добавлении поста');
        }
    });
});

function loadPosts() {
    $.ajax({
        url: '/Posts/Get',
        type: 'GET',
        success: function (response) {
            $('.post-feed').html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

$(document).on('click', '.like-toggle', function (e) {
    var $this = $(this);
    var $post = $this.closest(".post");
    var postId = $post.data("post-id");
    var isLiked = !$this.attr("src").includes("not-liked.svg");
    var url = isLiked ? `/Likes/Delete?postId=${postId}` : `/Likes/Add?postId=${postId}`;

    $.ajax({
        url: url,
        type: isLiked ? "DELETE" : "POST",
        success: function () {
            if (isLiked) {
                $this.attr("src", "/images/not-liked.svg");
                $post.find(".likes-count").text(function (i, text) {
                    return parseInt(text) - 1;
                });
            } else {
                $this.attr("src", "/images/liked.svg");
                $post.find(".likes-count").text(function (i, text) {
                    return parseInt(text) + 1;
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("Error: " + error);
        }
    });
});

$(document).on('click', '.username', function (e) {
    var userId = $(this).closest('.username-link').data('user-id');
    window.location.href = `/Users/Profile?userId=${userId}`;
});

$(document).on('click', '.post-menu-button', function (e) {
    e.preventDefault();
    var $menuContent = $(this).siblings('.post-menu-content');
    $('.post-menu-content').not($menuContent).hide();
    $menuContent.toggle();
});

$(document).on('mouseleave', '.post-menu-content', function (e) {
    $('.post-menu-content').hide();
});

$(document).on('click', '.delete-post', function (e) {
    e.preventDefault();
    var $postId = $(this).closest('.post').data('post-id');
    $.ajax({
        url: `/Posts/Delete`,
        type: 'POST',
        data: { id: $postId },
        success: function (data) {
            loadPosts();
        },
        error: function (data) {
            alert('Ошибка при удалении поста');
        }
    });
});