# Security


The security features controls the user authentication and authorisation.

## Roles
There are two main roles:
- **Ordinary users**. A user can be group manager and/or a WiFRED manager. There roles must be assiged by an administrator.
- **Administrators**. An administrator can have rights to manage things in their country or residence or be a global administrator.

## Authorisation
The application uses claims based authorisation. 
There are extension methods for *ClaimsPrincipal* that simplifies to decide what a user may do.
There are also authorization policies for *User* and *Administrator* that can be used to limit access to pages.

## Authentication
Users are authenticated by retrieving their *primary email address* and *hashed password* from the **User** table.

## Database access
The app connects with the SQL login `ModuleRegistryApp` (since 2026-11-14; before, it used the server admin): roles `db_datareader` and `db_datawriter`, plus `EXECUTE` on schema `dbo` for the stored procedures. It can't change the schema. The connection string is the Key Vault secret `ConnectionStrings--TimetablePlanningDatabase`, read at startup via `VaultUri`.

- Schema changes (DACPAC publish, later EF migrations) use an admin login, never the app's.
- Local testing against production uses the read-only login `Agent` (`db_datareader` only), stored in the developer's user secrets as `ConnectionStrings:TimetablePlanningDatabaseReadOnly`.
- Passwords are only kept in Key Vault and user secrets, never in the repository.

## Security Policy Headers
In addition, the site uses a strict policy for what resources the app may access from the browser.

## Design Considerations
The site uses handcrafted code for authentication and authorisation. 
It is a deliberate choice to *not* use out-of-the-box solutions for example ASP.NET standard authentication, solution based on *Indentity Server* or Microsofts *Azure AD*.
There are several reasons:
- The app requirements are quite limited. For example user self registration is not applicable.
- The out-of-the-box solutions will create a dependency on external code/services that are overcomplicated in view of the requirements.
