using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Events;
using Microsoft.EntityFrameworkCore;

namespace ProjectService.Services
{
    public class MemberService
    {
        private readonly AppDbContext _context;
        private readonly IEventPublisher _eventPublisher;

        public MemberService(AppDbContext context, IEventPublisher eventPublisher)
        {
            _context        = context;
            _eventPublisher = eventPublisher;
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

            await _eventPublisher.PublishAsync("project.member.added", new MemberAddedEvent
            {
                MemberId  = member.Id,
                ProjectId = member.ProjectId,
                UserId    = member.UserId,
                Role      = member.Role,
                JoinedAt  = member.JoinedAt
            });

            return member;
        }

        public async Task<Member?> UpdateMemberRoleAsync(Guid memberId, int newRole)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return null;

            member.Role = newRole;
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

        public async Task<int?> GetUserRoleInProjectAsync(Guid projectId, Guid userId)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            return member?.Role;
        }
    }
}