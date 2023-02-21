# Changelog

New features, fixed bugs, known defects and other noteworthy changes to each release of the Catena-X Portal Backend.

## 1.0.0-RC10

### Change
* Seeding: updated base data image

### Feature
n/a

### Technical Support
* readme files updated and example values added

### Bugfix
* Self Description encoding fixed to improve readablity of the json file
* Administration Service
  * decline registration endpoint enhanced by sending rejection message (if added) inside the email template to the respective company user
  * updated permissions for clearinghouse-self-description endpoints (controller: registration & connector)
  * TRIGGER_OVERRIDE_CLEARING_HOUSE step removed from SD checklist flow and added to clearinghouse process
  * Checklist Handler updated by adding missing process steps for manual process flow
* Seeding - fixed incorrect registration role name

## 1.0.0-RC9

### Change
* Seeding: updated base data image
* Autosetup functionality: autoset base url inside the new created keycloak client
* Administration Service
  * user management: user account creation email enabled for ownIdP and bulk user account creation
* App Service
  * app service enhanced by a new endpoint for the operator to get details of the app under review

### Feature
* Application Checklist Worker ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * Clearinghouse interface enhanced and further defined to enable full functionality with VC approval and SD creation
* Document endpoints reworked in the business logic regarding access permissions of users to documents; additionally two new endpoints release to view SD documents and operator endpoint for application linked documents ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)

### Technical Support
* Application Checklist Worker
  * unify service call error handling across the application checklist processes

### Bugfix
* Application Checklist Worker
  * bpdm data push converted from enum to unique identifier string value
  * checklist-worker fix checklist-processor failure on creation of subsequent process-steps
* Email content for app subscription activation fixed with user specific values and app name

## 1.0.0-RC8

### Change
* Registration Service: restructured endpoint GET api/registration/companyRoleAgreementData due to new db relationship for documents (see technical changes below) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* App Service:
  * restructured endpoint GET: /api/apps/appreleaseprocess/agreementData due to new db relationship for documents (see technical changes below) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * added mandatory validation on app roles loaded for PUT: /api/apps/appreleaseprocess/{appId}/submit

### Feature
* Administration Service - application worker: 
  * added retrigger functionality for application approval worker
  * added automated process for bpdm data pull and push
  * changed SD IF from synchron to async requests
  * removed administration/registration/application/{applicationId}/declineRequest endpoint ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Seeding data: added agreement_assigned_company_roles in the base image file
* Offer (app) attribute released "Privacy_Policy" (new db tables; db relation updates; app endpoints enhanced with new attribute)

### Technical Support
* DB structure: agreement_assigned_document table relation updated to 1:n instead of n:m
* Seeding process for static data added and unit tests released
* enable registration and administration service to use config from env vars
* temp fix for cve-2023-0286

### Bugfix
* Application approval checklist process handling fixed
* Email "App Activation" - company name and URL attribute fixed

## 1.0.0-RC7

### Change
n/a

### Feature
n/a

### Technical Support
* enable provisioning, appmarketplace and services service to use config from env vars

### Bugfix
* double creation of notifications for app activation fixed
* registration approval: removing the company user assigned roles for a specific client id while activating the company

## 1.0.0-RC6

### Change
* Apps & Service Services: auto set the releaseDate of an offer with the approval endpoint
* App Service:
  * POST endpoint to upload documents for services in currently under creation
  * Endpoint controller switch of /app/decline from apps to appReleaseProcess ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Checklist-Worker for Registration Service: updates implemented (full feature delivery still ongoing)

### Feature
n/a

### Technical Support
* enable migrations, portal maintenance and checklist-worker jobs as well as notification service to use config from env vars
* remove initdb container setup - replaced by configmap in portal helm chart

### Bugfix
n/a

## 1.0.0-RC5

### Change
* Identity Provider mappers "tenant" auto created for new identity provider created via administration service and registration service removed
* Identity provider mappers "username" and "organisation" added company idp registration endpoints
* Services Service - added document types and ids inside response of GET services/{serviceID}

### Feature
* Registration Service: add unique-identifiers to GET /CompanyWithAddress registration endpoint
* Administration Service
  * Further enhancements on the company application approval worker by including auto approval and sd factory jobs

### Technical Support
n/a

### Bugfix
* Welcome email "Join Now" button hyperlink added
* Username added inside the 'app subscription activation email'
* Switched long to short description attribute inside the GET services/active

## 1.0.0-RC4

### Change
* Registration Service: new GET endpoint released for /legalEntityAddress/{bpn} with reworked and enhanced response body to support legal entity address and unique identifier handling
* Administration Service: 
  * POST & PUT endpoints for service provider url merged into PUT
  * user creation endpoint - role attribute mandatorily set - minimum one role is mandatory
* DB offer attribute "thumbnailUrl" removed and impacted endpoints updated ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)

### Feature
* Administration Service: Gaia-X Compliance release preparation with new wallet endpoint to create MIW for new CX members

### Technical Support
* Checklist worker enabled to support future application checklist process planned for R#3.0 with automatic process trigger based on configured application status sets
* IdP creation logic - identity provider mapper "tenant" decommissioned

### Bugfix
* n/a

## 1.0.0-RC3

### Change
* n/a

### Feature
* Registration service: GET and POST endpoints for applications enhanced by company unique identifier
* Marketplace service: new endpoint to fetch app documents got released

### Technical Support
* n/a

### Bugfix
* Marketplace service: GET /app/details updated to exclude app images from document response

## 1.0.0-RC2

### Change
* n/a

### Feature
* Registration service new endpoints for unique identifiers released (GET & POST)

### Technical Support
* Migration base image data load extended by country_assigned_unique_identifiers
* Migration base image data load updates for countries and documents

### Bugfix
* App Release endpoints for updating app details and validation for submitting apps fixed/changed
* Get /services/active response fix for shortDescription


## 1.0.0-RC1

### Change
* Service Provider Detail Endpoints - ID deleted from path url; information fetched from user token
* GET company application filters enabled
* App LeadPicture (GET /api/apps/{appId} & GET /api/apps/appreleaseprocess/{appId}/appStatus) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* App LeadImage/Thumbnail URL got exchanged from a document name to an document id to be able to fetch the image from the portal.documents table


### Feature
* App Service
  * enable filtering for app approval management function GET /inReview - (marketplace service; controller: appreleaseprocess)
  * app deactivation endpoint created to enable marketplace deactivations - (marketplace service; controller: apps)
* Registration Service
  * enhanced business logic of POST /submitregistration by locking application related documents
* Administration Service
  * enhanced endpoint for GET /registration/applications by adding applied company roles
  * enhanced endpoint for GET /companyDetailswithAddress by adding applied company roles, agreement consent status and invited users
* Service cutting features - Service Account creation logic got updated after db attribute enhancement (see technical support section) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * Administration Service POST /owncompany/serviceaccounts business logic updated to handle service_account_type and subscription_id
  * Administration Service GET /owncompany/serviceaccounts business logic and response body updated to handle service_account_type, subscription_id and  offer_name
  * Service Service POST/autosetup business logic updated to handle service_account_type and store subscription_id

### Technical Support
* Migration: Data Seeding for db enabled with initial base data files for all db tables ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat) The data seeding enables delta data load
* Db tables for unique identifier handling of companies added (portal.unique_identifiers; portal.country_assigned_identifiers; portal.company_identifiers)
* Db attribute enhancement for company_service_accounts ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Remove migration dev history by merging migration files to provide one initial release 1.0.0-RC1 db migration ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Remove companies.tax_id attribute from portal db, data load files & api response
* Email image urls of static images changed to new repo

### Bugfix
* Email template layout fixed for /nextsteps.html & /appprovider_subscription_request.html
* Email parameter update for /declineappsubscription template
* Registration service POST /application/{applicationId}/companyDetailsWithAddress address storing and overwrite logic fixed

## 0.10.0

### Change
* App Service
   * Get all provided apps: exclude apps with status "CREATED" and "IN REVIEW"
   * App subscription autosetup endpoint enhanced by email notification to the customer/requester
   * App subscription activation endpoint enhanced by email notification to the customer/requester
   * Add service endpoint request body enhancement by adding short and long description key values
* Administration Service
   * Updated endpoint for get application/documents to fetch all documents uploaded under the same application ID

### Feature
* Services Service
   * Implemented endpoint to enable service provider to submit service for review (incl. notification)
   * Implemented endpoint to enable operator/CX admin to decline a service release request with message (notification & email)
   * Implemented endpoint to enable operator/CX admin to approve a service release request (incl. notification)
* Administration Service
   * Added new endpoint to submit registration details of an company to BPDM gateway for BPN creation 
   * IdP creation: updated config of the new idp login flow from "First Login Flow" (keycloak default) to "Login without auto user creation" (new custom flow)
* App Service
   * Implemented endpoint to enable operator/CX admin to decline an app release request with message (notification & email)
   * Implemented endpoint to enable operator/CX admin to approve an app release request (incl. notification)

### Technical Support
* Portal to DAPS interface communication change to technical user / service account
* Migrations DB EF Core Enabling for provisioning service

### Bugfix
* Updated deletion logic of the Maintenance App Batch Delete Service by validating app assigned document foreign key relation before running the deletion of a "Inactive" document and ignore "Pending" documents
* Welcome email business logic updated. "Send email" logic was connected to SD and Wallet api call success; the relation got disconnected. Email will get send always as long as the application is put to the status "APPROVED"
* Notification Service: filter by notification_topic got added
* Registration Service: Application consent status storage logic got corrected to overwrite existing consent for given agreement
* Service activation notification logic update: only send notification to the requester company user with the role "IT Admin"
* App subscription/autosetup logic corrected

## 0.9.0

### Change
* Services Service
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
