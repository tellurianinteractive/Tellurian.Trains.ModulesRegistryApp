## Freight customers and Freights
An important part of a timetable session is freights.
The freights can be controlled in two ways:
- Wagons in fixed turnus, where they run on a timetable.
- Wagon loads controlled by waybills.

### Waybills
In the **Module Registry**, waybills can be produced in two ways:
- Per timetable session where you create waybills that can be reused from meeting to meeting.
- Waybills for freight between the goods stations that are in a certain meeting layout.

The **Module Registry** basically has the same functions as the *Yellow Pages*, but with some differences.

### Goods customers
In the **Module Registry**, you enter freight customers and freight flows for which goods are sent and received.
You can enter freight customers and goods flows for your own module station,
but also for *external stations*.

An *external station* is a real station that must be entered with historically correct data
about the station, its customers and freight flows.

### This is how waybills are created
Unlike the *Yellow Pages*, you cannot manually create waybills yourself in the **Module Register**.
Instead, the application creates waybills by matching the freight flow of the sending and receiving freight customers.
The matching is done on the following data:
- **Theme** for the sending and receiving station must match. This prevents e.g. American and European themes are mixed.
- **Scale** to avoid creating freights between modules of different scale. External stations have no scale, so they are matched regardless of scale.
- **Type of load** must match. This is matched on the load's internal id, and not on the name. A custom load type name *does* not affect the match.
- **Years** must overlap: the time periods for *type of goods*, *sending and receiving station and freight feed*.
This means that waybills are created historically accurate.

### Customize and print waybills
The waybills that are created can be customized in several ways:
- Number to print.
- If you want to print a given number or specified number per operation day.
- If you want a return waybill for an empty car.

For departing wagon loads, you can also print empty wagon orders, that you place at the shadow yard.