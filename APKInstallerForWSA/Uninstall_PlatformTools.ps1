# Check for admin access
if (!((New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator))) {
    Write-Host "This script must be run with administrative privileges." -ForegroundColor Red
    exit 1
}

# $currentDir = (Get-Location).Path
$sdkPath = "C:\Program Files\Android\android-sdk"
$platformTools = "$sdkPath\platform-tools"

# Remove platform-tools from PATH
$_ENV = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH).path
if ( ($_ENV).split(";").contains($platformTools) ) {
    Write-Host "Removing SDK from PATH..." -NoNewline
    $oldpath = $_ENV
    $newpath = $oldpath
    $newpath = $newpath.replace($platformTools, "")
    $newpath = $newpath.replace(";;", ";")
    # Remove trailing semicolon
    if ($newpath.EndsWith(";")) {
        $newpath = $newpath.Substring(0, $newpath.Length - 1)
    }
    #Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH -Value $newpath

    $PATH = [Environment]::GetEnvironmentVariable("PATH")
    $PATH_Machine = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    # Remove $platformTools from $PATH
    $PATH = $PATH.Replace($platformTools, "")
    $PATH_Machine = $PATH_Machine.Replace($platformTools, "")

    # Remove trailing semicolon from $PATH and $PATH_Machine
    if ($PATH.EndsWith(";")) {
        $PATH = $PATH.Substring(0, $PATH.Length - 1)
    }
    if ($PATH_Machine.EndsWith(";")) {
        $PATH_Machine = $PATH_Machine.Substring(0, $PATH_Machine.Length - 1)
    }

    # Replace ;; with ; in $PATH and $PATH_Machine
    $PATH = $PATH.Replace(";;", ";")
    $PATH_Machine = $PATH_Machine.Replace(";;", ";")

    [Environment]::SetEnvironmentVariable("PATH", "$PATH")
    [Environment]::SetEnvironmentVariable("PATH", "$PATH_Machine", "Machine")
    Write-Host " done" -ForegroundColor Green
} else {
    Write-Host "SDK is not in PATH. Continuing..." -ForegroundColor Yellow
}

# Remove platform-tools directory
if (Test-Path $platformTools) {
    Write-Host "Removing SDK from $platformTools..." -NoNewline
    Remove-Item $platformTools -Recurse -Force
    Write-Host " done" -ForegroundColor Green
} else {
    Write-Host "$platformTools does not exist. Nothing to remove." -ForegroundColor Yellow
}
Write-Host "Done."