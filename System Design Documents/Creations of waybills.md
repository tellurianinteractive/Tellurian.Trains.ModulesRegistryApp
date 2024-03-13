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
Waybills can be created by *generating waybills* for a freight customer. 
This method finds all matching cargo customers at other module stations and
external stations, and is stored for each station customer in the 
table *StationCustomerWaybill*. 
The gererantion can be repeated, and only adds waybills that are not 
in the station customer's list.

There are also an option to create waybills per meeting layout,
with internal freights only. 

### Modifying Waybills
After waybills are gererated, they can be edited, for example 
number to print (where zero disable printing), and
if an empty return waybill should be created, etc.


### Printing Waybills
Nornally, waybills are printed 12 per page, first 6 in top row and
then 5 in last row.
The printing function always print waybills with empty return first,
so they can be cut in pairs and folded.

## Technical Solution
### Cargo Flow
The *cargo flow* is edited in the user interface and stored in tables:
a) **StationCustomerCargo** for module stations, and
b) **ExternalStationCustomerCargo** for external stations.
### Generated Waybills
The generation of waybills uses stored procedures 
*AddGeneratedModuleWaybills* and
*AddGeneratedExternalWaybills*
to add rows in  the table **StationCustomerWaybill**.
This table holds a reference to the station customer and its specific cargo flow,
and to the *other* end of the waybill as a relation to ether another 
*StationCustomerCargo*, 
an *ExternalStationCustomerCargo*
while the other one is NULL.

> There is no need to manually create waybills to other stations, 
because the generated one cover all possible cases.




