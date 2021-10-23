# Check for admin access
if (!((New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator))) {
    Write-Host "This script must be run with administrative privileges." -ForegroundColor Red
    exit 1
}

# Get current directory
$currentDir = (Get-Location).Path

# Delete platform-tools-latest-windows.zip if the file exists
if (Test-Path "platform-tools-latest-windows.zip") {
    Write-Host "Deleting platform-tools-latest-windows.zip"
    Remove-Item "platform-tools-latest-windows.zip" -Force
}

# Delete platform-tools directory if it exists 
if (Test-Path "platform-tools") {
    # Write-Host "Deleting platform-tools directory"
    Write-Host "Cleaning up old files..."
    Remove-Item "platform-tools" -Force -Recurse
}

# Download the latest version of platform-tools for Windows to platform-tools-latest-windows.zip
Write-Host "Downloading platform-tools for Windows"
$url = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip"
$dest = Join-Path -Path $currentDir -ChildPath "platform-tools-latest-windows.zip"
Start-BitsTransfer -Source $url -Destination $dest

Write-Host "Decompressing..."
# Decompress the file
[Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
[System.IO.Compression.ZipFile]::ExtractToDirectory($dest, "$currentDir")

$sdkPath = "C:\Program Files\Android\android-sdk"
$platformTools = "$sdkPath\platform-tools"

# Move contents of the unzipped folder to $platformTools
if((Test-Path $sdkPath) -eq $false) {
    Write-Host "Creating $sdkPath... " -NoNewline
    # Create directory
    mkdir $sdkPath
    
    <code class="language-powershell">$ACL = Get-ACL -Path $sdkPath
    # Add everyone to the ACL with read and write permissions recursively
    $AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "Allow")
    $ACL.SetAccessRule($AccessRule)

    Write-Host "done" -ForegroundColor Green
}

Write-Host "Moving platform-tools to $sdkPath... " -NoNewline
Move-Item -Path "platform-tools" -Destination $sdkPath -Force
Write-Host "done" -ForegroundColor Green

# Add platform-tools to PATH
$_ENV = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH).path
if ( ($_ENV).split(";").contains($platformTools) ) {
    Write-Host "Android SDK path already added. Skipping..."
} else {
    Write-Host "Adding Android SDK path to PATH... " -NoNewline
    $oldpath = $_ENV
    $newpath = "$oldpath;$platformTools"
    # Remove trailing semicolon if present
    if ($newpath.EndsWith(";")) {
        $newpath = $newpath.Substring(0, $newpath.Length - 1)
    }
    $newpath = $newpath.Replace(";;", ";")
    # Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH -Value $newpath
    
    $PATH = [Environment]::GetEnvironmentVariable("PATH")
    $PATH_Machine = [Environment]::GetEnvironmentVariable("PATH", "Machine")
    $PATH_New = "$PATH;$platformTools"
    $PATH_Machine_New = "$PATH_Machine;$platformTools"

    # Remove trailing semicolon from $PATH_New and $PATH_Machine_New
    if ($PATH_New.EndsWith(";")) {
        $PATH_New = $PATH_New.Substring(0, $PATH_New.Length - 1)
    }
    if ($PATH_Machine_New.EndsWith(";")) {
        $PATH_Machine_New = $PATH_Machine_New.Substring(0, $PATH_Machine_New.Length - 1)
    }

    # Replace ;; with ; in $PATH_New and $PATH_Machine_New
    $PATH_New = $PATH_New.Replace(";;", ";")
    $PATH_Machine_New = $PATH_Machine_New.Replace(";;", ";")

    [Environment]::SetEnvironmentVariable("PATH", "$PATH_New")
    [Environment]::SetEnvironmentVariable("PATH", "$PATH_Machine_New", "Machine")
    Write-Host "done" -ForegroundColor Green
}

Write-Host "Setting permissions..." -NoNewline

Write-Host " done" -ForegroundColor Green

# Clean up
Write-Host "Cleaning up..."
Remove-Item $dest -Force

Write-Host "Done."