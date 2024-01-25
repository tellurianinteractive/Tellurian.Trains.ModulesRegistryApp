## Godskunder och frakter
En viktig del av ett trafikspel är frakter.
Frakterna kan styras på två sätt:
- Vagnar i fasta omlopp, där de går på tidtabell.
- Vagnlaster som styrs med fraktsedlar.

### Fraktsedlar
I **Modulregistret** kan man producera fraktsedlar på två sätt:
- Per trafikplats där man skapar fraktsedlar som kan återanvändas från träff till träff.
- Fraktsedlar för frakter mellan de trafikplatser som finns med på en viss träff.

Modulregistret har i princip samma funktioner som *Gula Sidorna*, men med vissa skillnader.

### Godskunder
I **Modulregistret** lägger man in godskunder och godsflöden för vilka varor som skickas och mottas.
Man kan lägga in godskunder och godsflöden för sin egen modulstation, 
men även för *externa stationer*.

En *extern station* är en verklig station som måste läggas in med historiskt korrekta data
om stationen, dess kunder och godsflöden.

### Så här skapas fraktsedlar
Till skillnad från *Gula Sidorna* kan man inte själv skapa valfria fraktsedlar i **Modulregistret**.
Istället skapar applikationen fraktsedlar genom att matcha avsändande och mottagande godskunders godsflöde.
Matchningen görs på följande data:
- **Tema** för avsändande och mottagande station skall överensstämma. Detta förhindrar att t.ex. amerikanskt och europeiskt tema blandas.
- **Skala** för att undvika att skapa fratsedlar mellan moduler av olika skala. Externa stationer har ingen skala, så de matchas oavsett skala.
- **Typ av last** skall överensstämma. Detta matchas på lastens interna id, och inte på namnet. Ett eget namn på lasttypen påverkar *inte* matchningen.
- **Årtal** skall överlappa: tidsperioderna för *typ av gods*, *avsändande och mottagande station och godsföde*. 
Detta innebär att fraktsedlar blir historikst korrekta.

### Anpassa och skriva ut fraktsedlar
De fraktsedlar som skapas kan anpassas på flera sätt:
- Antal att skriva ut.
- Om man vill skriva ut givet antal eller angivet antal per trafikdag.
- Om man vill ha en returfraktsedel för tomvagn.

För avgående vagnslatser kan kan även skriva ut tomvagnsbeställningar, fraktsedlar som man placerar vid tågmagasin.
