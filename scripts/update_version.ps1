param(
    [string]$name,
    [string]$version
)

# Initialize an array to store updated directories
$updated_directories = @()

# Initialize an array to store projects that need to be updated
$projects_to_update = @()

# Initialize a hash table to store projects that have been already updated
$already_updated_projects = @{}

# Function to update the version
function Update-Version {
    param(
        [string]$currentVersion,
        [string]$newVersionType
    )
    
    $major = 0
    $minor = 0
    $patch = 0

    $currentVersionComponents = $currentVersion.Split('.')
    
    if ($currentVersionComponents.Length -ge 1) {
        $major = [int]$currentVersionComponents[0]
    }
    if ($currentVersionComponents.Length -ge 2) {
        $minor = [int]$currentVersionComponents[1]
    }
    if ($currentVersionComponents.Length -ge 3) {
        $patch = [int]$currentVersionComponents[2]
    }
    
    switch ($newVersionType) {
        "major" {
            $major++
            $minor = 0
            $patch = 0
        }
        "minor" {
            $minor++
            $patch = 0
        }
        "patch" {
            $patch++
        }
    }

    $updatedVersion = "$major.$minor.$patch"
    
    return $updatedVersion
}

# Function to update a project
function Update-Project {
    param(
        [string]$directory,
        [string]$updatedName,
        [string]$newVersion
    )

    $propsFile = Join-Path $directory "Directory.Build.props"

    if (Test-Path $propsFile -PathType Leaf) {
        [xml]$xml = Get-Content $propsFile

        $currentVersionPrefix = $xml.SelectSingleNode("//VersionPrefix").InnerText
        $currentVersionSuffix = $xml.SelectSingleNode("//VersionSuffix").InnerText

        switch ($version) {
            "major" {
                $updatedVersionPrefix = Update-Version $currentVersionPrefix "major"
                $updatedVersionSuffix = $currentVersionSuffix
            }
            "minor" {
                $updatedVersionPrefix = Update-Version $currentVersionPrefix "minor"
                $updatedVersionSuffix = $currentVersionSuffix
            }
            "patch" {
                $updatedVersionPrefix = Update-Version $currentVersionPrefix "patch"
                $updatedVersionSuffix = $currentVersionSuffix
            }
            "alpha", "beta" {
                $updatedVersionPrefix = $currentVersionPrefix
                $currentSuffixVersion = $currentVersionSuffix.Split('.')[0]

                if ($currentSuffixVersion -ne $version) {
                    $updatedVersionSuffix = $version
                }
                else {
                    if ($currentVersionSuffix -eq "alpha" -or $currentVersionSuffix -eq "beta") {
                        $updatedVersionSuffix = "$currentVersionSuffix.1"
                    }
                    else {
                        $numericPart = [int]($currentVersionSuffix -replace "[^0-9]")
                        $newNumericPart = $numericPart + 1
                        $updatedVersionSuffix = "${version}.${newNumericPart}"
                    }
                }
            }
            default {
                Write-Host "Invalid version argument. Valid options: major, minor, patch, alpha, beta"
                exit 1
            }
        }

        $xml.SelectSingleNode("//VersionPrefix").InnerText = $updatedVersionPrefix
        $xml.SelectSingleNode("//VersionSuffix").InnerText = $updatedVersionSuffix

        $xml.Save($propsFile)

        Write-Host "Updated version in $propsFile to $updatedVersionPrefix $updatedVersionSuffix"
    }
    else {
        Write-Host "Directory.Builds.props file not found in $directory\$updatedName"
    }
}

# Function to update .csproj files recursively
function Update-Csproj-Recursive {
    param(
        [string]$updatedName,
        [string]$updatedVersion
    )

    if ($already_updated_projects.ContainsKey($updatedName)) {
        return
    }

    $already_updated_projects[$updatedName] = $true

    # Iterate over directories in the Framework directory
    $frameworkDirs = Get-ChildItem -Path "./src/Framework/" -Directory
    foreach ($dir in $frameworkDirs) {
        # Search for .csproj files in the current directory
        $csprojFiles = Get-ChildItem -Path $dir.FullName -File -Filter "*.csproj"
        foreach ($projectFile in $csprojFiles) {
            $directoryName = [System.IO.Path]::GetFileNameWithoutExtension($projectFile.Name)

            if (Select-String -Pattern $updatedName -Path $projectFile.FullName) {
                # Only update the project if it has not been updated before
                if (-not $already_updated_projects.ContainsKey($directoryName)) {
                    Update-Project -directory $dir.FullName -updatedName $directoryName -newVersion $updatedVersion
                    $projects_to_update += $directoryName
                    $already_updated_projects[$directoryName] = $true
                }
            }
        }
    }

    # Recursively update projects that depend on the updated projects
    foreach ($projectName in $projects_to_update) {
        # Only update projects if they haven't been updated before
        if (-not $already_updated_projects.ContainsKey($projectName)) {
            Update-Csproj-Recursive -updatedName $projectName -updatedVersion $updatedVersion
        }
    }
}

# Function to iterate over directories in the Framework directory
function Iterate-Directories {
    param(
        [string]$updatedName
    )

    # Iterate over directories in the Framework directory
    $frameworkDirs = Get-ChildItem -Path "./src/Framework/" -Directory
    foreach ($dir in $frameworkDirs) {
        if ($dir.Name -eq "Framework.$updatedName") {
            $directoryName = [System.IO.Path]::GetFileNameWithoutExtension($dir.Name)
            Update-Project -directory $dir.FullName -updatedName $directoryName -newVersion $version
            $projects_to_update += $directoryName
            $already_updated_projects[$directoryName] = $true
        }
    }

    # Update all projects that depend on the updated projects recursively
    foreach ($projectName in $projects_to_update) {
        # Only update projects if they haven't been updated before
        if (-not $already_updated_projects.ContainsKey($projectName)) {
            Update-Csproj-Recursive -updatedName $projectName -updatedVersion $version
        }
    }
}

# Call the Iterate-Directories function to start the script
Iterate-Directories -updatedName $name
