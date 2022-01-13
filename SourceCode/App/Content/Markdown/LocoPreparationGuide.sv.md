## Förbered ditt lok för modulmötet
Här är några riktlinjer för att förbereda dina lok innan du sätter dem på rätt spår
på modulträffar.

### Förberedelse inför möte
Det här är de saker du kan göra hemma eller på klubben innan du reser till mötet.
 
#### Förberedelse av dekoderinställningar
Om du själv kan eller har någon som kan hjälpa dig, gör följande för att justera inställningarna för ditt loks DCC-dekoder:
- Läs CV 29. Bitarna är numrerade från 0 till 7.
  - Om bit 2 är inställd indikerar det analog drift. Denna måste stängas av genom att nollställa denna bit. Detta förhindrar att loket rusar i full fart om en booster går sönder.
  - Om bit 3 är inställd indikerar det att RailCom är på. Detta måste stängas av genom att sätta denna bit till noll. Detta gör DCC lite mer effektivt.
  - Rekommenderade CV 29-värden är:
	- 0 är ett lok med kort adress 1-99 och 14 hastighetssteg.
	- 2 är ett lok med kort adress 1-99 och 28/126 hastighetssteg.
	- 32 är ett lok med lång adress 128-9999 och 14 hastighetssteg.
	- 34 är ett lok med lång adress 128-9999 och 28/126 hastighetssteg.
	- För lok i *omvänd* riktning, lägg till 1 till ovanstående CV29-värden.
- Om CV 19 är > 0 är detta adressen som loket kommer att svara på. Det rekommenderas att ställa in CV 19=0 för att inaktivera denna adress.
- Lokadress nummer 3 är förbjuden. Detta är den vanligaste standardadressen för nya avkodare.
- Få ditt lok att starta och stanna mjukt, helst en hastighet som är proportionell mot hur mycket du vrider på FRED-ratten. Ett lok som hoppar iväg är inte kul att köra.
  - CV 3 styr accelerationen och du kan använda vilket värde som helst, men inte för länge.
  - CV 4 kontrollerar retardation och du bör använda ett litet värde så att loket kan stoppas på en kort sträcka.
- Lokets toppfart bör vara "säker". Justera värdet CV 5 för detta.

#### Förberedelse av lokmaskineri
- Rengör hjulen och kraftkontaktbladen för optimal energiförbrukning.
- Om ditt lok har slirskydd, kontrollera att de är okej. Lok med hjul avsedda för slirskydd kommer inte att fungera bra utan dessa ringar.
– Testa dina lok före träffen genom att köra dem på en bana, inte bara i en provbädd.
– Om ditt lok går sönder träffen mötet finns det förhoppningsvis någon som kan tillhandahålla ett reservlok. Så ta med lite extra lok.

#### Förberedelse av persontåg med lok och manövervagn
– Det rekommenderas att loket och manövervagnen i andra änden har synkroniserade ljus som växlar med tågets riktning.
Det betyder att de måste ha samma DCC-adress och samma strålkastar- och bakgrundsbelysningsfunktioner. Dessa inställningar ändras i dekoderns inställningar.

#### Förberedelse av FRED
FRED-körhandtaget är det enda som används vid FREMO-möten.
- Se till att din FRED har märkts med operatör, littera och fordonsnummer, exempel **DB MZ 1456**. Detta hjälper människor att hitta rätt FRED.
- Det rekommenderas också att ange viktiga lokfunktioner på FRED-dekalen. Minst ljusfunktioner, signalhorn och motor bör dokumenteras.
- Observera att funktioner ovanför F8 ibland inte fungerar vid FREMO-träffar, på grund av begränsningar i den digitala centralen som används. Så gör alla viktiga funktioner tillgängliga på F1-F8.
- Även om du inte har en FRED på dina lok, förbered en FRED-dekal.

#### Förberedelse av lokkort
– Alla lok som kör linjetåg brukar ha lokkort.
- Ett lokkort måste ha en genomskinlig inpackad kreditkortsstorlek, där lokets omloppsplan sätts.

### På mötet
Du **får** inte sätta ett lok med en icke-reserverad adress på banan.
Vid modulträffar ska lokadresser samordnas. Det finns två huvudsakliga sätt att göra detta:
- **Använd reserverade FREMO-lokadresser:** du har fått en uppsättning lokadresser av FREMO, eller så måste du låna en reserverad adress från en som har, eller
- **Använd adressreservation per möte:** Boka din befintliga lokadress i förväg eller på platsen innan du sätter den på banan.
Om din adress redan är upptagen av någon annan måste du reservera en annan adress som är gratis och ändra ditt loks adress till den.