# Module Registry App
Welcome to the *open source* project for creating a registry for *model railway modules*
and a *module meeting* magagement tool.

The basic idea is to centralise data storage while decentralise data management. 
You find the application [here](https://moduleregistry.azurewebsites.net/). 
It is updated quite frequently, so check out the [release notes](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/blob/master/RELEASENOTES.md).

You need an invitation to create an account and log in. Contact your [country administrator](https://moduleregistry.azurewebsites.net/contacts). 
Read more in the [WIKI](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/wiki) and
follow the [discussions](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/discussions) about new features and other related things.

#### Goal
The project's man goal is to create an application 
for maintaning data about *model railway modules* with the following qualities:
- Handle all important aspects of module data to enable to plan meetings.
- Support for managing modules in any scale in any standard.
- Available to any user within the module railwayers community.
- Different levels of user access for flexible data management.
- Support any language that is required by the module railwayers community.
- Available as a cloud application.

#### Status August 2022
All functions for managing modules are now in place, including:
* Managing **your modules, stations and station freight customers**.
* **Transfer of module ownership** - part or whole - to other person or group.
* **Manage groups and group members**, including group member's modules, stations and station freight customers.
* Administrators can **manage meetings, external stations, regions, cargo types, module standards and module end profiles**.
* Users and meeting organisers can **register a participant** and add modules to a layout.
* **Create waybills** for individual or all stations at a meeting. 
Take a look at the [example waybills](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/tree/master/Examples) created for past real meetings.
* Domains **change group visibiliy** for groups within same domain. FREMO is domain.
* **User management** for administrators and regular users.
* **Help texts** for most forms with recommendation how to enter data.
* **Supported languages:**
  * in user interface: **English, German, Danish, Swedish and Norwegian**.
  * Cargo types in four additional languages: **French, Dutch, Italian and Polish**.
  * Waybills for **cross border freights are bi-language**, with sender and receiver information in respective language.

#### Forthcoming work
You can now see the planned work under [Projects](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/projects/2). 
You are welcome to contribute, there is a [guide](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/blob/master/CONTRIBUTING.md) describing different ways to to that.

## Related Projects
#### Timetable Planning App
The *Timetable Planning App* takes meeting planning further.
In the *Timetable Planning App*, you will be able finalize the planing by defining the streches 
between the layout's stations and the modules on these stretches in the order they appear.
From that you create timetabled lines, 
plan the trains, plan the circulation of rolling stock, 
make driver duties and can print the documentation needed for all participants.

Currently, a lot of work is made on the [experimental version](https://github.com/fjallemark/TimetablePlanningApp) 
where the database structure and the printed documentation is evaluated
based on planning actual meetings and user's feedback from those meetings. 
This version also can import XPLN-files.

The [online version](https://github.com/tellurianinteractive/Tellurian.Trains.TimetablePlanningApp) is only a skeleton app and
will not be further developed until the Module Registry has the required supporting features and the experimentat timetable planning app has
all reports moved to web technology.
