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
You can improve a translations in an already supported language, 
or if you want the application in yet another language, you can make a new complete translation.

> The preferred way is to create *pull requests* with translations is using [Visual Studio](https://visualstudio.microsoft.com/downloads/) with and these extensions:
> - **ResX Manager** makes it very easy to edit RESX-files for existing languages and adding new ones.
> - **Markdown Editor** makes is easy to edit *markdown* files, there is a preview pane for viewing the result.

### Improving the translation to an already supported language
Follow the instructions below. The only thing you don't need to do is adding new files.
Change files of the existing langauge and create a *pull request*.

### Adding the new language to the app
There are two main ways to add your translation to the app:
- Create a *pull-request* using Git. This is the preferred way, so use it if you are familiar with it. 
You need to now the [basics of GitHub](https://lab.github.com/githubtraining/introduction-to-github).
You can *check-out* the repository, add a translation and then submit a *pull-request*.
- Or just send translated files in a ZIP-file. 

### Files to translate
Below a description of what files to translate. There are two types of files:
- **Markdown texts** (.md) are text files using the simple [*markdown* syntax](https://www.markdownguide.org/).
- **Resource files** (.resx) are text files using XML syntax.

#### Markdown texts
Markdown texts are longer texts with formatting. They are located [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Content/Markdown). 
1. Make a copy of the *English version*. These are the one without a language code in the file name, example *About.md*.
2. Rename your copied files to use your language's two letter language code, example *About.hu.md* for Hungarian.
3. Use a Markdown editor, it is important to preserve all formatting. There are free online of for download.
4. Translate the texts and save. It works using Google Translate, but the translation add spaces that ruins the format, so you will have to fix that manually.
After translation with Google, you can refine the translation if necessary.
5. Save the file locally. If you work with GitHub *pull requests*, add the translated files to your local repository.

> NOTE: Do not translate the file *TermsOfUse.md*.

#### Resource files
Resource files are XML-files with shorter texts or single words. However, the content is very noisy, and its important to not
change anything but the translated words/sentences. The files are located in three places:
* App.xx.resx [App/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/App/Resources),
* Strings.xx.resx and Validators.xx.resx [Data/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Data/Resources),
* Strings.xx.resx [Services/Resources](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Services/Resources)

Where *xx* is the two-letter language code for the translated language.

1. Make a copy of the *English version* files. These are the one without a language code in the file name, example *Strings.md*.
2. Rename your copied files to use your language's two letter language code, example *Strings.hu.resx* for Hungarian.
3. Translate the texts and save. It is important to preserve the placeholders like {0} {1} etc.
4. Add the translated files to your local repository.

### When the Translation Is Ready
If you are working with GitHub, commit locally and comment it as *Translated to x* where x is the name of the langauge. 
Create a pull-request and push it to the remote repository.
Otherwise; send an email with the translated files.

## As Developer
If you want to contribute to the development of the application, there are two major paths:
- As an **App developer**; developing user interface and services to handle data.
- As a **Database administrator**; analyse an optimize performance, suggest improvements of how queries are made using *Entity Framework*.
- As a **DevOps specialist**; setting up an maintaining DevOps for application and database.

> NOTE: Full development and testing against a database is easiest with Visual Studio on a Windows machine. Deployment is automated, see *Releasing a new version* below.
> Changing source code can be made in any text editing tool on any platform, and the application can be build using the *dotnet* Command Line Interface,
> which comes with the .NET SDK installation for your platform.

##### Recommended Skills
To contribute, you need some experience in the following fields:
* [**.NET**](https://docs.microsoft.com/en-us/dotnet/core/dotnet-five) development. We use the [latest version of .NET](https://dotnet.microsoft.com/).
* Using [**Blazor**](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) as web user interface framework. We do <u>not</u> use JavaScript based frameworks like Angular, React or Vue. Front-end is written in **C#**, **HTML** and **CSS**.
* Back-end is written in **C#** and **SQL**. 
* Using [**Entity Framework**](https://docs.microsoft.com/en-us/ef/core/) as data access layer. Note that the *migrations* feature is <u>not</u> used.
* Using [**Sql Server**](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for local development and tests, and [**Azure SQL Database**](https://azure.microsoft.com/en-us/products/azure-sql/database/) as production database. The database structure and some logic are coded in **SQL** and is also checked in as [source code in GitHub](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/SourceCode/Database/dbo).
* Using [**Visual Studio**](https://visualstudio.microsoft.com/) as development environment.
Visual Studio has excellent support for coding and publishing apps written in C#, HTML/CSS, and built in database design- and deployment tools. 
There is a free version available.
* Knowledge of version management in **GitHub**.
* Using **GitHub Actions** for build, test and deployment, see *Releasing a new version* below.

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

#### Releasing a new version
The app is built, tested and deployed by the GitHub Actions workflow [`build-and-deploy.yml`](.github/workflows/build-and-deploy.yml).
Publishing from Visual Studio is not possible: password-based publishing (basic authentication and FTP) is turned off on the App Service.

- **Every push and pull request** to `master` is built and tested.
- **A push to `master` deploys** to production only when the version has been stepped up, that is, when `<Version>` in
[`Directory.Build.props`](Directory.Build.props) has no matching git tag `v<Version>` yet.

To release:
1. Step up `<Version>` and `<AssemblyVersion>` in `Directory.Build.props`.
2. If the change affects the users or the app's security, add a very short entry under `## Release <Version>` in
[`RELEASENOTES.md`](RELEASENOTES.md). Internal changes (refactoring, tests, documentation) get no entry.
3. Push to `master`. The workflow builds, tests, publishes and deploys, checks that the deployed files match the build,
and then creates the tag `v<Version>` and a GitHub release with the release notes.
4. Check the site in a browser. The App Service runs on the Free plan, so the first page load after a deploy can be slow.

Other things to know:
- **Redeploy** the current version: *Actions → Build and deploy → Run workflow* with *force* checked.
- **Roll back**: revert the faulty commit, step up the version and push.
- **Terms of Use**: `TermsOfUse.md` starts with a `<!-- LastModified: ... -->` line. This date decides whether users must accept
the terms again, so change it only when the terms really change. File timestamps can't be used, since a checkout resets them.
- **The database** is not deployed by the workflow. Schema changes are still published from Visual Studio (`Database.sqlproj`).

#### Working with databases
It is often better to work against a database with real data to see effects of changes and to catch real errors.
There are two options: 
- To get a local copy of the production database. 
Then you need to install [SQL Server Developer Edition](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) and 
request a fresh copy of the production database using the [*sqlpackage.exe*](https://docs.microsoft.com/en-us/sql/tools/sqlpackage/) 
utility program and then import this create a local database, or
- Work directly against the production database. Here are the options:
  - With read/write only. You can test and modify application features but not delete any data and not modify database schema.
  - With full rights to modify database schema. This option is only for experience and trusted developers.

For running the application, you must add a [local *secrets* file](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets), see example below.
The file must be located at *&percnt;APPDATA&percnt;\Microsoft\UserSecrets\ModulesRegistryDevelopmentSecrets\secrets.json*.

````
{
  "ConnectionStrings:TimetablePlanningDatabase": "Server=localhost\\mssqlserver01;Database=Tellurian.Trains.Database;Trusted_Connection=True;TrustServerCertificate=True",
  "TestUsername": "your account email",
  "TestPassword": "yout account password"
  }
````
