## Help - Regions
A *region* is a geographical part of a *country*. 
Regions for most countries are entered into the Module Registry.
This is a task that is managed by the country administrators.
 
There are several usages for regions:
- You can assign your stations to a *region*. This is required for working with *waybills*.
- A *shadow station* can represent the external world for a layout by one or several *regions*.
When you have several shadow stations in a layout,
they usually represent different parts of a country,
and sometimes also all or parts of another country.
- Cargo *origin* or *destination stations* not present in a *layout* 
will be sent from/to the shadow station that represents
the region where the station is located.

A station **not** present in a layout can be of the following types:
- A **module station** not present in the layout, but assigned to a *region*. 
Such module station is said to be *external* to the current layout.
Freights to this module will be sent to the *shadow station* representing the region the module station belongs to.
- A **real station**, defined as an *external station* with *freight customers*.
An external station always belongs to a *region* and the waybill destination will have the region colour.
- Any **other destination** outside the layout. 
These require geographical skills to know which *region* the destination belongs to.

The *waybills* you can create with the Module Registry uses *regions* to
select colours of origin and destination, and also for detect when freights
are international. Then waybills will be bi-language and also contains the flag of
the destination country. So *regions* are an important concept in the Module Registry.
