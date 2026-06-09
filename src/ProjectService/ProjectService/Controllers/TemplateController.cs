using Microsoft.AspNetCore.Mvc;
using ProjectService.Templates;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class TemplateController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTemplates()
        {
            var data = TemplateDefinitions.All.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Icon,
                t.IconBg,
                t.IconColor,
                t.Recommended,
                t.Tags,
                Columns        = t.Columns.Select(c => c.Name).ToList(),
                ColumnDetails  = t.Columns.Select((c, i) => new { c.Name, c.Type, Position = i }),
                t.SprintCount,
                MilestoneNames = t.Milestones.Select(m => m.Name).ToList(),
            });

            return Ok(new { success = true, data });
        }
    }
}
