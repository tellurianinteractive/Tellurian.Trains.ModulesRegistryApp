# Contributing Guidelines

This page describes the ways you can contribute to the development of the **Module Registry** web application. 
All contribution requires you to have a [GitHub account](https://github.com/).
For all contributions, exept for *test user*, you need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).

## As Test User
As a test user you use the application and report back with:
* any functional problems you may find. This is reported as [issues](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues).
* suggestions for new features. You can start a [discussion](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/discussions) and write a description of what you want.

## As Translator
If you want the application in yet another language, you can make a translation.

#### Adding the new language to the app
There are two main ways to add your translation to the app:
- Create a *pull-request* using Git. This is the preferred way, so use it if you are familiar with it.
- Send translated files in a ZIP-file. 

##### Using Git and GitHub
You need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).
You can *check-out* the repository, add a translation and then submitting a *pull-request*.
Below a description of what files to translate.

#### Markdown texts
Markdown texts are longer texts with formatting. They are located [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Content/Markdown). 
1. Make a copy of the *English version*. These are the one without a language code in the file name, example *About.md*.
2. Rename your copied files to use your language's two letter language code, example *About.hu.md* for Hungarian.
3. Translate the texts and save. Use a Markdown editor and it is important to preserve all formatting. 
A good start is to translate using Google and then refine the translation.
4. Add the translated files to your local repository.

#### Resource files
Resource files are XML-files with shorter texts or single words. They are located in three places:
* App.xx.resx [App/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Resources),
* Strings.xx.resx [Data/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Data/Resources),
* Strings.xx.resx [Services/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Services/Resources)

Where *xx* is the two-letter language code for the translated language.

1. Make a copy of the *English version*. These are the one without a language code in the file name, example *Strings.md*.
2. Rename your copied files to use your language's two letter language code, example *Strings.hu.resx* for Hungarian.
3. Translate the texts and save. It is important to preserve the placeholders like {0} {1} etc.
4. Add the translated files to your local repository.

## As Developer
> Full development with deployment and testing against a database must be made with Visual Studio on a Windows machine.
> Changing source code can be made in any text editing tool on any platform, and the application can be build using the *dotnet* Command Line Interface.

If you want to contribute to the development of the application, there are two major paths:
- As an **app developer**; developing user interface and services to handle data.
- As a **database administrator**; analyse an optimize performance, suggest improvements of how queries are made using *Entity Framework*.

#### Skills
To contribute, you need some experience in the following fields:
* [**.NET**](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five) development. We use the latest version of .NET, from nov 2020 it is .NET 5.0.
* Coding in **C#, HTML, CSS** and **SQL**. Both backend and web user interface is written in C#, and <u>not</u> JavaScript.
* Using [**Blazor**](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) as web user interface framework. We do <u>not</u> use JavaScript based frameworks like Angular, React or Vue.
* Using [**Entity Framework**](https://docs.microsoft.com/en-us/ef/core/) as data access layer.
* Using [**Sql Server**](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for local development and tests, and [**Azure SQL Database**](https://azure.microsoft.com/en-us/products/azure-sql/database/) as production database.
* Using [**Visual Studio**](https://visualstudio.microsoft.com/) as development environment.
Visual Studio has excellent support for coding and publishing apps written in C#, HTML/CSS, and built in database design- and deployment tools. 

If you have experiences from writing web applications using HTML/CSS and JAVA, 
you will propably get up to speed with *Blazor* and C# in relative short term.

#### Get started
>Please report back any problems with this instruction.
1. Download latest version of the free [*Visual Studio Community Edition*](https://visualstudio.microsoft.com/downloads/).
It normally comes bundled with *.NET SDK* and *Git*-tooling, or you can opt-in during installation. 
2. During installation, you should select the workloads:
- *ASP.NET and web development*,
- *Azure development*,
- *.NET desktop development*,
- *Data storage and processing*,
- *.NET Core cross-platform development*.
3. Install the following extensions from within Visual Studio:
- *ResXManager*,
- *Markdown Editor*, 
3. Clone the *Module Registry App* repository from GitHub. You can do that from within Visual Studio.
4. Open the solution and verify that it compiles.
5. You can now make modifications and pull-requests.

#### Working with databases
>Please report back any problems with this instruction.

In order to run the application locally, you also need to install *SQL Server*.
1. Download [*SQL Server Developer Edition*](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) and run the installation.
2. Build the *Database* project in the solution, and the publish it. 
3. In the *Publish dialog* you select your local server and giv the database a name of your choice.
4. The database is created and the schema is published to the database.
5. You need to fill some tables with data. In the folder *dbo/Scripts/Initial data* are SQL-scrips per table that you need to run. 
You can open a file, connect to your database, and execute the SQL-script.

After the database is created and filled with initial data, you need to configure the application to connect to it.
1. Store the database connection string in your local [app secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) using the key *ConnectionStrings:TimetablePlanningDatabase*.
2. In order to login you need to patch a record into the *User* table. You can create a hashed password by modifying *PasswordTests.cs* in project *Services.Tests*.
3. Debug the application by starting it from within Visual Studio. It should open in a browser and you should be able to login. 

Optional installation
1. Install the *Entity Framework Core* [powershell tools](https://docs.microsoft.com/en-us/ef/core/cli/powershell).
