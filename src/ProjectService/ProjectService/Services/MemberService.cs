using ProjectService.Data;
using ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectService.Services
{
    public class MemberService
    {
        private readonly AppDbContext _context;

        public MemberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetProjectMembersAsync(Guid projectId)
        {
            return await _context.Members
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Member> AddMemberAsync(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<bool> RemoveMemberAsync(Guid memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return false;

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}