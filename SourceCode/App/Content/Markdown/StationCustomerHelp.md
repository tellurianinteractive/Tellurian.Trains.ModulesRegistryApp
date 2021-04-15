## Station Customer Help
Besides the more obvius fields in the form, some fields need some additional explanation:

#### Track no. or loading area
This is a free text field where you can define where in the station 
the customers cargo will normally be picked up or delivered.
Sometimes this is a *track numeber* and other times it is a dedicated *loading area* with its own designation.
You can override it in each *carco flow* if a specific cargo flow is picked up or delivered at another track or loading area.

If you have several customers at a station and want to improve the orientation for peoble shunting at your station, 
you can give each place for cargo exchange a **colour**. 
These colours should correspond to some orientation documentation available at the station.

#### Opened and Closed year
Only fill in years if it historically relevant. 
In particular, don't fill in the *closed year* if you want your customer to stay in business *until further notice*.

### Cargo flow
Defines any cargo that is relevant that the customer supply or consume:
- **Cargo type** may not contain the cargo that your lookingh for. Administrators can add new cargo, so ask any of them to help you with that.
- **Other name** gives uou an option to override the name of the *cargo type*. 
Note that *cargo type* names are language aware, but your *other name* will not.
- **Days** are the days when the customer ships or expects to receive the cargo. Default to daily.
- **Direction** is if the customer supplies (*send* or *export*) or conssumes (*recieve* or *import*) garco. 
The *import* and *export* can be used for customers in harbors etc. 
- **Ouantity** and **Quantity unit** is volume of cargo in each shipment. 
The simplest way is to use *wagons*, but other more advanced cargo volumes can be used,
that forces the user to calculate how many wagons are actually needed, in repect to each wagon type's capacity.
- **Loading/Unloading ready** is used to control when wagons becomes ready to use 
for another shipment or be sent empty to some other place. 
You can select relative or absolute times, or set it as *not applicable*.
- **Other track no. or loading area** should only be filled if it deviates from what is specified for the customer.
You can also then select another colour.
- **From and Up to year**
Only fill in years if it historically relevant. 
In particular, don't fill in the *up to year* if you want your customer to stay in business *until further notice*.





