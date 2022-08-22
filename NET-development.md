# .NET Development
*By [Stefan Fj�llemark](https://github.com/fjallemark), Tellurian Interactive AB, Sweden, August 2022*

> *Updated 2022-08-17 with more information about SQL Server projects*

This article aim to describe technology choices, when developing software in modern .NET.
At the time of writing 2022, all new .NET development should use .NET 6 or later.

Modern .NET is fully open source; the framework libraries, the runtime, and the compilers.
You can build any type of applications on almost any platform. 
.NET also runs in the web browser in web assembly and on many single board computers.
This makes .NET a one stop shop where you can reuse a lot of your .NET skills for any type of application.

.NET Framework 4.x.x is Windows only and considered legacy and should not be used for new applications.
The last .NET Framework version is 4.8, and it will be maintained as long as Windows is supported.

### Languages
All languges on .NET uses the same runtime and the same type system, so you can usually combine libraries 
written in any .NET program with each other. There are some rules to follow to make it work.
- **C#** is the most common language used in .NET development. Is a C-family type of language and 
shares most of its properties with the Java language.
- **F#** is a functional first language with a pragmatic approach. F# has a much more succint syntax, 
which means that F# programs are significantly smaller that correponding C# programs.
- **VB.NET** is the modern variant of the BASIC programming language. 
It has almost the same features as C# and a simple and easy to understand syntax.

Regardless of .NET langauge you use, you will depend on the same .NET core library code.
So much of the knowledge is about getting the most out of the ready made software
that Microsoft and other library writers has to offer.

### Developmemt tools
This section only covers free tools for developing .NET applications.
- **Visual Studio Code** is now one of the most popular editor for writing software in almost any language.
It is a cross-platform application that runs om *Windows*, *macOS* and *Linux*.
Customisation is made by installing diffrerent plugins for a certain language or other purposes.
- **Visual Studio** has a *community edition* that is free and very capable for development .NET applications.
Customisation is made by choosing to install one or several *workloads*. 
There are also many useful free plugins to install from the Visual Studio Markeplace.
Visual Studio is a Windows only application, but there is also a separate version for *macOS*.
- **SQL Server** has a free *developer edition* that is a full featured version with some limitations. 
There is also a free *express edition* ideal for smaller applications. 
SQL Server also has a cloud version but no free tier, the smallest SQL Server cloud database
cost around �5 per month.
- **SQL Server Management Studio** is a free Windows application for SQL Server database creation and management.
It works with both local databases and cloud databases. It will be installed with the *SQL Server Developer Edition*.
- **Visual Studio** has great support for building and maintaing databases in *Microsoft SQL Server*. 
Firstly, you have similar access to the database as with *Sql Server Management Studio*, and 
secondly, there is a special *Database* project type, which you can maintain your SQL code
with version control and deploy updates and migrations to your existing database or create new ones.

### User interfaces
There are a lot of ways to create applications  with a user interface with .NET. 
The main factor is the target platform you intend to build software for:
- **Windows only**: You intend to develop an application that should only run on Windows machines.
- **Web**: The application should be accessible in a web browser.
- **Cross platform**: Your app should run on *ioS*, *Andriod*, *masOs* and/or *Windows*.

##### Technologies:
- **Windows Forms** is a simple and fast way to create Windows only applications with simple forms.
- **Windows Presentation Foundation** abbreviate **WPF** is a more advanced way to create 
user interfaces on Windows only. 
- **ASP.NET** is a framework for creating web applications.
ASP.NET applications can be hosted and run on all platforms supported by .NET, including cloud platforms.
Using the *Razor* syntax you can write web pages as a mix of HTML, CSS and C#.
- **Blazor** is a special web framework flavour of ASP.NET. 
You write components with a mix of HTML, CSS and C#. You can interop with JavaScript, and there are
many ready made components avaliable.
The components written in Blazor can be rendered in several environments: a) 
on a web server that updates the web user interface via websockets, b) running the component on web assembly in the browser, and c) in a cross platform MAUI application.
Blazor components can also be embedded in any other web application using any framework.
This makes Blazor components the most highly resuable user interface components in .NET.
- **Multiplatform Application User Interface** abbreviated **MAUI** is the newest technologi
aimed to make it easier to create apps that runs natively on  *ioS*, *Android*, *macOs* and *Windows*
using the native user interface of each of these platforms. MAUI is an evolution of *Xmamarin Forms*.
You can also embed *Blazor* components in a MAUI application, 
or  write a MAUI applications only using HTML, CSS and C# with Blazor.

### Data storage
There are several types of ways to store data and access it.
The main types of databases are:
- **Relation databases** that stores data in tables and usually accessed by the *SQL* query language.
Examples of databases are *Microsoft SQL Server*, *Postgress*, *My SQL* and *SQL Lite*.
- **Document databases** that stores whole structures of objects as one piece as a hierachical structure.
Examples of databases are *Mongo DB*, and *Azure Cosmos DB*.
- **Cloud storage** are of course databases but also other options as *blob storage* or *key/value storage*.

Databases adds complexity to your application. 
If the demand for data storage and querying is limited, simply storing the application data on disk 
in a single file or several files is definitley an option. 
With the built-in *JSON and XML serialization* technologies, writing and reading data to/from files on disk can be robust and geared for 
changes in your data structures.

##### Data access technologies
In order to access data in databases, a library for data access is required.
- **ADO.NET** is the basic library for all database access in .NET. 
It is quite low-level but also very performant, but you need to write a lot of boiler-plate code to use it.
ADO.NET also works for cloud databases.
- **Dapper** is is a simple object mapper for .NET and is virtually as fast as using a raw ADO.NET data reader.
Using Dapper reduces the amount of boiler-plate code to write, but still you have 
to manage much of the logic in your code, especially when saving data.
- **Entity Framework Core** is an object-relational mapper that enables working with databases. 
It eliminates the need for most of the data-access code that developers usually need to write.
It supports a variety of databases and makes database brand more or less transparent.
It also supports database scaffolding and versioning.

### Data communication
You can use raw communication standards like TCP and UDP using *sockets*. 
More often, you will use a framework that builds upon these standards.
- **ASP.NET** is also a framework to build web API's based on the HTTP protocol.
ASP.NET support almost any feature you want to use and is also highly customiseable.
- **gPRC** is a modern, high-performance, lightweight *remote procedure call* (RPC) framework.
gPRC is also cross-platform and compatible with services/clients written in any other langaue
on any other platform. .NET has a very efficient implementation and tooling to make
developent of both gRPC client and server applications.

### Cloud development
Modern .NET is created with the cloud in mind. You can easily deploy apps to the cloud, and .NET has 
support on several cloud platforms including *Microsoft Azure* and *Amazon Web Services (AWS)*.

You often get some level of free cloud computing, which usually is enough for applications with limited usage.
It is also easy to scale up with moderate costs.

The easiest  way to deploy to a cloud enviromnet is called *software as a service*, where 
the management of the underlying infrastucture is managed by the cloud provider.

### Code management
It is strongly recommended to use an online *source control system* to manage your code and other assets 
of your application. 

Support for **GitHub** is built-in in Visual Studio, which makes GitHub a recommended choice.
Besides code management, GitHub has a lot of other useful features to build and deploy your app, documentation,
handling issues, project plans etc. GitHub is free to use for most hobbyist projects.

### Learn more
- **[.NET](https://dotnet.microsoft.com/)**
- [**Visual Studio** all versions](https://visualstudio.microsoft.com/)
- **[IoT with .NET](https://dotnet.microsoft.com/en-us/apps/iot)**
- **[Entity Framework](https://docs.microsoft.com/en-us/ef/)**
- **[Dapper](https://dapper-tutorial.net/dapper)**
- **[gPRC on .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/)**
- **[GitHub](https://github.com/)**
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)**
- **[SQL Server Database projects](https://docs.microsoft.com/en-us/visualstudio/data-tools/creating-and-managing-databases-and-data-tier-applications-in-visual-studio)**