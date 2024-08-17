using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        // GET: Comments/5
        public async Task<IActionResult> Get(long postId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return PartialView("_PostsListPartial", new List<PostModel>());
                }

                var count = await dataBase.Comments
                .CountAsync(l => l.PostId == postId);

                return Ok(new { count });
            }
        }

        // POST: Comments/{content,postId}
        public async Task<IActionResult> Add(string content, long postId)
        {
            try
            {
                using (var serviceScope = ServiceActivator.GetScope())
                {
                    var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                    if (dataBase == null)
                    {
                        return BadRequest();
                    }

                    var post = await dataBase.Posts.FindAsync(postId);
                    if (post == null)
                    {
                        return BadRequest();
                    }

                    var comment = new CommentModel()
                    {
                        Content = content,
                        CreatedAt = DateTime.UtcNow,
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        Post = post
                    };

                    await dataBase.Comments.AddAsync(comment);
                    await dataBase.SaveChangesAsync();

                    var comments = await dataBase.Comments
                        .Where(c => c.PostId == post.Id)
                        .Include(c => c.User)
                        .OrderBy(c => c.CreatedAt)
                        .ToListAsync();

                    return PartialView("_CommentsPartial", comments);
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        // DELETE: Comments/{id}
        public async Task<IActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return StatusCode(500, "Database access failed.");
                }

                var comment = await dataBase.Comments.FindAsync(id);
                var post = await dataBase.Posts.FindAsync(comment.PostId);
                if (comment != null)
                {
                    dataBase.Comments.Remove(comment);
                    await dataBase.SaveChangesAsync();
                }

                var comments = await dataBase.Comments
                        .Where(c => c.PostId == post.Id)
                        .Include(c => c.User)
                        .OrderBy(c => c.CreatedAt)
                        .ToListAsync();

                return PartialView("_CommentsPartial", comments);
            }
        }
    }
}
