$folderPath = "./packages"
$extension = "*.nupkg"

dotnet pack src/framework/Framework/Framework.csproj -c Release -o $folderPath

$files = Get-ChildItem -Path $folderPath -Filter $extension

foreach ($file in $files) {
  dotnet nuget push $file.FullName --source "local" 
}

Remove-Item -Path $folderPath -Recurse