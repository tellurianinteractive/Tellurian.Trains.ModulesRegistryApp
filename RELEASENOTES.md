## Release Notes

#### Version 1.7.2
Release date 2022-09-29
- **Meeting Specific Waybills** disabled until feature is changed and tested.
- **Extended Menu** with links to *loco- and wagon card app* and *fast clock app*.
- **Bug fix** of meeting info, where venue was missing.

#### Version 1.7.1
Release date 2022-09-05
- **Waybill design** slightly improved.

#### Version 1.7.0
Release date 2022-09-04

This is a major release with an improved **waybill** handling. 
Many thanks to Fredrik Petterson, a Swedish FREMO-member, that suggested waybill handling similar to *Yellow Pages*.
In this release, this goal is partly implemented.
- **Customer Waybill** can now be generated for your freight customers. These are saved for later. 
The generation matches all your existing freights with freights at other module stations in the same scale, plus all external stations.
In upcoming releses of the Module Registry, you will be able to edit waybills for return freights, printed count etc. and
also create additional waybills.
- **Waybill printing** is now possible. In this relase, you can print all waybills for your station.
In upcoming releases, you will also be able to print each freight customer separately, and select what to print etc.

#### Version 1.6.21
Release date 2022-08-29
- **Sender Email** now verified so that invitations and password reset mails is not likley to end up in your spam folder.
- **Freigh Customer Waybills** has now the first step implemented: to be able to generate waybills for each freight customer.
These waybills will be saved for that customer.
>NOTE: In fortcoming releases, you will be able to edit and add waybills manually, and to preview and print them.

#### Version 1.6.20
Release date 2022-08-25
- **Bug fix** of password validation when containing 'W' or 'w'. Thanks to Detlef Born.
- **Bug fix** module meeting status.

#### Version 1.6.19
Release date 2022-08-22
- **Meeting Registration** now displays message if you try yo register too early.
- **Alerts** are now styled in a consequent way.

#### Version 1.6.18
Release date 2022-08-21
- **Meeting Registration** feedback for non-logged-in site visitors is improved, with instructions to how register.
- **Layout Registration** can now be disabled for registration by users of the Module Registry.
If all layouts in a module meeting are disabled for registration, the meeting will never appear to be open for registration.
- **Layout Help** is updated to reflect the new features introduced for meeting track layouts.

#### Version 1.6.17
Release date 2022-08-19
- **Help texts** corrections and additions.
- **Formatting** fixes in tables and other places.
- **Contact Person** added for *layout*
- Further preparations for the fortcoming waybill functionality.

#### Version 1.6.16
Release date 2022-08-18
- This release is a preparation phase for the fortcoming waybill functionality.

#### version 1.6.15
Release date 2022-08-17
- **Meeting** can now be flagged as *internal* for the organising group. 
Only logged in group members will see this meeting and can register.
- **Meeting Status** extended and show now if meeting is *open* or *closed* for registration.
A meeting is closed for registration when all layouts are closed for registration.

#### Version 1.6.14
Release date 2022-08-16
- **Menu items** previously publicly visible are now accessible only for logged in users.
- **FAQ** page added.
- **Text content** formatting improved for better readability.
- **Meeting** views are splitted over several pages: 1) meeting details, 2) layout details and 3) participants and registered modules.
- **Meeting** *isFremo* checkbox replaced with selecting a *group domain*, for example FREMO if the meeting is a FREMO-meeting.
- **Layout** can now be described in detail using *markdown*.
- **Completed translations** of all help- and information texts, 
except for *Terms of Use* that will continue to be in English only.

#### Version 1.6.13
Release date 2022-08-15
- **Meetings** can now be described with additional free texts for *details* and *accomodation* using *markdown*.
NOTE that similar functionality will be added soon for individual *meeting layouts*.
- **Operation Days** drop down now displays days in order, with the most common ones first, 
and the rest by the first day (issue #148)

#### Version 1.6.12
Release date 2022-08-09
- **Get Started** page added.
- **Help texts** for module meeting related things are now translated to all the supported languages.
- **API** extended with *layout* thematic operation period.

#### Version 1.6.11
Release date 2022-08-06
- **End profile** is now the new term for previous *module gable*. 
Thanks to Gino Damen, Klaus Weibezahn, and Dirk Witvrouwen.
The change is made not only int the user interface, but in all code and in the database.

#### Version 1.6.10
Release date 2022-08-04
- **Users** can now edit their *email address* and *city of residence* under **Settings**.
- **User rights** can now be changed by a country administrator when editing module owners form. 
- **Locked out user** can now be reset by a country administrator when editing module owners form.
- **Bug fix** of missing translations of *cargo packaging units*.

#### Version 1.6.9
Release dare 2022-07-17
- **Security update** with latest patches from Microsoft.

#### Version 1.6.8
Release date 2022-06-12
- **Bug fix** of issue #154 freight customers for external stations causing error. Thanks to Robert Halvarsson for reporting this.
- **Bug fix** of issue #142 group administrator cannot edit member's station customers. Thanks to Anders Östvall for reporting this.
- **Country and Flags** in external stations freight customer list only shows when all countries are shown.
- **Station List** now display *variant* or *package* label for the module associated with the station.

#### Version 1.6.7
Release date 2022-06-10
- **API Improvements** where users with API-access now can retrieve their API-key at the **Settings** page.
The routing is also improved. Currently the API supports *meetings* and *cargoes*.
- **Sorted Lists** of modules, stations, end profiles and module standards.
- **Reserved Loco Addresses** assigned by FREMO can now be entered for a *person*. Issue #152.
These addresses will be reserved for the person when registering for a meeting layout.
- **Help Texts Update** and some translations to all supported languages.

#### Version 1.6.6
Release date 2022-06-07
- **Representative station** for a *region*, which will be origin or destination of cargo for that region (issue #143).
- **List boxes data** optimised by using database views instead of *Entity Framework* complex queries.
- **Server Timeout** increased from 3 to 10 minutes, so the request to *Reload* should happen more infrequently.
- **Norwegian** now have the correct language code NB (and not NO), that will make the user interface to display in Norwegian bokmål.
- **Language Select** is now possible to override your browser's request language settings. Append *?culture=XX* to the url, where XX is the two-letter ISO language code.
The following languages are supported: 
  - DA=Danish, 
  - DE=German, 
  - EN=English, 
  - NB=Norwegian (bokmål),
  - SV=Swedish.
  > Example use German: https://moduleregistry.azurewebsites.net/?culture=DE
- **Selected Language** is now stored in cookie and overrides your browser's request language settings. 

#### Version 1.6.5
Release date 2022-05-01
- **Drop down menu** now shows on screens up to 1280 px. 
- **External stations** for user's country now show up directly without need for selecting country.

#### Version 1.6.4
Release date 2022-04-27
- **Module angle** now has one decimal.
- **Persons modules** now sorted alphabetical.
- **Bug fix** in cloning module data, *straight* value was not cloned.

#### Version 1.6.3
Release date 2022-04-26
- **Statistics** per country of usage.
- **Waybills** now also generated to/from station modules not present in a layout.
- **Waybills** now print *quantity* of *packaging unit* when type of quantity is *pieces*.
- **Wagon class** added in cargo customer overview.
- **Tool-tip** for *module status* added.
- **Missing translations** added.


#### Version 1.6.2
Release date 2022-04-05
- **Landscape season** added for modules. Issue #129.
- **Deletion of module meeting** can now be made by an *administrator*, and only if no modules are registered. 
Deletion also requests confirmation to avoid deletions by mistake. Issue #128.
- **Waybills created in own window**. Issue #135.
- **Updated Readme** to reflect status in April 2022.

#### Version 1.6.1
Release date 2022-02-28
- **Optimised waybill printing** where printing for a specific station 
only contains relevant waybills and no duplicates printed for other stations.
- **Region colour** for shadow stations destination and origin removed, 
because these are considered as a station in the layout.

#### Version 1.6.0
Release date 2022-02-27
- **New meeting registration** is ready.
- **Waybills** can now be printed again for layout stations.

#### Version 1.5.2
Release date 2022-02-11
- **Content security policy** updated.
- **PDF documents** now opens directly when clicked. Drawing files still needs to be downloaded before opening.
- **Start page** image lesser in height.
- **Loco preparation guide** updated. You find it under the **Tools** menu.

#### Version 1.5.1
Release date 2022-01-24
_ **Packaging units** added: *bundles*, *big sacks* and *rolls*.
- **Bug fix** of issue #138 Wrong station name in cargo station list.
- **Bug fix** of issue #139 Back-button causes app to crash.

#### Version 1.5.0
Release date 2022-01-15
- **Upgrade to .NET 6.0**, which has long time support to at least 2024.
- **Re-factored** to C# 10 syntax.
- **Bug fix** of file upload of documents and drawings.
- **Translations** for *Tools* page and *Loco preparation guide*.

#### Version 1.4.11
Release date 2022-01-13
- **Loco Preparation Guide** translated to German and Swedish.
- **Bug fix** of Invitation button in Module owners page.

#### Version 1.4.10
Release date 2022-01-10

**This release is starting point for implementing a improved module meeting registration.**
**Therefore, meeting registration is temporary disabled until new functionality is implemented.**

Other features in this release are:
- **Tools** added to menu, with a page containing useful links to tools and documentation to improve quality of module meetings.
- **Meeting info** now visible for anybody, also non registered visitors to the Module Registry.

#### Version 1.4.9
Release date 2021-10-20
- **Styling update** to Bootstrap 5.1.3.
- **Extended user status** added to *Module owners* list.
- **Viewing regions** is now selectable per country.
- **Viewing stations** with cargo customers is now selectable per country, and content has been restructured to be more consistent.
- **No waybills to print** now shows information for what you need to get waybills printed.
- **Repented invitation** now updates user's registration time.
- **Fixed** issues with Font Awesome not accessible from CDN.

#### Version 1.4.8
Release date 2021-10-05
- **Bug fix** of failing email validation when saving person's data.
- **Bug fix** of getting back to country list of persons when saving person's data.

#### Version 1.4.7
Release date 2021-09-27
- **Immediately** added as *cargo ready time*. Thanks to Urban Johansson for the idéa.
- **Security headers** applied for improved security of the application.

#### Version 1.4.6
Release date 2021-09-15
- **H0-US end profiles** added.
- **Group owned modules** can now be submitted to meetings by *group administrators* or group member that *may borrow the group's modules*. The latter is a new checkbox for group members.
- **Waybill typography** improved and *import agent* and *export agent* are now translated to origin and destination languages.
- **Bug fix** of making group-owned module to a station.

#### Version 1.4.5
Release date 2021-09-06
- **Cargo names** now also supported for Italian and French.
- **Waybills** now contain track or area for load and unload if this is specified for the cargo customer or cargo flow.
- **Bug fix** of issue #125 can't edit other member modules, even as group data administrator. Tanks Lars Ljungberg for reporting this.

#### Version 1.4.4
Release date 2021-09-05
- **End profile list** are now sorted by scale and end profile name.
- **Cargo flow** help text and field labels updated to better describe meaning of fields.
- **Cargo lists** now shows NMH-code and is sorted in NHM-code order. 
Read more about [NHM-codes](https://uic.org/freight/freight-IT/article/nhm).
- **Meeting dates** cannot be changed if any modules are registered or after seven days before first day of meeting. Help text is updated with this restriction.
Only *country administrators* will then be able to change dates.
- **Italy** added as country but no language support.
- **Bug fix** of group member administrator creating new persons in other countries to add to group.
- **Bug fix** of issue #122 creating new persons to add to a group. Thanks to Jérômee Chavel and Lars Ljungberg for reporting this.
- **Bug fix** of issue #123 creating new meeting. Thanks to Jérômee Chavel and Lars Ljungberg for reporting this.
- **Bug fix** of available modules for registration in meeting layout. Previously not all available modules was shown due to module visibility restrictions.
- **Bug fix** of user rights to modify group member.

#### Version 1.4.3
Release date 2021-09-04
- **Updated help** for modules.
- **Bug fix** of adding group members.

#### Version 1.4.2
Release date 2021-09-03
- **Meeting status** is colourised.
- **Meeting dates** cannot be changed more that 7 days, to permit minor date changes, but prevent reusing an old meeting as a new meeting.
- **Waybill sending days** now displayed in language of origin station.
- **Service release** with latest .NET security patches and bug fixes.

#### Version 1.4.1
Release date 2021-09-03
- **Waybill improvements** where *days*, *loading and unloading instructions* has been added, and the visual appearence is better.

#### Version 1.4.0
Release date 2021-09-03
- **Waybill print** for specific meeting layout.
- **Waybill print** for specific station in meeting layout.

#### Version 1.3.1
Release date 2021-09-02
- **Bug fix** of issue #109 access to editing meeting layouts. Thanks to Fredde Pettersson for reporting this.

#### Version 1.3.0
Release date 2021-08-18
- **List of modules** registered for a meeting layout can now be seen by any meeting participant.
- **External stations and freight customers** can be added by any user in their country of residence.
- **Improved name validation** of persons, modules etc.
- **SketchUp drawings** can be uploaded by selected users.
- **Bug fix** of adding cargo flow to station customer.
- **Service release** with latest .NET security patches and bug fixes.

#### Version 1.2.19
Release date 2021-08-11
- **Bug fix** of access to listing group members modules and stations. Thanks to Stefan Kloppenburg for reporting this.
- **Improved editing of cargo flow** by moving buttons up and adding new cargoflow at the top, and option to clone existing flow. Thanks to Stefan Kloppenburg for suggesting this.

#### Version 1.2.18
Release date 2021-08-09
- **Delete layout from meeting**, issue #105, is now added. Thanks to Fredde Pettersson for reporting this.
- **Password reset** show failed when actually successful. Thanks to Stefan Kloppenburg for reporting this.
- **Improved group list** now directly loads groups for user's country.
- **Bug fix** of #106 broken invite-link for group members.

#### Version 1.2.17
Release date 2021-08-03
- **Number of trough track** for *modules* can now be zero (thanks to Jonas Hjelm).
- **Max track speed** is raised to 350 km/h (thanks to Jonas Hjelm, a TGV enthustiast).
- **Overhead wire** feature can now be set to *planned* (thanks to Jonas Hjelm).

#### Version 1.2.16
Release date 2021-07-29
- **Edit Station Customer** is improved and corrected.

#### Version 1.2.15
Release date 2021-07-28
- **Packaging unit** can now be specified for a freight customer's cargo flow.
- **Module Registration Closing Date** added for *meeting layout*.
- **Password comfirnation** improved with new messagge if comfirmation fails.

#### Version 1.2.14
Release date 2021-07-27
- **Improved validation** of names, where some words now are considered valid with lower case initial letter.
- **Service release** with latest .NET security patches and bug fixes.

#### Version 1.2.13
Release date 2021-07-17
- **Available modules** for a meeting layout now displays all modules in each package.
- **Intivation mail** now clarifies that the app is not suitable for small screens.

#### Version 1.2.12
Release date 2021-07-12
- **Administrators** can now add modules to a meeting also after registration end date.
- **Bug fix** of file upload of larger files than 512 kB.
- **Translation fixes** of missing or added translations.
 
#### Version 1.2.11
Release date 2021-06-23
- **User's API-key** now change after confirmed password reset.
- **User's login email** now synched after changing person's email.
- **Start page content** for logged in user is changed. Release notes are now accessible via a link.

#### Version 1.2.10
Release date 2021-06-17
- **Service release** with latest .NET security patches and code cleanup.

#### Version 1.2.9
Release date 2021-06-12
- **Width** can now be specified for modules.
- **Length** of modules is now calculated as the sum of *straight* and *curved* lengths.
There is a new optional *straight* length field. The *length* field is now read-only.
- **Trainset** added as a *quantity unit*, and it is also possible to specify *max length* of the trainset.
- **Improvements** of some user interface functionality.

#### Version 1.2.8
Release date 2021-06-11
- **Other Wagon Class** can now be specified for each station customer's *cargo flow* 
if you want to override the default classes for the *cargo type*.
- **Transfer Module Ownership** makes it possibible to transfer part or whole ownership of a module
to another person. You can only transfer your own ownership and it cannot be undone.
- **Bug fix** of resetting password for persons with more than one email address.

#### Version 1.2.7
Release date 2021-05-26
- **Bug fix** of displaying cargo names for station customers.
- **Improved** meeting participant registration and cancellation.
- **Improved** German translations. Thanks to Stefan Kloppenburg.
- **Improved** English translation. Thanks to Daniel Jung.

#### Version 1.2.6
Release date 2021-05-17
- **Bug fix** of viewing meeting details.

#### Version 1.2.5
Release date 2021-05-15
- **Is Stand-Alone** added for modules. 
This shold be checked for modules that have legs enough to stand for themselves.
The default is that modules is stand-alone.
- **Delete** of cargo customer now asks to confirm delete or cancel the action.

#### Version 1.2.4
Release date 2021-05-14
- **Register group owned modules** to a meeting layout can now be made by any persion that is a data administrator of the group.
This fixes issue #93. Thanks to Fredrik Petterson for noticing this.

#### Version 1.2.3
Release date 2021-05-10
- **Module names** must now be unique for each owner. Either *name* or *name* and *variant* must be unique for each owner. 
A *name* without a *variant* is unique, so you can't have the same *name* with *variant*. All modules with same *name* must all have a *variant*.
This also fix the error of registering some modules to a meeting's layout.
Thanks to Fredrik Petterson for testing and reporting this.
- **Bug fix** of issue #91 where cloning a module failed.
- **Bug fix** of issue #90 error when displaying groups. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of issie #89 when removing module from meeting layout. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of listing *cargo* due to missing translations. It is now required to translate caro names into all supported languages.
Thanks to Jens Ehlers for reportning this.

#### Version 1.2.2
Release date 2021-05-05
- **Station freight customers** overview now improved so you have everything on one page, issue #86. Thanks to Fredrik Pettersson for insist on this.

#### Version 1.2.1
Release date 2021-05-05
- **Registration of modules** for a *layout* is ready to use. 
The *meeting organiser* can register any person's modules, and a normal *user* can register their own modules.

#### Version 1.2.0
Release date 2021-05-03
- **Registration of persons** to module meetings. The *meeting organiser* can register any person from any country, and a normal *user* can register themselves.
- **Registration of modules** for a *layout* is in **preview**. It does not yet save registered modules.
- **Browning groups** is improved for users that can see groups in different countries.

#### Version 1.1.5
Release date 2021-04-30
- **Bug fix** of issue #83 *error when viewing group modules*. This fix also made FREMO-name of group modules display correctly.

#### Version 1.1.4
Release date 2021-04-29
- **Meeting** summary information view added. Also publicly available. The *meeting* edit view now only available for authorized users. 
- **Latest registration date** for modules added for layouts. Each layout in a meeting can have different dates.
- **Make station** now gets a default name of associated module, but you can change it.
- **Module standards** page now made public.
- **Module end profiles** page now made public and with improved add/edit for administrators.
- **Cargo types** page now made public.
- **Regions** page now made public.
- **Document download** now working. You can download all documentation file types.

#### Version 1.1.3
Release date 2021-04-28
- **Make station** button added on the *module* edit form to make it more obvious that it has to be a station to add *tracks* and *freight customers*. 
Thanks to Seth Olofsson for notifying about this problem.
- **Freight customer** documentation corrected. Thanks to Fredrik Petterson to notifying this.
- **Bug fix** issue #81 viewing stations from group list. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of visibility of data for administrators regarding *modules*, *stations* and *freight customers*.

#### Version 1.1.2
Release date 2021-04-26
- **Better data visibility** which means that FREMO members will see all groups, group members and modules within the FREMO domain, regardless of country.
- **Delete station customer** is now possible. 
- **Delete group member** is now possible.
- **Bug fix** of issue #77 *Checkboxes linked together*. Thanks to Dag-Cato Skårvik for reporting this.
- **Bug fix** of issue #78 *Remove cargo flow*. Thanks to Benny Tjäder for reporting this.

#### Version 1.1.1
Release date 2021-04-22
- **Freight Customers** overview including supplied and consumed cargo types.
- **Bug fixes** in external *freight customers*

#### Version 1.1.0
Release date 2021-04-21
- **External stations** to describe real world stations with *freight customers* and *cargo flow*.
- **Component update** to latest releases.

#### Version 1.0.6
Release date 2021-04-19
- **Add group member** from other country, issue #52.
- **Help texts** added for *person*, *group*, *cargo types* and *meeting*.
- **Page headings** now contains a *back*-button and a *help*-button if a help text is available.
Layout of heading is also inproved.

#### Version 1.0.5
Release date 2021-04-17
- **Group Domain** can now be set for groups by administrators only. Currently, the *FREMO* domain is available.
Later, extended *data visibility* will be added for groups in the same domain.
- **Track Direction** now replaces *Is scheduled*. It is now possible to define a track as unidirectional. Thanks to Jürgen Riedl for the idea.
- **Modules** can now be a part of a *station*. The system ensures that there is at least one module linked. Thanks to Benny Tjäder for the idea.
- **Delete** of a module is now possible if a number of conditions are met. They are checked on the *delete*-page.
- **Help texts** improved and corrected.
- **Module list** graphical improvements showing *variant* or *box label*.
- **Api** for fecthing *cargo types* with translated names in supported langauges. You need a personal *API key* to test it.

#### Version 1.0.4
Release date 2021-04-15
- **Help texts** are now available in English for editing *modules*, *stations* and *station customer*. It's for evaluation by users before going further.
- **Back button** are now available on the pages for editing *modules*, *stations* and *station customer*. It's for evaluation by users before going further.
- **Cargo flow** colouring is improved,  issue [#64](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/64). Thanks to Fredrik Petterson for the idea.
- **Variant/Box label** now is displayed in *modules* lists, issue [#61](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/61). Thanks to Benny Tjäder for the idea.
- **Bug fix** of issue [#59 Cannot edit station data](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/59).

#### Version 1.0.3
Release date 2021-04-14
- **Bug fix** of issue [#55 Cannot edit station cargo or cargo customers](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/55).

#### Version 1.0.2
Release date 2021-04-13
- **Users** can now see more data: *groups* that you are member of and non-private *modules*, *stations* and *freight customers* in these groups.
- **Visibility** for *modules* means that the module owner now can select if other users should see it. Thanks to Thomas Woditsch and others for this suggestion.
>NOTE that *visibility* is currently only implemented for *private* and *group members*, and *group members* is default.
- **Theme** free text added for *layout*.
- **Cargo units** *trailer* and *container* added. Thanks to Alexander Ehn for this suggestion.
- **Loading/unloading** ready time can now be set to *not applicable*.
- **Web API** for *meetings* with *layouts* is available.
- **API key** access to *web API*. Users that want to try it out can request a personal *api key* through the FREMO forum.

#### Version 1.0.1
Release date 2021-04-12
- **Validation** improvements of *regions*, *station customers* and *station customer cargo flow*.
- **Bug fix** of issue #45 *The password reset link does not work*. Thanks to Alexander Ehn.
- **Contributing** guidelines updated.

## Version 1.0.0
Release date 2021-04-11

Module and station management are now feature complete, so its time to release the first version.
- **Freight Customers** for *stations* can now be added and maintained.
- **Cargo Flow** for each *freight customer* can now be added and maintained.

#### Version 0.9.5
Release date 2021-04-09
- **Bug fix** of user registration. Users may now create password again. Thanks to Seth Olofsson for reporting this.

#### Version 0.9.4
Release date 2021-04-07
- **Menu** made more compact.
- **Bug fix** of editing *stations*.

#### Version 0.9.3
- **Regions** in countries can now be defined. They are intended for cargo origins and destinations outside a *layout*. 
- **File upload** improved user experience.
- **Contribution guidelines** added [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/blob/master/CONTRIBUTING.md).

#### Version 0.9.2
Release date 2021-04-01
- **Stricter validation** of text in fields where only ordinary text should be written.
- **File upload** maximum file size now checked and aborted if file exceeds the limit.
- **Component update** to latest releases.

#### Version 0.9.1
Release date 2021-03-31
- **Upload** of *AutoCad* drawings of modules and PDF-documentation of modules and stations.

#### Version 0.9.0
Release date 2021-03-30
- **Meetings and Layouts** now available.
- **Bug fixes** of missing translations.

#### Version 0.8.9
Release date 2021-03-28
- **Exits** new name of end profiles in module edit form.
- **Bug fix** of issue #23 adding group owned stations.
- **Bug fix** of rounding length and speed for modules.
#### Version 0.8.8
Release date 2021-03-24
- **Cargo types** added with a first set of common items, translated to supported languages and coded according to NHM.
Administrators can edit and add cargo types. You should provide all translations. 
- **Easier to invite** group member to become user.

#### Version 0.8.7
Release date 2021-03-24
- **Database schema** changed. Thanks for idea from Jürgen Riedl.
- **Navigation improvements** between group members modules and stations.
- **Added east/west** direction of module entry. [Read more...](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/wiki/Module-Entries-and-Reachable-Tracks)
- **Bug fix** of missing module drop-down in station edit form.

#### Version 0.8.6
Release date 2021-03-22
- **Service release** with internal changes only. No functional changes. Please, report any problem!
- **Added message** in user settings for logged in users.
- **Bug fix** of rights in managing group owned modules.

#### Version 0.8.5
Release date 2021-03-21
- **Password reset** info added on start page.
- **Adding stations** now are more strict. The must be a module first in order to extends it as a station.
- **Countries** added: now persons in UK and NL can be added. No NL translation yet, though.
- **Bug fix** of issie #11 misaligned field. Thanks to Alexander Ehn.
- **Update** of system components.

#### Version 0.8.4
Release date 2021-03-20
- **Bug fix** of issue #7 when saving new modules. Thanks to Johan Dahlgren.
- **Bug fix** of langauge in user invitation mail. 
- **Type of Cargo** can now be managed in all supported languages.
- **NHM-codes** levels 1 and 2 imported to classify *type of cargo*.

#### Version 0.8.3
Release date 2021-03-16
- **Bug fix** of error when adding *module end profiles*.
- **Validation** added for *module end profiles*.
- **Changed** layout for adding *module end profiles* and *station tracks*.

#### Version 0.8.2
Release date 2021-03-11
- **Password reset** attemps are limited to a few times, and after that nothing will happen.

#### Version 0.8.1
Release date 2021-03-10
- **Area of Resposibility** for administrators on contact page to help users to find the right administrator. 
- **Group Membership** for groups that administrator also administer are now shown.
- **About** page now contains links to find out more.
- **Password reset** now available under *User settings*.
- **Bug fix** of adding new person.

Thanks to Daniel Bergkvist for the ideas to improve this information.
#### Version 0.8.0
Release date 2021-03-09
- **Group Member Administrator** can now add members to a group.
- **Group Data Administrator** can now add and edit all group members *modules* and *stations*.
- **Group Data Administrator** can now add and edit the groups *modules* and *stations*.

#### Version 0.7.3
Release date 2021-03-09
- **Bug fix** of invitation and registration.
- **Bug fix** of invitation language when not supported in Azure.

#### Version 0.7.2
Release date 2021-03-07
- **Contact page** now shows all administrators and in which country they are located.
- **Bug fix** of wrong message shown when saving unmodified *module* or *station* (thanks to Daniel Bergkqvist).
- **Bug fix** of search person for adding as group member.
- **Improved translations** (thanks Jérôme Chavel for German suggestions).
- **Improved navigation** with new buttons to quicker find your way around.

#### Version 0.7.1
Release date 2021-03-06
- **Stations** and **Tracks** are now validated when user edits.
- **Modules** can be cloned for easier create similar modules.
- **FREMO Owner Prefix** can now be entered for FREMO members. 

#### Version 0.7.0
Release date 2021-04-05
- **Stations** and **Tracks** can now be added and edited.
- **Note** as a free text is added on module.
- **Signal Feature** which may be *No*, *Optional* or *Fixed*. Replaces **Is Signal**.
- **Overhead Line Feature** which may be *No*, *Optional*, *Only Posts* or *Fixed*. Replaces **Catenary Description**. 
Any notes on type of catenary should be entered in new *Note* field.
- **Max Speed** of trains passing module is added. When the module is added to a *layout*, the max speed can be lowered but not exceeded.
- **Version** and **Box Label** fields for *modules* are now mutual exclusive. Only one of the two fields may be entered.
- **Bug fix** of module represents year. (Thanks to Jérôme Chavel).

#### Version 0.6.0
Release date 2021-03-04
- *Terms of use* updated.
- User now required to accept *terms of use* in order to use application.
- Additional data about modules: *catenary description*, *number of sections*
and *end plates* with *direction* and *end profiles*.
- Administration of *end profiles**.

#### Version 0.5.1
Release date 2021-02-28
- Bug fix of *password policy* (thanks to Daniel Bergqvist)
- Password policy translated and formatted for better readability.
- Less content in *module standard* list so lines don't break.

#### Version 0.5.0
Release date 2021-02-27
- Administrators can now add and edit modules.
- Edit group member now works.
- List any person's modules now works
- Additional improvements of style.

#### Version 0.4.2
Release date 2021-02-26

- Upgraded to Boostrap 5.
- Upgraded to Font Awesome 5.
- Toasts showing action results for *save* and *delete*.
- Additional styling of lists and forms.
- Improved user rights filtering on content.

#### Version 0.4.1
Release date 2021-02-25

- Improved appearence of forms.
- Added *variant* and *box label* to *module*.
- Some minor bug fixes.

#### Version 0.4.0
Release date 2021-02-23

- User can now view, create and edit *personal modules*.
- Administrators can now add and edit *module standards*.
- The most common *scales* are added.
- All *functional*- and *landscape* states are implemented.

#### Version 0.3.0
Release date 2021-02-21

- Added user permission filtering to all services.
- Prepared services and database for adding modules.
- Start page for logged in users now shows *release notes*.

#### Version 0.2.4
Release date 2021-02-20

- Improved user login status.
- Improved module owner state.
- Added user permissions filtering to *PersonService*.
- Bug fix of password policy (thanks to Uwe Stegemann).
- Correct app name for making broser shortcut (thanks to Uwe Stegemann).

#### Version 0.2.3
Release date 2021-02-19

- Inviter of person is now always the logged in user.
- Invitation text is refined.

#### Version 0.2.2
Release date 2021-02-19
- Added translations to German and Norwegian.
- Invitation now sent in receivers language.
- Added confirmation of sent invitation.
 
#### Version 0.2.1
Release date 2021-02-18

Added invitation of persons, mail sending and confirmation with setting password.

#### Version 0.2.0
Release date 2021-02-15

- Removed dependency of *Azure Active Directory B2C*. 
User validation is now made against the users in the database.
- Added preliminary *Terms of Use*.
- Improved user interface for selecting country.

#### Version 0.1.3
Release 2021-02-12

- Improved editing.

#### Version 0.1.2
Release 2021-02-08

- Added some administrative functions only availavle for administrators.

#### Version 0.1.1
Release 2021-01-15

- Release for verifying that the cloud setup is working and that users can sign on.
