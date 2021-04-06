# Contributing Guidelines

This page describes the ways you can contribute to the development of the **Module Registry** web application. 
All contribution requires you to have a [GitHub account](https://github.com/).
For all contributions, exept for *test user*, you need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).

## Test User
As a test user you use the application and report back with:
* any functional problems you may find. This is reported as [issues](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues).
* suggestions for new features. You can start a [discussion](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/discussions) and write a description of what you want.

## Translator
If you want the application in yet another language, you can make a translation.
You need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).
You can *check-out* the repository, add a translation and then submitting a *pull-request*.
Below a description of what files to translate.
#### Markdown texts
Markdown texts are longer texts with formatting. They are located [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Content/Markdown). 
1. Make a copy of the *English version*. These are the one without a language code in the file name, example *About.md*.*
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

## Developer
If you want to contribute to the development of the application, you need the following:

#### Knowledge
To contribute, you need some experience in the following fields:
* **.NET** development. We use the latest version of .NET, from nov 2020 it is .NET 5.0.
* Coding in **C#, HTML, CSS** and **SQL**. Both backend and web user interface is written in C#, and <u>not</u> JavaScript. 
* Using **Entity Framework** as data access layer and **Blazor** as web user interface framework. We do <u>not</u> use JavaScript based frameworks like Angular, React or Vue.
* Using **Visual Studio** or **Visual Studio Code** as development environment.

If you have experiences from writing web applications using HTML/CSS and JAVA, 
you will propably get up to speed with *Blazor* and C# in relative short term.

#### Get started
1. Download the free *Visual Studio Community Edition*. 
2. During installation, you should select the workloads
*ASP.NET and web developmenmt*,
*Azure development*,
*.NET desktop development*,
*Data storange and processing*,
and *.NET Core cross-platform development*.
3. After installation, install the following extensions:
*ResXManager*,
*Markdown Editor*, 
and *GitHub Extensions for Visual Studio*. 
Some of these may already be pre-installed.
3. Clone the *Module Registry App* repository. This can be made from within Visual Studio.
4. Open the solution and verify that it compiles.
5. You can now make modifications and pull-requests.

#### Working with data
In order to run the application locally, you also need to install SQL Server.
1. Download *SQL Server Developer Edition* and run the installation.
2. If *SQL Management Studio* is not included, you may need to install it separately.
2. Open *SQL Management Studio* and create a database, you can name it whatsoever. 
3. Store the database connection string in your local [app secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) using the key *ConnectionStrings:TimetablePlanningDatabase*.
4. Install the *Entity Framework Core* [powershell tools}(https://docs.microsoft.com/en-us/ef/core/cli/powershell)
5. Open the *Module Registry App* solution in Visual Studio.
6. Create database creation script by running command *Script-Migration* as [documented here](https://docs.microsoft.com/en-us/ef/core/cli/powershell).
7. Open the script *SQL Management Studio* and run it.
8. In order to login you need to patch a record into the *User* table. You can create a hashed password by modifying *PasswordTests.cs* in project *Services.Tests*.
9. Debug the application by starting it from within Visual Studio. It should open in a browser and you should be able to login. 
