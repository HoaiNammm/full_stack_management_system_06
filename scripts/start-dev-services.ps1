$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$connectionPassword = "thehoang123!"
$env:DOTNET_ROLL_FORWARD = "Major"

function Start-ServiceProcess {
    param(
        [string]$Name,
        [string]$Project,
        [string]$Database
    )

    $command = @"
`$env:DOTNET_ROLL_FORWARD = "Major"
`$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=$Database;User Id=sa;Password=$connectionPassword;TrustServerCertificate=True"
dotnet run --project "$Project" --launch-profile http
"@

    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
    Start-Process -FilePath powershell.exe `
        -ArgumentList "-NoProfile", "-EncodedCommand", $encoded `
        -WorkingDirectory $root `
        -WindowStyle Hidden `
        -PassThru
}

Start-ServiceProcess `
    -Name "NotifyService" `
    -Project (Join-Path $root "src\NotifyService.Api\NotifyService.Api.csproj") `
    -Database "NotifyDB"

Start-ServiceProcess `
    -Name "ProjectService" `
    -Project (Join-Path $root "src\ProjectService\ProjectService\ProjectService.csproj") `
    -Database "ProjectDB"

Start-ServiceProcess `
    -Name "TaskService" `
    -Project (Join-Path $root "src\TaskService\TaskService\TaskService.csproj") `
    -Database "TaskDB"
