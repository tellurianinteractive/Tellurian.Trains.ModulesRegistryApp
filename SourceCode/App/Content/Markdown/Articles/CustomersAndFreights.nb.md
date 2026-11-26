## Fraktkunder og frakter
En viktig del av et trafikkspill er frakter. De kan styres på to måter:
- Vogner i faste omløp, som går etter ruteplan.
- Vognlaster som styres med fraktbrev.

### Fraktbrev
I **Modulregisteret** lages fraktbrev på to måter:
- **Per stasjon:** fraktbrev for en stasjons fraktkunder, som lagres og kan gjenbrukes fra treff til treff.
- **Per treffanlegg:** fraktbrev for frakter mellom stasjonene i et anlegg på et treff.

Du skriver ikke fraktbrevene selv. **Modulregisteret** lager dem ved å matche avsendende og mottakende fraktkunders laststrømmer.

### Fraktkunder og laststrømmer
Du legger inn fraktkunder og deres laststrømmer, varene de sender og mottar, for dine egne modulstasjoner og også for *eksterne stasjoner*. En *ekstern stasjon* er en virkelig stasjon som legges inn med historisk korrekte data om stasjonen, dens kunder og laststrømmer.

En laststrøm har en retning:
- **Send** og **Motta**: innenfor samme land.
- **Eksport** og **Import**: også til og fra andre land.

En laststrøm kan også ha årstall og ukedager. Uten egne årstall har laststrømmen kundens årstall.

### Hva som matches
**Varer med NHM-kode** matches mot laststrømmer på alle andre modulstasjoner og eksterne stasjoner:
- Samme varetype, der den ene sender (send eller eksport) og den andre mottar (motta eller import). Det matches på varetypen, så et eget navn på varen påvirker ikke matchingen.
- Innenfor samme land matcher alle disse retningene. Mellom land matcher bare eksport mot import.
- Mellom modulstasjoner må **hovedtemaet** (Europa eller Nord-Amerika) være det samme, og også **skalaen**. Eksterne stasjoner har ingen av dem, så de matches likevel.
- **Årstallene** for de to laststrømmene må overlappe, og varen må finnes i den perioden. Det gjør fraktbrevene historisk korrekte.

**Varer uten NHM-kode**, som stykkgods, post og ekspressgods, matches ikke mot enkeltstasjoner, siden det ville gitt tusenvis av fraktbrev. De går til eller fra den *representative stasjonen* i din stasjons region. En stasjon uten region, eller i en region uten representativ stasjon, får ingen slike fraktbrev.

**På et treffanlegg** handler anleggets stasjoner med hverandre, også over landegrenser. Eksterne stasjoner er ikke med, og laststrømmene må være i drift innenfor anleggets årstall.

### Lage, tilpasse og skrive ut fraktbrev
På en fraktkundes side for fraktbrev lager **Oppdater** fraktbrevene for alle kundens laststrømmer. Et fraktbrev mellom to modulstasjoner hører til den mottakende kunden og skrives ut av den mottakende stasjonen. Du kan oppdatere igjen når du vil: bare nye treff legges til, og fraktbrevene du har beholder antallet som skal skrives ut.

Fraktbrevene kan tilpasses på flere måter:
- Antall som skal skrives ut. Null betyr at fraktbrevet ikke skrives ut, og **Merk alle som skrevet ut** setter null for fraktbrevene som vises.
- Dagene. Et fraktbrev sendes på den avsendende kundens dager.
- Ett fraktbrev per driftsdag i stedet for ett for alle dagene.
- Et returfraktbrev for den tomme vognen, som sendes daglig.

For avgående vognlaster kan du også skrive ut *tomvognordrer*, som du plasserer på skyggebanegården.