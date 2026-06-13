namespace ProjectService.Templates
{
    public record ColumnDef(string Name, string Type);

    public record MilestoneDef(string Name, int OffsetDays, bool UseEndDate = false);

    public record ProjectTemplate(
        string Id,
        string Name,
        string Description,
        string Icon,
        string IconBg,
        string IconColor,
        bool Recommended,
        List<string> Tags,
        List<ColumnDef> Columns,
        int SprintCount,
        List<MilestoneDef> Milestones);

    public static class TemplateDefinitions
    {
        public static readonly List<ProjectTemplate> All = new()
        {
            new ProjectTemplate(
                Id:          "software-dev",
                Name:        "Phát triển phần mềm",
                Description: "Quy trình Scrum với 3 sprint liên tiếp. Phù hợp cho các dự án lập trình, xây dựng sản phẩm.",
                Icon:        "sprint",
                IconBg:      "bg-primary/10",
                IconColor:   "text-primary",
                Recommended: true,
                Tags:        new() { "3 Sprints", "Backlog", "Review", "Milestone" },
                Columns: new()
                {
                    new("Backlog",     "backlog"),
                    new("To Do",       "active"),
                    new("In Progress", "active"),
                    new("Review",      "active"),
                    new("Testing",     "active"),
                    new("Done",        "done"),
                },
                SprintCount: 3,
                Milestones: new()
                {
                    new("Alpha Release", OffsetDays: 14),
                    new("Beta Release",  OffsetDays: 28),
                    new("Final Release", OffsetDays: 42),
                }),

            new ProjectTemplate(
                Id:          "website-project",
                Name:        "Website Project",
                Description: "Quy trinh thiet ke, phat trien, kiem thu va ban giao website.",
                Icon:        "language",
                IconBg:      "bg-primary/10",
                IconColor:   "text-primary",
                Recommended: false,
                Tags:        new() { "Website", "UI/UX", "QA", "Launch" },
                Columns: new()
                {
                    new("Backlog",     "backlog"),
                    new("To Do",       "active"),
                    new("In Progress", "active"),
                    new("Review",      "active"),
                    new("Testing",     "active"),
                    new("Done",        "done"),
                },
                SprintCount: 2,
                Milestones: new()
                {
                    new("Design Sign-off", OffsetDays: 10),
                    new("UAT",             OffsetDays: 24),
                    new("Go Live",         OffsetDays: 30, UseEndDate: true),
                }),

            new ProjectTemplate(
                Id:          "mobile-app",
                Name:        "Mobile App Project",
                Description: "Theo doi vong doi phat trien ung dung mobile tu prototype den release.",
                Icon:        "phone_iphone",
                IconBg:      "bg-secondary/10",
                IconColor:   "text-secondary",
                Recommended: false,
                Tags:        new() { "Mobile", "Sprint", "Testing", "Release" },
                Columns: new()
                {
                    new("Backlog",     "backlog"),
                    new("To Do",       "active"),
                    new("In Progress", "active"),
                    new("Review",      "active"),
                    new("Testing",     "active"),
                    new("Done",        "done"),
                },
                SprintCount: 3,
                Milestones: new()
                {
                    new("Prototype", OffsetDays: 14),
                    new("Beta Test", OffsetDays: 35),
                    new("Store Release", OffsetDays: 45, UseEndDate: true),
                }),

            new ProjectTemplate(
                Id:          "marketing-campaign",
                Name:        "Marketing Campaign",
                Description: "Lap ke hoach, san xuat noi dung, trien khai va do luong chien dich marketing.",
                Icon:        "campaign",
                IconBg:      "bg-tertiary/10",
                IconColor:   "text-tertiary",
                Recommended: false,
                Tags:        new() { "Campaign", "Content", "Approval", "Report" },
                Columns: new()
                {
                    new("Ideas",       "backlog"),
                    new("Planning",    "active"),
                    new("Production",  "active"),
                    new("Approval",    "active"),
                    new("Scheduled",   "active"),
                    new("Done",        "done"),
                },
                SprintCount: 0,
                Milestones: new()
                {
                    new("Campaign Brief", OffsetDays: 7),
                    new("Launch",         OffsetDays: 21),
                    new("Final Report",   OffsetDays: 35, UseEndDate: true),
                }),

            new ProjectTemplate(
                Id:          "research",
                Name:        "Nghiên cứu khoa học",
                Description: "Theo dõi tiến trình nghiên cứu từ ý tưởng đến báo cáo. Phù hợp cho luận văn, đề tài khoa học.",
                Icon:        "science",
                IconBg:      "bg-secondary/10",
                IconColor:   "text-secondary",
                Recommended: false,
                Tags:        new() { "Không sprint", "3 Milestone", "Báo cáo" },
                Columns: new()
                {
                    new("Ý tưởng",         "backlog"),
                    new("Đang nghiên cứu", "active"),
                    new("Viết báo cáo",    "active"),
                    new("Hoàn thành",      "done"),
                },
                SprintCount: 0,
                Milestones: new()
                {
                    new("Nộp đề cương",          OffsetDays: 14),
                    new("Nộp báo cáo giữa kỳ",   OffsetDays: 28),
                    new("Bảo vệ",                OffsetDays: 42),
                }),

            new ProjectTemplate(
                Id:          "event-mgmt",
                Name:        "Quản lý sự kiện",
                Description: "Lên kế hoạch và theo dõi tổ chức sự kiện từ đầu đến khi kết thúc.",
                Icon:        "event",
                IconBg:      "bg-tertiary/10",
                IconColor:   "text-tertiary",
                Recommended: false,
                Tags:        new() { "Không sprint", "3 Milestone", "Sự kiện" },
                Columns: new()
                {
                    new("Lên kế hoạch",  "backlog"),
                    new("Đang chuẩn bị", "active"),
                    new("Đang diễn ra",  "active"),
                    new("Kết thúc",      "done"),
                },
                SprintCount: 0,
                Milestones: new()
                {
                    new("Chốt địa điểm", OffsetDays: 7),
                    new("Mở đăng ký",    OffsetDays: 14),
                    new("Ngày sự kiện",  OffsetDays: 30, UseEndDate: true),
                }),

            new ProjectTemplate(
                Id:          "blank",
                Name:        "Trống",
                Description: "Bắt đầu với bảng Kanban cơ bản. Tự thêm sprint, milestone và cột theo nhu cầu.",
                Icon:        "add_box",
                IconBg:      "bg-surface-container-high",
                IconColor:   "text-on-surface-variant",
                Recommended: false,
                Tags:        new() { "Linh hoạt", "Tự cấu hình" },
                Columns: new()
                {
                    new("Backlog",     "backlog"),
                    new("To Do",       "active"),
                    new("In Progress", "active"),
                    new("Review",      "active"),
                    new("Testing",     "active"),
                    new("Done",        "done"),
                },
                SprintCount: 0,
                Milestones: new()),
        };

        public static ProjectTemplate? Get(string id) =>
            All.FirstOrDefault(t => t.Id == id);
    }
}
