## Prepare your loco for the module meeting
Here are some guidelines to prepare your locos before putting them on track
at module meetings.

### Preparation before meeting
These are the things that you can do at home or club before travelling to the meeting.
 
#### Preparation of decoder settings
If you have the skill or have someone that can help you, do the following to adjust the settings of your loco DCC-decoder:
- Read CV 29. Bits are numbered from 0 to 7. 
  - If bit 2 is set it indicates analogue operation. This must be turned of by setting this bit to zero. This prevents the loco to rush at full speed if a booster fails.
  - Recommended CV 29 values are:
	- 0 is a loco with short address 1-99 and 14 speed steps.
	- 2 is a loco with short address 1-99 and 28/126 speed steps.
	- 32 is a loco with long address 128-9999 and 14 speed steps.
	- 34 is a loco with long address 128-9999 and 28/126 speed steps.
	- For loco in *reverse* direction, add 1 to the above CV29 values.
- If CV 19 is > 0 this is the address the loco will respond to. Its recommended to set CV 19=0 to disable this address.
- Loco address number 3 is forbidden. This is the most common default address of new decoders.
- Make your loco start and stop smooth, ideally a speed that is proportional to how much you turn the FRED knob. 
A loco that jumps away is not fun to drive.
  - CV 3 controls acceleration and you can use any value, but not too long.
  - CV 4 controls retardation and you should use a small value so the loco can be stopped on a short stretch.
- Top speed of loco should be “safe”. Adjust value CV 5 for this. 

#### Preparation of loco machinery
- Clean the wheels and power contact blades for optimal power consumption. 
- If your loco has slip protection rubber rings, check that they are okay. Locos with wheels for slip protection rubber rings will not run well without these rings.
- Test your locos before the meeting by running them on a track, not just in a test bed.
- If your loco fails during the meeting, there will hopefully be someone that can provide a spare loco. So bring some extra locos.
- Use wheels that meets the meeting requirements, [NEM 310](https://www.morop.eu/images/NEM_register/NEM_E/nem310_en_2009_20111116.pdf) should be regarded as a minimum requirement,
- but [RP 25](https://www.nmra.org/sites/default/files/standards/sandrp/pdf/RP-25%202009.07.pdf) is often required.
 
For H0-locomotives:
- Use FREMO-approved couplings - *Fleischmann 6511* or [*Weinert 8641*](https://weinert-modellbau.de/shop/weinert-modellbau-h0/bauteile-h0/grosspackung-kupplungen-zum-einsetzen-in-die-pufferbohle-detail).
- Adjust wheel back-to-back to be 14.5mm ± 0.1 mm. 

#### Preparation of push-pull passenger trains
- It is recommended that the loco and the control car in the other end has synchronized lights that shifts with the trains direction. 
This means they must have the same DCC-address and same headlight and backlight functions. These settings are changed in the decoder settings.

#### Preparation of FRED
The FRED-throttle is the only one to be used at FREMO meetings. 
- Ensure that your FRED has been labelled with the operator, class and inventory number, example **DB MZ 1456**. This helps people to find the right FRED.
- It is also recommended to put essential loco functions on the FRED sticker. At least Light-functions, Horn and Motor should be documented. 
- Note that functions above F8 sometimes don’t work at FREMO meetings, due to limitations in the digital central used. So make all essential functions available on F1-F8.
- Even if you don’t have a FRED to your locos, prepare a FRED-sticker.

#### Preparation of Loco card
- All locos that run scheduled trains usually must have a loco card. 
- A loco card must have a transparent pocked in credit card size, where the loco schedule will be put.

### At the meeting
You **cannot** just put a loco with a non-reserved address on the track.
At module meetings, loco addresses must be coordinated. There are two main ways to do this:
- **Use reserved FREMO-loco addresses:** you have been given a set of loco-addresses by FREMO, or you have to borrow a reserved address from one that has, or
- **Use per-meeting address reservation:** Reserv your existing loco-address in advance or at the venue before putting it on the track. 
If your address already is occupied by someobe else, you must reserve another address that is free, and change your loco's address to that one.
