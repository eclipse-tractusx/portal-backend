# Check dependencies

Dependencies are checked by the [Eclipse Dash License Tool](https://github.com/eclipse/dash-licenses) with a GitHub workflow (dependencies.yaml).

This workflow uses the executable jar in the download directory.

In order to update the executable jar run the following command from the root directory:

    curl -L --output ./scripts/download/org.eclipse.dash.licenses-1.0.2.jar 'https://repo.eclipse.org/service/local/artifact/maven/redirect?r=dash-licenses&g=org.eclipse.dash&a=org.eclipse.dash.licenses&v=1.0.2'
