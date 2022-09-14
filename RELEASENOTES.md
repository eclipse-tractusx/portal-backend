# Release Notes

New features, fixed bugs, known defects and other noteworthy changes to each release of the Catena-X Portal Backend.

### 0.5.4 (2022-09-19) - ongoing

* Feature - App Release Process Controller enhanced with additional endpoints to support the app release feature (Post app roles, Get app roles, submit app for release)
* Defect Fix - Keycloak shared realm creation - technical user for realm management moved from master to company realm


### 0.5.3 (2022-09-05)

* Feature - Service Provider Marketplace v1 microservice released (Get Services, Get Service Details, Post Services, Post Agreement, Get Agreement, etc.)
* Feature - App Release Process Controller enhanced with additional endpoints to support the app release feature with "Put App Documents"
* Update - Portal Db Refactoring by merging service and app table and recall them "offers". Additionally al related app tabled have been renamed where suitable to "offer" instead of "app"


### 0.5.2 (2022-08-23)

* Feature - Refactoring of portal db to enable multi app management
* Feature - Identity Provider Endpoints to switch IdPs for existing CX Members and move users to the IdP
* Feature - DB enhancements (User "DELETE" enum, creation of service tables, etc.)
* Feature - App Release Management PUT endpoint implementation for "CREATE App" and "App Details"
* Feature - Static Data endpoints implemented - GET use cases, language, company data
* Feature - DB Auditing released for app subscription, company user, company application, user assigned roles
