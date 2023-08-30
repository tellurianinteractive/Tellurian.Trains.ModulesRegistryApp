# wiFRED throttles
*wiFRED*s are wireless throttles that can be used instead of FREMO FRED's. 
The concept are developed by the FREMO members Heiko Rosemann and Detlef Born.

There is a need for simple handling assignment of wiFRED to DCC-address at meetings.
It is also desirable to prepare as much as possible before the meeting.
The purpose of this document is to describe how the handling of wiFREDs is implemented.

## Main features
- **wiFRED registration** for all FREMO-members, including printing a list of wiFREDs with QR-codes.
- **wiFRED administration** including verification of the entered MAC-address. 
This can be only be carried out by users with wiFRED-administration permission.
- **wiFRED deletion** is handled in two ways:
  - Before the wiFRED is verified, the entry is deleted.
  - After the wiFRED is verified, it is marked as deleted, but remains in the database and can be re-registerd
    by any FREMO-member.

> NOTE: To be regarded as a FREMO member in the Module Registry, the person must have the member number reistered.
This can only be made by country administrators.

## Data required

### wiFRED
- MAC address
- Number (to be in QR code)
- Name given by user
- Configuration read from wiFRED. Reading configiration should also update relevant properties.
- Registration date/time, automatically set when saved for the first time
- Verification date/time set by administrator when checked. After this date is set, MAC address should be locked for change.
- Person owning the wiFRED, *probably a reference to Person.Id*.
- Loco addresse 1-4, should be 1-9999 or not assigned.

### Vehicle Operator
We need to pre-fill this with all usual operators.
- Country of registration (reference to Country.Id), 
- Signature (RICS if available, otherwise historical signature)
- Name of company
- FromYear first year of operation
- UptoYear last year of operation

### Vehicle
- Operator (reference to Operator.Id)
- Class (ex V100)
- Number (ex 1252)
- FromYear in this embodiment, optional, empty means before 1900.
- UptoYear in this embodiment, optional, emtpy means until furter notice.
- InteroperabilityCode according to UIC (from a pre-defined list) 90-99 for locos, goods wagons https://en.wikipedia.org/wiki/UIC_wagon_numbers
- PrototypeManufacturer (pre-populated list with previously entered values)
- Scale (reference to Scale.Id)
- ModelManufacturer (pre-populated list with previously entered values)
- ModelImage (upload and automatic resize to standard)
- Coupling (from a pre-defined list depending on scale) FL6511, OBK, Winert-hake, etc
- Wheels (from a pre-defined list depending on scale) RP25, NEM, etc.
- IsWeathered
- OwningPerson (reference to Person.Id)
- Theme (from a pre-defined list) EUROPE, AMERICAN
- InventoryNumber
#### Loco is a subtype of Vehicle
- TractionType (steam, dieselelectric, dieselhydraulic, electric, electric with diesel for last mile)
- DCC-address
- HasRemoteCouplings
- DecoderType
- HasSound

Additional data can be added to be able to produce wagon/loco cards as with https://wagoncardapp.azurewebsites.net

### Interoperability Codes
- Interoperability code (2-digit)
- DescripionResourceCode (will be translated to all supported languages)