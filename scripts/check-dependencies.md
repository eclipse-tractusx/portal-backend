# Check dependencies

Dependencies are checked by the [Eclipse Dash License Tool](https://github.com/eclipse/dash-licenses) with a GitHub workflow (dependencies.yaml).

This workflow uses the executable jar in the download directory.

In order to update the executable jar run the following command from the root directory:

    curl -L --output ./scripts/download/org.eclipse.dash.licenses-0.0.1-SNAPSHOT.jar 'https://repo.eclipse.org/service/local/artifact/maven/redirect?r=dash-licenses&g=org.eclipse.dash&a=org.eclipse.dash.licenses&v=0.0.1-SNAPSHOT'
