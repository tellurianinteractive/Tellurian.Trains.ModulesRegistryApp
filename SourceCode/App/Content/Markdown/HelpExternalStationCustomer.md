## Help: External Station Customer
An *external customer* is a real business that existed or exists at this station.

Besides the more obvius fields in the form, some fields need some additional explanation:
- **Name** should be a real business name that existed or exists at the station.
- **Opened**- and **Closed year**
should only be fill in if it historically relevant. 
In particular, don't fill in the *closed year* if you want your customer to stay in business *until further notice*.
Only *freight customers* in business under any of the operating years of the *layout* will be used for *cargo matching*.
### Cargo flow
Defines any cargo that is relevant that the customer supply or consume:
- **Cargo type** is from a predefined list. You may not find the cargo that your looking for. 
Administrators can add new cargo, so ask any of them to help you with that.
New *cargo types* needs to be entered with translations for all supported languages and a relevant NHM-code.
- **Other name** gives you an option to override the name of the *cargo type*. 
Note that *cargo type* names are language aware, but your *other name* is not.
- **Days** are the days when the customer ships or expects to receive the cargo. Default to daily. 
Other than *daily* are only relevant for meetings where trains are operated per weeday.
- **Direction** defines if the customer supplies (*send* or *export*) or consumes (*recieve* or *import*) the cargo. 
The *import* and *export* can be used for customers in harbors etc. 
- **Ouantity** and **Quantity unit** is volume of cargo in each shipment. 
The simplest way is to use *wagons*, but other more advanced cargo volumes can be used,
that forces the user to calculate how many wagons are actually needed, in repect to each wagon type's capacity.
- **From year** and **Up to year** should only filled if historically relevant. 
In particular, don't fill in the *up to year* if you want the cargo flow to exists *until further notice*.





