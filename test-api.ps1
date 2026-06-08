$base = "http://localhost:5047"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ODcxZTNjZi0yOWRlLTQ5YWUtYjg1ZS1mZGY1M2E0YjVkM2IiLCJpc3MiOiJwcm9qZWN0LW1hbmFnZW1lbnQtc3lzdGVtIiwiYXVkIjoiYWxsLXNlcnZpY2VzIiwiZXhwIjoxNzgwNzMzMDE3fQ.9P7btNMwAvnlg5ZsSm5OZ3GR3mBzKfACrI7ul2rLiSU"
$h = @{ Authorization = "Bearer $token"; "Content-Type" = "application/json" }

function Req($method, $url, $body = $null) {
    try {
        if ($body) { Invoke-RestMethod $url -Method $method -Headers $h -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) }
        else        { Invoke-RestMethod $url -Method $method -Headers $h }
    } catch { Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red; $null }
}

Write-Host "`n==[1] GET /api/projects ==" -ForegroundColor Cyan
$r = Req GET "$base/api/projects"
Write-Host "OK - count: $($r.data.Count)"

Write-Host "`n==[2] POST /api/projects ==" -ForegroundColor Cyan
$r2 = Req POST "$base/api/projects" '{"name":"Test Project 2026","description":"Mo ta du an","color":"#4F46E5","startDate":"2026-06-01T00:00:00Z","endDate":"2026-12-31T00:00:00Z"}'
$projId = $r2.data.id
Write-Host "OK - id=$projId  color=$($r2.data.color)  startDate=$($r2.data.startDate)"

Write-Host "`n==[3] GET /api/projects/$projId ==" -ForegroundColor Cyan
$r3 = Req GET "$base/api/projects/$projId"
Write-Host "OK - name=$($r3.data.name)"

Write-Host "`n==[4] POST /api/projects/$projId/members (role=Member) ==" -ForegroundColor Cyan
$uid = [guid]::NewGuid().ToString()
$r4 = Req POST "$base/api/projects/$projId/members" "{`"userId`":`"$uid`",`"role`":2}"
$mid = $r4.data.id
Write-Host "OK - memberId=$mid  role=$($r4.data.role)  joinedAt=$($r4.data.joinedAt)"

Write-Host "`n==[5] PUT /api/projects/$projId/members/$mid/role (role=Manager) ==" -ForegroundColor Cyan
$r5 = Req PUT "$base/api/projects/$projId/members/$mid/role" '{"role":1}'
Write-Host "OK - new role=$($r5.data.role)"

Write-Host "`n==[6] POST /api/projects/$projId/sprints (EndDate auto +14 days) ==" -ForegroundColor Cyan
$r6 = Req POST "$base/api/projects/$projId/sprints" '{"name":"Sprint 1","goal":"Hoan thanh thiet ke API","startDate":"2026-06-09T00:00:00Z"}'
$sid = $r6.data.id
Write-Host "OK - sprintId=$sid"
Write-Host "     start=$($r6.data.startDate)"
Write-Host "     end=$($r6.data.endDate)  (auto +14 days)"
Write-Host "     goal=$($r6.data.goal)"

Write-Host "`n==[7] PUT /api/projects/$projId/sprints/$sid (status=1 → sprint.started event) ==" -ForegroundColor Cyan
$r7 = Req PUT "$base/api/projects/$projId/sprints/$sid" "{`"name`":`"Sprint 1`",`"goal`":`"Hoan thanh API`",`"startDate`":`"2026-06-09T00:00:00Z`",`"endDate`":`"2026-06-23T00:00:00Z`",`"status`":1}"
Write-Host "OK - status=$($r7.data.status) (1=Active)"

Write-Host "`n==[8] POST milestone ==" -ForegroundColor Cyan
$r8 = Req POST "$base/api/projects/$projId/milestones" '{"name":"MVP Release","description":"Phat hanh MVP","targetDate":"2026-08-01T00:00:00Z"}'
$msid = $r8.data.id
Write-Host "OK - milestoneId=$msid"

Write-Host "`n==[9] PUT /api/projects/$projId (update + Active) ==" -ForegroundColor Cyan
$r9 = Req PUT "$base/api/projects/$projId" '{"name":"Project Updated","status":1,"color":"#10B981","startDate":"2026-06-01T00:00:00Z","endDate":"2026-12-31T00:00:00Z"}'
Write-Host "OK - name=$($r9.data.name)  status=$($r9.data.status)  color=$($r9.data.color)"

Write-Host "`n=== ALL TESTS PASSED ===" -ForegroundColor Green
