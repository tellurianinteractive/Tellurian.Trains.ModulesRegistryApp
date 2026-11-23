## Fragtkunder og fragter
En vigtig del af en køreplansession er fragter. De kan styres på to måder:
- Vogne i fast turnus, der kører efter køreplan.
- Vognlæs, der styres med fragtbreve.

### Fragtbreve
I **Modulregistret** laves fragtbreve på to måder:
- **Pr. station:** fragtbreve for en stations fragtkunder, som gemmes og kan genbruges fra træf til træf.
- **Pr. træfanlæg:** fragtbreve for fragter mellem stationerne i et anlæg på et træf.

Du skriver ikke selv fragtbrevene. **Modulregistret** laver dem ved at matche de afsendende og modtagende fragtkunders laststrømme.

### Fragtkunder og laststrømme
Du indtaster fragtkunder og deres laststrømme, de varer de sender og modtager, for dine egne modulstationer og også for *eksterne stationer*. En *ekstern station* er en rigtig station, der indtastes med historisk korrekte data om stationen, dens kunder og laststrømme.

En laststrøm har en retning:
- **Send** og **Modtage**: inden for samme land.
- **Eksport** og **Import**: også til og fra andre lande.

En laststrøm kan også have årstal og ugedage. Uden egne årstal har laststrømmen kundens årstal.

### Hvad der matches
**Varer med NHM-kode** matches med laststrømme på alle andre modulstationer og eksterne stationer:
- Samme varetype, hvor den ene sender (send eller eksport) og den anden modtager (modtage eller import). Der matches på varetypen, så et eget navn på varen påvirker ikke matchningen.
- Inden for samme land matcher alle disse retninger. Mellem lande matcher kun eksport med import.
- Mellem modulstationer skal **hovedtemaet** (Europa eller USA) være det samme, og også **skalaen**. Eksterne stationer har ingen af dem, så de matches alligevel.
- **Årstallene** for de to laststrømme skal overlappe, og varen skal findes i den periode. Det gør fragtbrevene historisk korrekte.

**Varer uden NHM-kode**, som stykgods, post og ekspresgods, matches ikke med enkelte stationer, da det ville give tusindvis af fragtbreve. De går til eller fra den *repræsentative station* i din stations region. En station uden region, eller i en region uden repræsentativ station, får ingen sådanne fragtbreve.

**På et træfanlæg** handler anlæggets stationer med hinanden, også på tværs af landegrænser. Eksterne stationer er ikke med, og laststrømmene skal være i drift inden for anlæggets årstal.

### Opret, tilpas og udskriv fragtbreve
På en fragtkundes side for fragtbreve opretter **Opdater** fragtbrevene for alle kundens laststrømme. Et fragtbrev mellem to modulstationer hører til den modtagende kunde og udskrives af den modtagende station. Du kan opdatere igen når som helst: kun nye match tilføjes, og dine eksisterende fragtbreve beholder deres antal til udskrivning.

Fragtbrevene kan tilpasses på flere måder:
- Antal til udskrivning. Nul betyder, at fragtbrevet ikke udskrives, og **Markér alle som udskrevet** sætter nul for de viste fragtbreve.
- Dagene. Et fragtbrev sendes på den afsendende kundes dage.
- Ét fragtbrev pr. driftsdag i stedet for ét for alle dagene.
- Et returfragtbrev for den tomme vogn, som sendes dagligt.

For afgående vognlæs kan du også udskrive *tomvognsordrer*, som du placerer på skyggebanegården.