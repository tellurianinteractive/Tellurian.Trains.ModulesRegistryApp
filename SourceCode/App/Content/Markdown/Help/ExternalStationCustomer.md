## Help: External Station Customer
An *external customer* is a real business that existed or exists at this station.

Besides the more obvious fields in the form, some fields need some additional explanation:
- **Name** should be a real business name that existed or exists at the station.
- **Opened year** and **Closed year** should only be filled in if historically relevant.
In particular, don't fill in the *closed year* if you want your customer to stay in business *until further notice*.
Only *freight customers* in business during any of the operating years of the *layout* will be used for *cargo matching*.
### Cargo flow
The cargo flows are listed below the customer when it has been saved. **Add** or **Edit** opens a page for one cargo flow,
and **Delete** asks you to confirm before the cargo flow is deleted.

A cargo flow defines any cargo that the customer supplies or consumes:
- **Cargo type** is from a predefined list. You may not find the cargo that you are looking for.
Administrators can add new cargo, so ask any of them to help you with that.
New *cargo types* need to be entered with translations for all supported languages and a relevant NHM code.
- **Packaging unit** is optional. You can select how the cargo is packed when loaded on a wagon.
This includes options to select any container type.
- **Days** are the days when the customer ships or expects to receive the cargo. The default is *daily*.
Other days than *daily* are only relevant for meetings where trains are operated per weekday.
- **Direction** defines if the customer supplies (*send* or *export*) or consumes (*receive* or *import*) the cargo.
*Import* and *export* will match waybills across borders, *send* and *receive*
will only match waybills with customers in the same country.
- **Quantity** and **Quantity unit** are the volume of cargo in each shipment.
The simplest way is to use *wagons*, but other more advanced cargo volumes can be used,
which forces the user to calculate how many wagons are actually needed, with respect to each wagon type's capacity.
#### Other...-fields
You should only enter these fields to override what is specified for the *customer* and the selected *cargo type*:
- **Other wagon class** overrides the default wagon class(es) for the selected *cargo type*.
- **Other name** gives you an option to override the name of the *cargo type*.
Note that *cargo type* names are language aware, but your *other name* is not.
- **From year** and **Up to year** should only be filled in if historically relevant and different from the *opened* and *closed* years of the customer.
In particular, don't fill in the *up to year* if you want the cargo flow to exist *until further notice*.