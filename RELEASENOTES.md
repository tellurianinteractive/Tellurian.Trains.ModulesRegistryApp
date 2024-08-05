# Release Notes
> Release notes are only published in English.
The release notes are summaries of important changes and fixes in each release. 
There is maximum one release per date, but this can be performed as one or several deployments during a day.
## Release 1.11.5
Release date 2024-08-05
- **Bug fix** of requiring non-existing *scale* when adding new modules.
- **Filtering and sorting** improved in several pages that displays many items.
## Release 1.11.4
Release date 2024-07-25
- **My Vehicles** now have more data about prototype and model, also prototype image and loco functions F0-F29.
- **My Vehicles** can now be filtered and sorted. The filter remains active until you change or remove it.
- **Component update** to latest versions of external components.
## Release 1.11.3
Release date 2024-07-15
- **My Vehicles** has relaxed validation and some fields made optional.
- **My Vehicles** import from CSV under test for eventual future release.
- **My Vehicles** can be sorted and filtered for *prototype* and *model*.
- **Component update** to latest versions of external components.
## Release 1.11.2
Release date 2024-07-03
- **Polishing off** some rough edges in the user interface for *Module* and *Module standard*.
- **Main theme** for modules, i.e. *Europe/US* now more structured in *module standards*.
- **Modules list** can now also be filtered by *scale* and *theme*.
- **Component update** to latest versions of external components.
## Release 1.11.1
Release date 2024-06-03
- **Vehicles** can now also be managed by administrators.
- **Mailing** can now also be made by administrators only to all active users in a selected country. 
- **Component update** to latest versions of external components.
## Release 1.11.0
Release date 2024-06-03
- **My Vehicles** is added. Now you can also enter data about your model locos. 
## Release 1.10.5
Release date 2024-05-13
- **Meeting waybills** can now be viewed and printed from the module registration page.
- **Bug fix** Upcoming meetings where the user is registered now visible under *my Meetings*. Thanks to Jerker Andersson for reporting this.
- **Bug fix** User can now edit meeting and layout participation, but not remove any registered modules after registration is closed.
  > NOTE: If necessary, meeting organiser can delete modules from meeting.
## Release 1.10.4
Release date 2024-05-07
- **Bug fix** of deleting module, that is part of an operational place, from meeting layout. 
Deleting a module that is part of an operational place (station) will now delete all modules within the same operational places as well as the operational places.
- **Fix** of issue #198. When a partly owned module is registerd on a meeting layout, the module will be visible
for all owners that are registered, not just the owner that registered the module to the layout.
- **Buttons** made invisible when printing page.
- **FREMO module** prefix and module number are now only available for persons with a FREMO member number.
- **Component update** to latest versions of external components.
## Release 1.10.3
Release date 2024-04-24
- **Bug fix** of mail button emaal link.
- **Bug fix** of printing layout internal waybills (works from 2024-04-08 16:40).
- **Component update** to latest versions of external components.
## Release 1.10.2
Release date 2024-04-06
- **Cargo** now have properties for *is express* and *requires cooling*. This will be noted on waybills, issue #197.
- **Waybill** will print *express* frights with red cargo name and have a red cross over line, issue #197.
- **Waybill** return can now be with same cargo as origin cargo. Thanks to Fredrik for the idea, issue #196.
- **Bug fix** of creating waybills to foreign external stations, now the external customers flows *imports* is respected.
- **Component update** to latest versions of external components.
## Release 1.10.1
Release date 2024-03-21
- **Mailing all participants** in a meeting or only all participants in a specific layout is now possible. 
When you click on *Send mail to all* it starts your preferred mail application and fills in all receivers as BCC (*blind carbon copy*),
that prevent you from spreading the participants email addresses to other receivers of the mail.
I also select yourself as primary receiver. Because it is an ordinary mail, you can write and attach any content you want.
  >NOTE: Only administrators including members with administrator rights in the meetings organiser group can send mail this way.
- **Component update** to latest versions of external components.
## Release 1.10.0
Release date 2024-03-19
- **Operational location** replaces the term *Station* in order to make it clear that all
types of places where timing is required should be defined as a *Operation location*. 
Documentation has also been updated to reflect this.
- **Operational location** can now have cargo customers disabled. This does not delete existing cargo customers, just disable to add new ones.
- **Translations** added for help texts for *Operation locations*.
## Release 1.9.19
Release date 2024-03-13
- **List of modules** for a meeting has layout improvements.
- **Meeting registration** information when not logged in is improved.
- **Security updates** of external components.
## Release 1.9.18
Release date 2024-03-11
- **Bug fix** of text that only shoud be visible if meeting registration is permitted.
- **Bug fix** of meeting duration for club events.
## Release 1.9.17
Release date 2024-02-26
- **Bug fix** of some links not working. Thanks to Jan Petter Kamnes for reporting this.
## Release 1.9.16
Release date 2024-02-24
- **Empty wagon order** improved and missing texts added. Fix of issue #195. Thanks to Fredrik Pettersson.
- **Security updates** of external components.
## Release 1.9.15
Release date 2024-02-21, thanks for your feedback!
- **My meetings** page added for easier way to edit meeting and layout details you are attending to.
- **User interface improvements**:
  - Easier to navigate between *modules* and *stations* pages.
  - Meeting registration now also possible on the meeting information page.
  - More descriptive text that tells you to also register in one or several meeting layouts.
- **Bug fix** of deleting *station*. It now also deletes it from past meetings.
- **Security updates** of external components.
## Release 1.9.14
Release date 2024-01-24
- **Meetings** of type *ClubEvent* or *Market* can now be given opening and closing times.
The opening times will be visible if it's a one day arrangement.
The icons for these meeting types are also fixed.
- **Bug fix** of issue #192 *Group administrators cannot create meeting*.
- **Bug fix** of meeting status tooltip, now showing what the icon means.
- **Translations** added for how to enter *external stations*.
- **Documentation** added for how to create cargo flows and waybills.
- **Printing** improvements, menu and top row will not be visible when printing any page. Also buttons are hidden when printing.
- **Buttons** now have tooltip when hovering over it.
- **Security update** to latest releases of external components.
## Release 1.9.13
Release date 2024-01-13
- **Access** to meeting registration is now resticted to users that have accepted *terms of use*.
- **Not Authorised** message now has texts added that describes possible reasons.
- **Security update** to latest releases of external components.
## Release 1.9.12
Release date 2023-12-02
- **Meeting list** now contains layout scales (only if layouts are added to the meeting).
## Release 1.9.11
Release date 2023-11-20
- **Is key required** issue #191 added for stations that require a key for loco driver to access sidings.
- **Bug fix** of #190 where waybill owner for group owned modules did not work.
- **Upgrade** to .NET 8.
## Release 1.9.10
Release date 2023-10-25
- **Security update** to latest releases of external components.
## Release 1.9.9
Release date 2023-10-17
- **Wagon Error Card** improved. Signature of issuer added.
- **Bug fix** of *empty car order* displaying German or English texts twice.
## Release 1.9.8
Release date 2023-10-15
- **Stations** can now be labelled as *harbour*. This will be used later to extend waybill matching.
- **User settings** now permits user to add their FREMO module user signature.
- **Bug fix** extends what is considerdd as valid characterns in text input fields.
- **Bug fix** of FREMO-module number format.
- **No Waybill** explanatory text is updated. This informs the reasons why waybills don't show in preview/print.
## Release 1.9.7
Release date 2023-10-14
- **Meeting** page improved: direct links to external meeting web pages, flags indicating country for meeting.
- **Waybill preview** for layout waybills now consider validity years for cargo customer/cargo flow.
- **Waybill translations** now also fully supports Polish and Hungarian. 
- **Empty Wagon Order** is now implemented (issue #167). You can print orders to request empty wagons for your outgoing freights. 
- **Buttons** now hides button text when page with under 1600 px to preserve horisontal space.
- **Bug fix** of error when saving cargo flow with incomplete data.
## Release 1.9.6
Release date 2023-10-09
- **Menu items** now organised in groups.
- **Module owners** menu item now changed to **Persons** to better reflect that a person can use the application in more ways (issue #185).
- **Waybills** now prints the owner(s) of the station as owner(s) of the waybill (issue #183).
- **Bug fix** of user status now showing all possible statuses.
- **Bug fix** of page history.
- **Bug fix** of language selector.
- **Fix** of permitted characters and capitalisation in station customer name. 
- **Fix** of language selection in waybills for unsupported languages for cargo names.
- **Time-out** improvements of user interface.
- **Delete**-button moved for *meetings* and *persons* (affects administrators).
## Release 1.9.5
Release date 2023-10-01
- **Terms of Use** updated. They hasn't been changed since the application was released and therefore needed to be updated to stay in sync with the actual use.
The changes are primarily regarding what data that is collected for persons and users plus a few other minor things to make it clearer.
**Note** than you must agree to the updated terms of use before you can continue to use the application.
- **Bug fix** of issue #188 file upload for module/station documentation not working.
## Release 1.9.4
Release date 2023-09-18
- **Region** must now be selected for station, except when the station is a shadow yard.
- **FREMO member number** now displayed in moduleowner list (available only for administrators).
- **FREMO member number** can now be entered with short four-digit or long seven-digit format. The four-digit format appends country number automatically.
## Release 1.9.3
Release date 2023-09-12
- **Bug fix** of issue #187 deleting modules registered to a meeting. Thanks to Seth Olofsson for reporting. 
- **Bug fix** of printing station specific waybills for layout. Thanks to Peter Alsén for reporting.
## Release 1.9.2
Release date 2023-09-04
- **Bug fix** of empty listbox for wiFRED owner. Thanks to Louis Michielsen for reporting this.
- **Invitation mail** German translation improved. Thanks to Detlef Born.
## Release 1.9.1
Release date 2023-08-31
- **Icons** for meeting status with explaning tootip.
- **Ownership** status also added for stations.
- **Bug fix** of module ownership transfer and assignment of assistant maintainer.
## Release 1.9.0
Release date 2023-08-30 - Milestone: Fully support for wiFRED management.
- **Language** of app can now be selected by clicking flags.
- **wiFREDs** can now be entered by FREMO members and be printed with QR-codes for marking your wiFRED. 
- **wiFRED** re-registration of deleted wiFRED now supported.
- **Module assistant** is a person that can be selected to get access to a modules maintenace without
being in a group or be a part owner of the station.
## Release 1.8.25
Release date 2023-08-28
- **wiFRED** list can now be created for printing. Includes QR-code with wiFRED data.
- **wiFRED** can now be deleted.
## Release 1.8.24
Release date 2023-08-26
- **WiFred validation** now available for wiFRED administrators.
- **Bug fix** of MAC-address validation.
- **Bug fix** of who can view and edit wiFRED data.
- **wiFRED Help** in German added.
- **Translation** improvements.
## Release 1.8.23
Release date 2023-08-25
- **Countries added**: Austria, Czech Republic, Slovakia and Hungary. 
It is now possible to add persons in these countries. NOTE: There is no translations for these countries.
- **wiFRED** registration improvements. NOTE: Currently only for test by selected users.
- **wiFRED Help** section added. Will be translated when text is final.
- **wiFRED owners** can only be persons with a FREMO member number.
## Release 1.8.22
Release date 2023-08-24
- **wiFred** registration pre-released for test by selected users.
- **FREMO** module owner prefix and member number kan now also be edited by users in *User Settings*.
- **Status icon** in module owner list now indicate kind of administrator.
## Release 1.8.21
Release date 2023-08-20
- **Bug fix** of user login email not updated when person email changed.
## Release 1.8.20
Release date 2023-08-17
- **Layouts** can now have a short name.
- **Layout waybills** for cross-border freights now have flags.
- **Waybill** format improvenments with dynamic font size for origins and destinations.
- **Bug fix** of external customer name not showed in station waybill list.
- **Bug fix** of file upload that occasionally failed saving complete document.
- **New front image** from the Milevsko-meeting 2023.

## Release 1.8.19
Release date 2023-08-16
- **Layout Vehicles** can now be retrieved in the web API.
- **Meetings API** can now be filtered on one or several countries.
- **User status** in adminstrator mode now also shows if user has created password but not yet logged in.
- **User's API-key** now is displayed in the administrators edit form for *persons*.
- **Bug fix** in API-key validation.
- **Bug fix** of issue #182 administrators can no logner register participants when no layout permits it.
- **Fix** of issue #180 replacing text with '-' when no package unit is specified.
- **Fix** of issue #181 replacing text with '-' when no loading/unloading ready time is specified.1.8.19

## Release 1.8.18
Release date 2023-08-01
- **Meetings API**: data now sorted after meeting start date.
- **Waybill preview** now faster.
- **Owner transfer** of modules can now be made to any person registered in the Module registry, also person in another country.
- **User status** now also shows if user has not accepted *Terms of Use*.

## Release 1.8.17
Release date 2023-07-29
- **Last country used** is now remembered (almost, it can be futher improved).
- **Waybill flag** now removed from layout internal waybills.
- **Bug fix** of waybill generation to/from shadow yard.
- **Bug fix** of persons name validation.

## Release 1.8.16
Release date 2023-07-27
- **Station region** can now be selected for all countries with regions defined. Thanks to Louis Michielsen for the suggestion.
- **Bux fix** of fast clock menu link when user is not logged in.

## Release 1.8.15
Release date 2023-06-02
- **Bug fix** of not registering *layout stations* correctly.
- **Meeting registration** can now be changed by administrators after meeting registration closed.
- **Module deletion** now works for administrators.

## Release 1.8.14
Release date 2023-05-20
- **Bug fix** of unhandled error submitting modules to a meeting layout.
- **Bug fix** of not all borrowable modules shows up as available.

## Release 1.8.13
Release date 2023-04-27
- **Menu** link to *fastclock* now includes user's given name if logged in.

## Release 1.8.12
Release date 2023-04-13
- **Meeting Participant** that was cancelled can now be re-registered.
- **Group Administrators** can now create a meeting with their group as organiser.

## Release 1.8.11
Release date 2023-03-25
- **Meeting information** improved.
- **Component update** to latest versions.

## Release 1.8.10
Release date 2023-03-06
- **Layout name** improved. Thanks to Erling Kjeldsen for noting this.
- **Loco Preparation Guide** corrections of typo errors and refinded translations.

## Release 1.8.9
Release date 2023-02-20
- **Waybill edit** now display correct station names. Thanks to Alexander Ehn for reporting this.
- **Loco Preparation Guide** has minor edits and format corrections.
- **Translations** added.

## Release 1.8.8
Release date 2023-02-06
- **Improved meeting registration** of modules to a layout. Some bugs fixed and improved page design.
- **Waybill design** adjustments.

## Release 1.8.7
Release date 2023-02-02
- **Track of Area** field for cargo load/unload location extended to 20 characters.
- **Cargo list** page is now paged and can be searched.
- **Bug fix** of meeting status *closed for registration* shown for meeting that does not allow registrations.
- **Bug fix** of translations for group member rights.

## Release 1.8.6
Release date 2023-01-15
- **Preliminary** added for meeting status.
- **Bug fix** to not show cancelled meeting participants as participating.
- **Bug fix** of meeting organisers and administrators not able to register anc cancel particpation after registration closing date. 
- **Component update** to latest versions available.
- **New style** for toast messages.

## Release 1.8.5
Release date 2023-01-03
- **Packaing units** added: *30' Bulk container*, *30' Tank container*, and *Trailer*.

## Release 1.8.4
Release date 2023-01-02
- **Bug fix** of waybill preview. Some waybills were duplicated, other were irrelevant.
- **Bug fix** of waybill preview error, caused by database failure. 

## Release 1.8.3
Release date 2022-12-15
- **Bug fixes** of displaying the expected waybills.
- **Component updates** to .NET 7.0.1 that fixes some Microsoft bugs.

## Release 1.8.2
Release date 2022-12-14
- **Waybills** can now be created for cargo types with NHM-code 0. These waybills will always be to or from a shadow station.
- **Meeting participation** can now be canceled. 
- **Top note** added for important messages.
- **Modules** table can now be searched and sorted on selected columns.

## Release 1.8.1
Release date 2022-12-13
- **Waybill** editing improved and hopefully more descriptive names on buttons.
- **Bug fix** of logic for updating waybills.
- **Bug fix** for displaying region of *other* station in waybills edit page.
- **Bug fix** of error when deleting station customers and station customer cargo, due to breaking changes in .NET 7.

## Release 1.8.0
Release date 2022-12-12
- **Upgrade to .NET 7**, which opens for some improvements in performance and features.
- **Module owners** table can now be sorted and searched.
- **Cargo types** table can now be sorted and searched.

## Release 1.7.13
Release date 2022-12-06
- **Waybill quantity changed**, bearer quantity are now always 1. Bearers are wagons, containers, trailers and trainsets.
- **Station delete** implemented.
- **Added robots.txt** disallowing all user-agents.
- **Techical updates** of application start-up.

## Release 1.7.12
Release date 2022-11-22
- **Bug fix** of *document upload*. Files where truncated and couldn't be read when downloaded.
Files that have this error need to be uploaded again.
- **Bug fix** to hide option to view layout modules when registration is not permitted.
- **Meeting participants** now sorted by name.

## Release 1.7.11
Release date 2022-10-26
- **Module Status** icon and title text improved. Now warns for incomplete or untested modules.
- **Documents** now opens in separate browser window.
- **Generating waybills** now also updates existing waybills when operation day changes.
- **Fix** of issue #158 by also showing station's variant in drop down list. Thanks to Benny Tjäder for suggestion.
- **Bug fix** of issue #162 to control which module that is the primary one for stations. Thanks to Benny Tjäder for bug report.
- **Bug fix** of issue #160 where both individual cargo flows and cargo customers now can be deleted. Thanks to Benny Tjäder for bug report.

## Release 1.7.10
Release date 2022-10-20
- **NHM-codes** added. Now, up to five significant digits can be selected.
## Release 1.7.9
Release date 2022-10-14
- **Security Update**: NuGet Elevation of Privilege Vulnerability.
- **Documentation** page added, which is an overview with links to other documentation content.
- -**Bug fix** of meeting not displayed as cancelled.

## Release 1.7.8
Release date 2022-10-10
- **New Waybill Design** with most label texts replaced by icons. This saves some space and maybe is better for international meetings.
- **Waybill Print** now also for individual cargo customers.
- **Main Theme** added for module standard, to be able to create waybills only for modules of same theme. The default theme is *Europe*.
- **Changed Waybill Generation**, where no waybills from your station's customers TO other module stations are created.
- **Language** on waybills improved by adding *package unit prepositions* and adding missing translations.
- **External stations** now can be given an international name if it differs from its domestic name.
- **External stations** can now be deleted by administrators.

## Release 1.7.7
Release date 2022-10-08
- **Login** now stays on the page you log in from and not redirects you to the start page.
- **Meeting Registration** now have a list of participants that meeting administrators can see, that makes editing participants easier.
- **Available Modules** for meeting participant is now divided in *My Modules* and *Other modules*.
- **Origin** for *empty return* waybills is now without region colouring, because the waybill will only be used when the station is present at the meeting.
- **Bug fix** of waybill quantity, quantity unit and package unit. It is now from the *receiver* and not the *sender* of cargo.
- **Wagons vs. Trainsets** on waybills is now changed if quantity unit is greater that one to *Trainset with X Wagons*.
- **Wagon Class** on waybills now displays the receiver's required class, and not the senders.
- > NOTE: Based on feedback from Benny Tjäder and Michael Bunka, there will be some future reworking on the definitions of:

## Release 1.7.6
Release date 2022-10-07
- **Quantity** is now displayed on the waybill including the unit of quantity. 
Quantity and quantity unit is set in the freight customers cargo flow. 
For example, a waybill can be of *10 m<sup>3</sup>*, which means that several waybills may go with one wagon.
- **Bug fix** of waybill sending days.
- **Bug fix** of colorization of waybill *track or area colour*. All freight customer cargo flows are also cleaned up of duplicated *track or area colour* settings.
- **Component update** for typeahed search fields.

## Release 1.7.5
Release date 2022-10-06
### Waybill Improvements
- **Changed size** from 52 mm width to 48 mm, you now print 12 waybills per page instead of previous 10.

Options to set for each waybill in your freight customers list:
- **Empty return** waybills can now be created. Just check *Has empty return* for the selected waybill.
- **Number to print** can now be set for each waybill. Set to zero disables printing.
- **Print per operating day** is now possible. The waybill will be printed according the union set of operating days given for 
the origin and destination freight customers cargo flow.

> NOTE: Printing options adds up. Number to print x operating days x 2 for empty returns.
## Release 1.7.4
Release date 2022-10-03
- **Bulk** added as goods packaging unit.
- **Food** has now a separate entry in meeting description.
- **Module list** added for overview.
- **Bug fix** of downloaded file names. Removed trailing spaces so they can be opened in appropriate app.

## Release 1.7.3
Release date 2022-10-01
- **Group member** may give permission that group members may borrow his/her modules for module meetings.
- **Module owners** now shows up in module registration page for a meeting layout.
- **Bug fix** of deleting freight customer cargo flow. Related waybills will now also be deleted.

## Release 1.7.2
Release date 2022-09-29
- **Meeting Specific Waybills** disabled until feature is changed and tested.
- **Extended Menu** with links to *loco- and wagon card app* and *fast clock app*.
- **Bug fix** of meeting info, where venue was missing.

## Release 1.7.1
Release date 2022-09-05
- **Waybill design** slightly improved.

## Release 1.7.0
Release date 2022-09-04

This is a major release with an improved **waybill** handling. 
Many thanks to Fredrik Petterson, a Swedish FREMO-member, that suggested waybill handling similar to *Yellow Pages*.
In this release, this goal is partly implemented.
- **Customer Waybill** can now be generated for your freight customers. These are saved for later. 
The generation matches all your existing freights with freights at other module stations in the same scale, plus all external stations.
In upcoming releases of the Module Registry, you will be able to edit waybills for return freights, printed count etc. and also create additional waybills.
- **Waybill printing** is now possible. In this release, you can print all waybills for your station.
In upcoming releases, you will also be able to print each freight customer separately, and select what to print etc.

## Release 1.6.21
Release date 2022-08-29
- **Sender Email** now verified so that invitations and password reset mails is not likely to end up in your spam folder.
- **Freight Customer Waybills** has now the first step implemented: to be able to generate waybills for each freight customer.
These waybills will be saved for that customer.

## Release 1.6.20
Release date 2022-08-25
- **Bug fix** of password validation when containing 'W' or 'w'. Thanks to Detlef Born.
- **Bug fix** module meeting status.

## Release 1.6.19
Release date 2022-08-22
- **Meeting Registration** now displays message if you try to register too early.
- **Alerts** are now styled in a consequent way.

## Release 1.6.18
Release date 2022-08-21
- **Meeting Registration** feedback for non-logged-in site visitors is improved, with instructions to how register.
- **Layout Registration** can now be disabled for registration by users of the Module Registry.
If all layouts in a module meeting are disabled for registration, the meeting will never appear to be open for registration.
- **Layout Help** is updated to reflect the new features introduced for meeting track layouts.

## Release 1.6.17
Release date 2022-08-19
- **Help texts** corrections and additions.
- **Formatting** fixes in tables and other places.
- **Contact Person** added for *layout*.
- Further preparations for the forthcoming waybill functionality.

## Release 1.6.16
Release date 2022-08-18
- This release is a preparation phase for the forthcoming waybill functionality.

## Release 1.6.15
Release date 2022-08-17
- **Meeting** can now be flagged as *internal* for the organizing group. 
Only logged in group members will see this meeting and can register.
- **Meeting Status** extended and show now if meeting is *open* or *closed* for registration.
A meeting is closed for registration when all layouts are closed for registration.

## Release 1.6.14
Release date 2022-08-16
- **Menu items** previously publicly visible are now accessible only for logged in users.
- **FAQ** page added.
- **Text content** formatting improved for better readability.
- **Meeting** views are splitted over several pages: 1) meeting details, 2) layout details and 3) participants and registered modules.
- **Meeting** *isFremo* checkbox replaced with selecting a *group domain*, for example FREMO if the meeting is a FREMO-meeting.
- **Layout** can now be described in detail using *markdown*.
- **Completed translations** of all help- and information texts, except for *Terms of Use* that will continue to be in English only.

## Release 1.6.13
Release date 2022-08-15
- **Meetings** can now be described with additional free texts for *details* and *accommodation* using *markdown*.
> NOTE that similar functionality will be added soon for individual *meeting layouts*.
- **Operation Days** drop down menu now displays days in order, with the most common ones first, and the rest by the first day (issue #148).

## Release 1.6.12
Release date 2022-08-09
- **Get Started** page added.
- **Help texts** for module meeting related things are now translated to all the supported languages.
- **API** extended with *layout* thematic operation period.

## Release 1.6.11
Release date 2022-08-06
- **End profile** is now the new term for previous *module gable*. 
Thanks to Gino Damen, Klaus Weibezahn, and Dirk Witvrouwen.
The change is made not only in the user interface, but in all code and in the database.

## Release 1.6.10
Release date 2022-08-04
- **Users** can now edit their *email address* and *city of residence* under **Settings**.
- **User rights** can now be changed by a country administrator when editing module owners form. 
- **Locked out user** can now be reset by a country administrator when editing module owners form.
- **Bug fix** of missing translations of *cargo packaging units*.

## Release 1.6.9
Release dare 2022-07-17
- **Security update** with latest patches from Microsoft.

## Release 1.6.8
Release date 2022-06-12
- **Bug fix** of issue #154 freight customers for external stations causing error. Thanks to Robert Halvarsson for reporting this.
- **Bug fix** of issue #142 group administrator cannot edit member's station customers. Thanks to Anders Östvall for reporting this.
- **Country and Flags** in external stations freight customer list only shows when all countries are shown.
- **Station List** now display *variant* or *package* label for the module associated with the station.

## Release 1.6.7
Release date 2022-06-10
- **API Improvements** where users with API-access now can retrieve their API-key at the **Settings** page.
The routing is also improved. Currently the API supports *meetings* and *cargoes*.
- **Sorted Lists** of modules, stations, end profiles and module standards.
- **Reserved Loco Addresses** assigned by FREMO can now be entered for a *person*. Issue #152.
These addresses will be reserved for the person when registering for a meeting layout.
- **Help Texts Update** and some translations to all supported languages.

## Release 1.6.6
Release date 2022-06-07
- **Representative station** for a *region*, which will be origin or destination of cargo for that region (issue #143).
- **List boxes data** optimized by using database views instead of *Entity Framework* complex queries.
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

## Release 1.6.5
Release date 2022-05-01
- **Drop down menu** now shows on screens up to 1280 px. 
- **External stations** for user's country now show up directly without need for selecting country.

## Release 1.6.4
Release date 2022-04-27
- **Module angle** now has one decimal.
- **Persons modules** now sorted alphabetical.
- **Bug fix** in cloning module data, *straight* value was not cloned.

## Release 1.6.3
Release date 2022-04-26
- **Statistics** per country of usage.
- **Waybills** now also generated to/from station modules not present in a layout.
- **Waybills** now prints *quantity* of *packaging unit* when type of quantity is *pieces*.
- **Wagon class** added in cargo customer overview.
- **Tool-tip** for *module status* added.
- **Missing translations** added.


## Release 1.6.2
Release date 2022-04-05
- **Landscape season** added for modules. Issue #129.
- **Deletion of module meeting** can now be made by an *administrator*, and only if no modules are registered. 
Deletion also requests confirmation to avoid deletions by mistake. Issue #128.
- **Waybills created in own window**. Issue #135.
- **Updated Readme** to reflect status in April 2022.

## Release 1.6.1
Release date 2022-02-28
- **Optimized waybill printing** where printing for a specific station only contains relevant waybills and no duplicates printed for other stations.
- **Region colour** for shadow stations destination and origin removed, because these are considered as a station in the layout.

## Release 1.6.0
Release date 2022-02-27
- **New meeting registration** is ready.
- **Waybills** can now be printed again for layout stations.

## Release 1.5.2
Release date 2022-02-11
- **Content security policy** updated.
- **PDF documents** now opens directly when clicked. Drawing files still needs to be downloaded before opening.
- **Start page** image lesser in height.
- **Loco preparation guide** updated. You find it under the **Tools** menu.

## Release 1.5.1
Release date 2022-01-24
_ **Packaging units** added: *bundles*, *big sacks* and *rolls*.
- **Bug fix** of issue #138 Wrong station name in cargo station list.
- **Bug fix** of issue #139 Back-button causes app to crash.

## Release 1.5.0
Release date 2022-01-15
- **Upgrade to .NET 6.0**, which has long time support to at least 2024.
- **Re-factored** to C# 10 syntax.
- **Bug fix** of file upload of documents and drawings.
- **Translations** for *Tools* page and *Loco preparation guide*.

## Release 1.4.11
Release date 2022-01-13
- **Loco Preparation Guide** translated to German and Swedish.
- **Bug fix** of Invitation button in Module owners page.

## Release 1.4.10
Release date 2022-01-10

**This release is starting point for implementing an improved module meeting registration.**
**Therefore, meeting registration is temporarily disabled until new functionality is implemented.**

Other features in this release are:
- **Tools** added to menu, with a page containing useful links to tools and documentation to improve the quality of module meetings.
- **Meeting info** now visible for anyone, also non registered visitors to the Module Registry.

## Release 1.4.9
Release date 2021-10-20
- **Styling update** to Bootstrap 5.1.3.
- **Extended user status** added to *Module owners* list.
- **Viewing regions** is now selectable per country.
- **Viewing stations** with cargo customers is now selectable per country, and content has been restructured to be more consistent.
- **No waybills to print** now shows information for what you need to get waybills printed.
- **Repented invitation** now updates user's registration time.
- **Fixed** issues with Font Awesome not accessible from CDN.

## Release 1.4.8
Release date 2021-10-05
- **Bug fix** of failing email validation when saving person's data.
- **Bug fix** of getting back to country list of persons when saving person's data.

## Release 1.4.7
Release date 2021-09-27
- **Immediately** added as *cargo ready time*. Thanks to Urban Johansson for the idea.
- **Security headers** applied for improved security of the application.

## Release 1.4.6
Release date 2021-09-15
- **H0-US end profiles** added.
- **Group owned modules** can now be submitted to meetings by *group administrators* or group member that *may borrow the group's modules*. The latter is a new checkbox for group members.
- **Waybill typography** improved and *import agent* and *export agent* are now translated to origin and destination languages.
- **Bug fix** of making group-owned module to a station.

## Release 1.4.5
Release date 2021-09-06
- **Cargo names** now also supported for Italian and French.
- **Waybills** now contain track or area for load and unload if this is specified for the cargo customer or cargo flow.
- **Bug fix** of issue #125 can't edit other member modules, even as group data administrator. Tanks Lars Ljungberg for reporting this.

## Release 1.4.4
Release date 2021-09-05
- **End profile list** are now sorted by scale and end profile name.
- **Cargo flow** help text and field labels updated to better describe the meaning of the fields.
- **Cargo lists** now shows NMH-code and is sorted in NHM-code order. 
Read more about [NHM-codes](https://uic.org/freight/freight-IT/article/nhm).
- **Meeting dates** cannot be changed if any modules are registered or after seven days before first day of meeting. Help text is updated with this restriction.
Only *country administrators* will then be able to change dates.
- **Italy** added as country but no language support.
- **Bug fix** of group member administrator creating new persons in other countries to add to group.
- **Bug fix** of issue #122 creating new persons to add to a group. Thanks to Jérômee Chavel and Lars Ljungberg for reporting this.
- **Bug fix** of issue #123 creating new meeting. Thanks to Jérômee Chavel and Lars Ljungberg for reporting this.
- **Bug fix** of available modules for registration in meeting layout. Previously not all available modules were shown due to module visibility restrictions.
- **Bug fix** of user rights to modify group member.

## Release 1.4.3
Release date 2021-09-04
- **Updated help** for modules.
- **Bug fix** of adding group members.

## Release 1.4.2
Release date 2021-09-03
- **Meeting status** is colorized.
- **Meeting dates** cannot be changed more that 7 days, to permit minor date changes, but prevent reusing an old meeting as a new meeting.
- **Waybill sending days** now displayed in language of origin station.
- **Service release** with latest .NET security patches and bug fixes.

## Release 1.4.1
Release date 2021-09-03
- **Waybill improvements** where *days*, *loading and unloading instructions* has been added, and the visual appearance is better.

## Release 1.4.0
Release date 2021-09-03
- **Waybill print** for specific meeting layout.
- **Waybill print** for specific station in meeting layout.

## Release 1.3.1
Release date 2021-09-02
- **Bug fix** of issue #109 access to editing meeting layouts. Thanks to Fredde Pettersson for reporting this.

## Release 1.3.0
Release date 2021-08-18
- **List of modules** registered for a meeting layout can now be seen by any meeting participant.
- **External stations and freight customers** can be added by any user in their country of residence.
- **Improved name validation** of persons, modules etc.
- **SketchUp drawings** can be uploaded by selected users.
- **Bug fix** of adding cargo flow to station customer.
- **Service release** with latest .NET security patches and bug fixes.

## Release 1.2.19
Release date 2021-08-11
- **Bug fix** of access to listing group members modules and stations. Thanks to Stefan Kloppenburg for reporting this.
- **Improved editing of cargo flow** by moving buttons up and adding new cargo flow at the top, and option to clone existing flow. Thanks to Stefan Kloppenburg for suggesting this.

## Release 1.2.18
Release date 2021-08-09
- **Delete layout from meeting**, issue #105, is now added. Thanks to Fredde Pettersson for reporting this.
- **Password reset** show failed when actually successful. Thanks to Stefan Kloppenburg for reporting this.
- **Improved group list** now directly loads groups for user's country.
- **Bug fix** of #106 broken invite-link for group members.

## Release 1.2.17
Release date 2021-08-03
- **Number of through track** for *modules* can now be zero (thanks to Jonas Hjelm).
- **Max track speed** is raised to 350 km/h (thanks to Jonas Hjelm, a TGV enthusiast).
- **Overhead wire** feature can now be set to *planned* (thanks to Jonas Hjelm).

## Release 1.2.16
Release date 2021-07-29
- **Edit Station Customer** is improved and corrected.

## Release 1.2.15
Release date 2021-07-28
- **Packaging unit** can now be specified for a freight customer's cargo flow.
- **Module Registration Closing Date** added for *meeting layout*.
- **Password confirmation** improved with new message if confirmation fails.

## Release 1.2.14
Release date 2021-07-27
- **Improved validation** of names, where some words now are considered valid with lower case initial letter.
- **Service release** with latest .NET security patches and bug fixes.

## Release 1.2.13
Release date 2021-07-17
- **Available modules** for a meeting layout now displays all modules in each package.
- **Invitation mail** now clarifies that the app is not suitable for small screens.

## Release 1.2.12
Release date 2021-07-12
- **Administrators** can now add modules to a meeting also after registration end date.
- **Bug fix** of file upload of larger files than 512 kB.
- **Translation fixes** of missing or added translations.
 
## Release 1.2.11
Release date 2021-06-23
- **User's API-key** now change after confirmed password reset.
- **User's login email** now synched after changing person's email.
- **Start page content** for logged in user is changed. Release notes are now accessible via a link.

## Release 1.2.10
Release date 2021-06-17
- **Service release** with latest .NET security patches and code cleanup.

## Release 1.2.9
Release date 2021-06-12
- **Width** can now be specified for modules.
- **Length** of modules is now calculated as the sum of *straight* and *curved* lengths.
There is a new optional *straight* length field. The *length* field is now read-only.
- **Trainset** added as a *quantity unit*, and it is also possible to specify *max length* of the trainset.
- **Improvements** of some user interface functionality.

## Release 1.2.8
Release date 2021-06-11
- **Other Wagon Class** can now be specified for each station customer's *cargo flow* if you want to override the default classes for the *cargo type*.
- **Transfer Module Ownership** make it possible to transfer part or whole ownership of a module to another person. You can only transfer your own ownership and it cannot be undone.
- **Bug fix** of resetting password for persons with more than one email address.

## Release 1.2.7
Release date 2021-05-26
- **Bug fix** of displaying cargo names for station customers.
- **Improved** meeting participant registration and cancellation.
- **Improved** German translations. Thanks to Stefan Kloppenburg.
- **Improved** English translation. Thanks to Daniel Jung.

## Release 1.2.6
Release date 2021-05-17
- **Bug fix** of viewing meeting details.

## Release 1.2.5
Release date 2021-05-15
- **Is Stand-Alone** added for modules. 
This should be checked for modules that have legs enough to stand for themselves.
The default is that a module is stand-alone.
- **Delete** of cargo customer now asks to confirm delete or cancel the action.

## Release 1.2.4
Release date 2021-05-14
- **Register group owned modules** to a meeting layout can now be made by any person that is a data administrator of the group.
This fixes issue #93. Thanks to Fredrik Petterson for noticing this.

## Release 1.2.3
Release date 2021-05-10
- **Module names** must now be unique for each owner. Either *name* or *name* and *variant* must be unique for each owner. 
A *name* without a *variant* is unique, so you can't have the same *name* with *variant*. All modules with same *name* must all have a *variant*.
This also fixes the error of registering some modules to a meeting's layout.
Thanks to Fredrik Petterson for testing and reporting this.
- **Bug fix** of issue #91 where cloning a module failed.
- **Bug fix** of issue #90 error when displaying groups. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of issue #89 when removing module from meeting layout. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of listing *cargo* due to missing translations. It is now required to translate cargo names into all supported languages.
Thanks to Jens Ehlers for reporting this.

## Release 1.2.2
Release date 2021-05-05
- **Station freight customers** overview now improved so you have everything on one page, issue #86. Thanks to Fredrik Pettersson for insist on this.

## Release 1.2.1
Release date 2021-05-05
- **Registration of modules** for a *layout* is ready to use. 
The *meeting organizer* can register any person's modules, and a normal *user* can register its own modules.

## Release 1.2.0
Release date 2021-05-03
- **Registration of persons** to module meetings. The *meeting organizer* can register any person from any country, and a normal *user* can register themselves.
- **Registration of modules** for a *layout* is in **preview**. It does not yet save registered modules.
- **Browning groups** is improved for users that can see groups in different countries.

## Release 1.1.5
Release date 2021-04-30
- **Bug fix** of issue #83 *error when viewing group modules*. This fix also made FREMO-name of group modules display correctly.

## Release 1.1.4
Release date 2021-04-29
- **Meeting** summary information view added. Also publicly available. The *meeting* edit view now only available for authorized users. 
- **Latest registration date** for modules added for layouts. Each layout in a meeting can have different dates.
- **Make station** now gets a default name of associated module, but you can change it.
- **Module standards** page now made public.
- **Module end profiles** page now made public and with improved add/edit for administrators.
- **Cargo types** page now made public.
- **Regions** page now made public.
- **Document download** now working. You can download all documentation file types.

## Release 1.1.3
Release date 2021-04-28
- **Make station** button added on the *module* edit form to make it more obvious that it has to be a station to add *tracks* and *freight customers*. 
Thanks to Seth Olofsson for notifying about this problem.
- **Freight customer** documentation corrected. Thanks to Fredrik Petterson to notifying this.
- **Bug fix** issue #81 viewing stations from group list. Thanks to Benny Tjäder for reporting this.
- **Bug fix** of visibility of data for administrators regarding *modules*, *stations* and *freight customers*.

## Release 1.1.2
Release date 2021-04-26
- **Better data visibility** which means that FREMO members will see all groups, group members and modules within the FREMO domain, regardless of country.
- **Delete station customer** is now possible. 
- **Delete group member** is now possible.
- **Bug fix** of issue #77 *Checkboxes linked together*. Thanks to Dag-Cato Skårvik for reporting this.
- **Bug fix** of issue #78 *Remove cargo flow*. Thanks to Benny Tjäder for reporting this.

## Release 1.1.1
Release date 2021-04-22
- **Freight Customers** overview including supplied and consumed cargo types.
- **Bug fixes** in external *freight customers*.

## Release 1.1.0
Release date 2021-04-21
- **External stations** to describe real world stations with *freight customers* and *cargo flow*.
- **Component update** to latest releases.

## Release 1.0.6
Release date 2021-04-19
- **Add group member** from other country, issue #52.
- **Help texts** added for *person*, *group*, *cargo types* and *meeting*.
- **Page headings** now contains a *back*-button and a *help*-button if a help text is available.
Layout of heading is also improved.

## Release 1.0.5
Release date 2021-04-17
- **Group Domain** can now be set for groups by administrators only. Currently, the *FREMO* domain is available.
Later, extended *data visibility* will be added for groups in the same domain.
- **Track Direction** now replaces *Is scheduled*. It is now possible to define a track as unidirectional. Thanks to Jürgen Riedl for the idea.
- **Modules** can now be a part of a *station*. The system ensures that there is at least one module linked. Thanks to Benny Tjäder for the idea.
- **Delete** of a module is now possible if a number of conditions are met. They are checked on the *delete*-page.
- **Help texts** improved and corrected.
- **Module list** graphical improvements showing *variant* or *box label*.
- **Api** for fetching *cargo types* with translated names in supported languages. You need a personal *API key* to test it.

## Release 1.0.4
Release date 2021-04-15
- **Help texts** are now available in English for editing *modules*, *stations* and *station customer*. It's for evaluation by users before going further.
- **Back button** are now available on the pages for editing *modules*, *stations* and *station customer*. It's for evaluation by users before going further.
- **Cargo flow** colouring is improved,  issue [#64](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/64). Thanks to Fredrik Petterson for the idea.
- **Variant/Box label** now is displayed in *modules* lists, issue [#61](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/61). Thanks to Benny Tjäder for the idea.
- **Bug fix** of issue [#59 Cannot edit station data](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/59).

## Release 1.0.3
Release date 2021-04-14
- **Bug fix** of issue [#55 Cannot edit station cargo or cargo customers](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/issues/55).

## Release 1.0.2
Release date 2021-04-13
- **Users** can now see more data: *groups* that you are member of and non-private *modules*, *stations* and *freight customers* in these groups.
- **Visibility** for *modules* means that the module owner now can select if other users should see it. Thanks to Thomas Woditsch and others for this suggestion.
> NOTE that *visibility* is currently only implemented for *private* and *group members*, and *group members* is default.
- **Theme** free text added for *layout*.
- **Cargo units** *trailer* and *container* added. Thanks to Alexander Ehn for this suggestion.
- **Loading/unloading** ready time can now be set to *not applicable*.
- **Web API** for *meetings* with *layouts* is available.
- **API key** access to *web API*. Users that want to try it out can request a personal *api key* through the FREMO forum.

## Release 1.0.1
Release date 2021-04-12
- **Validation** improvements of *regions*, *station customers* and *station customer cargo flow*.
- **Bug fix** of issue #45 *The password reset link does not work*. Thanks to Alexander Ehn.
- **Contributing** guidelines updated.

## Version 1.0.0
Release date 2021-04-11

Module and station management are now feature complete, so it's time to release the first version.
- **Freight Customers** for *stations* can now be added and maintained.
- **Cargo Flow** for each *freight customer* can now be added and maintained.

## Release 0.9.5
Release date 2021-04-09
- **Bug fix** of user registration. Users may now create a password again. Thanks to Seth Olofsson for reporting this.

## Release 0.9.4
Release date 2021-04-07
- **Menu** made more compact.
- **Bug fix** of editing *stations*.

## Release 0.9.3
- **Regions** in countries can now be defined. They are intended for cargo origins and destinations outside a *layout*. 
- **File upload** improved user experience.
- **Contribution guidelines** added [here](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/blob/master/CONTRIBUTING.md).

## Release 0.9.2
Release date 2021-04-01
- **Stricter validation** of text in fields where only ordinary text should be written.
- **File upload** maximum file size now checked and aborted if file exceeds the limit.
- **Component update** to latest releases.

## Release 0.9.1
Release date 2021-03-31
- **Upload** of *AutoCad* drawings of modules and PDF-documentation of modules and stations.

## Release 0.9.0
Release date 2021-03-30
- **Meetings and Layouts** now available.
- **Bug fixes** of missing translations.

## Release 0.8.9
Release date 2021-03-28
- **Exits** new name of end profiles in module edit form.
- **Bug fix** of issue #23 adding group owned stations.
- **Bug fix** of rounding length and speed for modules.
## Release 0.8.8
Release date 2021-03-24
- **Cargo types** added with a first set of common items, translated to supported languages and coded according to NHM.
Administrators can edit and add cargo types. You should provide all translations. 
- **Easier to invite** group member to become user.

## Release 0.8.7
Release date 2021-03-24
- **Database schema** changed. Thanks for idea from Jürgen Riedl.
- **Navigation improvements** between group members modules and stations.
- **Added east/west** direction of module entry. [Read more...](https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp/wiki/Module-Entries-and-Reachable-Tracks)
- **Bug fix** of missing module drop-down in station edit form.

## Release 0.8.6
Release date 2021-03-22
- **Service release** with internal changes only. No functional changes. Please, report any problem!
- **Added message** in user settings for logged in users.
- **Bug fix** of rights in managing group owned modules.

## Release 0.8.5
Release date 2021-03-21
- **Password reset** info added on start page.
- **Adding stations** now are more strict. There must be a module first in order to extend it as a station.
- **Countries** added: now persons in UK and NL can be added. No NL translation yet, though.
- **Bug fix** of issue #11 misaligned field. Thanks to Alexander Ehn.
- **Update** of system components.

## Release 0.8.4
Release date 2021-03-20
- **Bug fix** of issue #7 when saving new modules. Thanks to Johan Dahlgren.
- **Bug fix** of language in user invitation mail. 
- **Type of Cargo** can now be managed in all supported languages.
- **NHM-codes** levels 1 and 2 imported to classify *type of cargo*.

## Release 0.8.3
Release date 2021-03-16
- **Bug fix** of error when adding *module end profiles*.
- **Validation** added for *module end profiles*.
- **Changed** layout for adding *module end profiles* and *station tracks*.

## Release 0.8.2
Release date 2021-03-11
- **Password reset** attempts are limited to a few times, and after that nothing will happen.

## Release 0.8.1
Release date 2021-03-10
- **Area of Responsibility** for administrators on contact page to help users to find the right administrator. 
- **Group Membership** for groups that administrator also administer are now shown.
- **About** page now contains links to find out more.
- **Password reset** now available under *User settings*.
- **Bug fix** of adding new person.

Thanks to Daniel Bergkvist for the ideas to improve this information.
## Release 0.8.0
Release date 2021-03-09
- **Group Member Administrator** can now add members to a group.
- **Group Data Administrator** can now add and edit all group members *modules* and *stations*.
- **Group Data Administrator** can now add and edit the groups *modules* and *stations*.

## Release 0.7.3
Release date 2021-03-09
- **Bug fix** of invitation and registration.
- **Bug fix** of invitation language when not supported in Azure.

## Release 0.7.2
Release date 2021-03-07
- **Contact page** now shows all administrators and in which country they are located.
- **Bug fix** of wrong message shown when saving unmodified *module* or *station* (thanks to Daniel Bergkqvist).
- **Bug fix** of search person for adding as group member.
- **Improved translations** (thanks Jérôme Chavel for German suggestions).
- **Improved navigation** with new buttons to quicker find your way around.

## Release 0.7.1
Release date 2021-03-06
- **Stations** and **Tracks** are now validated when user edits.
- **Modules** can be cloned for easier create similar modules.
- **FREMO Owner Prefix** can now be entered for FREMO members. 

## Release 0.7.0
Release date 2021-04-05
- **Stations** and **Tracks** can now be added and edited.
- **Note** as a free text is added on module.
- **Signal Feature** which may be *No*, *Optional* or *Fixed*. Replaces **Is Signal**.
- **Overhead Line Feature** which may be *No*, *Optional*, *Only Posts* or *Fixed*. Replaces **Catenary Description**. 
Any notes on type of catenary should be entered in new *Note* field.
- **Max Speed** of trains passing module is added. When the module is added to a *layout*, the max speed can be lowered but not exceeded.
- **Version** and **Box Label** fields for *modules* are now mutual exclusive. Only one of the two fields may be entered.
- **Bug fix** of module represents year. (Thanks to Jérôme Chavel).

## Release 0.6.0
Release date 2021-03-04
- *Terms of use* updated.
- User now required to accept *terms of use* in order to use application.
- Additional data about modules: *catenary description*, *number of sections* and *end plates* with *direction* and *end profiles*.
- Administration of *end profiles**.

## Release 0.5.1
Release date 2021-02-28
- Bug fix of *password policy* (thanks to Daniel Bergqvist).
- Password policy translated and formatted for better readability.
- Less content in *module standard* list so lines don't break.

## Release 0.5.0
Release date 2021-02-27
- Administrators can now add and edit modules.
- Edit group member now works.
- List any person's modules now works.
- Additional improvements of style.

## Release 0.4.2
Release date 2021-02-26

- Upgraded to Boostrap 5.
- Upgraded to Font Awesome 5.
- Toasts showing action results for *save* and *delete*.
- Additional styling of lists and forms.
- Improved user rights filtering on content.

## Release 0.4.1
Release date 2021-02-25

- Improved appearance of forms.
- Added *variant* and *box label* to *module*.
- Some minor bug fixes.

## Release 0.4.0
Release date 2021-02-23

- User can now view, create and edit *personal modules*.
- Administrators can now add and edit *module standards*.
- The most common *scales* are added.
- All *functional*- and *landscape* states are implemented.

## Release 0.3.0
Release date 2021-02-21

- Added user permission filtering to all services.
- Prepared services and database for adding modules.
- Start page for logged in users now shows *release notes*.

## Release 0.2.4
Release date 2021-02-20

- Improved user login status.
- Improved module owner state.
- Added user permissions filtering to *PersonService*.
- Bug fix of password policy (thanks to Uwe Stegemann).
- Correct app name for making browser shortcut (thanks to Uwe Stegemann).

## Release 0.2.3
Release date 2021-02-19

- Inviter of person is now always the logged in user.
- Invitation text is refined.

## Release 0.2.2
Release date 2021-02-19
- Added translations to German and Norwegian.
- Invitation now sent in receiver's language.
- Added confirmation of sent invitation.
 
## Release 0.2.1
Release date 2021-02-18

Added invitation of persons, mail sending and confirmation with setting password.

## Release 0.2.0
Release date 2021-02-15

- Removed dependency of *Azure Active Directory B2C*. 
User validation is now made against the users in the database.
- Added preliminary *Terms of Use*.
- Improved user interface for selecting country.

## Release 0.1.3
Release 2021-02-12

- Improved editing.

## Release 0.1.2
Release 2021-02-08

- Added some administrative functions only available for administrators.

## Release 0.1.1
Release 2021-01-15

- Release for verifying that the cloud setup is working and that users can sign on.
