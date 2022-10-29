# Changelog

New features, fixed bugs, known defects and other noteworthy changes to each release of the Catena-X Portal Backend.

## 0.6.0

### Change
* Improvements App/Service Auto Setup logic
* API Endpoint GET owncompany/user: enable fuzzy search via email

### Feature
* Administration/User Service: Enables companies to invite, change and delete own users with bulk and single actions, as well as direct keycloak iam costumers as well as federated own company solutions
* Administration/Connector: Register own and managed connectors, including self-description creation & storage (Gaia-X)
   * Interface Connection to SD-Factory enabled
   * Discovery Service of connector endpoints integrated
   * API business logic updated to support connector registration for multiple companies ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Registration: Registration service got released to support the full company registration cycle including workflow management
* Marketplace: Marketplace services got released supporting the discovery of offers on the marketplace and enable subscription to those apps and services (incl. manual steps)

### Technical Support
* Database auditing: AuditId, DateLastChanged, AuditOperationId added inside the audit tables and removed from original table (if not needed)

### Bugfix
* Security findings

## 0.5.5

* Feature - Enhancements App/Service Marketplace Service (Agreement, Auto Setup, Release Process)
* Feature - Dataspace Discovery Service
* Enabler - Preparation for migration to eclipse-tractusx organisation
* Bugfix - Error Handling

## 0.5.4

* Feature - App Release Process Controller enhanced with additional endpoints to support the app release feature (Post app roles, Get app roles, submit app for release)
* Feature - Service Provider Marketplace Service
* Bugfix - Keycloak shared realm creation - technical user for realm management moved from master to company realm
* Enabler - Relocate Keycloak.Net and upgrade to .Net 6.0
* Enabler - Run images as non root user

## 0.5.3

* Feature - Service Provider Marketplace v1 microservice released (Get Services, Get Service Details, Post Services, Post Agreement, Get Agreement, etc.)
* Feature - App Release Process Controller enhanced with additional endpoints to support the app release feature with "Put App Documents"
* Update - Portal Db Refactoring by merging service and app table and recall them "offers". Additionally, al related app tabled have been renamed where suitable to "offer" instead of "app"

## 0.5.2

* Feature - Refactoring of portal db to enable multi app management
* Feature - Identity Provider Endpoints to switch IdPs for existing CX Members and move users to the IdP
* Feature - DB enhancements (User "DELETE" enum, creation of service tables, etc.)
* Feature - App Release Management PUT endpoint implementation for "CREATE App" and "App Details"
* Feature - Static Data endpoints implemented - GET use cases, language, company data
* Feature - DB Auditing released for app subscription, company user, company application, user assigned roles
