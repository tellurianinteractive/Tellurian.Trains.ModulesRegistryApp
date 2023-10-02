### 1. Preamble
In order to use this site - the *module registy web application* - you must accept the following terms of use,
also each time the terms are changed.
This is required to conform to GDPR.

#### 1.1 What is GDPR?
The *General Data Protection Regulation* (GDPR) is a regulation in EU law on data protection and privacy in the European Union (EU) and the European Economic Area (EEA). The GDPR's primary aim is to give individuals control over their personal data and to simplify the regulatory environment for international business by unifying the regulation within the EU. Read more about [GDPR on Wikipedia](https://en.wikipedia.org/wiki/General_Data_Protection_Regulation).

#### 1.2 Scope
The descriptions in this document concerns *The Module Registry* web application and API's, 
hereinafter referred to as *the application*.

#### 1.3 Definitions
- **Person** is an individual that is registered as *owner of modules, throttles, rolling stock*, and/or *participant in a meeting*. 
- **User** is a *person* that has established a personal user account in the application.
- **Group** is a set of *persons* that have a reason to *manage modules collectively* and/or *arrange meetings*.

### 2. Security

#### 2.1 Passwords
The password you select must *not* be like any other password you otherwise use for sensitive or critical data.
The password must conform to the password policy valid at the time of registration. 
The administrator may require you to create a new password at any time.

#### 2.2. Cookies
Cookies are small pieces of data stored locally in your browser.
This site uses cookies for storing data about authentication and authorisations for logged in users.
The data is encrypted.
You must accept cookies from this site to be able to login. 

#### 2.3. Data Handling
Data in *the application* is stored in an *Azure SQL Database* in *Microsoft Azure* on servers located within Europe.
The database is only direct accessible by the system administrator.

#### 2.4 Transparency
All application code for *the application* is *open source*. 
The frameworks that the code is built on are used are also *open source*; *.NET*, *ASP.NET*, *Blazor* and *Entity Framework*.
*The application* uses the following proprietary services: *Microsoft Azure App Services*, *Microsoft Azure Sql Server*, and *Twilio SendGrid*.

### 3. Personal Data

#### 3.1 Collected Data
Data is connected for the purposes mentioned under 1.3.
You must agree to that the following *mandatory* personal data is collected  :
- First- and last name.
- City and country of residence.

Non-mandatory data that may be collected:
- Middle name or initials. 
- Email adresses (required for *users*)
- Group membership including permissions in a group.
- What modules you own completely, partially or is engaged as assistant only.
- Your participations in module meetings and the modules you eventually register.

Additional data for registered users able to login:
- Registration time.
- Last login time.
- Last time email was confirmed.
- Last time terms of use was accepted.
- Number of failed login attempts.
- Number of failed password reset attempts.

#### 3.2 Handling personal data
In *the application* personal data is not available publicly.
- Only *logged in users* have access to data according to the rights given.
- Some users are promoted to *administrators*, and there are three levels of rights: *global*, *country* and *group*.
- Only *administrators* have access to enter and modify personal data in the context they have rights.
- Only *administrators*, *meeting organizer* and *module owners* can see any personal data and the modules owned by a person, and only in the context or their role.

This means that the visibility of personal data, including owned modules are very restricted, without compromising flexibility. 
- Any *user* can maintain their own modules.
- A *group administrator* can add/remove members in the group and maintain all modules owned by persons that are member of the group.
- A *country administrator* can maintain groups in the country and maintain all modules owned by persons that are member of these groups.
- A *global administrator* can maintain groups in all countries and all modules.
- A *meeting organizer* can maintain all registrations of persons and modules.

#### 3.3 User Consent
A *user* with *administrator rights* may register other *persons* as owner of *modules*. 
In order to do so, the *user* must have that person's consent, and is also responsible for getting that consent.
The *user* can get this consent in several ways:
- By personal consent from the person in question.
- By specifying conditions for membership in a club or community.
- By specifying conditions for participating in a module meeting.

#### 3.3 Deleting Data
For security reason, ordinary users cannot delete any data.
You, as an *administrator*, *must* delete data for modules and their owners within one year after a personal written request. 