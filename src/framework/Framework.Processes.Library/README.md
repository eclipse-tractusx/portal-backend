# Catena-X Portal Backend Framework Processes Library

The Catena-X Portal Backend Framework Processes Library provides some base models.

This content is produced and maintained by the Eclipse Tractus-X project.

* Project home: https://projects.eclipse.org/projects/automotive.tractusx

## Installation

dotnet add package Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library

## Usage

To use the package make sure to implement the interfaces of the repositories: `IRepositories`, `IProcessStepRepository<Process<TProcessTypeId, TProcessStepTypeId>, ProcessStep<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>`.
Provide an implementation for `IProcess`, `IProcessStep` & `IProcessStepStatus`.

An alternative option is to use the `Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete` package

## Source Code

The project maintains the following source code repositories in the GitHub organization https://github.com/eclipse-tractusx:

- https://github.com/eclipse-tractusx/portal-backend


## License

Distributed under the Apache 2.0 License.
