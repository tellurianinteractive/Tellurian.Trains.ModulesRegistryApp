## Fraktkunder og frakter
En viktig del av ett trafikspil er frakt.
Fraktene kan styres på to måter:
- Vogne i fast turnus, hvor de kjører på ruteplan.
- Vognlast styret av fraktbrev.

### Fraktbrev
I **Modulregisteret** kan fraktbrev produseres på to måter:
- Fraktbrev som kan gjenbrukes fra treff til treff.
- Fraktbrev for frakt mellom godsstasjonene som er i et bestemt treffanlegg.

**Modulregisteret** har i utgangspunktet de samme funksjonene som *Gule sider*, men med noen forskjeller.

### Fraktkunder
I **Modulregisteret** legger du inn fraktkunder og fraktstrømmer som varer sendes og mottas for.
Du kan legge inn fraktkunder og varestrømmer for din egen modulstasjon,
men også for *eksterne stasjoner*.

En *ekstern stasjon* er en reell stasjon som skal legges inn med historisk korrekte data
om stasjonen, dens kunder og godsstrømmer.

### Slik lages fraktsedler
I motsetning til *Gule sider*, kan du ikke manuelt lage fraktsedler selv i **Modulregisteret**.
I stedet oppretter applikasjonen fraktsedler ved å matche fraktflyten til avsendende og mottakende fraktkunder.
Matchningen gjøres på følgende data:
- **Tema** for sende- og mottaksstasjonen må samsvare. Dette forhindrer f.eks. amerikanske og europeiske temaer er blandet.
- **Skala** for å unngå å lage frakt mellom moduler av ulik skala. Eksterne stasjoner har ingen skala, så de matches uansett skala.
- **Type last** må samsvare. Dette matches på lastens interne id, og ikke på navnet. Et tilpasset lastetypenavn *påvirker* ikke samsvaret.
- **År** må overlappe: tidsperiodene for *type gods*, *sendings- og mottaksstasjon og fraktfôr*.
Dette betyr at fraktsedler lages historisk nøyaktige.

### Tilpass og skriv ut fraktsedler
Fraktbrevene som opprettes kan tilpasses på flere måter:
- Antall som skal skrives ut.
- Hvis du ønsker å skrive ut et gitt antall eller spesifisert antall per operasjonsdag.
– Hvis du vil ha returfraktbrev for en tomvogn.

For avgående vognlass kan du også skrive ut tomme vognbestillinger, som du legger inn på skyggebanegården.