# Contributing Guidelines

**Welcome to contribute to the development and documentation of the **Module Registry** web application.**

This page describes the ways you can contribute. 
In general, contribution requires you to have a [GitHub account](https://github.com/).
However, translations can be also submitted through e-mail.
For most other contributions, you need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).

## As Test User
As a test user you use the application and report back with:
* any functional problems you may find. This is reported as [issues](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues).
* suggestions for new features. You can start a [discussion](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/discussions) and write a description of what you want.

## As Translator
If you want the application in yet another language, you can make a translation.


#### Adding the new language to the app
There are two main ways to add your translation to the app:
- Create a *pull-request* using Git. This is the preferred way, so use it if you are familiar with it. 
You need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).
You can *check-out* the repository, add a translation and then submit a *pull-request*.
- Or just send translated files in a ZIP-file. 

#### Files to translate
Below a description of what files to translate. There are two types of files:
- **Markdown texts** (.md) are text files using the simple [*markdown* syntax](https://www.markdownguide.org/).
- **Resource files** (.resx) are text files using XML syntax.

##### Markdown texts
Markdown texts are longer texts with formatting. They are located [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Content/Markdown). 
1. Make a copy of the *English version*. These are the one without a language code in the file name, example *About.md*.
2. Rename your copied files to use your language's two letter language code, example *About.hu.md* for Hungarian.
3. Use a Markdown editor, it is important to preserve all formatting. There are free online of for download.
4. Translate the texts and save. It works using Google Translate, but the translation add spaces that ruins the format, so you will have to fix that manually.
After translation with Google, you can refine the translation if necessary.
5. Save the file locally. If you work with GitHub *pull requests**, add the translated files to your local repository.

> NOTE: Do not translate the file *TermsOfUse.md*.

##### Resource files
Resource files are XML-files with shorter texts or single words. However, the content is very noisy, and its important to not
change anything but the translated words/sentences. The files are located in three places:
* App.xx.resx [App/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Resources),
* Strings.xx.resx [Data/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Data/Resources),
* Strings.xx.resx [Services/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Services/Resources)

Where *xx* is the two-letter language code for the translated language.

1. Make a copy of the *English version* files. These are the one without a language code in the file name, example *Strings.md*.
2. Rename your copied files to use your language's two letter language code, example *Strings.hu.resx* for Hungarian.
3. Translate the texts and save. It is important to preserve the placeholders like {0} {1} etc.
4. Add the translated files to your local repository.

##### When the Translation Is Ready
If you are working with GitHub, commit locally and name it as *Translated to x* where x is the name of the langauge. 
Create a pull-request and push it to the remote repository.
Otherwise; send an email with the translated files.

## As Developer
If you want to contribute to the development of the application, there are two major paths:
- As an **App developer**; developing user interface and services to handle data.
- As a **Database administrator**; analyse an optimize performance, suggest improvements of how queries are made using *Entity Framework*.
- As a **DevOps specialist**; setting up an maintaining DevOps for application and database.

> NOTE: Full development with deployment and testing against a database must be made with Visual Studio on a Windows or machine.
> Changing source code can be made in any text editing tool on any platform, and the application can be build using the *dotnet* Command Line Interface,
> which comes with the .NET SDK installation for your platform.

##### Recommended Skills
To contribute, you need some experience in the following fields:
* [**.NET**](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five) development. We use the latest version of .NET, from nov 2021 it is .NET 6.0.
* Using [**Blazor**](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) as web user interface framework. We do <u>not</u> use JavaScript based frameworks like Angular, React or Vue. Front-end is written in **C#**, **HTML** and **CSS**.
* Back-end is written in **C#**. 
* Using [**Entity Framework**](https://docs.microsoft.com/en-us/ef/core/) as data access layer. Note that *mogrations* is not used.
* Using [**Sql Server**](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for local development and tests, and [**Azure SQL Database**](https://azure.microsoft.com/en-us/products/azure-sql/database/) as production database. The database structure and some logic are coded in **SQL** and is also checked in as [source code in GitHub](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Database/dbo).
* Using [**Visual Studio**](https://visualstudio.microsoft.com/) as development environment.
Visual Studio has excellent support for coding and publishing apps written in C#, HTML/CSS, and built in database design- and deployment tools. 
There is a free version available.
* Knowledge of version management in **GitHub**.
* Setting up DevOps using GitHub actions and Azure DevOps deployment slots. This is not in place, and help is needed.

If you have experiences from writing web applications using HTML/CSS and JAVA, 
you will propably get up to speed with *Blazor* and C# in relative short term.

#### Get started
>Please report back any problems with these instructions.
1. Download latest version of the free [*Visual Studio Community Edition*](https://visualstudio.microsoft.com/downloads/).
It normally comes bundled with *.NET SDK* and *Git*-tooling, or you can opt-in during installation. 
2. During installation, you should select the workloads:
- *ASP.NET and web development*,
- *Azure development*,
- *.NET desktop development*,
- *Data storage and processing*,
3. Install the following extensions from within Visual Studio:
- *ResXManager*,
- *Markdown Editor*, 
4. Fork the *Module Registry App* repository from GitHub. You can do that from within Visual Studio.
5. Open the solution and verify that it compiles.
6. You can now make modifications and pull-requests.

#### Working with databases
It is possible to develop and test against the production database. 
You will get read and write rights but initially no rights to modify the database schema.

>Please report back any problems with the instructions below.

In order to run the application locally, you also need to install *SQL Server*.
1. Download [*SQL Server Developer Edition*](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) and run the installation.
2. Build the *Database* project in the solution, and the publish it. 
3. In the *Publish dialog* you select your local server and giv the database a name of your choice.
4. The database is created and the schema is published to the database.
5. You need to fill some tables with data. In the folder *dbo/Scripts/Initial data* are SQL-scrips per table that you need to run. You can open a file, connect to your database, and execute these SQL-script.
6. You can also request a fresh copy of the production database using the [*sqlpackage.exe*](https://docs.microsoft.com/en-us/sql/tools/sqlpackage/) utility program and then import this create a local database.

After the database is created and filled with initial data, you need to configure the application to connect to it.
1. Store the database connection string in your local [app secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) using the key *ConnectionStrings:TimetablePlanningDatabase*.
2. In order to login you need to patch a record into the *User* table. You can create a hashed password by modifying *PasswordTests.cs* in project *Services.Tests*. Dont commit your changes in *PasswordTests.cs*!
3. Debug the application by starting it from within Visual Studio. It should open in a browser and you should be able to login. 

Optional installation
1. Install the *Entity Framework Core* [powershell tools](https://docs.microsoft.com/en-us/ef/core/cli/powershell).
