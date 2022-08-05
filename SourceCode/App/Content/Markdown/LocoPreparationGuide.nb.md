## Forbered lokoteket for modultreffer
Her er noen retningslinjer for å forberede lokene dine før du setter dem på sporet
på modultreff.

### Forberedelse før treff
Dette er tingene du kan gjøre hjemme eller i klubben før du reiser til treffet.
 
#### Forberedelse av dekoderinnstillinger
Hvis du har ferdighetene eller har noen som kan hjelpe deg, gjør følgende for å justere innstillingene til loco DCC-dekoderen:
- Les CV 29. Bits er nummerert fra 0 til 7.
  - Hvis bit 2 er satt, indikerer det analog drift. Denne må slås av ved å sette denne biten til null. Dette hindrer lokomotivet i å rushe i full fart hvis en booster svikter.
   - Anbefalte CV 29-verdier er:
    - 0 er et loko med kort adresse 1-99 og 14 hastighetstrinn.
    - 2 er et loko med kort adresse 1-99 og 28/126 hastighetstrinn.
    - 32 er et loko med lang adresse 128-9999 og 14 hastighetstrinn.
    - 34 er et loko med lang adresse 128-9999 og 28/126 hastighetstrinn.
    - For loko i *revers* retning, legg til 1 til CV29-verdiene ovenfor.
- Hvis CV 19 er> 0, er dette adressen lokomotivet vil svare på. Det anbefales å sette CV 19 = 0 for å deaktivere denne adressen.
- Lokoadresse nummer 3 er forbudt. Dette er den vanligste standardadressen til nye dekodere.
- Få lokomotivet til å starte og stoppe jevnt, ideelt sett en hastighet som er proporsjonal med hvor mye du dreier på FRED-knappen. Et loko som hopper unna er ikke morsomt å kjøre.
  - CV 3 styrer akselerasjonen og du kan bruke hvilken som helst verdi, men ikke for lang.
  - CV 4 kontrollerer retardasjon og du bør bruke en liten verdi slik at lokomotivet kan stoppes på en kort strekning.
- Toppfart på lokomotivet skal være "sikkert". Juster verdi CV 5 for dette.

#### Klargjøring av lokomaskiner
- Bruk hjul som oppfyller kravene, [NEM 310](https://www.morop.eu/images/NEM_register/NEM_E/nem310_en_2009_20111116.pdf) bør betraktes som et minimumskrav,
men [RP 25](https://www.nmra.org/sites/default/files/standards/sandrp/pdf/RP-25%202009.07.pdf) er ofte nødvendig.
- Rengjør hjulene og kontaktbladene for optimalt strømforbruk.
- Hvis lokomotivet ditt har sklibeskyttelsesgummieringer, sjekk at de er i orden. Locos med gummiringer for sklisikring vil ikke gå bra uten disse ringene.
- Test lokoene dine før møtet ved å kjøre dem på en bane, ikke bare i en prøveseng.
- Hvis lokoet ditt svikter under møtet, er det forhåpentligvis noen som kan skaffe et reservelok. Så ta med noen ekstra lok.
 
For H0-lokomotiver:
- Bruk FREMO-godkjente koblinger - *Fleischmann 6511* eller [*Weinert 8641*](https://weinert-modellbau.de/shop/weinert-modellbau-h0/bauteile-h0/grosspackung-kupplungen-zum-einsetzen-in-die-pufferbohle-detalj).
- Juster hjulenes back-to-back til 14,5 mm ± 0,1 mm.

#### Klargjøring av persontog med styrevogn
Det anbefales at loket og styrevognen i den andre enden har synkroniserte lys som skifter med togenes retning.
Dette betyr at de må ha samme DCC-adresse og samme frontlys- og bakgrunnslysfunksjoner. Disse innstillingene endres i dekoderinnstillingene.

#### Forberedelse av FRED
FRED-kørehåndtak er den eneste som skal brukes på FREMO-treff.
- Sørg for at din FRED er merket med operatør, klasse og inventarnummer, eksempel **DB MZ 1456**. Dette hjelper folk til å finne den rette FRED.
- Det anbefales også å sette viktige lokofunksjoner på FRED-klistremerket. Minst lys-funksjoner, horn og motor bør dokumenteres.
- Merk at funksjoner over F8 noen ganger ikke fungerer på FREMO-møter, på grunn av begrensninger i den digitale sentralen som brukes. Så gjør alle viktige funksjoner tilgjengelige på F1-F8.
- Selv om du ikke har en FRED på lokomotivene dine, forbered et FRED-klistremerke.

#### Utarbeidelse av Loco-kort
– Alle lok som kjører rutetog skal vanligvis ha lokokort.
- Et lokokort må ha en gjennomsiktig innpakket kredittkortstørrelse, hvor omløpskort vil bli satt.
- Lokomotivkort kan du f.eks. produsere med [lokomotiv- og vognkort-appen](https://wagoncardapp.azurewebsites.net/).### På møtet

#### På treffet
Du **kan ikke** bare sette et loko med en ikke-reservert adresse på banen.
Ved modulmøter skal lokoadresser koordineres. Det er to hovedmåter å gjøre dette på:
- **Bruk reserverte FREMO-lokoadresser:** du har fått et sett med lokoadresser av FREMO, eller du må låne en reservert adresse fra en som har, eller
- **Bruk adressereservasjon per møte:** Reserver din eksisterende loko-adresse på forhånd eller på stedet før du setter den på banen.
Hvis adressen din allerede er okkupert av noen andre, må du reservere en annen adresse som er gratis, og endre lokoadressen til den.
