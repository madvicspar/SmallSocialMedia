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
    var userId = $(this).data('user-id');
    window.location.href = `/Users/Profile?userId=${userId}`;
});