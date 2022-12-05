# Changelog

New features, fixed bugs, known defects and other noteworthy changes to each release of the Catena-X Portal Backend.

### Unreleased
n/a

## 0.9.0

### Change
* Services Endpoints
   * added service types support the filtering and tagging of services (one service can have multiple service types)
   * enabled service sorting
   * enable service updates
   * merged /subscribe and /subscribe-consent endpoints

### Feature
* App Release Process
   * enabled Get & Post company SalesManager assigned to an app offer
   * document upload enpoint enable jpeg and png for app image upload
* Registration Service
   * registration document deletion endpoint released
   * registration data publishin endpoint document types reduced

### Technical Support
* DB Auditing for app instances enabled

### Bugfix
* Fixed new company user user invite mixups which deleted user accounts with similar or same name/email

## 0.8.0

### Change
* Email Template
   * refactored email templates for registration and administration services (style and component update)
   * added CompanyName to all "Invite" email templates
* IdP Administration
   * refactored put and post endpoints by merging the user create and update endpoint 
   * added the displayName as optional parameter inside the idp create endpoint
* Service & App Subscribe endpoints enhanced by adding the submission of the consent agreement details inside the request body

### Feature
* Notification Service ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
   * enabled pagination for get/notification endpoint
   * created count-detail endpoint to retrieve additional metadata (unread, unread per type, etc.)
   * added notification type info to support the filtering and tagging of notification areas (info, action, offer)
   * enabled notification sorting

### Technical Support
* Enabled "Debug" Logging mechanism for 3rd party interfaces by implementing a "debug" config level inside the service config files

### Bugfix
* Create new user account email template changed; wrong email template was fetched
* Add user role endpoint got refactored; multi subscription offers did result into an exception and have been fixed by a interim workaround to assign the role to all specific offer app-instances which the company as assigned for

## 0.7.0

### Change
* Connector Registration - managed/own connector registration endpoint refactoring. Change request body content and enhancing backend business logic to validate the respective host and provider. Additionally technical user auth is now supported for managed connectors. ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* App role assignment - assign and update app roles assigned to an user. Role assignment (add, delete) is managed by the PUT api/administration/user/app/{appId}/roles endpoint

### Feature
* Notifications
   * Service Subscription: Enable prodiver notification creation and email trigger (if applicable) after subscription was triggered by a customer.
   * Service Subscription Activation: Enable customer notification creation and email trigger (if applicable) after service subscription was triggered by the service provider.
* Company Role / User Role connection
   * Creation of new db tables and connections between company roles, roles collections (new) and user roles ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
   * Backend business logic enhancement of user invite endpoints by adding user-role-collection restriction of assignable user-roles based on the company role 
* App Release Management Process
   * released create and delete app roles endpoints
   * enhanced GET app release status/details endpoint by addding consent agreement, documents and sales manager
   * enhanced POST app release status/details endpoint by adding sales manager value

### Technical Support
n/a

### Bugfix
* App instance/tenant management fixed to ensure correct company client/tenant displayed inside "my business app"

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
