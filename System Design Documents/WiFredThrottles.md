# wiFRED throttles
*wiFRED*s are wireless throttles that can be used instead of FREMO FRED's. 
The concept are developed by the FREMO members Heiko Rosemann and Detlef Born.

There is a need for simple handling assignment of wiFRED to DCC-address at meetings.
It is also desirable to prepare as much as possible before the meeting.
The purpose of this document is to describe the motivation for the solution and describe requirements and suggested implementation.

## Use Cases by suggested priority
- **Manage wiFREDs** in a register that contains the data necessary to identify the individual wiFREDs
and their owners.
- **Register** user's vehicle (including locos) contribution to the layout, by assigning the contribution to a vehicle schedule.
- **Assigning** of wiFRED to a single DCC-address at the meeting by primarily using QR-codes of wiFRED and barcode of loco. 

## Implementation

### Features implemented in the Module Registry
- **Management of wiFREDs**. Users must have an account in the Module Registry to manage their wiFREDS.
- **Administrator management** of wiFREDS including **verifying** wiFRED data and **locking of MAC adress**. 
A special right for this have to be granted by a global administrator.
Only permitted for members of the meeting or layout organiser group.
- **Registration of user's vehicles**. Users must have an account in the Module Registry to manage their vehicles.

### Features implemented in the Module Meeting App?
The Module Meeting App does not have user authentication. 
And this is not necessary. 
The only identity needed is reading the QR-code of the wiFRED, and this will be saved in local storage in the app.
- **Reading QR code of wiFRED**. This identify which wiFRED to attach a DCC-address to.
- **Reading QR code of loco card**.
- **Updating** database with loco address assignments to wiFREDs. Reading order of QR-codes of wiFRED and DCC-addres should not matter.

### Features implemented in local running application
- **Reading wiFRED-loco assignments** in order to update local wiThrottle servers and wiThrottles.
- **Scanning QR-codes** to assign wiFREDs to DCC-addresses.

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