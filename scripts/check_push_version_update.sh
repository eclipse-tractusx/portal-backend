#!/bin/bash

# Check if the correct number of arguments are provided
if [ "$#" -ne 2 ]; then
  echo "Usage: $0 <baseBranch> <currentBranch>"
  exit 1
fi

# Assign the arguments to variables
baseBranch="$1"
currentBranch="$2"

# Initialize a global arrays to store data
projects_to_update=()
already_checked_projects=()
version_update_needed=()

# get the directory.build files to check the updated versions
IFS=$'\n' read -d '' -ra updatedVersions < <(git diff HEAD~1 --name-only | grep 'Directory.Build.props')

check_version_update(){
  local directory="$1"
  local updated_name="$2"
  local props_file="src/framework/"$(basename "$directory")"/Directory.Build.props"
  # Check if the Directory.Builds.props file exists
  if [[ " ${updatedVersions[@]} " =~ " $props_file " ]]; then
    already_checked_projects+=($directory)
    # Update the depending solutions
    update_csproj_files_recursive "$updated_name"
  else
    version_update_needed+=($directory)
    already_checked_projects+=($directory)
    update_csproj_files_recursive "$updated_name"
  fi
}

# Function to search and check the version update recursively
update_csproj_files_recursive() {
  local updated_name="$1"

  for dir in ./src/Framework/*/; do
    if [ -d "$dir" ]; then
      csproj_files=("$dir"*.csproj)
      for project_file in "${csproj_files[@]}"; do
        if grep -q "$updated_name" "$project_file"; then
          project=$(basename "$dir")
          if [[ ! " ${already_checked_projects[*]} " == *"$project"* ]]; then
            check_version_update "$dir" "$project"
            if [[ ! " ${already_checked_projects[*]} " == *"$project"* ]]; then
              already_checked_projects+=("$project")
            fi
          fi
        fi
      done
    fi
  done

  # Recursively update projects that depend on the updated projects
  for project_name in "${projects_to_update[@]}"; do
    # Only update projects if they haven't been updated before
    if [[ ! " ${already_checked_projects[*]} " == *"$project_name"* ]]; then
      update_csproj_files_recursive "$project_name"
    fi
  done
}

# iterate over directories in the Framework directory and check if the version was updated
iterate_directories() {
  local updated_name="$1"
  
  for dir in ./src/framework/*/; do
    if [ -d "$dir" ]; then
      if [[ $dir == "./src/framework/$updated_name/" ]]; then
        check_version_update "$dir" "$updated_name"
        if [[ ! " ${projects_to_update[*]} " == *"$updated_name"* ]]; then
          projects_to_update+=("$updated_name")
        fi
        if [[ ! " ${already_checked_projects[*]} " == *"$updated_name"* ]]; then
          already_checked_projects+=("$updated_name")
        fi
      fi
    fi
  done

  # Update all projects that depend on the updated projects recursively
  for project_name in "${projects_to_update[@]}"; do
    # Only update projects if they haven't been updated before
    if [[ ! " ${already_checked_projects[*]} " == *"$project_name"* ]]; then
      update_csproj_files_recursive "$project_name"
    fi
  done
}

# find out which directories were changed with the last push
IFS=$'\n' read -d '' -ra changedPackages < <(git diff HEAD~1 --name-only | xargs dirname | sort | uniq | grep '^src/framework/.*')

if [ ! -z "${changedPackages[*]}" ]; then
  for dir in "${changedPackages[@]}"; do
    package="$(basename "$dir")"
    iterate_directories "$package"
  done
fi

# return all packages that still need a version update
for dir in "${version_update_needed[@]}"; do
  echo "$dir"
done 