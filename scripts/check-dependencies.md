# Check dependencies

Dependencies are checked by the [Eclipse Dash License Tool](https://github.com/eclipse/dash-licenses) with a GitHub workflow (dependencies.yaml).

This workflow uses the executable jar in the download directory.

In order to update the executable jar run the following command from the root directory:

    curl -L --output ./scripts/download/org.eclipse.dash.licenses-1.1.1.jar 'https://repo.eclipse.org/service/local/artifact/maven/redirect?r=dash-licenses&g=org.eclipse.dash&a=org.eclipse.dash.licenses&v=LATEST'

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2021-2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/portal-backend
