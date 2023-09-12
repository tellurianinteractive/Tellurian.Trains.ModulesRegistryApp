# Security


The security features controls the user authentication and authorisation.

## Roles
There are two main roles:
- Ordinary users. A user can be group manager and/or a WiFRED manager. There roles must be assiged by an administrator.
- Administrators. An administrator can have rights to manage things in their country or residence or be a global administrator.

## Authorisation
The application uses claims based authorisation. 
There are extension methods for *ClaimsPrincipal* that simplifies to decide what a user may do.
There are also authorization policies for *User* and *Administrator* that can be used to limit access to pages.

## Authentication
Users are authenticated by retrieving the *primary email address* and *hashed password* from the **User** table.

## Security Policy Headers
In addition, the site uses a strict policy for what resources the app may access from the browser.

## Design Considerations
The site uses handcrafted code for authentication and authorisation. 
It is a deliberate choice to *not* use out-of-the-box solutins for example ASP.NET standard authentication, solution based on *Indentity Server* or Microsofts *Azure AD*.
There are several reasons:
- The app requirements are quite limited. For example user self registration is not applicable.
- The out-of-the-box solutions will create a dependency on external code/services that are overcomplicated in view of the requirements.