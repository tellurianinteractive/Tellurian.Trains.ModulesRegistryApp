### Forberedelse inden træf
Det er de ting, du kan gøre derhjemme eller i klubben, inden du rejser til træffet.
 
#### Forberedelse af dekoderindstillinger
Hvis du har evnerne eller har nogen, der kan hjælpe dig, så gør følgende for at justere indstillingerne for din loko DCC-dekoder:
- Læs CV 29. Bits er nummereret fra 0 til 7.
  - Hvis bit 2 er indstillet, indikerer det analog drift. Denne skal slås fra ved at sætte denne bit til nul. Dette forhindrer lokomotivet i at haste med fuld fart, hvis en booster svigter.
  - Hvis bit 3 er indstillet, indikerer det, at RailCom er tændt. Dette skal slås fra ved at sætte denne bit til nul. Dette gør DCC lidt mere effektivt.
  - Anbefalede CV 29-værdier er:
- 0 er et loko med kort adresse 1-99 og 14 hastighedstrin.
- 2 er et loko med kort adresse 1-99 og 28/126 hastighedstrin.
- 32 er et loko med lang adresse 128-9999 og 14 hastighedstrin.
- 34 er et lokomotiv med lang adresse 128-9999 og 28/126 trin.
- For lokomotiv i *omvendt* retning, læg 1 til ovenstående CV29-værdier.
- Hvis CV 19 er > 0, er dette den adresse, lokomotivet vil svare på. Det anbefales at indstille CV 19=0 for at deaktivere denne adresse.
- Lokoadresse nummer 3 er forbudt. Dette er den mest almindelige standardadresse for nye dekodere.
- Få dit lokomotiv til at starte og stoppe jævnt, ideelt set en hastighed, der er proportional med, hvor meget du drejer på FRED-knappen. Et lokomotiv, der springer væk, er ikke sjovt at køre.
  - CV 3 styrer accelerationen, og du kan bruge enhver værdi, men ikke for lang.
  - CV 4 styrer retardation, og du bør bruge en lille værdi, så lokomotivet kan stoppes på en kort strækning.
- Lokomotivets tophastighed skal være "sikker". Juster værdi CV 5 for dette.

#### Klargøring af lokomaskiner
- Brug hjul, der opfylder kravene, [NEM 310](https://www.morop.eu/images/NEM_register/NEM_E/nem310_en_2009_20111116.pdf) bør betragtes som et minimumskrav,
men [RP 25](https://www.nmra.org/sites/default/files/standards/sandrp/pdf/RP-25%202009.07.pdf) er ofte påkrævet.
- Rengør hjulene og kraftkontaktbladene for optimalt strømforbrug.
- Hvis dit lokomotiv har gummiringe forskridsikre, så tjek at de er i orden. Hjul til gummiringe vil ikke køre godt uden disse ringe.
- Test dine lokomotiver før mødet ved at køre dem på en bane, ikke kun i en testleje.
- Hvis dit loko svigter under mødet, er der forhåbentlig nogen, der kan stille et reservelok til rådighed. Så tag nogle ekstra lokomotiver med.
 
For H0-lokomotiver:
- Brug FREMO-godkendte koblinger - *Fleischmann 6511* eller [*Weinert 8641*](https://weinert-modellbau.de/shop/weinert-modellbau-h0/bauteile-h0/grosspackung-kupplungen-zum-einsetzen-in-die-pufferbohle-detalje).
- Juster hjulet back-to-back til at være 14,5 mm ± 0,1 mm.

#### Klargøring af passagertog med stytrevogn
Det anbefales, at lokomotivet og styrevognen i den anden ende har synkroniseret lys, der skifter med togenes retning.
Det betyder, at de skal have samme DCC-adresse og samme forlygte- og baggrundslysfunktioner. Disse indstillinger ændres i dekoderindstillingerne.

#### Forberedelse af FRED
FRED-kørehåndtag er det eneste, der skal bruges til FREMO-træf.
- Sørg for, at din FRED er mærket med operatør, klasse og inventarnummer, eksempel **DB MZ 1456**. Dette hjælper folk med at finde den rigtige FRED.
- Det anbefales også at sætte væsentlige lokofunktioner på FRED-mærkatet. Som minimum skal lysfunktioner, horn og motor dokumenteres.
- Bemærk, at funktioner over F8 nogle gange ikke virker ved FREMO-træf på grund af begrænsninger i den digitale central, der anvendes. Så gør alle væsentlige funktioner tilgængelige på F1-F8.
- Selvom du ikke har en FRED til dine lokomotiver, skal du forberede et FRED-merkat.

#### Forberedelse af lokokort
Alle lokomotiver, der kører rutetog, skal normalt have et lokokort.
Et lokokort skal have et gennemsigtigt indskudt kreditkortstørrelse, hvor lokomotivet vil blive sat.

### Til mødet
Man **kan** ikke bare sætte et lokomotiv med en ikke-reserveret adresse på banen.
Ved modulmøder skal lokoadresser koordineres. Der er to hovedmåder at gøre dette på:
- **Brug reserverede FREMO-lokoadresser:** du har fået et sæt lokoadresser af FREMO, eller du skal låne en reserveret adresse fra en, der har, eller
- **Brug adressereservation pr. møde:** Reserver din eksisterende loko-adresse på forhånd eller på stedet, før du sætter den på banen.
Hvis din adresse allerede er optaget af en anden, skal du reservere en anden adresse, der er gratis, og ændre dit lokos adresse til den.skridsikre gummiringe, så tjek at de er i orden. Locos med hjul til skridsikring gummiringe vil ikke køre godt uden disse ringe.
- Test dine lokomotiver før mødet ved at køre dem på en bane, ikke kun i en testleje.
- Hvis dit loko svigter under mødet, er der forhåbentlig nogen, der kan stille et reservelok til rådighed. Så tag nogle ekstra lokomotiver med.

#### Klargøring af push-pull passagertog
Det anbefales, at lokomotivet og styrevognen i den anden ende har synkroniseret lys, der skifter med togenes retning.
Det betyder, at de skal have samme DCC-adresse og samme forlygte- og baggrundslysfunktioner. Disse indstillinger ændres i dekoderindstillingerne.

#### Forberedelse af FRED
FRED-kørehåndtag er det eneste, der skal bruges til FREMO-møder.
- Sørg for, at din FRED er mærket med operatør, klasse og inventarnummer, eksempel **DB MZ 1456**. Dette hjælper folk med at finde den rigtige FRED.
- Det anbefales også at sætte væsentlige lokofunktioner på FRED-mærkatet. Som minimum skal lysfunktioner, horn og motor dokumenteres.
- Bemærk, at funktioner over F8 nogle gange ikke virker ved FREMO-møder på grund af begrænsninger i den digitale central, der anvendes. Så gør alle væsentlige funktioner tilgængelige på F1-F8.
- Selvom du ikke har en FRED til dine lokomotiver, skal du forberede et FRED-klistermærke.

#### Forberedelse af lokokort
Alle lokomotiver, der kører rutetog, skal normalt have et lokokort.
Et lokokort skal have et gennemsigtigt indskudt kreditkortstørrelse, hvor lokomotivet vil blive sat.

### Til mødet
Man **kan** ikke bare sætte et lokomotiv med en ikke-reserveret adresse på banen.
Ved modulmøder skal lokoadresser koordineres. Der er to hovedmåder at gøre dette på:
- **Brug reserverede FREMO-adresser:** du har fået et sæt adresser af FREMO, eller du skal låne en reserveret adresse fra en, der har, eller
- **Brug adressereservation pr. møde:** Reserver din eksisterende loko-adresse på forhånd eller på stedet, før du sætter lokomotivet på banen.
Hvis din adresse allerede er optaget af en anden, skal du reservere en anden adresse, der er fri, og ændre dit lokos adresse til den.
