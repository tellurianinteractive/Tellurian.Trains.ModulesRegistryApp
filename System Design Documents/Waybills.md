# Waybills: how they work today

This document describes what the current implementation (stored procedures and views in `SourceCode/Database`) actually does, as surveyed 2026-11-23 (Migration step 5.1). The intended rules are in [Creations of waybills.md](Creations%20of%20waybills.md); the differences are listed at the end. The golden-master tests in `TestCode/Database.Tests` pin this behaviour before it moves to code.

## Cargo flows
A cargo flow is a row in **StationCustomerCargo** (module station) or **ExternalStationCustomerCargo** (external station).

| Direction | IsSupply | IsInternational |
|---|---|---|
| Receive | 0 | 0 |
| Send | 1 | 0 |
| Import | 0 | 1 |
| Export | 1 | 1 |

A flow's years are its own *from* and *up to* years, or else the customer's opened and closed years.

A module station's flows take part only if the station has cargo customers, belongs to a region, and its primary module belongs to the station. Scale, standard and main theme (USA or Europe) come from the station's modules. An external station's flows take part if the station belongs to a region.

## Generated waybills (stored)
*Update* on a customer's waybills page runs three procedures for **all** of that customer's flows, in this order. The user must be allowed to edit the station. Rows are only added, never deleted; an existing row is never added again.

### 1. Module station to module station (`AddGeneratedModuleWaybills`)
For each of the customer's flows that **receives** (Receive or Import) a cargo with an NHM code, it adds a row for every flow at another module station that:
- has the same cargo,
- sends it (Send or Export), at another station and another customer,
- has the same main theme, and the same scale (or one of them has none),
- is in the same country, **or** both are international (Import ← Export). So in the same country an Export also matches a Receive, and an Import matches a Send.
- has years that overlap the receiving flow's years.

The row is stored **only at the receiving customer**. Generating for a sending customer adds no module-to-module rows. Values:
- the receiver's operating day (since 1.17.4 the sender's, see *Days* below),
- the sender's region,
- print count = the quantity when the unit is *piece*, otherwise 1,
- no empty return and no same-cargo return.

Rows that already exist get their operating day and region updated; their print count is kept.

### 2. Module station to external station (`AddGeneratedExternalWaybills`)
For each of the customer's flows with an NHM code, in **both** directions, it adds a row for every external flow that has the same cargo and the opposite direction. The country and year rules are the same as above. Scale and main theme are not checked. The row is stored at the module customer. Print count = the quantity when the unit is a *bearer* (wagon, container, trailer, trainset), otherwise 1.

### 3. Shadow yard (`AddGeneratedShadowYardWaybills`)
This step covers the customer's flows of cargo **without an NHM code**: General cargo, Unspecified, Secret cargo, Mail, Express goods, Baggage, Refrigerated goods, Water, Service goods, Frozen goods, Agricultural products. A flow gets **one** row with no other end if somewhere in the same country there is a flow with:
- the same cargo,
- the opposite direction,
- the same main theme,
- overlapping years,
- and a region that has a representative external station.

The other flow can be at any station, not only at shadow stations: `Station.IsShadow` isn't checked. The row gets:
- the customer's **own** region,
- print count 1,
- *same-cargo return* set.

When printed, the other end is the customer's own region's shadow yard, named after the region's representative external station, or the region if it has none.

### Production (2026-11-23)
- **Stored rows:** 3,153 module-to-module, 1,890 module-to-external and 272 shadow-yard rows; none manually created.
- **Flows by direction:** 1,636 Receive, 1,007 Send, 292 Import, 187 Export.
- **Stale rows:** 189 module-to-module rows no longer match, because the cargo or direction has changed. 26 module-to-module rows are stored at the sender, from before only receivers were generated for.

## Days (since 1.17.4)
- **Generation** stores the sending flow's days on the waybill (`OperatingDayId`): the other flow's when the own flow receives, the own flow's when it sends. A shadow-yard waybill has the own flow's days. Regenerating sets the days again, as before; the print count is kept.
- **A station's printout** uses the stored days, which can be changed on the waybill. Layout and group waybills have no stored rows and use the sending flow's days.
- **Returns:** a return with the same cargo is sent on its own sender's days (the outgoing waybill's receiver), an empty return daily.
- Existing waybills got the sending flow's days with the one-off script `dbo/Scripts/SetWaybillSendingDays.sql`.

## Removing waybills
Only the cascade triggers delete rows: when a station customer, a cargo flow or an external cargo flow is deleted. A row stays when a flow changes cargo, direction, years or country. Printing hides most such rows, because the print views check cargo, direction and country again. The customer's waybills page still lists them.

*Disabling* a waybill means setting its print count to zero. *Mark all as printed* does the same.

## Printing a station's waybills (`GetStationCustomerWaybills`)
The procedure returns the union of six views, for one station and optionally one customer, receiving and/or sending:
- as receiver: `ModuleSupplierWaybill`, `ExternalSupplierWaybill`, `ShadowYardSupplierWaybill`;
- as sender: `ModuleConsumerWaybill`, `ExternalConsumerWaybill`, `ShadowYardConsumerWaybill`.

Each view joins a stored row with the sender's and receiver's flows. It also checks the cargo's own years against the **sender's** years. Since module-to-module rows are stored at the receiver, the receiving station prints them; the sending station only sees the 26 old rows.

**Since Migration step 5.2** this is done in code, with the same rules: `StationWaybillGenerator` (Services/Waybills) combines the station's stored waybills with the flows loaded by `CargoFlowQueries`. The views joined the other end on its id alone, so a flow of the other kind (module or external) with the same id could give a false waybill; the code doesn't reproduce that. The procedure and the views stay in the database until step 5.3.

## Layout waybills (`GetLayoutWaybills2`), computed, not stored
For a meeting layout, optionally for one station in it:
- **Receivers:** receiving flows at the layout's stations, from participants who haven't cancelled.
- **Senders:** sending flows at stations in any layout at the same meeting with the same main theme and scale.
- **Matching:**
  - same cargo, another station;
  - cargo **with** an NHM code: Send → Receive, in **any** country;
  - cargo **without** an NHM code: Export → Import, between different countries only;
  - both flows' years must overlap the layout's first and last year.
- **Not used:** stored rows and their settings (print count, disabled).
- **Print count:** the smaller of the two quantities for bearer units, otherwise 1.

## Group waybills (`GetGroupWaybills`), computed, not stored
The same as layout waybills, but within a group's modules (functional state above 3). There is no year check and no rule about NHM codes, countries or international directions.

## Empty wagon orders (`GetEmptyWagonOrders`)
The station's sending flows whose cargo's NHM group has *empty wagon order*, with the quantity as the number of wagons.

## Defects found
These are pinned by the golden-master tests as they are, and are fixed only when the code replaces them:
- **Comparisons across id spaces:**
  - `AddGeneratedExternalWaybills` compares a module customer id with an external customer id (`ME.StationCustomerId <> OTHER.StationCustomerId`).
  - `AddGeneratedShadowYardWaybills` compares a module station id with an external station id. With no representative external station the comparison is NULL, so no shadow-yard row is created.
- **Mixed id spaces in the views:** `SupplierCustomerCargo` and `ConsumerCustomerCargo` union external and module flows, whose ids come from different tables. The shadow-yard views join on the stored flow id alone, so an external flow with the same numeric id can produce a false row.
- **Print count rules differ:** *piece* for module rows, *bearer* for external rows, the smaller of the quantities for layouts.
- **Dead code:** *same-cargo return* can never be set for module and external rows, since those require an NHM code.
- **Column mix-up:** `ExternalCustomerCargo` puts the direction's *IsSupply* into the column that the other views use for the flow's *EmptyReturn*. The columns `EmptyReturn` and `MatchReturn` aren't used when generating.

## Differences from the intended rules
Decided 2026-11-23; the rules are in [Creations of waybills.md](Creations%20of%20waybills.md).

| Rule | Today | Change needed |
|---|---|---|
| In the same country, Send or Export matches Receive or Import; between countries, only Export matches Import | The same | No |
| A module-to-module waybill is stored at the receiving customer, who prints it | The same | No |
| Module stations need the same main theme, and the same scale unless one has none | The same | No |
| A new waybill gets print count 1; an existing one keeps its count | New module and external rows get the quantity for some units | Yes |
| Generate for one cargo flow, or all of a customer's flows | Always all of the customer's flows | Yes |
| Waybills that no longer match are deleted on both sides when a flow changes, or when the waybill is disabled | Deleted only when a flow or customer is deleted; stale rows are hidden when printing; disabling = print count 0 | Yes |
| The stations' years overlap, and the cargo exists within that window | The flows' years overlap when generating; the cargo's years are checked against the sender's years only, when printing | Yes |
| A waybill gets the sending flow's days | Done in 1.17.4 | No |
| Generated waybills: cargo without an NHM code matches only the representative station of the station's own region, to or from it, with an empty return waybill the other way | The other end is the own region (its representative station, or the region's name if it has none); a row is created only if an opposite flow of the same cargo exists in the same country, in a region with a representative station; *same-cargo return* is set, not *empty return*; id comparisons across tables | Yes |
| Harbours receive any export and send any import, within the country | Not implemented | New feature |
| Layout waybills, for all cargo (also without an NHM code, since a layout has few stations), ignore the country and the difference between Send/Export and Receive/Import | Cargo with an NHM code: Send → Receive only; without: Export → Import between stations in different countries | Yes |
| — (not described) | Group waybills, empty wagon orders | — |