﻿## Station Customer Help
Besides the more obvius fields in the form, some fields need some additional explanation:
- **Track no. or loading area**
is a free text field where you can define the identificatiion of position where a customer's cargo will normally be picked up or delivered.
It could be a *track number*, or a dedicated *loading area* with its own designation.
You can later override it in each *cargo flow* when cargo is picked up or delivered at another track or loading area.
for example when arriving and departing goods are handled at different positions.
- **Colour** can be used if you have several customers at a station and want to improve the orientation for people shunting at your station. 
You can assign a *colour* to each place where you have cargo exchange. 
These colours should correspond to some orientation documentation available at the station.
The colours for your station customers should not be confused with the colours of external destination.
- **Opened**- and **Closed year** controls under which operating period the customer is relevant.
In particular, don't fill in the *closed year* if you want your customer to stay in business *until further notice*.
Only *freight customers* in business under any of the operating years of the *layout* will be used for *cargo matching*.
### Cargo flow
Defines any cargo that is relevant that the customer supply or consume:
- **Cargo type** must be selected. Try to select a correct cargo type, or a similar one.
If the list does ontain the cargo that your looking for,
administrators can add new cargo, so ask any of them to help you with that.
- **Packaging Unit** is an additional information of how the cargo is packed, i.e. the units of loaded or unloaded items, 
and it also include most types of *containers*. You can also set it as *not applicable*.
- **Wagon Class** gives you an option to specify a specific wagon class 
that overrides the default wagon classes for the *cargo type*.
- **Other name** gives you an option to override the name of the *cargo type*. 
**DO NOT** use this instead of selecting a proper carco type.
Selecting *cargo type* names are language aware, but your name in this field is not.
- **Days** are the days when the customer ships or expects to receive the cargo. Default to daily. 
Other than *daily* are only relevant for meetings where trains are operated per weeday and 
where *cargo matching* is used.
- **Direction** defines if the customer supplies (*send* or *export*) or consumes (*recieve* or *import*) the cargo. 
*Import* and *export* will make waybill matching cross border, *send* and *receice* 
will obly match waybills with customers in the same country. 
- **Ouantity** and **Quantity unit** is volume of cargo in each shipment. 
The simplest way is to use *wagons* or *trainset*, but other more advanced cargo volumes can be used,
that forces the user to calculate how many wagons are actually needed, in repect to each wagon type's capacity.
- **Max trainset length** is the maximum number of *wagons* that can be recieved in one trainset.
- **Loading/Unloading ready** is used to control when wagons becomes ready to use 
for another shipment or be sent empty to some other place. 
You can select relative or absolute times, or set it as *not applicable*.
#### Other...-fields
You should only enter these fields to override what is specified for the *customer* and the selected *cargo type*:
- **Other wagon class** overrides the default wagon class(es) for the selected *cargo type*-
- **Other name** gives you an option to override the name of the *cargo type*. 
Note that *cargo type* names are language aware, but your *other name* is not.
- **Other track or area** lets you override the *track or area* specified for the customer.
- **Other colour** lets you override the *colour* you specified for the customer-
- **Other from year** and **Other to year** should only filled if historically relevant and different from *opening* and *closing* dates of the sustomer.
In particular, don't fill in the *up to year* if you want the cargo flow to exists *until further notice*.


