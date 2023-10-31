#!/bin/bash

# Check if the correct number of arguments are provided
if [ "$#" -ne 2 ]; then
  echo "Usage: $0 <name> <version>"
  exit 1
fi

# Assign the arguments to variables
name="$1"
version="$2"

folderPath="./packages"
extensions="*.nupkg"

# Initialize a global arrays to store data
updated_directories=()
projects_to_update=()
already_updated_projects=()

# Define the version update functions
update_major() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$1+=1; $2=0; $3=0; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_minor() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$2+=1; $3=0; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_patch() {
  local version="$1"
  local updated_version="$(echo "$version" | awk -F. '{$3+=1; print}' | tr ' ' '.')"
  echo "$updated_version"
}

update_pre() {
  local version="$1"
  local current_suffix=$(grep '<VersionSuffix>' "$props_file" | sed -n 's/.*<VersionSuffix>\(.*\)<\/VersionSuffix>.*/\1/p' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//' | tr -d '\n')
  local current_suffix_version="${current_suffix%%"."*}"
  if [ "$current_suffix_version" != "$version" ]; then
    updated_suffix="$version"
  else
    if [[ "$current_suffix" == "alpha" || "$current_suffix" == "beta" ]]; then
      updated_suffix="${current_suffix}.1"
    else
      numeric_part=$(echo "$current_suffix" | sed 's/[^0-9]//g')
      new_numeric_part=$((numeric_part + 1))
      updated_suffix="${version}.${new_numeric_part}"
    fi
  fi
  echo "$updated_suffix"
}

# Function to search and update .csproj files recursively
update_csproj_files_recursive() {
  local updated_name="$1"
  local updated_version="$2"

  for dir in ./src/Framework/*/; do
    if [ -d "$dir" ]; then
      if [[ " ${updated_directories[*]} " != *"$dir"* ]]; then
        csproj_files=("$dir"*.csproj)
        for project_file in "${csproj_files[@]}"; do
          if grep -q "$updated_name" "$project_file"; then
            directory_name=$(basename "$dir")
            if [[ ! " ${already_updated_projects[*]} " == *"$directory_name"* ]]; then
              update_version "$dir" "$directory_name" "$updated_version"
              if [[ ! " ${projects_to_update[*]} " == *"$directory_name"* ]]; then
                projects_to_update+=("$directory_name")
              fi
              if [[ ! " ${already_updated_projects[*]} " == *"$directory_name"* ]]; then
                already_updated_projects+=("$directory_name")
              fi
            fi
          fi
        done
      fi
    fi
  done

  # Recursively update projects that depend on the updated projects
  for project_name in "${projects_to_update[@]}"; do
    # Only update projects if they haven't been updated before
    if [[ ! " ${already_updated_projects[*]} " == *"$project_name"* ]]; then
      update_csproj_files_recursive "$project_name" "$updated_version"
    fi
  done
}

update_version(){
  local directory="$1"
  local updated_name="$2"
  
  local props_file=$directory"Directory.Build.props"
  # Check if the Directory.Builds.props file exists
  if [ -f "$props_file" ]; then
    # Extract the current version from the XML file
    current_version=$(awk -F'[<>]' '/<VersionPrefix>/{print $3}' "$props_file")
    current_suffix=$(awk -F'[<>]' '/<VersionSuffix>/{print $3}' "$props_file")

    case "$version" in
      major)
        updated_version=$(update_major "$current_version")
        updated_suffix="$current_suffix"
        ;;
      minor)
        updated_version=$(update_minor "$current_version")
        updated_suffix="$current_suffix"
        ;;
      patch)
        updated_version=$(update_patch "$current_version")
        updated_suffix="$current_suffix"
        ;;
      alpha|beta|pre|rc)
        updated_version="$current_version"
        updated_suffix=$(update_pre "$version")
        ;;
      *)
        echo "Invalid version argument. Valid options: major, minor, patch, alpha, beta, pre, rc"
        exit 1
        ;;
    esac

    # Update the VersionPrefix and VersionSuffix in the file
    awk -v new_version="$updated_version" -v new_suffix="$updated_suffix" '/<VersionPrefix>/{gsub(/<VersionPrefix>[^<]+<\/VersionPrefix>/, "<VersionPrefix>" new_version "</VersionPrefix>")}/<VersionSuffix>/{gsub(/<VersionSuffix>[^<]+<\/VersionSuffix>/, "<VersionSuffix>" new_suffix "</VersionSuffix>")}1' "$props_file" > temp && mv temp "$props_file"
    echo "Updated version in $props_file to $updated_version $updated_suffix"

    updated_directories+=($directory)
    # Update the depending solutions
    update_csproj_files_recursive "$updated_name"
  else
    echo "Directory.Builds.props file not found in $directory$updated_name"
  fi
}

# Function to iterate over directories in the Framework directory and update the project version
iterate_directories() {
  local updated_name="$1"
  
  for dir in ./src/framework/*/; do
    if [ -d "$dir" ]; then
      if [[ $dir == "./src/framework/$updated_name/" ]]; then
        update_version "$dir" "$updated_name"
        if [[ ! " ${projects_to_update[*]} " == *"$updated_name"* ]]; then
          projects_to_update+=("$updated_name")
        fi
        if [[ ! " ${already_updated_projects[*]} " == *"$updated_name"* ]]; then
          already_updated_projects+=("$updated_name")
        fi
      fi
    fi
  done

  # Update all projects that depend on the updated projects recursively
  for project_name in "${projects_to_update[@]}"; do
    # Only update projects if they haven't been updated before
    if [[ ! " ${already_updated_projects[*]} " == *"$project_name"* ]]; then
      update_csproj_files_recursive "$project_name" "$updated_version"
    fi
  done
}

# Call the iterate_directories function to start the script
iterate_directories "$name"

for proj in "${already_updated_projects[@]}"; do
  echo "pack $proj"
  dotnet pack src/framework/$proj/$proj.csproj -c Release -o "$folderPath"
done

dotnet nuget push "$folderPath/$extensions" --source "local"

rm -r "$folderPath"
