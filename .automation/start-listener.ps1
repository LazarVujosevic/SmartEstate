# Starts the SmartEstate automation listener in a visible window
# Run this once at the start of each dev session
# Keep the window open while working

$listenerPath = "D:\Projects\SmartEstate\.automation\listener.ps1"

$wtAvailable = Get-Command wt -ErrorAction SilentlyContinue
if ($wtAvailable) {
    $wtArgs = "new-tab --title `"SmartEstate Listener`" -- powershell.exe -NoExit -ExecutionPolicy Bypass -File `"$listenerPath`""
    Start-Process wt -ArgumentList $wtArgs
} else {
    Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "`"$listenerPath`""
}

Write-Host "Listener started in a new window." -ForegroundColor Green
