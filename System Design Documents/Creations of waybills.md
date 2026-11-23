# Creation of Waybills
Creating waybills involves three steps:
1. Users add freight customers and their **cargo flows**.
2. Waybills are **generated** from the cargo flows, and can then be adjusted.
3. Waybills are **printed**.

This document describes how it works today. Planned changes and the technical details are in [Waybills.md](Waybills.md).

## Main principles

### Cargo flows
A *cargo flow* is a single type of goods that a freight customer sends or receives. Cargo flows are defined for customers at *module stations* by the module's owners, and for customers at *external stations* (real stations outside the layout) by any user who wants a real station to trade with.

Each cargo flow has a direction:
- **Send** and **Receive**: within the same country.
- **Export** and **Import**: also to and from other countries.

A cargo flow can also have years (from and up to) and days of the week. Without its own years, a cargo flow has the customer's years.

### Generating waybills
On a customer's waybills page, **Update** finds the matching cargo flows at other stations for all the customer's cargo flows, and adds a waybill for each match the customer doesn't already have. Existing waybills keep their print count; their days are set again from the sending cargo flow.

- A waybill between two module stations belongs to the **receiving** customer, and it is the receiving station that prints it.
- A waybill with an external station belongs to the module station's customer, whether it sends or receives.

### Years
Year matching makes the use of cargo more realistic and historically correct:
- The two customers' cargo flows must have overlapping years. This makes a time window.
- The cargo itself must exist within that window.

A waybill to or from a region's representative station (see *Cargo without an NHM code* below) has the cargo flow's own years as its window.

### Days
Sending cargo can take several days (timetable sessions), so what matters is the day the cargo is sent:
- A waybill is sent on the **sending** cargo flow's days.
- An empty return is sent daily.
- A return with the same cargo is sent on the days of its own sender, which is the receiver of the outgoing waybill.

### Adjusting waybills
After waybills are generated, they can be filtered and edited: the number to print (zero means not printed), the days, whether a return waybill is printed, and more. *Mark as printed* sets the print count of the waybills shown to zero.

### Printing waybills
Waybills are printed 12 per page, 6 in each row. Waybills with a return are printed first, so they can be cut in pairs and folded. A waybill printed *per operating day* gives one waybill for each sending day.

## What matches

### Cargo with an NHM code
A customer's cargo flow at a module station matches cargo flows at **all other module stations and external stations**:
- The same cargo type, where one sends (Send or Export) and the other receives (Receive or Import).
- In the same country, any of these combinations matches. Between countries, only an export matches an import.
- Between two module stations: the same main theme (Europe or USA), and the same scale unless one of them has none.
- The years, as described above.

### Cargo without an NHM code
Cargo without an NHM code (general cargo, mail, express goods and so on) doesn't match individual stations, otherwise there would be thousands of waybills. It matches only the **representative station of the station's own region**: the waybill goes to or from that station, with a return waybill with the same cargo the other way.

A station without a region, or in a region without a representative station, gets no such waybills.

### Meeting layouts
For a layout at a meeting, waybills are made when they are printed, without being stored:
- They are made only between the layout's module stations (internal freights), never with external stations.
- The country of the stations is ignored, so stations from different countries in the same layout trade with each other.
- Receiving customers are at the layout's stations; sending customers are at stations in the layouts at the same meeting with the same main theme and scale. Stations of participants who have cancelled are left out.
- Cargo with an NHM code: a sending customer matches a receiving customer. Cargo without an NHM code: an export matches an import between stations in different countries.
- Both cargo flows must be in operation within the layout's years.
- Wagons and other bearer units get an empty return waybill.

## Technical Solution
The cargo flows are stored in **StationCustomerCargo** (module stations) and **ExternalStationCustomerCargo** (external stations).

Generating adds rows to **StationCustomerWaybill**. A row refers to the customer and its cargo flow, and to the *other* end of the waybill: another **StationCustomerCargo** or an **ExternalStationCustomerCargo**, with the other one NULL; with neither, the other end is the region's representative station. More details, and how the current implementation works, are in [Waybills.md](Waybills.md).