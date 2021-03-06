﻿$repositoryLocation = "c:\source\silverback\nuget"
$cacheLocation = Join-Path $env:USERPROFILE ".nuget/packages"
[bool]$clearCache = $FALSE

foreach ($arg in $args)
{
	if ($arg -eq "-c")
	{
		$clearCache = $TRUE
	}
}

$buildConfiguration = "Debug"

$sources =
    ("Silverback.Core", "..\silverback-core\src\Silverback.Core\bin\$buildConfiguration"),
    ("Silverback.Core.EntityFrameworkCore", "..\silverback-core\src\Silverback.Core.EntityFrameworkCore\bin\$buildConfiguration"),
    ("Silverback.Core.Rx", "..\silverback-core\src\Silverback.Core.Rx\bin\$buildConfiguration"),
	("Silverback.Integration", "..\silverback-integration\src\Silverback.Integration\bin\$buildConfiguration"),
    ("Silverback.Integration.EntityFrameworkCore", "..\silverback-integration\src\Silverback.Integration.EntityFrameworkCore\bin\$buildConfiguration"),
    ("Silverback.Integration.FileSystem", "..\silverback-testing\src\Silverback.Integration.FileSystem\bin\$buildConfiguration"),
    ("Silverback.Integration.Kafka", "..\silverback-integration-kafka\src\Silverback.Integration.Kafka\bin\$buildConfiguration"),
    ("Silverback.Integration.Configuration", "..\silverback-integration-configuration\src\Silverback.Integration.Configuration\bin\$buildConfiguration")

function Check-Location()
{
    [string]$currentLocation = Get-Location

    if ($currentLocation -ne $repositoryLocation)
    {
        Write-Host "This script is supposed to run in $repositoryLocation!" -ForegroundColor Red
        $choice = Read-Host "Wanna swith to $repositoryLocation ? [Y/n]"
        if ($choice -ne "n")
        {
            cd $repositoryLocation
        }
    }
}

function Delete-All()
{
    Write-Host "Deleting everything in target folder..." -ForegroundColor Yellow -NoNewline

    Get-ChildItem -exclude Update.ps1 |
    Remove-Item -Force -Recurse |
    Out-Null

    Write-Host "OK" -ForegroundColor Green
}

function Copy-All()
{
    Write-Host "Copying packages..." -ForegroundColor Yellow

    foreach ($source in $sources)
    {
        $name = $source[0]
        $sourcePath = Join-Path $source[1] "*.nupkg"

        Copy-Package $name $sourcePath

		if ($clearCache)
		{
        	Delete-Cache $name
		}
    }

    Write-Host "`nAvailable packages:" -ForegroundColor Yellow
    
    # Show-Files $destination
    Show-Summary $destination
}

function Copy-Package([string]$name, [string]$sourcePath)
{
    Write-Host "`t$name..." -NoNewline

    $destination = $repositoryLocation

    Ensure-Folder-Exists $destination

    Copy-Item $sourcePath -Destination $destination -Recurse

    Write-Host "OK" -ForegroundColor Green
}

function Ensure-Folder-Exists([string]$path)
{
    if (!(Test-Path $path))
    {
        New-Item -ItemType Directory -Force -Path $path | Out-Null
    }
}

function Show-Files([string]$path)
{
    Get-ChildItem $path -Recurse -Filter *.nupkg | 
    Foreach-Object {
        Write-Host "`t" -NoNewline
        Write-Host $_.Name.Substring(0, $_.Name.Length - ".nupkg".Length)
    }
}

function Show-Summary([string]$path)
{
    $hashtable = @{}

    $files = Get-ChildItem $path -Recurse -Filter *.nupkg

    foreach ($file in $files)
    {
        Add-Version $file $hashtable
    }

    foreach ($source in $sources)
    {
        $key = $source[0]
        
        Write-Host "`t[" -NoNewline
        Write-Host "$($hashtable[$key].major).$($hashtable[$key].minor).$($hashtable[$key].revision).$($hashtable[$key].patch)" -NoNewline -ForegroundColor Green
        Write-Host "] $($key)"
    }
}

function Add-Version([string]$path, [hashtable]$hashtable)
{
    $name = ""
    $major = 0
    $minor = 0
    $revision = 0
    $patch = 0
    $versionTokenIndex = 0

    foreach ($token in $path.Split("."))
    {
        if ($token -match "^\d+$")
        {
            if ($versionTokenIndex -eq 0)
            {
                $major = [int]$token
            }
            elseif ($versionTokenIndex -eq 1)
            {
                $minor = [int]$token
            }
            elseif ($versionTokenIndex -eq 2)
            {
                $revision = [int]$token
            }
            elseif ($versionTokenIndex -eq 3)
            {
                $patch = [int]$token
            }

            $versionTokenIndex++;
        }
        else
        {
            if ($versionTokenIndex -gt 0)
            {
                break;
            }

            if ($name.Length -gt 0)
            {
                $name += "."
            }

            $name += $token
        }
    }

    if ($hashtable.ContainsKey($name))
    {
        $previousVersion = $hashtable[$name];

        if ($previousVersion.major -gt $major -Or 
            ($previousVersion.major -eq $major -And $previousVersion.minor -gt $minor) -Or
            ($previousVersion.major -eq $major -And $previousVersion.minor -eq $minor -And $previousVersion.revision -gt $revision) -Or
            ($previousVersion.major -eq $major -And $previousVersion.minor -eq $minor -And $previousVersion.revision -eq $revision -And $previousVersion.patch -gt $patch))
        {
            return;
        }
    }

    $hashtable[$name] = @{ 
        major = $major
        minor = $minor
        revision = $revision
        patch = $patch
    }
}

function Delete-Cache([string]$name)
{
    Write-Host "`tClearing cache..." -NoNewline

    $cache = Join-Path $cacheLocation $name

    Get-ChildItem $cache -Recurse |
    Remove-Item -Force -Recurse |
    Out-Null

    Write-Host "OK" -ForegroundColor Green
}

Check-Location
Delete-All
Copy-All
