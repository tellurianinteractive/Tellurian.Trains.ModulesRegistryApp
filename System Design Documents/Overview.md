# System Design Overview

> WORK IN PROGRESS

This document describes the overall system design of the *Module Registry*.
It is intended as an introduction to a developer
that want to learn about the principles for how 
the system is designed.

## Development Environment
The application is developed with [Visual Studio](https://visualstudio.microsoft.com/).
The source code can be edited in other tools, but Visual Studio is the simplest way to build, test and deploy the application.

The requires skills, and what you need to develop the application is described in 
[contribution guidelines](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/blob/master/CONTRIBUTING.md).

## Runtime Environment
The web application is developed using *.NET* and *Blazor* server-side rendering of the user interface.
It is a cloud application running as an *Microsoft Azure App Service*, 
and uses a *Microsoft Azure SQL* database.
*Azure Key Vault* is used for storing secrets.
*Azure SignalR* is used for server-client communication.

To access the runtime environment, you need an *Azure* account and
also apply for rights to administer the application's features in *Azure Portal*.

## Projects 
Each project in the solution performs a specific task:
- **App** contains code all user interaction.
- **Data** contains code for the domain model, some business logic, and the mapping with the database.
- **Services** contains additional business logic and the interaction with the database.
- **Database** contains all SQL to create and fill the database with initial basic data. 

Other projects in the solution is for test or experimental purposes. 

## User Interface
The user interface is build with *Razor Components* and located in the *App* project:
- The **Pages** folder are the *page* components that are *routeable*.
Pages are organised in subfolders to make it easier to see what pages are related.
- The **Components** folder contains the *non-page* components, which are used as parts in a *page* component.
- The **Content** folder contains primarily *markdown* files with help-texts and other documentation.
The markdown files are localised in the fully supported languages. 
There is a component *ContentView* that makes it easy to display a specific markdown content in the current language.
- The **Extensions** folder contains resuable extensions functions for various things related to the user interface.

### Language Support and Localisation
Localisation is made in several ways:
- **Resouce Files** for short strings. Mostly used in the user interface for name of fields, headings, etc.
- **Markdown Files** for long texts, for example the help texts.
- **Database Columns** for each language. This is used for things where the users provide content in several languages.

Currently, only the tables *NHM* and *Cargo* contais multi-language content. There are a number of extension functions
that are intended to retrive such content in the current language.

Resource files (.RESX) is primarily located in the *App* and *Services* project.
Markdown files (.MD) is only located in the *App* project.

## Database Design
To work with the database, good knowledge of SQL and database administration is a must.
It is also important with knowledge of how to use *Microsoft SQL Management Studio* and
the features of the *database* project type in Visual Studio.

The database design is maintaned as CREATE statements, one file per table, view and stored procedure.
The table definitions also contains the definitions of the relations between tables (reference integrity), default values
views, and triggers. 

> NOTE: Triggers are only used to overriding the default behaviour of cascade deletes.

The Visual Studio database tooling checks the consistency of all SQL code at design time.
The tooling also makes it possible to make changes in the table/view definitions and publish 
them to an existing database, only updating things that actually changed.

>NOTE that *Entity Framework*'s migration feature is NOT used.

You must *manually* maintain consistency between the *domain objects*, their *database mapping* in Entity Framework,
and the actual table design and column naming. When changing a property name of a domain object, the corresponding 
column name must also be changed. This cannot be made simultaneously. 
- First change everything in code, both domain objects and SQL table definitions to match.
- Then publish the database. This will of course cause runtime errors in the application when accessing the affected table,
- Publish a new release of the app. Now database and app is in sync again. 

> IMPORTANT: Try do make this in small steps, one change at a time.

Sometimes publishing of the database fails, and the error must be investigated. 
Sometimes a change must be made with *SQL Management Studio*, and in this case, the change must also be
updated in the local CREATE statements. 
Always test that publishing the database works after these steps.




