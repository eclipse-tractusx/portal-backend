#!/bin/bash

folderPath="./packages"
extension="*.nupkg"

dotnet pack src/framework/Framework.Web/Framework.Web.csproj -c Release -o "$folderPath"

files=($(find "$folderPath" -name "$extension"))

for file in "${files[@]}"; do
    dotnet nuget push "$file" --source "local"
done

rm -r "$folderPath"