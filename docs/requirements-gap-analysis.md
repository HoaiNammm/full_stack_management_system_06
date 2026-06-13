# Project Management System - Requirements Gap Analysis

## Source Inputs

- Excel file: `d:\Vớ vẩn\Nhóm 4.xlsx`
- User requirement: complete project management and task assignment website using the existing FE/BE source.

## Current Architecture

- Frontend: `src/frontend`
  - Vue 3, Vue Router, Pinia, Axios, TailwindCSS, Vite.
- Backend:
  - `ProjectService`: projects, members, sprints, milestones, templates.
  - `TaskService`: tasks, subtasks, kanban columns, time logs.
  - `NotifyService.Api`: auth/users, comments, notifications, activity/audit/system logs.

## Excel Summary

The Excel file separates the project into 3 service groups:

- Group 1: Project & Member
  - Dashboard, project CRUD, project detail, project members, role assignment, members overview.
- Group 2: Task & Kanban
  - Kanban board, task list, task form, sprint management, task detail.
- Group 3: Comment & Notify
  - Comment panel, mentions, notification page/dropdown, activity log.
- Integration
  - Shared layout, light/dark mode, Tailwind style, N2 uses N1 project/member data, N3 uses N1/N2 data.

The new requirement supersedes the old mock-data note: use real APIs whenever available.

## API Coverage

### Auth & Users

Available:

- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `POST /api/users/register`
- `PUT /api/users/{id}`
- `PATCH /api/users/{id}/status`
- `PATCH /api/users/{id}/role`
- `DELETE /api/users/{id}`

Missing or needs verification:

- Change password endpoint.
- Refresh-token endpoint.
- Explicit logout/revoke token endpoint.
- Profile update endpoint for current user, unless `PUT /api/users/{id}` is accepted.

### Project & Member

Available:

- `GET /api/templates`
- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`
- `GET /api/projects/{projectId}/members`
- `POST /api/projects/{projectId}/members`
- `PUT /api/projects/{projectId}/members/{memberId}/role`
- `DELETE /api/projects/{projectId}/members/{memberId}`
- `GET /api/projects/{projectId}/sprints`
- `POST /api/projects/{projectId}/sprints`
- `PUT /api/projects/{projectId}/sprints/{id}`
- `DELETE /api/projects/{projectId}/sprints/{id}`
- `GET /api/projects/{projectId}/milestones`
- `POST /api/projects/{projectId}/milestones`
- `PUT /api/projects/{projectId}/milestones/{id}`
- `DELETE /api/projects/{projectId}/milestones/{id}`

Missing or needs verification:

- Project progress endpoint/calculation.
- Project comments endpoint, unless comments are only task-scoped.
- Project activity endpoint exists in NotifyService, but FE route needs integration.
- Template-generated sample tasks may need cross-service creation.

### Task & Kanban

Available:

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `PUT /api/tasks/{id}/column`
- `DELETE /api/tasks/{id}`
- `GET /api/kanban-columns`
- `POST /api/kanban-columns`
- `PUT /api/kanban-columns/{id}`
- `DELETE /api/kanban-columns/{id}`
- `GET /api/subtasks/task/{taskId}`
- `GET /api/subtasks/{id}`
- `POST /api/subtasks`
- `PUT /api/subtasks/{id}`
- `DELETE /api/subtasks/{id}`
- `GET /api/timelogs/task/{taskId}`
- `POST /api/timelogs`
- `DELETE /api/timelogs/{id}`

Missing or needs verification:

- Dedicated task status endpoint besides column move.
- Task assignment history display endpoint.
- Review and Testing default kanban columns must exist or be created per project.

### Comments & Notifications

Available:

- `GET /api/comments/task/{taskId}`
- `POST /api/comments`
- `PUT /api/comments/{id}`
- `DELETE /api/comments/{id}`
- `GET /api/notifications`
- `GET /api/notifications/my`
- `GET /api/notifications/unread-count`
- `PUT /api/notifications/{id}/read`
- `PUT /api/notifications/{userNotificationId}/read`
- `PUT /api/notifications/read-all`
- `GET /api/activity-logs/task/{taskId}`
- `GET /api/activity-logs/project/{projectId}`
- `GET /api/audit-logs`
- `GET /api/system-logs`

Missing or needs verification:

- Project-level comments.
- Comment replies if the current model does not support parent comments.
- Mention search endpoint, unless FE filters project members locally.
- Realtime notification transport such as SignalR/WebSocket.

## Frontend Status

Fixed:

- Resolved FE merge conflicts in:
  - `src/frontend/package.json`
  - `src/frontend/src/main.js`
  - `src/frontend/vite.config.js`
  - `src/frontend/index.html`
  - `src/frontend/README.md`
- Added API default base URLs in `src/frontend/src/services/api.js`.
- Added default API export for older service files.
- Installed FE dependencies.
- Verified production build succeeds.
- Verified Vite dev server responds with HTTP 200 at `http://127.0.0.1:5173`.

Current design preview:

- `docs/fe-ui-design-preview.html`

## Recommended Implementation Order

1. Stabilize authentication and session handling.
   - Login/register using real NotifyService API.
   - Token storage, route guard, expired token handling.
   - Profile page using real user API.

2. Stabilize layout and navigation.
   - Fixed sidebar and modern header.
   - Responsive behavior.
   - Dark mode foundation.

3. Dashboard using real APIs.
   - Projects from ProjectService.
   - Users from NotifyService.
   - Tasks from TaskService.
   - Notifications from NotifyService.
   - Charts computed from real API responses.

4. Project management.
   - Project list search/filter/sort.
   - Project create/edit/delete.
   - Project detail tabs: overview, members, timeline, kanban, comments, activity.

5. Members and roles.
   - Add/remove members.
   - Assign Owner, Project Manager, Developer, Tester, Viewer.
   - Hide/disable UI actions by role.

6. Task and Kanban.
   - Default columns: Backlog, To Do, In Progress, Review, Testing, Done.
   - Drag and drop updates via TaskService.
   - Task detail modal/page.
   - Assignment, deadline, priority.

7. Timeline and Gantt.
   - Use project dates, sprints, milestones, task deadlines.
   - Drag/drop date changes only if backend update endpoints support it.

8. Comment and notification.
   - Task comments from NotifyService.
   - Mention members using project member list.
   - Notification dropdown/page.
   - Realtime only after backend transport is confirmed or added.

## Backend Endpoints To Add

Recommended additions:

- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `PUT /api/auth/me`
- `PUT /api/auth/change-password`
- `GET /api/dashboard/summary` or keep FE aggregation if preferred.
- `GET /api/projects/{projectId}/activity` or map FE to existing NotifyService activity route.
- `GET /api/projects/{projectId}/comments`
- `POST /api/projects/{projectId}/comments`
- `GET /api/projects/{projectId}/timeline`
- `PUT /api/tasks/{id}/schedule`
- `PUT /api/tasks/{id}/assignee`
- `GET /api/projects/{projectId}/members/search?q=`
- Realtime notifications via SignalR or WebSocket.
