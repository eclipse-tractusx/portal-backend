# Update nuget packages

Currently each project of the framework directory is build and provided as an nuget package.

To be able to build and test changes locally we recommend the following setup:

## Local nuget directory

1. Create a directory to store the locally build nuget packages

```bash
mkdir ~/packages

```

2. Add the directory to the nuget config

```bash
dotnet nuget add source ~/packages --name local

```

## Update version for packages

After the changes within the source code are done you can execute the following script from the root of the project to update the packages:

To update the version for all packages:

```bash
./scripts/update_all_framework_versions.sh <version>
```

To only update the packages that were changed, including the dependent packages you can run:

```bash
./scripts/update_framework_version.sh <location> <version>
```

When passing 'local' as the location the script will update all packages where local changes are available and not yet have been committed.
When passing 'build' as the location the script will update all packages where changes were made in the last commit.
For version there are the following options:
 - major
 - minor
 - patch
 - alpha
 - beta
 - rc
 - pre

Depending on the version the Directory.Build.props of the project will be updated.
For major, minor and patch the version will be incremented by 1.
For alpha, beta, rc, pre the Suffix will be set, if the version is already in the suffix it will be incremented by one.

Example:

|current value|used version|  new value  |
|-------------|------------|-------------|
|    1.0.0    |    major   |    2.0.0    |
|    1.1.0    |    major   |    2.0.0    |
| 1.1.0.alpha |    major   |    2.0.0    |
|    1.0.0    |    minor   |    1.1.0    |
|    1.1.0    |    minor   |    1.2.0    |
|    1.0.1    |    minor   |    1.1.0    |
| 1.1.0.alpha |    minor   |    1.2.0    |
|    1.0.0    |    patch   |    1.0.1    |
|    1.1.0    |    patch   |    1.1.1    |
|    1.0.1    |    minor   |    1.0.2    |
| 1.1.0.alpha |    minor   |    1.1.1    |
|    1.1.0    |    alpha   | 1.1.0.alpha |
|1.1.0.alpha.1|    alpha   |1.1.0.alpha.2|
| 1.1.0.beta  |    alpha   | 1.1.0.alpha |

## Build and push nuget packages

To build and push the changed nuget packages make sure to first update the package version, you should use one of the script mentioned above to make sure that all dependent packages are updated as well.

After all packages are updated to the wanted version you can run the following command from the root of the project to build and push the nuget packages:

To push the updated packages to the local source

```bash
./scripts/pack_and_push_packages.sh local
```

To update the version of a specific package:

```bash
./scripts/pack_and_push_packages.sh nuget <NUGET_API_KEY>
```

NUGET_API_KEY is the key generated on nuget.org. The last command should only be executed within the github action
