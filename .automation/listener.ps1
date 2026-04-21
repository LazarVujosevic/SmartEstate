# SmartEstate Claude Code Session Launcher
# Runs as HTTP listener on port 9876
# Receives triggers from n8n and opens new Windows Terminal sessions with Claude

$port = 9876
$projectRoot = "D:\Projects\SmartEstate"
$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add("http://localhost:$port/")
$listener.Start()

Write-Host "SmartEstate Automation Listener started on port $port" -ForegroundColor Green
Write-Host "Waiting for triggers from n8n..." -ForegroundColor Cyan

function Open-ClaudeSession {
    param (
        [string]$Role,
        [string]$Prompt
    )

    $mdFile = switch ($Role) {
        "backend"  { ".claude\backend-dev.md" }
        "frontend" { ".claude\frontend-dev.md" }
        "leaddev"  { ".claude\lead-dev.md" }
        default    { "CLAUDE.md" }
    }

    # Escape quotes in prompt for PowerShell
    $escapedPrompt = $Prompt -replace '"', '\"'

    # Build the initial message for Claude
    $initialMessage = "Read CLAUDE.md and $mdFile in full before starting. Then proceed with the following task: $escapedPrompt"

    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Opening $Role session..." -ForegroundColor Yellow

    # Open new Windows Terminal tab with Claude
    # Uses 'wt' (Windows Terminal) - opens a new tab
    $wtAvailable = Get-Command wt -ErrorAction SilentlyContinue
    if ($wtAvailable) {
        Start-Process wt -ArgumentList "new-tab", "--title", "Claude - $Role", "powershell.exe", "-NoExit", "-Command", @"
Set-Location '$projectRoot'
Write-Host 'SmartEstate - $Role Session' -ForegroundColor Cyan
Write-Host 'Task: $escapedPrompt' -ForegroundColor Yellow
Write-Host ''
claude --dangerously-skip-permissions
"@
    } else {
        # Fallback to plain PowerShell window
        Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Set-Location '$projectRoot'
Write-Host 'SmartEstate - $Role Session' -ForegroundColor Cyan
Write-Host 'Task: $escapedPrompt' -ForegroundColor Yellow
Write-Host ''
claude --dangerously-skip-permissions
"@
    }
}

while ($listener.IsListening) {
    $context = $listener.GetContext()
    $request = $context.Request
    $response = $context.Response

    try {
        $body = [System.IO.StreamReader]::new($request.InputStream).ReadToEnd()
        $data = $body | ConvertFrom-Json

        $role   = $data.role
        $prompt = $data.prompt

        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Received trigger: role=$role" -ForegroundColor Cyan

        Open-ClaudeSession -Role $role -Prompt $prompt

        $responseBody = '{"status":"ok","message":"Session launched"}'
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($responseBody)
        $response.ContentType = "application/json"
        $response.StatusCode = 200
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
    }
    catch {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Error: $_" -ForegroundColor Red
        $responseBody = '{"status":"error"}'
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($responseBody)
        $response.StatusCode = 500
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
    }
    finally {
        $response.Close()
    }
}
