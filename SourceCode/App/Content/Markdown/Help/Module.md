## Module Help
What is a module? If you have sections that must be assembled toghether, enter it as *one* module with a number of sections.
Typically this will be the case for a station module, but also other fixed combinations of sections should be considered as *one* module.
Only when a module section can be combined with arbitrary other modules, it should be entered as a separate module.
 
> NOTE: A *module* is not automatically a *station*. In order to add *station tracks* and *freight customers* you need to make it a *station*.
> That *station* will be linked to your module.

Besides the more obvius fields in the form, some fields need some additional explanation:
- **Name** and **FREMO no.**
You can give your module any descriptive name, it does not have to be your *FREMO owner initials*+FREMO number.
The given FREMO number will instead be combined with your *FREMO owner initials* stored in your personal data, ensuring that the module's FREMO-id will be correct.
#### Variant or Box Label
**Variant** or **Box label** are mutually exclusive. 
> **IMPORTANT: The combination of *name* and *variant* must be unique for all your modules.**
> There may be only one module *name* without *variant*, or all modules with same *name* must have a *variant*. 
- **Variant** is used when a module can be assembled in in different ways 
that requires you to enter each variant as *separate* modules with *same* name 
but with different characteristics and drawings. You enter a different *variant* for each, for example *short* and *long* module versions.
- **Box label** is used in any other case, and should be the identity of the box where the module is stored. If several modules are in the same box, they should have the same *box label*.
#### Years of representation
Neither **Represents from year** and **Represents to year** is mandatory.
- If you leave **Represents from year** empty, it means before 1900.
- If you leave **Represents to year** empty, it means *until further notice*.
#### Measures
A module can be a curve, a straigh or a combination of both. The length of a module is defined by:
- A **radius** and **angle** of curve part of module plus **straight** length part.
- The total **length** of the module is calculated by the application.
- You can also specfy a module **width** that should be the widest part of the module.
#### Number of through tracks
- One through track means a single track module.
- Two through tracks means a double track module.

Modules with more that two through tracks may exist, but rather seldom and for special layouts.
#### Checkboxes
- **Is Unavailable** means that the module cannot be registered for a meeting.
- **Is Stand-Alone** means that the module can stand freely on its legs. 
- **Is Signal** means that the module is a separate signal module that can be placed with other modules 
between the signal and the station or line it protects.
- **Is Turntable** means the module has or is a turntable.
- **Is Duckunder** means that the modules is suitable to use as a passway under the tracks. 
#### Visibility
You can control who can view your module and station data:
- **Private** means that only you and any *data administrator* in groups that you are a member of will be able to see your module/station.
- **Group members** means that *all members* of all groups that you are a member of will be able to see your module/station.
- **Domain members** means that *all members* of all groups you are a member of belonging to the same *domain* will be able to see your module/station.
- **All users** means that all logged-in users will be able to see your module/station.
>In all cases, only you and any *data administrator* in groups that you are a member of can modify data for your module/station. 
>When you submit a module to a meeting, the organising group's *data aministrator* can also modify data for your module/station. 

#### Exits
Each end plate of a module represents an *exit*.
Most modules only have two *exits*, for example line-modules; you will have one in each direction: *east* and *west*.
When the track ends on the module, you add only one exit.

When specifying exits for *station modules* you must imagine that you stand at the operation place and have the module to the north of you.
Then *west* exits will be the ones to the *left* and the *east* exits will be the ones to the right.
You may have several *exits* in each direction.
These directions becomes extra important when you later specify *station track* directions.

