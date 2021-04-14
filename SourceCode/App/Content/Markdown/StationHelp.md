## Station Help
Besides the more obvius fields in the form, some fields need some additional explanation:

#### Full name
A station has a full name. This is not necessarily the same name as the referred module.
#### Signature
This is the short destination used internally in railway operation.
The signature should fully identify a station on a layout.
#### Is Terminus?
Check this if the tracks ends in the station. 
#### Is Shadow Station
Shadow stations represents the outer world for the layout. 
#### Tracks
You need to add all tracks that should be available for scheduled trains.
These should be checked as *Is Scheduled*.

**Display order** is used to sort the tracks in various reports, 
for instance in the graphical schedule.

**Platform length** should only be specified if there *is* a platform
available. Also 0 means there is a platform, so leave empty to indicate no platform at all.

**Is through track** shall be checked if trains can run trough the 
station on that track. Also *sidings* can be through.

**Is siding** are any track that deviates from the main track(s).
Any *scheduled* track that is not a *siding* is considered as a main track.

#### Documents to upload
This must be a PDF-file with operation instruction and nothing else.

