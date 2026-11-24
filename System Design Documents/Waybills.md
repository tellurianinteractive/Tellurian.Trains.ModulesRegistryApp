# Waybills: implementation and plans

The technical companion to [Creations of waybills.md](Creations%20of%20waybills.md), which describes for users how waybills work. This document describes what the implementation (stored procedures and views in `SourceCode/Database`, and since Migration step 5.2 `Services/Waybills`) actually does, as surveyed 2026-11-23 (step 5.1) and updated since, its defects, and at the end the planned changes and ideas. The golden-master tests in `TestCode/Database.Tests` pin this behaviour while it moves to code.

## Cargo flows
A cargo flow is a row in **StationCustomerCargo** (module station) or **ExternalStationCustomerCargo** (external station).

| Direction | IsSupply | IsInternational |
|---|---|---|
| Receive | 0 | 0 |
| Send | 1 | 0 |
| Import | 0 | 1 |
| Export | 1 | 1 |

A flow's years are its own *from* and *up to* years, or else the customer's opened and closed years.

**Years since 1.17.5:** the two flows' years must overlap, which makes a window, and the cargo must exist within it; a shadow-yard waybill's window is the own flow's years. This applies when generating and when printing a station's waybills. The descriptions below are of the procedures before that change.

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

## Printing a station's waybills (`StationWaybillGenerator`)
`StationWaybillGenerator` (Services/Waybills) combines the station's stored waybills, for one station and optionally one customer, receiving and/or sending, with the cargo flows loaded by `CargoFlowQueries`:
- **Another module station:** the own and the other flow have opposite directions; the same country, or both international.
- **An external station:** opposite directions and the same cargo; the same country, or the receiving flow international (receiving), or both international (sending).
- **No other end:** the region's representative station; no waybill without one.
- **Always:** the year window (since 1.17.5).

Since module-to-module rows are stored at the receiver, the receiving station prints them; the sending station only sees the 26 old rows stored at the sender. The waybill is sent on the stored days (since 1.17.4).

Until release 1.17.2 the procedure `GetStationCustomerWaybills` did this, with the union of six views (`ModuleSupplierWaybill`, `ExternalSupplierWaybill`, `ShadowYardSupplierWaybill`, `ModuleConsumerWaybill`, `ExternalConsumerWaybill`, `ShadowYardConsumerWaybill`) over the unions `SupplierCustomerCargo` and `ConsumerCustomerCargo`. The views joined the other end on its id alone, so a flow of the other kind (module or external) with the same id could give a false waybill; compared for all stations in production, 12 of 5,095 waybills were such false matches, which the code doesn't reproduce. The procedure and the eight views were removed in step 5.3 (`dbo/Scripts/DropStationWaybillViews.sql`).

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
- **Mixed id spaces in the views** (fixed in 1.17.2, the views removed in step 5.3): `SupplierCustomerCargo` and `ConsumerCustomerCargo` unioned external and module flows, whose ids come from different tables, and the waybill views joined on the stored flow id alone, so a flow of the other kind with the same numeric id could produce a false row.
- **Print count rules differ:** *piece* for module rows, *bearer* for external rows, the smaller of the quantities for layouts.
- **Dead code:** *same-cargo return* can never be set for module and external rows, since those require an NHM code.
- **Column mix-up:** `ExternalCustomerCargo` puts the direction's *IsSupply* into the column that the other views use for the flow's *EmptyReturn*. The columns `EmptyReturn` and `MatchReturn` aren't used when generating.

## Planned changes and ideas
How waybills work today, for users, is in [Creations of waybills.md](Creations%20of%20waybills.md). This section collects what is decided or proposed but not yet implemented. Done: the sending flow's days (1.17.4), the year window and one waybill per day when printing per operating day (1.17.5).

### Decided
- **Remove waybills that no longer match** (step 5.5). A waybill that no longer matches the own or the other station's cargo flows is deleted from **StationCustomerWaybill** on both sides, as part of the update request: when a station no longer provides a cargo, or a flow changes from export to send; when the user disables the waybill; when the cargo itself is deleted (unlikely). Today waybills are only deleted when a cargo flow or customer is deleted; the printout hides most of the others.
- **Print count 1 for new waybills** (step 5.5). An existing waybill keeps its print count. Today a new waybill gets the quantity for some units: pieces for module stations, bearer units for external stations.
- **Cargo without an NHM code** (step 5.5): the waybill goes to or from the representative station of the station's own region, **with an empty return** the other way, and no waybill when the region has no representative station. Today the return is a *same-cargo return*; a waybill is only created if a flow of the same cargo in the opposite direction exists somewhere in the same country, in a region with a representative station; and a region without a representative station still gets a stored waybill (not printed). The procedure also compares ids from different tables.
- **Layout waybills** (step 5.4): for all cargo, also without an NHM code (a layout has few stations), the difference between Send/Export and Receive/Import is ignored, so a station that sends matches a station that imports the same cargo. Today cargo with an NHM code matches only sends to receives, and cargo without an NHM code only exports to imports between stations in different countries.

### Proposed
- **Generate for one cargo flow at a time**, besides all of a customer's flows.
- **Empty return and match return.** Use the flow's *EmptyReturn* to create empty return waybills for added waybills. Use *MatchReturn* to find a return waybill for a cargo that uses the same type of wagon class: if A sends bread to B in a G or H wagon, find something B can send to A in a G or H wagon. If both are set, a match return has precedence over an empty return; with no match return but an empty return, add an empty return; otherwise no return.
- **Add a cargo flow from another station's cargo flow.** A user wants to trade with a station and adds a cargo flow by selecting that station's cargo flow. In both the station customers list and the external station customers list, after selecting a customer, the list of its cargo flows comes up with a button *Add to my station*. A separate form lets the user select their own station and customer and add: this adds a cargo flow to the user's customer and matches it, which adds a waybill in the customer's **StationCustomerWaybill**. The two stations must overlap in time, and the cargo must exist within that time.
- **Harbours.** A module station that is a harbour can also be the destination of any export and the source of any import, limited to cargo flows in the same country, as a real harbour. Harbours are not used in waybills today (`Station.IsHarbour`).
- **Days of receiving flows.** For flows that receive or import, the days have no meaning, so the selection can be hidden and set to daily behind the scenes. The days are kept, since a flow can change direction, and then the days apply again.