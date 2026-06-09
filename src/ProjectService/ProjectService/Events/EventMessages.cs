namespace ProjectService.Events
{
    // project.created
    public class ProjectCreatedEvent
    {
        public Guid   ProjectId  { get; set; }
        public string TemplateId { get; set; } = string.Empty;
        public List<ColumnDefinition> Columns { get; set; } = new();
    }

    public class ColumnDefinition
    {
        public string Name     { get; set; } = string.Empty;
        public string Type     { get; set; } = string.Empty;
        public int    Position { get; set; }
    }

    // project.member.added
    public class MemberAddedEvent
    {
        public Guid MemberId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public int Role { get; set; }   // Owner=0, Manager=1, Member=2, Viewer=3
        public DateTime JoinedAt { get; set; }
    }

    // sprint.started
    public class SprintStartedEvent
    {
        public Guid SprintId { get; set; }
        public Guid ProjectId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
