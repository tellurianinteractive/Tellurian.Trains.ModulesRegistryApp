## Godskunder och frakter
En viktig del av ett trafikspel är frakter. De kan styras på två sätt:
- Vagnar i fasta omlopp, som går enligt tidtabell.
- Vagnslaster som styrs med fraktsedlar.

### Fraktsedlar
I **Modulregistret** görs fraktsedlar på två sätt:
- **Per station:** fraktsedlar för en stations godskunder, som sparas och kan återanvändas från träff till träff.
- **Per träffanläggning:** fraktsedlar för frakter mellan stationerna i en anläggning på en träff.

Du skriver inte fraktsedlarna själv. **Modulregistret** skapar dem genom att matcha avsändande och mottagande godskunders godsflöden.

### Godskunder och godsflöden
Du lägger in godskunder och deras godsflöden, det gods de skickar och tar emot, för dina egna modulstationer och även för *externa stationer*. En *extern station* är en verklig station som läggs in med historiskt korrekta data om stationen, dess kunder och godsflöden.

Ett godsflöde har en riktning:
- **Skicka** och **Motta**: inom samma land.
- **Export** och **Import**: även till och från andra länder.

Ett godsflöde kan också ha årtal och veckodagar. Utan egna årtal har godsflödet kundens årtal.

### Vad som matchas
**Gods med NHM-kod** matchas mot godsflöden på alla andra modulstationer och externa stationer:
- Samma typ av gods, där den ena skickar (skicka eller export) och den andra tar emot (motta eller import). Matchningen görs på godstypen, så ett eget namn på godset påverkar den inte.
- Inom samma land matchas alla dessa riktningar. Mellan länder matchas bara export mot import.
- Mellan modulstationer måste **huvudtemat** (Europa eller USA) vara detsamma, och även **skalan**. Externa stationer har inget av dem, så de matchas ändå.
- **Årtalen** för de två godsflödena måste överlappa, och godset måste finnas under den perioden. Det gör fraktsedlarna historiskt korrekta.

**Gods utan NHM-kod**, som styckegods, post och expressgods, matchas inte mot enskilda stationer, eftersom det skulle ge tusentals fraktsedlar. Det går till eller från den *representativa stationen* i din stations region. En station utan region, eller i en region utan representativ station, får inga sådana fraktsedlar.

**På en träffanläggning** handlar anläggningens stationer med varandra, även över landsgränser. Externa stationer är inte med, och godsflödena måste vara i drift under anläggningens årtal.

### Skapa, anpassa och skriva ut fraktsedlar
På en godskunds sida för fraktsedlar skapar **Uppdatera** fraktsedlarna för alla kundens godsflöden. En fraktsedel mellan två modulstationer hör till den mottagande kunden och skrivs ut av den mottagande stationen. Du kan uppdatera igen när du vill: bara nya matchningar läggs till, och de fraktsedlar du har behåller sitt antal att skriva ut.

Fraktsedlarna kan anpassas på flera sätt:
- Antal att skriva ut. Noll betyder att fraktsedeln inte skrivs ut, och **Markera alla som utskrivna** sätter noll för de fraktsedlar som visas.
- Dagarna. En fraktsedel skickas den avsändande kundens dagar.
- En fraktsedel per trafikdag, i stället för en för alla dagarna.
- En returfraktsedel för den tomma vagnen, som skickas dagligen.

För avgående vagnslaster kan du även skriva ut *tomvagnsbeställningar*, som du placerar vid tågmagasinet.