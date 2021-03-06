# Release Notes

## Version 0.7.1
Release date 2021-03-06
- **Stations** and **Tracks** are now validated when user edits.
- **Modules** can be cloned for easier create similar modules.

## Version 0.7.0
Release date 2021-04-05
- **Stations** and **Tracks** can now be added and edited.
- **Note** as a free text is added on module.
- **Signal Feature** which may be *No*, *Optional* or *Fixed*. Replaces **Is Signal**.
- **Overhead Line Feature** which may be *No*, *Optional*, *Only Posts* or *Fixed*. Replaces **Catenary Description**. 
Any notes on type of catenary should be entered in new *Note* field.
- **Max Speed** of trains passing module is added. When the module is added to a *layout*, the max speed can be lowered but not exceeded.
- **Version** and **Box Label** fields for *modules* are now mutual exclusive. Only one of the two fields may be entered.
- **Bug fix** of module represents year. (Thanks to Jérôme Chavel).

## Version 0.6.0
Release date 2021-03-04
- *Terms of use* updated.
- User now required to accept *terms of use* in order to use application.
- Additional data about modules: *catenary description*, *number of sections*
and *gables* with *direction* and *type of gable*.
- Administration of gable types.

## Version 0.5.1
Release date 2021-02-28
- Bug fix of *password policy* (thanks to Daniel Bergqvist)
- Password policy translated and formatted for better readability.
- Less content in *module standard* list so lines don't break.

## Version 0.5.0
Release date 2021-02-27
- Administrators can now add and edit modules.
- Edit group member now works.
- List any person's modules now works
- Additional improvements of style.

## Version 0.4.2
Release date 2021-02-26

- Upgraded to Boostrap 5.
- Upgraded to Font Awesome 5.
- Toasts showing action results for *save* and *delete*.
- Additional styling of lists and forms.
- Improved user rights filtering on content.

## Version 0.4.1
Release date 2021-02-25

- Improved appearence of forms.
- Added *variant* and *box label* to *module*.
- Some minor bug fixes.

## Version 0.4.0
Release date 2021-02-23

- User can now view, create and edit *personal modules*.
- Administrators can now add and edit *module standards*.
- The most common *scales* are added.
- All *functional*- and *landscape* states are implemented.

## Version 0.3.0
Release date 2021-02-21

- Added user permission filtering to all services.
- Prepared services and database for adding modules.
- Start page for logged in users now shows *release notes*.

## Version 0.2.4
Release date 2021-02-20

- Improved user login status.
- Improved module owner state.
- Added user permissions filtering to *PersonService*.
- Bug fix of password policy (thanks to Uwe Stegemann).
- Correct app name for making broser shortcun (thanks to Uwe Stegemann).

## Version 0.2.3
Release date 2021-02-19

- Inviter of person is now always the logged in user.
- Inviitation text is refined.

## Version 0.2.2
Release date 2021-02-19
- Added transtaltions to German and Norwegian.
- Invitation now sent in receivers language.
- Added confirmation of sent invitation.
 
## Version 0.2.1
Release date 2021-02-18

Added invitation of persons, mail sending and confirmation with setting password.

## Version 0.2.0
Release date 2021-02-15

- Removed dependency of *Azure Active Directory B2C*. 
User validation is now made against the users in the database.
- Added preliminary *Terms of Use*.
- Improved user interface for selecting country.

## Version 0.1.3
Release 2021-02-12

- Improved editing.

## Version 0.1.2
Release 2021-02-08

- Added some administrative functions only availavle for administrators.

## Version 0.1.1
Release 2021-01-15

- Release for verifying that the cloud setup is working and that users can sign on.