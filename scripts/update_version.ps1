param (
    [string]$name,
    [string]$version
)

# Initialize an array to store updated directories
$updatedDirectories = @()

# Define the version update functions (same as in Bash)

# Function to search and update .csproj files
Function Update-CsProjFiles {
    param (
        [string]$updatedName
    )

    $projectRef = "Framework.$updatedName.csproj"

    # Iterate over directories in the Framework directory
    Get-ChildItem -Path "./src/Framework/*" -Directory | ForEach-Object {
        $dir = $_
        # Check if the directory is already in updatedDirectories
        if (-not $updatedDirectories.Contains($dir.FullName)) {
            # Search for .csproj files in the current directory
            $csProjFiles = Get-ChildItem -Path "$dir/*.csproj"
            foreach ($projectFile in $csProjFiles) {
                $content = Get-Content $projectFile.FullName
                if ($content -match [regex]::Escape($projectRef)) {
                    $directoryName = [System.IO.Path]::GetFileName($dir.FullName)
                    Update-Version $dir.FullName $directoryName
                }
            }
        }
    }
}

# Function to update the version
Function Update-Version {
    param (
        [string]$directory,
        [string]$updatedName
    )

    $propsFile = Join-Path $directory "Directory.Build.props"
    # Check if the Directory.Build.props file exists
    if (Test-Path $propsFile) {
        # Extract the current version from the XML file
        $xml = [xml](Get-Content $propsFile)
        $currentVersion = $xml.SelectNodes("/PropertyGroup/VersionPrefix").InnerText
        $currentSuffix = $xml.SelectNodes("/PropertyGroup/VersionSuffix").InnerText

        switch ($version) {
            "major" {
                $updatedVersion = Update-Major $currentVersion
                $updatedSuffix = $currentSuffix
            }
            "minor" {
                $updatedVersion = Update-Minor $currentVersion
                $updatedSuffix = $currentSuffix
            }
            "patch" {
                $updatedVersion = Update-Patch $currentVersion
                $updatedSuffix = $currentSuffix
            }
            "alpha" {
                $updatedVersion = $currentVersion
                $updatedSuffix = Update-AlphaBeta $version $currentSuffix
            }
            "beta" {
                $updatedVersion = $currentVersion
                $updatedSuffix = Update-AlphaBeta $version $currentSuffix
            }
            default {
                Write-Host "Invalid version argument. Valid options: major, minor, patch, alpha, beta"
                exit 1
            }
        }

        # Update the VersionPrefix and VersionSuffix in the XML file
        $xml.SelectSingleNode("/PropertyGroup/VersionPrefix").InnerText = $updatedVersion
        $xml.SelectSingleNode("/PropertyGroup/VersionSuffix").InnerText = $updatedSuffix
        $xml.Save($propsFile)

        Write-Host "Updated version in $propsFile to $updatedVersion $updatedSuffix"
        $updatedDirectories += $directory
        # Update the depending solutions
        Update-CsProjFiles $updatedName
    }
    else {
        Write-Host "Directory.Builds.props file not found in $($directory)$updatedName"
    }
}

# Function to iterate over directories in the Framework directory
Function Iterate-Directories {
    param (
        [string]$updatedName
    )

    # Iterate over directories in the Framework directory
    Get-ChildItem -Path "./src/Framework/*" -Directory | ForEach-Object {
        $dir = $_
        # Check if a directory with the specified name exists
        if ($dir.Name -eq "Framework.$updatedName") {
            Update-Version $dir.FullName $updatedName
        }
    }
}

# Call the Iterate-Directories function to start the script
Iterate-Directories $name
