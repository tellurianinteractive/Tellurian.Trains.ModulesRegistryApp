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
Waybills can be created in three ways:
1. By *generating waybills* for a freight customer. 
This method finds all matching cargo customers at other module stations and
external stations, and is stored for each station customer in the 
table *StationCustomerWaybill*. 
The gererantion can be repeated, and only adds waybills that are not 
in the station customer's list.
2. By *manually create waybills*. These can only be to/from *regions*
and never to a specific station.
By *printing layout specific waybills*. These waybills will only
be the internal freights between the stations present in a specific
meeting layout, and they will not be saved.

### Printing Waybills
Nornally, waybills are printed 10 per page, first 5 in top row and
then 5 in last row.
The printing function has some trick to align forward and return waybills
together so they appear one under the other. 
In order to make it work, waybills are fist sorted so that waybills
and its returns are retrieve first and in order.
The printing rearranged the order 1,6,2,7,3,8,4,9,5,10, which
means that waybill 1 and its corresponding return will end in position 1 and 6, which
means that the return is printen under ist corresponding forward waybill.

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
### Manually Created Waybills
The user can add waybills and the *other* end can only be a *region*.
Also these are stored in **StationCustomerWaybill** and sets both
*StationCustomerCargo*, 
an *ExternalStationCustomerCargo*
to NULL.
> There is no need to manually create waybills to other stations, 
because the generated one cover all possible cases.




