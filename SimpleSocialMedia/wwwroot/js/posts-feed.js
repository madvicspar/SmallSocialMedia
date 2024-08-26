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
        url: '/Feed/GetNewsTab',
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

$(document).on('click', '.edit-post', function (e) {
    e.preventDefault();
    var $post = $(this).closest('.post');
    $post.find('.post-content').hide();
    $post.find('.post-actions').hide();
    $post.find('.post-comments').hide();
    $post.find('.post-menu').hide();
    $post.find('.edit-post-form').show();
    $post.find('.edit-post-content').val($post.find('.post-content p').text());
});


$(document).on('click', '.cancel-edit-button', function (e) {
    e.preventDefault();
    var $post = $(this).closest('.post');
    $post.find('.post-content').show();
    $post.find('.post-actions').show();
    $post.find('.post-comments').show();
    $post.find('.post-menu').show();
    $post.find('.edit-post-form').hide();
});

$(document).on('click', '.save-post-button', function (e) {
    e.preventDefault();
    var $post = $(this).closest('.post');
    var postId = $post.data('post-id');
    var content = $post.find('.edit-post-content').val().trim();

    if (!content) {
        alert('Пост не может быть пустым.');
        return;
    }

    $.ajax({
        url: `/Posts/Edit`,
        type: 'POST',
        data: {
            content: content,
            postId: postId
        },
        success: function () {
            $post.find('.post-content p').text(content);

            $post.find('.post-content').show();
            $post.find('.post-actions').show();
            $post.find('.post-comments').show();
            $post.find('.post-menu').show();

            $post.find('.edit-post-form').hide();
        },
        error: function (data) {
            alert('Ошибка при обновлении публикации');
        }
    });
});


$(document).on('click', '.new-comment-button', function (e) {
    e.preventDefault();
    var $post = $(this).closest('.post');
    var postId = $post.data('post-id');
    var content = $post.find('.new-comment-content').val();

    $.ajax({
        url: `/Comments/Add`,
        type: 'POST',
        data: {
            content: content,
            postId: postId
        },
        success: function (data) {
            $post.find('.new-comment-content').val('');
            $post.find('.existing-comments').html(data);
        },
        error: function (data) {
            alert('Ошибка при добавлении комментария');
        }
    });
});

$(document).on('click', '.comment-menu-button', function (e) {
    e.preventDefault();
    var $menuContent = $(this).siblings('.comment-menu-content');
    $('.comment-menu-content').not($menuContent).hide();
    $menuContent.toggle();
});

$(document).on('mouseleave', '.comment-menu-content', function (e) {
    $('.comment-menu-content').hide();
});

$(document).on('click', '.delete-comment', function (e) {
    e.preventDefault();
    var $post = $(this).closest('.post');
    var $commentId = $(this).data('id');
    $.ajax({
        url: `/Comments/Delete`,
        type: 'POST',
        data: { id: $commentId },
        success: function (data) {
            $post.find('.existing-comments').html(data);
        },
        error: function (data) {
            alert('Ошибка при удалении комментария');
        }
    });
});