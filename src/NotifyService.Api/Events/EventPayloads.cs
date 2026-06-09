namespace NotifyService.Api.Events;

// ── Incoming từ TaskService (task_events exchange) ──────────────────────────

public class TaskColumnChangedPayload
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid OldColumnId { get; set; }
    public Guid NewColumnId { get; set; }
    public string NewColumnType { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class TaskAssignedPayload
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid? PreviousAssignee { get; set; }
    public Guid? NewAssignee { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
}

// ── Incoming từ ProjectService (project_events exchange) ────────────────────

public class MemberAddedPayload
{
    public Guid MemberId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public int Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

// ── Envelope wrapper từ tất cả services ─────────────────────────────────────

public class EventEnvelope<T>
{
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public T? Payload { get; set; }
}
