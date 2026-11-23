# System Design
## Creation of Waybills
Creation of waybills is a bit complex because it involves several steps.
1. The first step is users adding freight customers and **cargo flow**.
2. The second step is to **generate** or **manually create waybills**. 
3. The third step is to **print waybills**.

Below these steps are descriped in more detail.

### Cargo Flow
A *cargo flow* is the definition of a single type of good being sent or received
for a freight customer. Cargo flows can be defined for *module stations* and 
*external station*. 
It is the cargo flow that forms the basis for creating waybills.

Cargo flows for module stations are created by the mainainer of the module. 
Cargo flows for external stations are created by a user that want to
have a real station sending or receiving cargo, maybe to match their module
stations cargo flow.

### Create Waybills
Waybills can be created by *generating waybills* for a cargoflows for a 
freight customer (often all cargo flows for a freight customer).
This method finds all matching cargo customers at other module stations and
external stations, and is stored for each station customer in the 
table *StationCustomerWaybill*. 
The gererantion can be repeated, and only adds waybills that are not 
in the station customer's list.
The cargo matching depends on the direction and scope:
- Sends matches same cargo id that other stations receives, and vice versa, 
  limitted to within the same country.
- Exports matches same cargo id that other stations receives, and vice versa, 
  limitted to outside the country.

  > There is no need to manually create waybills to other stations, 
because the generated one cover all possible cases.

#### Removing Unmatched Waybills
A waybill that not any longer matches the current and/or other stations cargo, 
should be deleted from **StationCustomerWaybill** on both sides:
- If my station no longer provides a cargo or it changes from exports to sends,
  corresponding waybills at both mine and other stations should be deleted
  as part of my update request.
- If I disable my waybill
- If the Cargo itself eventually are deleted (unlikley to happen).

#### Years Matching
The purpose of year matching is to improve realism and historical correctness of cargo use.
- Both stations must have overlapping years of operation. This creates a time window.
- Only cargo that exists under that time window should be considered.

#### Days Matching
Cargo flows can be defined to be limited on specific days of the week. 
Now, sending/exporting cargo can take several days (i.e. timetable sessions),
so the thing that only matters is which day to send cargo. 
The arrival days has no meaning, and can be omitted in the printed waybills.

Also, in cargo flows that are receive/import, the selection of days has no meaning and can be disabled/hidden and set to daily behind the scenes.
The reason to keep it like that, is that a single cargo flow can change direction, and then days applies again.

#### Improved Matching
We can add the follwing features:
- Option to updating waybills for only one cargo flow at a time.
- Use EmptyReturn to cerate empty return waybills for added waybills.
- Use MatchCargoFlow to find a return waybill for a cargo that uses same type of wagon.

#### Harbours
A module that is a harbour adds that it also can be a destination for any
exports, and a source for any imports, limitted to cargo flows in the same country.
This corresponds to the real world, where a harbour is a destination for any export, and a source for any imports.

#### Layout internal waybills
There are also an option to create waybills per meeting layout,
with internal freights only.
In this case, the waybills are created between all module stations 
ignoring the country, to also create waybills for internal freights between module stations in different countries.

### Modifying Waybills
After waybills are gererated, they can be filtered and edited, for example 
number to print (where zero disable printing), and
if an empty return waybill should be created, etc.

### Printing Waybills
Nornally, waybills are printed 12 per page, first 6 in top row and
then 5 in last row.
The printing function always print waybills with return waybills first,
so they can be cut in pairs and folded.

## Technical Solution
### Cargo Flow
The *cargo flow* is edited in the user interface and stored in tables:
a) **StationCustomerCargo** for module stations, and
b) **ExternalStationCustomerCargo** for external stations.

### Generated Waybills
The generation of waybills adds rows in  the table **StationCustomerWaybill**.
This table holds a reference to the station customer and its specific cargo flow,
and to the *other* end of the waybill as a relation to ether another 
*StationCustomerCargo*, 
an *ExternalStationCustomerCargo*
while the other one is NULL.






