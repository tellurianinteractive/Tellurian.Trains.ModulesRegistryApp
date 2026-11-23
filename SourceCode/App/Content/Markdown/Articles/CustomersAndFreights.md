## Freight customers and freights
An important part of a timetable session is freights. They can be controlled in two ways:
- Wagons in fixed turns, running to a timetable.
- Wagon loads controlled by waybills.

### Waybills
In the **Module Registry**, waybills are made in two ways:
- **Per station:** waybills for a station's freight customers, which are kept and can be reused from meeting to meeting.
- **Per meeting layout:** waybills for freights between the stations in a layout at a meeting.

You don't write the waybills yourself. The **Module Registry** makes them by matching the cargo flows of sending and receiving freight customers.

### Freight customers and cargo flows
You enter freight customers and their cargo flows, the goods they send and receive, for your own module stations, and also for *external stations*. An *external station* is a real station, entered with historically correct data about the station, its customers and cargo flows.

A cargo flow has a direction:
- **Send** and **Receive**: within the same country.
- **Export** and **Import**: also to and from other countries.

A cargo flow can also have years and days of the week. Without years of its own, a cargo flow has the customer's years.

### What matches
**Cargo with an NHM code** matches cargo flows at all other module stations and external stations:
- The same type of cargo, where one sends (send or export) and the other receives (receive or import). The match is on the cargo type, so a cargo name of your own doesn't affect it.
- In the same country, any of these directions match. Between countries, only an export matches an import.
- Between module stations, the **main theme** (Europe or USA) must be the same, and the **scale** too. External stations have neither, so they match regardless.
- The **years** of the two cargo flows must overlap, and the cargo must exist within that period. This makes the waybills historically correct.

**Cargo without an NHM code**, such as general cargo, mail and express goods, doesn't match individual stations, since that would give thousands of waybills. It goes to or from the *representative station* of your station's region. A station without a region, or in a region without a representative station, gets no such waybills.

**In a meeting layout**, the stations of the layout trade with each other, also across country borders. External stations are not included, and the cargo flows must be in operation within the layout's years.

### Create, adjust and print waybills
On a freight customer's waybills page, **Update** creates the waybills for all the customer's cargo flows. A waybill between two module stations belongs to the receiving customer, and is printed by the receiving station. You can update again at any time: only new matches are added, and the waybills you have keep their print count.

The waybills can be adjusted in several ways:
- The number to print. Zero means the waybill isn't printed, and **Mark all as printed** sets it to zero for the waybills shown.
- The days. A waybill is sent on the sending customer's days.
- One waybill per operating day, instead of one for all the days.
- A return waybill for the empty wagon, which is sent daily.

For departing wagon loads, you can also print *empty wagon orders*, which you place at the shadow yard.