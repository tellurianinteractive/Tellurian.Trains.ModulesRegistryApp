## Fragtkunder og fragter
En vigtig del af en køreplansession er fragt.
Fragterne kan styres på to måder:
- Vogne i fast turnus, hvor de kører efter køreplan.
- Vogne styret af fragtbreve.

### Fragtbreve
I **Modulregistret** kan fragtbreve fremstilles på to måder:
- Fragtbreve, der kan genbruges fra møde til møde.
- Fragtbreve for fragt mellem godsstationerne, der er i et bestemt træfanlæg.

**Modulregistret** har grundlæggende de samme funktioner som *De Gule Sider*, men med nogle forskelle.

### Fragtkunder
I **Modulregistret** indtaster du fragtkunder og fragtstrømme, som varer sendes og modtages for.
Du kan indtaste fragtkunder og varestrømme til din egen modulstation,
men også for *eksterne stationer*.

En *ekstern station* er en rigtig station, der skal indtastes med historisk korrekte data
om stationen, dens kunder og godsstrømme.

### Sådan oprettes fragtbreve
I modsætning til *De Gule Sider* kan du ikke selv oprette fragtbreve manuelt i **Modulregisteret**.
I stedet opretter applikationen fragtbreve ved at matche fragtstrømmen for de afsendende og modtagende fragtkunder.
Matchningen foretages på følgende data:
- **Tema** for sende- og modtagestationen skal matche. Dette forhindrer f.eks. amerikanske og europæiske temaer er blandet.
- **Skala** for at undgå at skabe fragt mellem moduler af forskellig skala. Eksterne stationer har ingen skala, så de matches uanset skala.
- **Type af vare** skal matche. Dette matches på varens interne id og ikke på navnet. Et tilpasset varenavn påvirker *ikke* matchningen.
- **År** skal overlappe: tidsperioderne for *varetype*, *sende- og modtagestation og fragtfoder*.
Det betyder, at fragtbreve er skabt historisk nøjagtige.

### Tilpas og udskriv fragtbreve
De fragtbreve, der oprettes, kan tilpasses på flere måder:
- Antal til udskrivning.
- Hvis du ønsker at udskrive et givet antal eller angivet antal pr. trafikdag.
- Hvis du vil have et returfragtbrev for en tomvogn.

Ved afgående vognlæs kan du også printe tomme vognordrer, som du afgiver på skyggebanegården.