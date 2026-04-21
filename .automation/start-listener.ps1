# Starts the SmartEstate automation listener in a visible window
# Run this once at the start of each dev session
# Keep the window open while working

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

$wtAvailable = Get-Command wt -ErrorAction SilentlyContinue
if ($wtAvailable) {
    Start-Process wt -ArgumentList "new-tab", "--title", "SmartEstate Listener", "powershell.exe", "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "$scriptPath\listener.ps1"
} else {
    Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "$scriptPath\listener.ps1"
}

Write-Host "Listener started in a new window." -ForegroundColor Green
