# FixProjectFormat.ps1
# PowerShell equivalent of the C# Program.cs

# ==================== Utils Functions ====================

function Ensure-UpgradeAssistantInstalled {
    Write-Host "Checking for upgrade-assistant installation..." -ForegroundColor Cyan

    # Check if upgrade-assistant command exists
    $upgradeAssistant = Get-Command upgrade-assistant -ErrorAction SilentlyContinue

    if ($upgradeAssistant) {
        Write-Host "upgrade-assistant is already installed`n" -ForegroundColor Green
        return $true
    }

    Write-Host "upgrade-assistant not found. Installing..." -ForegroundColor Yellow

    try {
        # Install upgrade-assistant using dotnet tool
        Write-Host "Running: dotnet tool install -g upgrade-assistant" -ForegroundColor Cyan
        $installProcess = Start-Process -FilePath "dotnet" `
            -ArgumentList "tool", "install", "-g", "upgrade-assistant" `
            -NoNewWindow `
            -Wait `
            -PassThru

        if ($installProcess.ExitCode -eq 0) {
            Write-Host "upgrade-assistant installed successfully`n" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "Failed to install upgrade-assistant (exit code: $($installProcess.ExitCode))" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "Error installing upgrade-assistant: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Get-TargetFrameworks {
    param(
        [string]$file,
        [string]$tagName = "TargetFrameworkVersion"
    )

    Write-Host "  Reading target frameworks from file..." -ForegroundColor Cyan
    $lines = Get-Content -Path $file
    $frameworks = $lines | ForEach-Object { $_.Trim() } `
        | Where-Object { $_.StartsWith("<$tagName>") } `
        | ForEach-Object {
            $parts = $_ -split '[<>]'
            $parts[2]
        } `
        | Select-Object -Unique

    Write-Host "  Found $($frameworks.Count) framework(s): $($frameworks -join ', ')" -ForegroundColor Green
    return @($frameworks)
}

function Invoke-UpgradeFileFormat {
    param(
        [string]$projectFile
    )

    try {
        Write-Host "  Running upgrade-assistant in separate window..." -ForegroundColor Cyan 

        # Run upgrade-assistant in a new window
        $process = Start-Process -FilePath "upgrade-assistant" `
            -ArgumentList "upgrade", "`"$projectFile`"", "--operation", "InPlace", "--targetFramework", "netstandard2.0", "--non-interactive" `
            -PassThru

        # Wait for the process with a timeout check (5 minutes max)
        $timeoutSeconds = 300
        $checkIntervalMs = 500
        $elapsed = 0

        while (!$process.HasExited -and $elapsed -lt ($timeoutSeconds * 1000)) {
            Start-Sleep -Milliseconds $checkIntervalMs
            $elapsed += $checkIntervalMs

            # Refresh process info
            try {
                $process.Refresh()
            }
            catch {
                # Process might have exited, break the loop
                break
            }
        }

        # Check if we timed out or completed
        if (!$process.HasExited) {
            Write-Host "Timeout!" -ForegroundColor Red
            Write-Host "  Process timed out after $timeoutSeconds seconds" -ForegroundColor Red
            $process.Kill()
            return $false
        }

        # Wait a moment to ensure all child processes are cleaned up
        Start-Sleep -Milliseconds 500

        Write-Host "  Upgrade completed successfully" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed!" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Invoke-FixBuildEvent {
    param(
        [string]$projectFile,
        [string]$eventType  # "Pre" or "Post"
    )

    $eventTag = "${eventType}BuildEvent"
    $targetName = "${eventType}Build"
    $targetTiming = if ($eventType -eq "Pre") { "BeforeTargets" } else { "AfterTargets" }

    Write-Host "  Checking for ${eventTag}..." -ForegroundColor Cyan
    try {
        $lines = @(Get-Content -Path $projectFile)

        $startIndex = -1
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Contains("<${eventTag}>")) {
                $startIndex = $i
                break
            }
        }

        if ($startIndex -lt 0) {
            Write-Host "  No ${eventTag} found, skipping" -ForegroundColor Yellow
            return $true
        }

        Write-Host "  ${eventTag} found, fixing format..." -ForegroundColor Cyan

        $endIndex = -1
        for ($i = $startIndex; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Contains("</${eventTag}>")) {
                $endIndex = $i
                break
            }
        }

        if ($endIndex -lt $startIndex) {
            Write-Host "  Error: Invalid ${eventTag} structure" -ForegroundColor Red
            return $false
        }

        # Extract the build event commands
        $events = @()
        $openTagLength = $eventTag.Length + 2  # Length of "<EventTag>"

        # Handle content extraction based on whether it's single-line or multi-line
        if ($startIndex -eq $endIndex) {
            # Single-line case: <EventTag>content</EventTag>
            $line = $lines[$startIndex]
            $openTagIndex = $line.IndexOf("<${eventTag}>")
            $closeTagIndex = $line.IndexOf("</${eventTag}>")
            if ($openTagIndex -ge 0 -and $closeTagIndex -gt $openTagIndex) {
                $content = $line.Substring($openTagIndex + $openTagLength, $closeTagIndex - $openTagIndex - $openTagLength).Trim()
                if ($content.Length -gt 0) {
                    $events += $content.Replace('"', '&quot;')
                }
            }
        }
        else {
            # Multi-line case
            # Extract content from the opening tag line (if any after the tag)
            $openLine = $lines[$startIndex]
            $openTagIndex = $openLine.IndexOf("<${eventTag}>")
            if ($openTagIndex -ge 0) {
                $contentAfterTag = $openLine.Substring($openTagIndex + $openTagLength).Trim()
                if ($contentAfterTag.Length -gt 0) {
                    $events += $contentAfterTag.Replace('"', '&quot;')
                }
            }

            # Extract content from lines between opening and closing tags
            for ($i = $startIndex + 1; $i -lt $endIndex; $i++) {
                $content = $lines[$i].Trim()
                if ($content.Length -gt 0) {
                    $events += $content.Replace('"', '&quot;')
                }
            }

            # Extract content from the closing tag line (if any before the tag)
            $closeLine = $lines[$endIndex]
            $closeTagIndex = $closeLine.IndexOf("</${eventTag}>")
            if ($closeTagIndex -gt 0) {
                $contentBeforeTag = $closeLine.Substring(0, $closeTagIndex).Trim()
                if ($contentBeforeTag.Length -gt 0) {
                    $events += $contentBeforeTag.Replace('"', '&quot;')
                }
            }
        }

        $aggregatedEvents = $events -join '&#xD;&#xA;'

        $replacementLines = @(
            "  <Target Name=`"$targetName`" $targetTiming=`"${eventTag}`">",
            "    <Exec Command=`"$aggregatedEvents`" />",
            "  </Target>"
        )

        Write-Host "  Replacing ${eventTag} with correct format..." -ForegroundColor Cyan

        # Find the PropertyGroup or PropertyItemGroup that contains the build event
        $propertyGroupEndIndex = -1
        for ($i = $endIndex + 1; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Trim() -match '</PropertyGroup>|</PropertyItemGroup>') {
                $propertyGroupEndIndex = $i
                break
            }
        }

        $newLines = @()

        # Add all lines up to startIndex
        for ($i = 0; $i -lt $startIndex; $i++) {
            $newLines += $lines[$i]
        }

        # Skip the build event section (from startIndex to endIndex inclusive)
        # Continue adding lines after build event until we close the PropertyGroup
        for ($i = $endIndex + 1; $i -le $propertyGroupEndIndex; $i++) {
            $newLines += $lines[$i]
        }

        # Add the Target element (now outside PropertyGroup)
        $newLines += $replacementLines

        # Add remaining lines
        for ($i = $propertyGroupEndIndex + 1; $i -lt $lines.Count; $i++) {
            $newLines += $lines[$i]
        }

        Set-Content -Path $projectFile -Value $newLines
        Write-Host "  ${eventTag} fixed successfully" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Invoke-FixPostBuild {
    param(
        [string]$projectFile
    )

    return Invoke-FixBuildEvent -projectFile $projectFile -eventType "Post"
}

function Invoke-FixPreBuild {
    param(
        [string]$projectFile
    )

    return Invoke-FixBuildEvent -projectFile $projectFile -eventType "Pre"
}

function Invoke-FixTargetFramework {
    param(
        [string]$projectFile,
        [string]$version
    )

    Write-Host "  Restoring target framework to: $version" -ForegroundColor Cyan
    try {
        if ($version.StartsWith("v")) {
            $version = $version.Replace("v", "net").Replace(".", "")
            Write-Host "  Converted version format to: $version" -ForegroundColor Cyan
        }

        $lines = @(Get-Content -Path $projectFile)

        $found = $false
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Contains("<TargetFramework>")) {
                $lines[$i] = "<TargetFramework>$version</TargetFramework>"
                $found = $true
            }
        }

        if ($found) {
            Set-Content -Path $projectFile -Value $lines
            Write-Host "  Target framework updated successfully" -ForegroundColor Green
        } else {
            Write-Host "  Warning: No TargetFramework tag found" -ForegroundColor Yellow
        }
        return $true
    }
    catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Invoke-RemoveAssemblyInfo {
    param(
        [string]$projectFile
    )

    Write-Host "  Removing AssemblyInfo.cs files and GenerateAssemblyInfo tag..." -ForegroundColor Cyan
    try {
        # Get the project directory
        $projectDir = Split-Path -Parent $projectFile

        # Find and delete all AssemblyInfo.cs files in the project directory
        $assemblyInfoFiles = @(Get-ChildItem -Path $projectDir -Filter "AssemblyInfo.cs" -Recurse | Select-Object -ExpandProperty FullName)

        if ($assemblyInfoFiles.Count -gt 0) {
            Write-Host "  Found $($assemblyInfoFiles.Count) AssemblyInfo.cs file(s)" -ForegroundColor Cyan
            foreach ($file in $assemblyInfoFiles) {
                Remove-Item -Path $file -Force
                Write-Host "  Deleted: $file" -ForegroundColor Green
            }
        } else {
            Write-Host "  No AssemblyInfo.cs files found" -ForegroundColor Yellow
        }

        # Remove GenerateAssemblyInfo tag from project file
        $lines = @(Get-Content -Path $projectFile)
        $newLines = @()
        $removed = $false

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Contains("<GenerateAssemblyInfo>")) {
                $removed = $true
                Write-Host "  Removed GenerateAssemblyInfo tag" -ForegroundColor Green
                continue
            }
            $newLines += $lines[$i]
        }

        if ($removed) {
            Set-Content -Path $projectFile -Value $newLines
        } else {
            Write-Host "  No GenerateAssemblyInfo tag found in project file" -ForegroundColor Yellow
        }

        return $true
    }
    catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ==================== Main Program ====================

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "Starting Project Format Upgrade Process" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# ***** Ensure upgrade-assistant is installed *****
if (!(Ensure-UpgradeAssistantInstalled)) {
    Write-Host "`nCannot proceed without upgrade-assistant. Please install it manually using:" -ForegroundColor Red
    Write-Host "dotnet tool install -g upgrade-assistant" -ForegroundColor Yellow
    exit 1
}

# ***** Upgrade the project files *****
Write-Host "Searching for .csproj files..." -ForegroundColor Yellow
$projects = @(Get-ChildItem -Path ".\" -Filter "*.csproj" -Recurse | Select-Object -ExpandProperty FullName)
Write-Host "Found $($projects.Count) project(s) to process`n" -ForegroundColor Yellow

$projectNumber = 1
foreach ($project in $projects) {
    Write-Host "`n========================================" -ForegroundColor Magenta
    Write-Host "Processing project $projectNumber of $($projects.Count)" -ForegroundColor Yellow
    Write-Host "Project: $project" -ForegroundColor Yellow
    Write-Host "========================================`n" -ForegroundColor Magenta

    # Get original target framework
    Write-Host "Step 1: Getting original target framework..." -ForegroundColor Yellow
    $originalFrameworks = Get-TargetFrameworks -file $project

    if ($originalFrameworks.Count -gt 0) {
        $frameworksList = $originalFrameworks -join ", "
        Write-Host "Original frameworks: $frameworksList`n" -ForegroundColor Yellow
    } else {
        Write-Host "No original frameworks found`n" -ForegroundColor Yellow
    }

    # Upgrade the project file format
    Write-Host "Step 2: Upgrading project file format..." -ForegroundColor Yellow
    Invoke-UpgradeFileFormat -projectFile $project | Out-Null
    Write-Host ""

    # Fix the pre-build and post-build processes
    Write-Host "Step 3: Fixing build events..." -ForegroundColor Yellow
    Invoke-FixPreBuild -projectFile $project | Out-Null
    Invoke-FixPostBuild -projectFile $project | Out-Null
    Write-Host ""

    # Fix targetFramework
    Write-Host "Step 4: Fixing target framework..." -ForegroundColor Yellow
    if ($originalFrameworks.Count -gt 0) {
        # Get the first framework - handle both array and string cases
        $firstFramework = if ($originalFrameworks -is [Array]) { $originalFrameworks[0] } else { $originalFrameworks }
        Invoke-FixTargetFramework -projectFile $project -version $firstFramework | Out-Null
    } else {
        Write-Host "  Skipping target framework fix (no original framework found)" -ForegroundColor Yellow
    }
    Write-Host ""

    # Remove AssemblyInfo.cs files and GenerateAssemblyInfo tag
    Write-Host "Step 5: Removing AssemblyInfo.cs files and GenerateAssemblyInfo tag..." -ForegroundColor Yellow
    Invoke-RemoveAssemblyInfo -projectFile $project | Out-Null

    $projectNumber++
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "Upgrade Process Completed!" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "`nRun 'dotnet build' to check if your solution is now compiling correctly with dotnet." -ForegroundColor Green
