using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly NotifyDbContext _context;

    public EventsController(NotifyDbContext context)
    {
        _context = context;
    }

    [HttpPost("incoming")]
    public async Task<IActionResult> ReceiveIncomingEvent([FromBody] IncomingEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventId))
        {
            return BadRequest(new { message = "EventId không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            return BadRequest(new { message = "EventType không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.SourceService))
        {
            return BadRequest(new { message = "SourceService không được để trống" });
        }

        var existed = await _context.IncomingEvents
            .AnyAsync(x => x.EventId == request.EventId);

        if (existed)
        {
            return Conflict(new { message = "EventId đã tồn tại, event bị trùng" });
        }

        var incomingEvent = new IncomingEvent
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId.Trim(),
            EventType = request.EventType.Trim(),
            SourceService = request.SourceService.Trim(),
            PayloadJson = request.PayloadJson,
            Status = "Received",
            ReceivedAt = DateTime.UtcNow
        };

        incomingEvent.EventProcessingLogs.Add(new EventProcessingLog
        {
            Id = Guid.NewGuid(),
            IncomingEventId = incomingEvent.Id,
            Step = "RECEIVED",
            Status = "Success",
            Message = "Event received by NotifyService",
            CreatedAt = DateTime.UtcNow
        });

        // Bước đầu chỉ đánh dấu Processed để chứng minh flow hoạt động.
    
        incomingEvent.Status = "Processed";
        incomingEvent.ProcessedAt = DateTime.UtcNow;

        incomingEvent.EventProcessingLogs.Add(new EventProcessingLog
        {
            Id = Guid.NewGuid(),
            IncomingEventId = incomingEvent.Id,
            Step = "PROCESSED",
            Status = "Success",
            Message = "Event processed successfully",
            CreatedAt = DateTime.UtcNow
        });

        _context.IncomingEvents.Add(incomingEvent);
        await _context.SaveChangesAsync();

        return Ok(new IncomingEventResponse
        {
            Id = incomingEvent.Id,
            EventId = incomingEvent.EventId,
            EventType = incomingEvent.EventType,
            SourceService = incomingEvent.SourceService,
            Status = incomingEvent.Status,
            ReceivedAt = incomingEvent.ReceivedAt,
            ProcessedAt = incomingEvent.ProcessedAt
        });
    }

    [HttpGet("incoming")]
    public async Task<IActionResult> GetIncomingEvents()
    {
        var events = await _context.IncomingEvents
            .OrderByDescending(x => x.ReceivedAt)
            .Select(x => new IncomingEventResponse
            {
                Id = x.Id,
                EventId = x.EventId,
                EventType = x.EventType,
                SourceService = x.SourceService,
                Status = x.Status,
                ReceivedAt = x.ReceivedAt,
                ProcessedAt = x.ProcessedAt
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("incoming/{id:guid}")]
    public async Task<IActionResult> GetIncomingEventById(Guid id)
    {
        var incomingEvent = await _context.IncomingEvents
            .Include(x => x.EventProcessingLogs)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (incomingEvent == null)
        {
            return NotFound(new { message = "Không tìm thấy event" });
        }

        return Ok(new
        {
            incomingEvent.Id,
            incomingEvent.EventId,
            incomingEvent.EventType,
            incomingEvent.SourceService,
            incomingEvent.PayloadJson,
            incomingEvent.Status,
            incomingEvent.ReceivedAt,
            incomingEvent.ProcessedAt,
            processingLogs = incomingEvent.EventProcessingLogs
                .OrderBy(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Step,
                    x.Status,
                    x.Message,
                    x.CreatedAt
                })
        });
    }
}