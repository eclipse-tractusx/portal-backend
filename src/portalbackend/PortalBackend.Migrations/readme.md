# Creating Migrations
After changing anything related to PortalDbContext, make sure your user secrets contain a valid connection string (template in appsettings.json, connection will be tested, nothing will be written). 
Then open a command line in this project's folder and execute the following instruction:

`dotnet ef migrations add <NAME>` 

where \<NAME> is a placeholder which should be filled with CamelCase description of what changed in the database schema.

Example: `dotnet ef migrations add CPLP-XXXX-AddNameToCustomerTable`

# Applying Migrations to database
To apply one or multiple migrations to a database, there are multiple options available.
First, make sure you have the proper connection string set either in appsettings or user secrets. The user credentials need to have permissions to change the schema.
Afterwards choose one of the following:
- `dotnet ef database update`
- Execute the project's program when executing the Migraitons Program, all pending migrations will be applied to the database and a base seeding will be done if necessary.

By default, the database will be updated incrementally to the most recent migration. Applying a migration will create a database if none exists with the specified name on the database server.
