using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GradeBook.Storage.Entities;

namespace GradeBook.Controllers
{

    [Authorize(Policy = "AdminOrGradeTransfer")]
    public class BlockController : Controller
    {
        IGradeBookContext _context;
        public BlockController(IGradeBookContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> BlockGradePage(int subjectId, int groupId)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if (subject is null || group is null) return NotFound();

            var block = await _context.BlockedPages.FirstOrDefaultAsync(b => b.SubjectId == subject.Id && b.GroupId == group.Id);
            if(block is null)
                _context.BlockedPages.Add(new BlockedPage()
                {
                    Subject = subject,
                    Group = group,
                    TimeStamp = DateTime.UtcNow
                });
            else
                block.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }


    }
}
