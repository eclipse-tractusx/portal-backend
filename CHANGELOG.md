# Changelog

New features, fixed bugs, known defects and other noteworthy changes to each release of the Catena-X Portal Backend.

## 2.0.0-RC2

### Change
* moved api paths from BPDM out of code into config / helm chart
* merged all migrations since 2.0.0-alpha into one 2.0.0-rc2

### Feature
* added DID to DID BPN resolver
* added new checklist steps

### Bugfix
* fixed company invite: changed invitation processStepType order and removed disposal of mimeMessage for mailing
* fixed mail not being set at new user invite

## 2.0.0-RC1

### Change
* **Backend Logic**
  * Save the error details of the clearinghouse service inside the portal db of application checklist/process worker
* **Apps Services**
  * updated backend logic of `PUT /api/apps/AppReleaseProcess/{appId}/submit` to allow the submission without defined/configured technical user profile
* **Administration Service**
  * remove obsolete endpoint `GET /api/user/app/{appId}/roles`
  * remove obsolete endpoint `PUT /api/user/app/{appId}/roles`
  * added connector url inside the response body of `GET /api/administration/Connectors`
  * added connector url inside the response body of `GET /api/administration/Connectors/managed`
  * added connector url inside the response body of `GET /api/administration/Connectors/{connectorID}`
* upgraded all services and jobs to .net 8
* upgraded nuget packages
* merged all migrations since v1.8.0-rc6 into one 2.0.0-alpha
* updated swagger (endpoint documentation, payload examples and allowed values)
* changed the CompanyInvitationData to class instead of record
* updated seeding:
  * removed service account sa-cl5-custodian-1
  * removed the following roles: BPDM Gate Read, BPDM Gate Read & Write, BPDM Partner Gate, BPDM Management, BPDM Pool
  * added the following roles: BPDM Sharing Admin, BPDM Sharing Input Manager, BPDM Sharing Input Consumer, BPDM Sharing Output Consumer, BPDM Pool Admin, BPDM Pool Consumer

### Feature
* **Certificate Management (Administration Service)**
  * released new endpoint to delete company owned company certificates `DELETE /api/administration/companydata/companyCertificate/document/{documentId}`
  * released new endpoint to view other companies certificates via the document ID `GET /api/administration/companydata/companyCertificates/documents/{documentId}`
  * released specific document endpoint to fetch owned company certificates by documentID `GET /api/administration/companydata/companyCertificates/{documentId}`
* **Registration Process Worker**
  * implemented new backend logic for the process step "IDENTITY_WALLET_CREATION" by separating the step logic (bpm credential creation separated and payload changed) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * added retrigger endpoint to restarted a failed dim wallet setup step
  * added postback endpoint to receive the did document and authentication information ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* **Agreement Status**
  * updated logic of POST and GET agreement endpoint (apps service) to only consider active agreements
  * updated logic of POST and GET agreement endpoint (services service) to only consider active agreements
  * enhanced response body payload by adding "mandatory" agreement flag inside the endpoint `GET /api/registration/companyRoleAgreementData`
* **Seeding Data updated**
  * new technical user profiles for BPDM services released inside the seeding files
* **Business Process Worker**
  * added new backend worker for invitations to run the invitation steps asynchronously
  * added mailing worker and moved all backend functions for sending emails into the worker
* Email Templates
  * Enabled email service for create user account under owned IdP as well as for migration of an user account from any IdP to a ownedIdP

### Technical Support
* adjusted the get_current_version script for nuget packages to only return the tag name
* introduced codeql scan
* removed veracode workflow
* removed unused deprecated packages
* improved workflows and documentation
* upgraded gh actions and change to pinned actions full length commit sha
* add dependabot.yml file

### Bugfix
* adjusted endpoint `GET api/administration/serviceaccount/owncompany/serviceaccounts` to filter for active service accounts by default
* fixed backend logic of the endpoint `POST /api/administration/companydata/companyCertificate` - document status is now automatically set tok "locked" with the document upload
* endpoint `POST /api/administration/connectors/discovery` was running on an empty response when calling the endpoint without a body (instead of an empty array). Backend behavior fixed to allow both calls
* corrected mail template that's send out after the network registration from 'CredentialRejected' to 'OspWelcomeMail'
* fixed GetCompanyWithAddressAsync
  * use identifier.Value instead of repeating its type
  * use CompanyUniqueIdData instead of UniqueIdentifierData
* fixed sonar findings
* fixed codeql findings
* CONTRIBUTING.md: linked to contribution details
* updated eclipse dash tool for dependencies check

## 1.8.0

### Change
* **Registration Service**
  * adjust endpoint `GET: /api/registration/applications` to additionally response the registrationType
  * input pattern harmonization of 'company name' endpoint `POST api/registration/application/{applicationId}/companyDetailsWithAddress`
  * enhanced backend logic implemented for endpoints posting business partner numbers to allow the input of lowercase BPNs and ensure the transition to uppercase by the backend logic (impact on registration business logic)
* **Administration Service**
  * added filters and lastEditor data for serviceAccounts to support the retrieval of 'Inactive' companyServiceAccounts via the `GET /serviceAccounts` endpoint
  * updated controller connector endpoints by enhancing the error to the new error handling method with extended user information
  * updated controller serviceAccount endpoints by enhancing the error to the new error handling method with extended user information
  * input pattern harmonization of 'company name' endpoints `POST api/administration/invitation` & `POST api/administration/registration/network/partnerRegistration`
  * search pattern harmonization of 'company name' endpoints `GET api/administration/registration/applicationsWithStatus` & `GET api/administration/registration/applications`
  * updated response body of `GET /api/administration/user/owncompany/users` endpoint by changing the "role" section to an array to include role client information ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * `GET /api/administration/identityprovider/owncompany/identityproviders/{identityProviderId}` enhanced with additional attributes ("metadataUrl", "authorizationUrl", "tokenUrl", "logoutUrl", "clientId", "hasClientSecret": true)
  * enhanced backend logic implemented for endpoints posting business partner numbers to allow the input of lowercase BPNs and ensure the transition to uppercase by the backend logic (impact on connector business logic, registration business logic, user business logic)
* **Apps Service**
  * input pattern harmonization of 'company name' endpoints `PUT /api/apps/appreleaseprocess/{appID}` & `POST /api/apps/appreleaseprocess/createapp`
  * enhanced `GET /api/apps/provided/subscription-status` endpoint by adding filter(s) to filter by companyName/customerName
* **Services Service**
  * enhanced `GET /api/services/provided/subscription-status` endpoint by adding filter(s) to filter by companyName/customerName
* **External Interface Details**
  * BPDM interface refactored - bpdm push process was updated to support the new interface spec of the bpdm gate service (incl automatic set of sharing state to ready) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * Clearinghouse interface updated - possible generated clearinghouse service error content is getting saved inside the application comment level
* **Email Template**
  * "cx_admin_invitation" enhanced by adding the section and link of the decline url
* **Seeding data generic/release scope updated**
  * added additional ssi credentials
  * adjusted existing template urls
  * released new technical user/service account roles `BPDM Gate read` and `BPDM Gate read&write`
* **IAM Seeding**
  * added user.session.note from client protocol mapper to seeding service

### Feature
* **Database structure update and impact to endpoints**
  * portal.countries table updated by introducing a new structure and help tables for multi language capability
  * new endpoint `GET /api/registration/staticdata/countrylist` release responding with a list of countries and multi language country long description
* **Decline Registration Feature (Registration Service & Email Template)**
  * new endpoint `GET /api/registration/applications/declinedata` to retrieve decline information (companyName, invited users)
  * new email template released 'Decline Registration'
* **Agreement Status**
  * updated portal.agreement table including agreementStatus column to display active agreements
  * updated logic of POST and GET agreement endpoint to only consider active agreements
  * added agreementView to display agreements per companyRole
* **Onboarding Service Provider Function**
  * enabled deactivation of managed idps (administration service) via the existing idp status update endpoint
  * enabled deletion of managed idps (administration service) via the existing idp delete endpoint
  * added new endpoint to enable customer to decline their own company application which was created by an osp
* **Manage user specific identity provider details (Administration Service)**
  * API endpoints for user account creation backend logic updated to set the providerID (unique username on the IdP which holds the user identity) is getting stored inside the portal db
    * `POST /api/administration/identityprovider/owncompany/usersfile`
    * `POST /api/administration/registration/network/{externalId}/partnerRegistration`
    * `POST /api/administration/invitation`
    * `POST /api/administration/user/owncompany/users`
    * `POST /api/administration/user/owncompany/identityprovider/{identityProviderId}/users`
    * `POST /api/administration/user/owncompany/identityprovider/{identityProviderId}/usersfile`
    * `POST /api/administration/user/owncompany/usersfile`
    * `POST /api/registration/application/{applicationId}/inviteNewUser`
  * added additional user identity provider attributes (such as idpDisplayName and providerID) for all GET user account data
    * `GET /api/administration/user/owncompany/users?page=0&size=5`
    * `GET /api/administration/user/owncompany/users/{userId}`
    * `GET /api/administration/user/ownUser`
* **Certificate Management (Administration Service)**
  * added database structure for company certificates (new tables and connections - for detail refer to the upgrade documentation)
  * added seeding for company certificates (certificate types; certificate type description and certificate type status)
  * released static data endpoint to retrieve supported certificate types - `GET /api/administration/staticdata/certificateTypes`
  * released endpoint for posting company certificates - `POST /api/administration/companydata/companyCertificate`
  * released new endpoint to fetch own company certificate data incl sorting and filters - `GET /api/administration/companydata/companyCertificates`
  * released new endpoint to fetch other company certificate data using business partner number via the new endpoint `GET /api/administration/companydata/{businessPartnerNumber}/companyCertificates`
* **Others (Common for all services)**
  * released support endpoint(s) returning for each backend service all supported error-types, error-codes and error-messages

### Technical Support
* Removed configuration values needed for the process identity - identity needed for the process worker is now done with a database request to get the needed values for the specified user
* Updated claims to include/set identityType and companyId
* Refactored the IdentityService implementation - IdentityData is read asynchronously from the database which is triggered by the respective policy in the controller. This avoids unnecessary accesses to the database in case only the identity_id or no identity-data at all is required to execute the respective business-logic
* Adjusted the path for portal backend dbaccess in the maintenance docker image
* Identity Service is now created only once per request to minimize database access
* Updated Swagger document schema - nullable and fix values updated
* IdentityService has been refactored using claims preferred_username or clientId from token querying the database for identityId or (for service_accounts) clientClientId instead of UserEntityId. As a fallback (for inconsistent test-data) the previous logic (using claim sub + UserEntityId) still exists. Code that makes use of UserEntityId or (ServiceAccount) ClientId has been refactored to use IdentityId and ClientClientId instead. The (now obsolete) ServiceAccountSync-process has been removed.
* Removed obsolete UserEntityId != null condition from queries being used in authorization
* Fixed security vulnerability for referenced external packages
* Updated dependencies file and file header template
* Updated the Newtonsoft.Json package to fix a high security finding
* Added additional image tags of type semver to release workflows
* Release workflow updated by adding additional image tag of type semver
* Upgraded external packages with security vulnerabilities
* Fixed sonar cloud finding to use correct pagination params
* Nuget Packages - provide Framework Packages as Nuget Packages
* Added scripts for an easy nuget package creation and update process
* Updated release workflow to not run release workflow when a new framework version is getting published
* Email Service - updated implementation of the email service allowing the configuration of the sender's email address to enable customization of the sender information for outgoing emails
* Changed portal-cd references to portal due to repository renaming
* Updated link to dockerfile in docker-notice files
* Updated README.md
  * mentioned `docs` folder in portal-assets repository
  * referenced docker notice files in notice section instead duplicating the content

### Bugfix
* fixed GET /api/services/{serviceId}/subscription/{subscriptionID}/provider to return clientClientId instead of the serviceAccountName
* fixed inner exception handling of the new error handling method implementation of 1.7.0 which resulted in a infinity loop
* endpoint POST /api/administration/registration/applications/{applicationId}/decline
  * fixed backend logic by setting the idp connection of the company to 'disabled' inside the IdP (keycloak)
  * fixed backend logic by fetching the user email upfront to deactivating the user
* disabled the duplicate bpn check for endpoint /api/registration/application/{applicationId}/companyDetailsWithAddress
* endpoint authorization on valid companyId fixed for
  * POST /api/apps/appreleaseprocess/consent/{appId}/agreementConsents
  * POST /api/services/servicerelease/consent/{serviceId}/agreementConsents
* changed claimTypes static class of clientId claim to client_id
* identityProvider Configuration - added cancellationToken to UpdateOwnCompanyIdentityProvider
* ExternalRegistration
  * added ValidCompany Attribute to endpoint POST api/registration/network/{externalId}/decline to initialize the companyId of the current user correctly
  * external Registration submission endpoint POST /api/registration/Network/partnerRegistration/submit fixed
* fixed endpoint GET /api/administration/user/owncompany/users/{userid} missing assignments of firstname, lastname and email were added to businesslogic and setters were removed from company-user related record-definitions

### Known Knowns
* Certificate Feature
  * POST /api/administration/companydata/companyCertificate uploads the document with the wrong status

## 1.7.0

### Change
* Registration Service
  * enhance GET /api/registration/applications registration customer endpoint by adding the registration approval flow status
  * added agreement_link to agreement table and enhanced existing agreement endpoint response to include the agreement link - GET /api/registration/companyRoleAgreementData
  * implemented business/backend logic to set/update company_applications.date_last_changed when running an update on the companyApplication
* Administration Service
  * enhanced DELETE ServiceAccount endpoint by adding a validation to allow provider as well as owner of the service account to trigger the deletion
  * added validation for DELETE ServiceAccount to not allow to deactivate if active subscription exists
  * enhanced DELETE connector business logic by automatically deactivate technical users which (if any) are linked to the connector
  * enhanced GET /administration/companydata/certificateTypes business logic to return only those certificateTypes which the users company is able to request
  * added agreement_link to agreement table and enhanced existing agreement endpoint response to include the agreement link - GET api/administration/companydata/companyRolesAndConsents
  * enhanced response body of GET /api/administration/Connectors/{connectorId}; GET /api/administration/connectors & GET /api/administration/connectors/managed by adding linked technical user data (id, name, role, etc.)
  * endpoint GET api/administration/partnernetwork/memberCompanies enhanced to allow to send specific set of BPNs inside the request payload to request membership status for those BPNLs only
* App Service
  * enhanced backend logic of /autosetup process worker to only create technical users for app linked technical user profiles which have a role assigned
  * added status filter option for endpoint GET /apps/provided
  * enhanced GET /api/Apps/{appId}/subscription/{subscriptionId}/subscriber endpoint by responding (if existing) with connector details connected to the subscription
  * enhanced GET /api/Apps/subscribed/subscription-status response body by adding subscriptionId
  * instead of creating the notification for an app subscription after triggering the provider, the notification will directly be created when subscribing to an offer
  * enhanced POST /{appId}/subscribe endpoint business logic enhanced to set offer_subscriptions.date_created value when running the endpoint
  * enhanced GET /api/apps/{appId} logic to fetch the "isSubscribe" value by the latest subscription record
  * enhanced GET /api/Apps/provided/subscription-status & GET /api/Apps/{appId}/subscription/{subscriptionId}/provider by adding processStepTypeId responding with the latest subscription processStepTypeId
  * added processStepTypeId field to provider/subscription endpoints
* Services Service
  * enhanced backend logic of /autosetup process worker to only create technical users for service linked technical user profiles which have a role assigned
  * enhanced endpoint GET /api/services/provided by "leadPictureId" & "lastChanged" date
  * service types renamed from 'Consultance_Service' to 'Consultancy_Service'
  * instead of creating the notification for an app subscription after triggering the provider, the notification will directly be created when subscribing to an offer
  * POST {appId}/subscribe endpoint business logic enhanced to set offer_subscriptions.date_created value when running the endpoint
  * added processStepTypeId field to provider/subscription endpoints
* Seeding Data
  * updated and added technical user role description & user role names
* Others
  * added email value validation for invitation, network registration and user invitation to enable the user input data validation for valid email

### Feature
* Administration Service
  * added /api/administration/staticdata/operator-bpn endpoint to fetch operator bpns
* App Service
  * unsubscribe OfferSubscription released
  * added email send function for endpoint PUT /api/apps/AppReleaseProcess/{appId}/approveApp
  * introduce TRIGGER_ACTIVATE_SUBSCRIPTION process step to manually trigger the offer subscription activation enable client and service accounts when activating the offer subscription
  * added endpoint to AppChange controller to allow app document change process - fetch app documents api: GET /apps/appchange/{appId}/documents
* Services Service
  * unsubscribe OfferSubscription released
  * added email send function for endpoint PUT /api/services/ServiceRelease/{serviceId}/approveService
  * updated permission validation of endpoint /serviceAgreementData/{serviceId}
  * introduce TRIGGER_ACTIVATE_SUBSCRIPTION process step to manually trigger the offer subscription activation enable client and service accounts when activating the offer subscription
* Notification Service
  * GET /api/notifications search string enabled to support search by notification content
* Onboarding Service Provider *new function* - released onboarding service provider functionality, incl.:
  * added new database structure for network to network
  * added seeding for n2n
  * enhanced POST /api/administration/identityprovider/owncompany/identityproviders endpoint by identityProviderType (managed; own) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * network process worker registered to use process worker for onboarding service provider registration flow
  * added seeding (test data only) for the osp realm and company including users
  * added POST /api/administration/registration/network/partnerRegistration/submit endpoint
  * added POST registration/network/partnerRegistration to register partners to the network
  * added new process to synchronize users with central keycloak instance for the partnerRegistration
  * added endpoint to get the callback url of an osp
  * added endpoint to set the callback url of an osp
  * added /api/administration/identityprovider/network/identityproviders/managed/{identityProviderId} endpoint to retrieve idp information regarding IdP connected companies
* Email templates
  * released new email template 'offer release approval'
  * released new email template 'welcome onboarding service provider registration company' (connected to feature release Onboarding Service Provider)

### Technical Support
* Swagger Documentation updated
  * enhanced error documentation for all services
  * updated endpoint summary documentation where necessary
* Test Automation runs implemented (external service health checks & administration and registration e2e journeys) via gitHub workflow
* added attribute logos in notice file
* remove unused notice file (Docker Hub)
* DB Changes
  * enhanced db table agreements by adding new attribute agreement_link to support links for agreement where needed
  * added "Identity_Provider_Types" table which is connected to portal.identity_providers table
  * added inside the new table "Identity_Provider_Types" an id as well as a label
  * new attribute identity_providers.owner_id added
  * added custom migration script for identity_provider_type_id and owner_id
  * added inside portal.offer_subscription new attribute "date_created", incl. a migration to set all existing offer subscriptiond dates to "1970-01-01"
  * database view 'Company-Connector-View' added
  * database view 'Company-IdP-View' added
  * database view 'Company-Role-Collection-Role-View' added
  * database view 'Company-User-View' added
* Auditing
  * new migration has been created that recreates all triggers according to the new naming scheme being introduced by version 7.1.1 of the trigger-framework
  * trigger-extensions have been adjusted ensuring a consistent order of properties to avoid unnecessary recreation of trigger-functions when creating new migrations
* removed dependency on 'DateTimeOffset.OffsetNow()' in custodian unit-test by replacing that dynamic testdata by a constant value.
* dependency to framework.DateTimeProvider was added to dockerfiles of modules 'Migrations' and 'Maintenance'
* .NET Update
  * upgrade .NET to v7
  * upgrade of nuget packages
  * removed trigger dependency
  * enabled to write audit entities automatically on DbContext SaveChanges
* removed e2e test files from sonar coverage check
* support build images also for arm64, in addition to amd64
* improve dockerfiles - 'dotnet build' not needed as implicit of 'dotnet publish'
* enhanced security.md file with newest release relevant guidelines regarding vulnerability/security finding handling
* change launchsettings
  * cors for localdev env
  * align applicationUrl for apps service
* Keycloak auth path config updated inside the portal backend code to support auth path configuration (needed for older keycloak version)
* adjusted keycloak seeding to exclude the following properties if already existing
  * firstname, lastname, email and configurable attributes for users
  * idp config urls
  * client urls
  * smtp server settings
* extend keycloak seeding to seed ClientScopeMappers
* Removed auth trail from the provisioning settings and added the use of the keycloak settings to set the correct useAuthTrail value
* adjusted process worker workflow to build the process worker when changes within the networkRegistration directory appear
* Controller slimlined
  * removse all identity related code from controllers
  * added identityservice to buisnessLogic to access idenitity
* Mask sensitive information in portal logs
* Extended logs for external service/component calls
* check constraints is_external_type_use_case, is_credential_type_use_case & is_connector_managed changed from function constraint to trigger function constraint
* Added new process to synchronize keycloak user with company service account to set the correct user entity id
* Released extended error response message method (incl. error-type, error-code, a message-template and multiple parameters) and enabled the same for administration POST endpoints /userfile and registration GET endpoint /companyDetailsWithAddress
* Updated email template dynamic keys to more generic technical keys and moved base url definition into the product config file of the specific environment ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)

### Bugfix
* Seeding data
  * fixed typos inside agreements.json and company_role_description.json
  * 'user_entity_id' updated in identity.json file to match with the keycloak service account/client id
  * added sd documents in company.json file
  * fixed connectors.json file (test data file only) value connectorType with owner/provider
  * fixed user role mapping to company role (App Provider and Service Provider)
* Administration Service - adding validation for offerUrl by not supporting hash characters PUT /api/administration/identityprovider/owncompany/identityproviders/{identityProviderId}
* GET app/provider/subscription-status endpoint results enhanced to include inactive subscriptions
* GET service/provider/subscription-status endpoint results enhanced to include inactive subscriptions
* Email template - nameCreatedBy handling for null values fixed by using default names in case of null
ServiceChange service url is mismatched, now it is fixed
* fixed GET: api/administration/identityprovider/owncompany/identityproviders to handle not existing idps in keycloak by using null values
* fixed GET: api/administration/identityprovider/owncompany/identityproviders/{identityTypeId} to handle not existing idps in keycloak by using null values
* autoSetup issues fixed to support scenario where no appInstanceSetup is configured
* fixed offerSubscription request email logic to get send to the respective offer manager (Sales Manager; Service Manager; App Manager) of the company
* Application approval/verification process: adjusted bpdm businessPartnerNumber pull process to handle an unset SharingProcessStarted and retry the process
* Updated userRole for service endpoint PUT api/services/{subscriptionId}/unsubscribe to "unsubscribe_services"
* Add user entity id when creating a company service account
* Adjusted the json property name for bpn within the BpdmLegalEntityOutputData
* Connector pagination of GET /api/administration/connectors fixed - pagination failed as soon as there were connectors with same provider but different hosts existing
* Fixed maintenance job - document hash null cases and only delete not linked documents
* Delete /api/apps/appreleaseprocess/{appId}/role/{roleId} 504 error fixed

### Known Knowns
* GET /api/services/{serviceId}/subscription/{subscriptionID}/provider - wrong property value for technicalUserData "name" responded
* declineFlow OSP currently not supported
* password reset in welcome user email currently not supported
* POST: api/administration/registration/application/{applicationId}/decline - does not disable the idp in keycloak when declining the application

## 1.6.0

### Change
* Registration Service
  * removed property/attribute countryDe from all required endpoints in registration and administration service ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
* Notification Service
  * added "doneState" as filter criteria for endpoint GET /api/notification
  * changed NotificationTopicId to nullable, to mitigate database query errors of missing links for notificationTypeId and NotificationTopicId
* Marketplace/App Service
  * Removed PUT: /api/apps/appreleaseprocess/updateapp/{appId}
  * added GET: /api/apps/owncompany/activesubscriptions
  * added GET: /api/Apps/owncompany/subscriptions
  * added single app subscription activation
  * change the endpoint subscription/{offerSubscriptionId}/activate-single-instance from post to put ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * change the appInstanceId to iamClientId for the /{appId}/subscription/{subscriptionId}/provider endpoint
  * LastDateChanged apps attribute configured to get updated with updating the offer or a related entity
  * endpoint PUT /apps/{subscriptionId}/activate - endpoint changed and backend business logic got updated. In case of an already existing INACTIVE subscription a new subscription record is getting created.
  * added AppInstanceId to the /{appId}/subscription/{subscriptionId}/provider endpoint
  * added app tenant url inside the response body of endpoint GET apps/{appId}/subscription/{subscriptionId}/provider
  * enhanced business logic of PUT: /api/apps/appreleaseprocess/{appId}/technical-user-profiles to remove technical user profiles where no roles are submitted
  * enhanced business logic of PUT: /api/apps/appreleaseprocess/{appId}/technical-user-profiles to not allow the creation of an profile without any assigned permission
  * backend business logic updated - ignore empty technical user profiles inside the response body of following endpoints
    * GET: /api/apps/{appId}/subscription/{subscriptionId}/subscriber
    * GET: /api/apps/{appId}
* Administration Service
  * removed PUT /users/{companyUserId}/resetpassword
  * removed POST /api/administration/Connectors/daps
  * removed POST /api/administration/Connectors/managed-daps
  * added validation to endpoint /api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId} to check if requesting user is either owner or provider
  * enabled search of clientId for GET /api/administration/serviceaccount/owncompany/serviceaccounts
  * enhanced response body of endpoint GET /companydata/ownCompanyDetails by CompanyRoles
  * enhanced endpoint backend logic of endpoint GET /owncompany/serviceAccount by including managed service accounts in the response of the app/service provider. Additionally, the relation is added inside the response body with the boolean value 'isOwner'
* BPDM Interface(s) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * BPDM Service calls are changed to the new bpdm partner endpoints (registration search, checklist worker push & pull)
* Services Service
  * LastDateChanged service attribute configured to get updated with updating the offer or a related entity
  * backend business logic updated - ignore empty technical user profiles inside the response body of following endpoints
    * GET: /api/services/{serviceId}
    * GET: /api/services/{serviceId}/subscription/{subscriptionId}/subscribe
* Application Checklist Worker
  * enhanced the logging of a successful create wallet process step
  * enhanced error handling and stored error messages to better human readable information
  * retrigger of an process step deleted the earlier stored error comment inside application_checklist.comments
* All Services
  * added an /api/info endpoint to retrieve specific api endpoints which can be used publicly for external services
* Email Templates
  * added "bpn" value inside the portal_welcome email
  * refactored email templates by removing redundant code and added additional structure for better readability and maintainability

### Feature
* Auditing
  * added audit table for ProviderCompanyDetails (for insert ,update and delete operation)
* SSI - enable Verified Credential request workflows for useCaseParticipant and company roles by certificates
  * added use case description table
  * added datamodel for the use case participation
  * added endpoint to get the UseCaseParticipations for the own company
  * added auditing for company ssi details
  * new email templates added: 'verified_credential_approved' and 'verified_credential_declined'
  * released endpoints to submit usecaseparticipation and ssi certificates for review
  * released endpoint to fetch all "in_review" ssi vc requests
  * released endpoint to approve a ssi vc request
  * released endpoint to reject a ssi vc request
* Registration Verfification/Activation - Administration Service
  * added membership credential creation call (via MIW) in the application activation process step
  * changed the bpdm interface url to fetch the legal entity bpn generator result with enhanced error message support
* Disabled DAPS connection for connector registration, change and related data provided in the GET endpoints
* Company Role config
  * enabled unassignment of roles
  * enhanced validations to ensure that minimum one role need to be assigned
* View CompanyLinkedServiceAccountsView released to easily access/view service account owner/provider
* Managed Connector subscription connection (administration service)
  * new mapping table to link connectors with offer subscriptions
  * added database view to see all offer subscription related links
  * added /api/administration/connectors/offerSubscriptions to get all offerSubscriptions for the connector view
* Added /api/administration/staticdata/operator-bpn endpoint to receive the operator bpn

### Technical Support
* Seeding
  * IAM - change base image from aspnet to runtime
  * IAM - change name in Docker Hub notice
  * enabled configurable path of the seeding data
  * added validation on application startup for the seeder settings
  * added seeding for keycloak realm-data from a json-file
* Migration Jobs
  * change base image from aspnet to runtime
* All Services
  * added missing file headers
* Logging
  * removed machine name, processId, threadId from the logging message
  * re-include http request & response messages in logging
  * introduced structured logging - integration of Serilog logging across all services
  * configure serilog for backend services
  * fixed the logging configuration where the logger had to be instantiated multiple times
* TRG
  * changed license notice for images
  * added file header to .tractusx
* adjusted naming of the technical user used for the process worker runs
* added check whether an endpoint should only be callable for a service account user
* added check whether an endpoint should only be callable for a company user
* lastEditorId configured to get set whenever an auditable entity is changed
* introduce 'identity' table to align 'company_users' and 'company_service_account'
  * moved UserEntityId from IamUser and IamServiceAccount into the new created table 'identity'
  * removed 'iam_users' table
  * moved client_id and client_client_id into 'company_service_account' table
  * removed 'iam_service_accounts'
  * added migration to support lossless migration of the data
* multi language handling for language table enabled
  * introduced new table 'language_long_names'
  * moved language_long_names from 'languages' to 'language_long_names'
  * endpoint /api/administration/staticdata/languagetags backend logic updated to fetch data from the new added table
* introduced GitHub workflow to enable 3rd party dependencies check with the Eclipse Dash License Tool
* changed dependencies file to '-summary' format from Dash Tool
* added legal information to distribution / include NOTICE.md, LICENSE and DEPENDENCIES file in output
* added copy of module Framework.Models and Framework.Linq to dockerfile of Module Portal.Migrations
* several swagger documentation updates (summary, description, endpoint example)
* removed the health-check paths from request logging

### Bugfix
* Administration Service
  * added check for active offerSubscriptions when deleting a connector
* Company Service Accounts
  * set identityTypeId when creating service accounts to company service account instead of company user
  * change client_id of service accounts in seeding data
  * added service accounts from cx-central base
* Marketplace/Apps Service
  * fixed validation for /api/apps/AppReleaseProcess/instance-type/{appId} to only be executable for apps in state CREATED
  * Fix POST /api/apps/appreleaseprocess/consent/{appId}/agreementConsents to save consents and set the correct user id
* Process Worker
  * fixed the logging of the wallet creation response to save the did in the database
* Authentication
  * changed case sensitive check for 'Bearer xxx' to 'bearer' xxx'
* Services Service
  * Fix POST /api/services/servicerelease/consent/{serviceId}/agreementConsents to save consents and set the correct user id
* Email Template Password email updated to ensure that password field includes no spaces generated by the template
* Notification endpoint PUT /api/notification/{notificationId}/read logic fixed to update the read flag true/false
* Change the Type field of the SD-Factory call from "legalperson" to "legalparticipant"

### Known Knowns
* Registration process: additionally invited user receives 'Welcome Email' without personal salutation due to missing first and last name
* Changing an app instance type (/api/apps/AppReleaseProcess/instance-type/{appId}) is not blocked as soon as the app is submitted for release
* Missing validation for app subscription activation endpoint /api/Apps/start-autoSetup & /api/Apps/autoSetup for special character "#"
* PUT endpoint /api/services/servicechanges/{serviceId}/deactivateService contains a typo which leads to nginx error
* App Subscription Request endpoint is sending the subscription request email to the stored contact email instead of informing the app manager(s) and sales manager(s)

## 1.5.1

### Bugfix
* Marketplace Service:  POST: /api/apps/autoSetup fixed mailing for subscription activation
* Services Service:  POST: /api/services/autoSetup fixed mailing for subscription activation

## 1.5.0

### Change
* Services Service
  * added technical user profile information incl assigned userRoles for endpoint GET /services/{serviceId}
  * endpoint validation of POST /api/services/servicerelease/addservice enhanced ('serviceType' and 'title' set to mandatory)
  * endpoint logic of PUT: /api/services/servicerelease/{serviceId}/declineService enhanced by setting documents in status "ACTIVE" to "PENDING" when declining a service
  * endpoint response of POST: /api/services/autoSetup enhanced to include 'endpoint URL' (if applicable), as well as 'technicalUserProfile'
  * endpoint response of GET: /api/services/provided/subscription-status enhanced by including a true/false flag for 'technicalUser'
* Apps Service
  * added technical user profile information incl userRoles for endpoint GET /apps/{appId}
  * endpoint logic of PUT: /api/apps/appreleaseprocess/{appId}/declineApp enhanced by setting documents in status "ACTIVE" to "PENDING" when declining an app
  * endpoint response of POST: /api/apps/autoSetup enhanced to include 'endpoint URL' (if applicable), as well as 'technicalUserProfile'
  * endpoint response of GET: /api/apps/provided/subscription-status enhanced by including a true/false flag for 'technicalUser'
  * endpoint logic of PUT: /api/apps/appReleaseProcess/{appId}/submit enhanced by setting 'technical user profile' and 'privacy policy' configured to mandatory - exception code added

### Feature
* Services Service
  * released GET: /api/services/subscribed/subscription-status endpoint to retrieve subscribed service offerings for the acting user assigned company
  * released PUT: /api/services/servicechanges/{serviceId}/deactivateService endpoint to enable serviceProviders to deactivate owned services offered on the CX marketplace
* Services Service
  * new extended /start-autosetup endpoint released to provide multi service type autosetup logics depending on tech-user-setup need
* Apps Service
  * new extended /start-autosetup endpoint released to provide multi app type autosetup logics depending on tech-user-setup and app instance configuration

### Technical Support
* Apps Service clean-up(s) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * endpoint GET: /api/apps/provided/subscription-status attribute key name change from 'serviceName' to 'offerName'
  * endpoint PUT: /api/apps/{appId}/subscription/company/{companyId}/activate updated to 'obsolete'
* Administration Service clean-up(s) ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * exchanged controller name from ../serviceprovider/.. too ../subscriptionconfiguration/.. controller
* added sonarcloud.properties file to manage duplications for automatic analysis

### Bugfix
* Registration Service
  * exception handling of Post: /api/registration/application/{applicationId}/submitRegistration updated in case of registration has an incorrect status
* DB Migration
  * added seeding data to link the role collection "CX Participant" with the technical user roles "BPDM Pool", "Connector User", "Dataspace Discovery", "Identity Wallet Management"

## 1.4.0

### Change
* Administration Service - Connector Controller
  * Delete Connector endpoint business logic enhanced to update DocumentStatus to InActive when connector is getting deleted
  * added status "INACTIVE" state for connectors
  * new endpoint to retrieve managed connectors (for service providers) GET /api/administration/connectors/managed
  * connectorLastChangeDate dateTimeOffset set to utcNow
  * enhanced user controller endpoint GET api/administration/user/ownUser by adding attribute to response back with the users company administrator contact
* Registration Service
  * POST api/registration/application/{applicationId}/documentType/{documentTypeId}/documents limited allowed documents types to only support "CX_FRAME_CONTRACT" and "COMMERCIAL_REGISTER_EXTRACT"
* App Service
  * GET api/apps/appreleaseprocess/inReview/appId  endpoint response body enhanced by privacypolicies attribute
  * enhanced endpoint GET api/apps/appreleaseprocess/inReview/{appId} by adding technical user profile attribute (key and permissions) assigned to the service inside the response body
  * updated endpoint GET /api/apps/business exchanged "id" content with the appId and added subscriptionId attribute
  * added leadimage id inside the endpoint api/apps/subscribed/subscription-status
  * added leadimage id inside the endpoint apps/provided/subscription-status
* Notification Service - enabled automatic notification done state
  * portal.notifications table enhanced by new attribute "done" (false/true)
  * added business logic to set respective related notifications to "done", when approving or declining an app / service
  * added offerId and offerName to notification content for TECHNICAL_USER_CREATION
* Services Service
  * enhanced endpoint PUT /api/services/updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents to support the documentType "SERVICE_LEADIMAGE" allowing png/jpeg/svg file types
  * enhanced endpoint GET api/services/provided/subscription-status by adding additional attribute i.e Customer Country, Email and Company BpnNumber
  * enhanced endpoint GET api/services/servicerelease/{serviceId}/serviceStatus by adding technical user profile attribute (key and permissions) assigned to the service inside the response body

### Feature
* Administration Service - Connector Change URL
  * Change connector endpoint released for change of the connector url PUT /api/administration/connectors/{connectorId}/connectorUrl
* Administration Service - Connector Registration
  * enable the optional linkage of technical users with connector registrations (managed as well as unmanaged connectors)
  * new attribute in table "connectors" added to link the service_account to an connector record
* App Service - released app instance handling (single instance vs multiple instances)
  * add endpoint to add app instance information
  * enhanced activate app process by differentiating the activation process steps based on the app instance setting
  * released single app instance subscription process
  * new table "app_instance_assigned_service_accounts"
  * new table "app_instance_setups"
* Services Service - released individual technical user profile for services (only relevant for service type "DATASPACE_SERVICE"
  * new table "technical_user_profiles" added
  * new table "technical_user_profile_assigned_user_roles" added
  * new endpoint GET /api/services/{serviceId}/technical-user-profiles released to view technical user profiles set for the specific service
  * new endpoint POST /api/services/{serviceId}/technical-user-profiles released to update technical user profiles set for the specific service
* App Service & Services Service - released app "license_type" to manage license type FOSS/COTS
  * new table "license_types" added
  * portal.offer table enhanced by "license_type_id" attribute
  * added license_type information to all response bodies for the GET endpoints ../inReview/{appId}, ../{appId}, .../active for apps and services service
  * added backend business logic to automatically set the license_type_id for a new offer to "COTS" as default value (endpoints: /createapp & /addservice)
  * new endpoint GET /api/administration/StaticData/licenseType released to view all available license types
* Services Service - added endpoints to retrieve subscription details
  * released DELETE endpoint /api/services/servicerelease/documents/{documentId} to allow to delete documents in status "PENDING" or "INACTIVE" which are of the type "SERVICE_LEADIMAGES" and "ADDITIONAL_DETAILS"
  * new endpoint GET /api/services/{serviceId}/subscription/{subscriptionId}/provider to retrieve subscription status and details for service providers
  * new endpoint GET /api/services/{serviceId}/subscription/{subscriptionId}/subscriber to retrieve subscription status and details for service customers
* App Service - added endpoints to retrieve subscription details
  * new endpoint GET /api/apps/{appId}/subscription/{subscriptionId}/provider to retrieve subscription status and details for app providers
  * new endpoint GET /api/apps/{appId}/subscription/{subscriptionId}/subscriber to retrieve subscription status and details for app customers
* App Service - Enable Providers to update the subscription instance url for a specific subscription/app registration
  * new endpoint PUT /api/apps/appchanges/{appId}/subscription/{subscriptionId}/tenantUrl released

### Technical Support
* Service clean-up ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * endpoints /api/apps/{appId}/appupdate/description moved from app controller to appChange controller
  * endpoints ../addservice, ../{serviceId}, ../{serviceId}/submit, ../{serviceId}/approveService, ../{serviceId}/declineService, ../updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents moved from services controller to serviceRelease controller
  * moved GET /api/services/{serviceId}/technical-user-profiles to serviceRelease controller
  * moved PUT /api/services/{serviceId}/technical-user-profiles to serviceRelease controller
* App clean-up ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * moved POST /api/apps/{appId}/appLeadImage endpoint to appChange controller
  * moved PUT /api/apps/{appId}/deactivateApp endpoint to appChange controller
  * moved GET /api/apps/{appId}/technical-user-profiles to appReleaseProcess controller
  * moved PUT /api/apps/{appId}/technical-user-profiles to appReleaseProcess controller
* changed release workflow to retrieve tag from github.ref_name (set-output command deprecated)
* added release workflow for release-candidates
* Db Auditing
  * audit table added for portal.consent
  * audit table added for portal.connector
* Checklist Worker
  * renamed to Processes Worker
* Provisioning Service has been discontinued
* changed container registry to Docker Hub
* restricted CI-based sonarcloud job to only run at pull request event if raised within same base repo
* added pull request template

### Bugfix
* updated busness logic to assign new user accounts the company bpn as user attribute (fix implemented for user accounts created for ownIdp customers)
* backend business logic fixed for GET api/administration/companydata/companyRolesAndConsents & POST api/administration/companydata/companyRolesAndConsents

## 1.3.0

### Change
* App Services
  * added additional attribute providedUri,contactEmail and contactNumber for update app PUT: /api/apps/appreleaseprocess/{appId}
* Services Service
  * change permission role for Get Service Details GET: /api/services/servicerelease/inReview/{serviceId}
  * updated api response body key form ServiceTypeIds to ServiceTypes for endpoint GET: /api/services/{serviceId} ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * POST: /api/services/addservice - added "providerUri" inside the request body to store the provider url
  * PUT: /api/services/{serviceId} - added "providerUri" for endpoint to store the provider url
* Seeding Data: update agreement name for app marketplace offers - CX Conformity

### Feature
* Administration Service - Company preferred use case settings released (Controller: CompanyData)
  * GET endpoint to receive company preferred use cases
  * POST endpoint to set new company preferred use cases
  * DELETE endpoint to delete a use case from company preference list
* Services Service
  * New endpoint to retrieve service documents released GET: /api/services/{serviceId}/serviceDocuments/{documentId} supporting service assigned documents
  * GET: /api/services/provided endpoint released to support the service management function via retrieve all my services for the service owner
  * GET: /api/serviceRelease/inReview released to support the service release decision management function via retrieve all active and waiting-for-decision services for the platform provider/operator
  * released document delete endpoint DELETE: /api/services/servicerelease/documents/{documentId}
* Migration/Seeding
  * added two new document types (CONFORMITY_APPROVAL_SERVICES, SERVICE_LEADIMAGE)

### Technical Support
* added temp fix for CVE-2023-0464
* added build workflow for v1.3.0 release candidate phase
* updated actions workflows
* updated sonarcloud workflow: use repo variables for project key and organization

### Bugfix
* App Service: added SaveAsync method call inside OfferService CreateOrUpdateProviderOfferAgreementConsent method to ensure data storage with POST /{appId}/agreementConsents call
* Fixed CX Admin/operator notification creation for endpoint PUT: /api/apps/appreleaseprocess/appId/submit and PUT: /api/services/servicerelease/serviceId/submit by deleting the companyId validation
* Notification Service: the notifications will only get created once per request for each user and notification type

## Known Knowns
* Registration Approval Flow - Wallet interface error logging not sufficient. Only error code is stored, but no error message.

## 1.2.0

### Change
* Update email template content for "Next Step" email
* App Service
  * enhance api response body key useCase in appStatus endpoint by including the useCase label and id ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)

### Feature
* Services Service
  * released new endpoints to retrieve service type master data for the service release process
  * released new endpoint to fetch agreement and consent data for an offer of service type
  * released new endpoint to receive pre-saved service information as part of the service release process
  * released new endpoint ro set the consent to agreements for a specific service
  * removed service serviceAgreementConsent endpoints from service controller
  * added attribute "providerUri" to the PUT endpoint services/{serviceId}
* Added audit entity for portal table company_assigned_roles
* Document Endpoints
  * implemented media type usage (mimeType) for all document uploads/downloads. Media type is now getting stored based on the upload file type inside the portal.documents table
  * new endpoint GET administration/document released to fetch cx frame documents as part of the portal app
  * new endpoint GET registration/document released to fetch cx frame documents as part of the registration app
* Administration Service
  * released new endpoints to change CompanyRole and Consent records

### Technical Support
* Unit test split for seeding data consortia vs. test data
* trg: add repo metafile
* trivy: fixed container registry
* Fixing missing dockerfile dependencies of maintenance and migration
* Checklist worker: enhance process step logic/table by enabling the storage of error messages against process steps

### Bugfix
* Registration Service: fix/updating query validating calling users association with application
* Checklist worker: fix racecondition on concurrent execution of same process
* Seeding data consortia updated by adding offer privacy policies
* Updated the business logic for service/submit by removing the validation for attribute user role t flow

## 1.1.0

### Change
* App Service
  * enhanced GET apps/subscribed/subscription-status endpoint by adding 'name' and 'provider' keys
  * new controller 'App Change' introduced
  * endpoint /appreleaseprocess/{appId}/role/activeapp moved to /appChange controller ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * endpoint /appChange/{appId}/role/activeapp business logic enhanced to add new role in existing keycloak clients/app instances
* Services Service
  * for offer with service type DATASPACE_SERVICE  - portal db subscription record is created and technical user creation is supported
  * for offer with service type CONSULTANCE_SERVICE - portal db subscription record is created with the subscription request; app_instance and technical users are no longer created/relevant

### Feature
* App Service
  * operator/CX Admin endpoint released to view all app details under "IN_REVIEW" for release approval /appreleaseprocess/inReview/{appId}
  * new endpoint to enable app provider to switch leadImage of an app - POST /api/apps/{appId}/appLeadImage
  * new endpoint to delete app under app release process - DELETE /apps/appreleaseprocess/{appId}
  * new endpoint to delete app documents for app under app release process - DELETE /api/apps/appreleaseprocess/documents/{documentId}
* Administration Service
  * company role management endpoint implemented - GET /administration/companydata/companyRolesAndConsents
* Login Theme
  * add customized login theme when inviting a company to register for CX (as part of the creation of the new realm)
  * change the login theme when activating the company registration (update of the realm)
* Checklist Worker Application - allowed process steps and process_step status update based on related jobs

### Technical Support
* Added the externalsystems path to trigger the build of the checklist worker
* Checklist Worker ![Tag](https://img.shields.io/static/v1?label=&message=BreakingChange&color=yellow&style=flat)
  * add new table portal.processes (enables portal to support several checklist processes in parallel)
  * add new table portal.process_types
  * enhanced portal.process_steps with the mandatory attribute "process_id"
  * enhanced portal.company_applications with optional attribute "checklist_process_id"
  * removal of table portal.application_assigned_process_steps
* Service Service
  * add new table portal.service_details
  * removal of table portal.service_assigned_service_types
* Base Data
  * Agreement & Unique Identifier base data updated
* Added temp fix for CVE-2022-1304

### Bugfix
* Checklist Worker Application
  * remove bpn process steps from manual steps
  * added status code value of external systems inside the service exception log
  * override_clearinghouse process step updated to create trigger_clearinghouse step
  * retrigger of bpdm push and pull added as manual process steps
  * bpn can get set manually, even if the checklist item is failed
  * ensure proper dispose of async enumerators
* App Service
  * GET /appreleaseprocess/{appId}/appStatus fetched db value of the key "price" updated
* Administration Service
  * Set user role in the user creation scenario as mandatory values (own IdP, bulk and single load)
* Others
  * offer decline permission updated to all users with a specific assigned permission
  * connector is set to active when daps was successfully triggered

## 1.0.0-RC10

### Change
* Seeding: updated base data image

### Feature
n/a

### Technical Support
* readme files updated and example values added

### Bugfix
* Self Description encoding fixed to improve readability of the json file
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
