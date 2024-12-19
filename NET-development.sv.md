# .NET-utveckling
*Av [Stefan Fj�llemark](https://github.com/fjallemark), Tellurian Interactive AB, Sverige, december 2024*

Den h�r artikeln syftar till att beskriva teknikval vid utveckling av mjukvara i modernt .NET. 
I skrivande stund b�r all ny .NET-utveckling anv�nda .NET 9 eller senare.

## Nuvarande .NET
Nuvarande .NET �r gratis och �ppen k�llkod; rambiblioteken, exekveringsmotorerna och kompilatorerna. 
Du kan bygga alla typer av applikationer p� n�stan vilken plattform som helst. 
.NET k�rs �ven i webbl�saren i web assembly och p� m�nga enkelkortsdatorer. 
Detta g�r .NET till en one-stop-shop d�r du kan �teranv�nda mycket av dina .NET-kunskaper f�r alla typer av applikationer.

> ### Missuppfattningar om .NET
> N�r du s�ker p� Internet och j�mf�r .NET med andra plattformar och tekniker verkar det som att mycket av skrivningen om 
> .NET �r f�r�ldrad eller helt enkelt fel. [Denna YouTube-video av Nick Chapsas](https://youtube.com/watch?v=AFNujHJfMtU) ger n�gra exempel.

> ### .NET Framework
> Dagens .NET �r *inte* samma sak som .NET Framework, som endast �r Windows och aldrig b�r anv�ndas f�r nya applikationer. 
> Den senaste versionen av .NET Framework �r 4.8, och den kommer att underh�llas s� l�nge som Windows st�ds.

## Programspr�k
Alla spr�k p� .NET anv�nder samma exekveringssystem och samma typsystem, 
s� du kan vanligtvis kombinera bibliotek skrivna i vilket .NET-program som helst med varandra. 
Det finns n�gra regler att f�lja f�r att f� det att fungera.

- **C#** �r det vanligaste spr�ket som anv�nds i .NET-utveckling. 
�r ett spr�k i C-familjen och delar de flesta av sina egenskaper med programmeringsspr�ket Java.
- **F#** �r ett funktionellt f�rstaspr�k med ett pragmatiskt f�rh�llningss�tt. 
F# har en mycket mer kortfattad syntax, vilket g�r att F#-program �r betydligt mindre �n motsvarande C#-program.
- **VB.NET** �r den moderna varianten av programmeringsspr�ket BASIC. 
Den har n�stan samma funktioner som C# och en enkel och l�ttf�rst�elig syntax.

Det finns �ven andra spr�k som k�rs p� .NET, till exempel  [Phyton](https://ironpython.net/) och [COBOL](https://portal.microfocus.com/s/article/KM000009164?language=en_US)..

## Programbibliotek
Oavsett vilket .NET-spr�k du anv�nder kommer du att vara beroende av samma .NET-basbibliotek. 
S� mycket av kunskapen handlar om att f� ut det mesta av den f�rdiga programvaran som Microsoft 
och andra biblioteksskribenter har att erbjuda:

- [**.NET Base Class Library**](https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview) (BCL) har en hel del inbyggd funktionalitet f�r n�stan allt du vill skriva kod f�r.
- Bilbliotekshanteraren [**NuGet**](https://www.nuget.org/) �r s�ttet du kan �teranv�nda komponenter skrivna av andra eller dig sj�lv.

## Utvecklarverktyg
Microsoft �r k�nt f�r sina bra utvecklingsverktyg. 
Det h�r avsnittet t�cker endast gratisverktyg f�r att utveckla .NET-applikationer.

- **Visual Studio Code** �r nu en av de mest popul�ra verktygen f�r att skriva programvara p� n�stan alla spr�k. 
Det �r en plattformsoberoende applikation som k�rs p� Windows, macOS och Linux. 
Anpassning g�rs genom att installera olika plugins f�r ett visst spr�k eller andra �ndam�l.
- **Visual Studio** har en community-utg�va som �r gratis och mycket kapabel f�r utveckling av .NET-applikationer. 
Anpassning g�rs genom att v�lja att installera en eller flera utvecklingspaket. 
Det finns ocks� m�nga anv�ndbara gratis plugins att installera fr�n Visual Studio Markeplace. 
Visual Studio �r ett endast Windows-program, men det finns ocks� en separat version f�r macOS.
- **SQL Server** har en gratis utvecklarutg�va som �r en komplett version med vissa begr�nsningar. 
Det finns ocks� en gratis expressutg�va som �r idealisk f�r mindre applikationer. 
SQL Server har ocks� en molnversion men ingen gratis niv�, den minsta SQL Server-molndatabasen kostar cirka 5 � per m�nad.
- **SQL Server Management Studio** �r en gratis Windows-applikation f�r att skapa och hantera SQL Server-databas. 
Den fungerar med b�de lokala databaser och molndatabaser. Den kommer att installeras med SQL Server Developer Edition.
- **Visual Studio** har bra st�d f�r att bygga och underh�lla databaser i **Microsoft SQL Server**. 
F�r det f�rsta har du liknande tillg�ng till databasen som med SQL Server Management Studio, 
och f�r det andra finns det en speciell databasprojekttyp, 
som du kan underh�lla din SQL-kod med versionskontroll och distribuera uppdateringar och 
migrering till din befintliga databas eller skapa nya.
- **Entity Framework Core** har ocks� bra st�d f�r databasdefinitioner och inkrementella databasuppgraderingar. 
Den st�der de vanligaste databaserna.

## Anv�ndargr�nssnitt
Det finns m�nga s�tt att skapa applikationer med ett anv�ndargr�nssnitt med .NET. Huvudfaktorn �r m�lplattformen du t�nker bygga mjukvara f�r:

- **Endast Windows**: Du har f�r avsikt att utveckla ett program som endast ska k�ras p� Windows-datorer.
- **Webb**: Applikationen ska vara tillg�nglig i en webbl�sare.
- **Cross-platform**: Din app b�r k�ras p� ioS, Andriod, macOs, Windows och/eller Linux.

### Anv�ndargr�nssnittsteknik
- **Windows Forms** �r ett enkelt och snabbt s�tt att skapa Windows-applikationer med enkla formul�r.
- **Windows Presentation Foundations** f�rkortning WPF �r ett mer avancerat s�tt att skapa anv�ndargr�nssnitt endast p� Windows.
- **ASP.NET** �r ett ramverk f�r att skapa webbapplikationer. 
ASP.NET-applikationer kan vara v�rd och k�ras p� alla plattformar som st�ds av .NET, inklusive molnplattformar. 
Med Razor-syntaxen kan du skriva webbsidor som en blandning av HTML, CSS och C#.
- **Blazor** �r ett speciellt webbramverk f�r ASP.NET. Du skriver komponenter med en blandning av HTML, CSS och C#. 
Du kan interagera med JavaScript och vilken kod som helst som kan kompileras till Web Assembly (C, Rust och andra). 
  Det finns m�nga f�rdiga komponenter tillg�ngliga. Komponenterna skrivna i Blazor kan renderas i flera milj�er:
  - p� en webbserver som uppdaterar webbanv�ndargr�nssnittet via websockets,
  - p� en webbserver som skickar vanlig html till webbl�saren (nytt i .NET 8),
  - inb�ddad i vilken webbapplikation som helst med vilket ramverk som helst,
  - k�r komponenten p� webbsammans�ttning i webbl�saren, och
  - i en plattformsoberoende MAUI-applikation.
  - Med den kommande .NET 8 kommer du �ven att kunna blanda renderingsl�gen i samma applikation. Detta g�r Blazor-komponenter till de mest �terst�llbara anv�ndargr�nssnittskomponenterna i .NET.
- **Multiplatform Application User Interface** f�rkortat MAUI �r den senaste teknologin som syftar till att g�ra det enklare att skapa appar som k�rs inbyggt p� iOS, Android, macOs och Windows med det inbyggda anv�ndargr�nssnittet f�r var och en av dessa plattformar. MAUI �r en utveckling av Xmamarin Forms. Du kan ocks� b�dda in Blazor-komponenter i en MAUI-applikation, eller skriva en MAUI-applikation enbart med Blazor-komponenter.
- **UNO Plattformen** �r avsedd f�r att bygga en enda kodbas f�r
mobil-, webb-, skrivbords- och inb�ddade appar med antingen XAML eller C# Markup.
UNO-projektet drivs som �ppen k�llkod.

## Datalagring
Det finns flera typer av s�tt att lagra data och komma �t den. De viktigaste typerna av databaser �r:

- **Relationsdatabaser** som lagrar data i tabeller och som vanligtvis n�s av SQL-fr�gespr�ket. Exempel p� databaser �r Microsoft SQL Server, Postgress, My SQL och SQL Lite.
- **Dokumentdatabaser** som lagrar hela strukturer av objekt i ett stycke som en hierakisk struktur. Exempel p� databaser �r Mongo DB och Azure Cosmos DB.
- **Molnlagring** �r givetvis alla ovanst�ende typer av databaser men �ven andra alternativ som bloblagring eller nyckel/v�rdelagring.
- **Inb�ddade databaser**, ett exempel �r SQL Lite kompilerad till webassembly s� att du kan k�ra den som en lokal lagring i din Blazor-webbapplikation.

Databaser g�r din applikation mer komplex. Om kraven p� datalagring och s�kning �r begr�nsad �r det definitivt ett alternativ att bara lagra applikationsdata p� disken i en enda fil eller flera filer.
D� kan man anv�nda de inbyggda JSON- och XML-serialiseringsteknikerna f�r att skriva och l�sa data till/fr�n filer p� disken. Detta �r en robust teknik och anpassad f�r f�r�ndringar i dina datastrukturer.

### Data�tkomstteknik
F�r att komma �t data i databaser kr�vs ett bibliotek f�r data�tkomst.

- **ADO.NET** �r det grundl�ggande biblioteket f�r all databas�tkomst i .NET. 
Den �r ganska l�g niv� men har h�g prestanda, men du m�ste skriva mycket kod f�r att anv�nda den. 
ADO.NET fungerar �ven f�r molndatabaser.
- **Dapper** �r en enkel objektmappare f�r .NET och �r praktiskt taget lika snabb som att anv�nda en r� ADO.NET. 
Att anv�nda Dapper minskar m�ngden kod att skriva, men �nd� m�ste du hantera mycket av logiken i din kod, speciellt n�r du sparar data.
- **Entity Framework Core** �r en objekt-relationell �verbyggnad som g�r det m�jligt att arbeta med databaser. 
- Det eliminerar behovet av det mesta av data�tkomstkoden som utvecklare vanligtvis beh�ver skriva. 
- Det st�der en m�ngd olika databaser och g�r databasens typ  mer eller mindre transparent. 
Den st�der ocks� databasmigrering och versionshantering.

## Datakommunikation
Du kan anv�nda r�a kommunikationsstandarder som TCP och UDP med sockets. 
Oftare kommer du att anv�nda ett ramverk som bygger p� dessa standarder.
- **ASP.NET** �r ocks� ett ramverk f�r att bygga webb-API:er baserade p� HTTP-protokollet. ASP.NET st�der n�stan alla funktioner du vill anv�nda och �r ocks� mycket anpassningsbara.
- **gPRC** �r ett modernt, h�gpresterande, l�tt RPC-ramverk (Remote Procedure Call). gPRC �r ocks� plattformsoberoende och kompatibel med tj�nster/klienter skrivna p� alla andra spr�k p� vilken annan plattform som helst. .NET har en mycket effektiv implementering och verktyg f�r att utveckla b�de gRPC klient- och serverapplikationer, .NET gRPC har �ven en funktion som g�r gRPC API:er tillg�ngliga som webb-API, s� du beh�ver inte implementera b�da.

## Interoperabilitet
.NET �r k�nt f�r interoperabilitet:
- Anropsfunktioner i Dynamic Link Libraries (DLL) kompilerade p� ett icke .NET-spr�k.
- C++ interop f�r att kapsla in en inbyggd C++-klass och anropa denna fr�n C# eller ett annat .NET-spr�k.
- Att exponera COM-komponenter f�r .NET s� att .NET-kod kan anropa dessa det (endast Windows). Detta st�d har f�rb�ttats i .NET 8.
- St�d f�r dynamiska objekt f�r interoperabilitet med spr�k som IronPython och IronRuby.
- Blazor interoperabilitet i webbl�saren med JavaScript och bibliotek p� valfritt spr�k som kompileras till Web Assembly.

## Molnutveckling
Dagens .NET �r skapat med molnet i �tanke. Du kan enkelt distribuera appar till molnet, och .NET har st�d p� flera molnplattformar inklusive Microsoft Azure och Amazon Web Services (AWS).

Du f�r ofta en viss niv� av gratis cloud computing, vilket vanligtvis r�cker f�r applikationer med begr�nsad anv�ndning. Det �r ocks� l�tt att skala upp med m�ttliga kostnader.
Det enklaste s�ttet att distribuera till ett molnmilj� kallas mjukvara som en tj�nst, d�r hanteringen av den underliggande infrastrukturen hanteras av molnleverant�ren.

.NET har ocks� utm�rkt st�d f�r distribution till containers och f�r att distribuera containers till orkestreringstj�nster som Kubernetes.

St�det f�r *ahead of time (AOT)* kompilering st�ds f�r konsolapplikationer och WEB API. 
Inneb�r mindre exekverbara filer och har kortare uppstartstid.
Microsoft h�ller p� med att anpassa fler typer av .NET applikationer f�r *ahead of time* kompilering.

## IoT-utveckling
Du kan bygga IoT-appar med C# och .NET som k�rs p� Raspberry Pi, HummingBoard, BeagleBoard, Pine A64 och mer. Det finns tre vanliga tillv�gag�ngss�tt:
- **.NET IoT Libraries** �r l�mplig f�r datorer som kan k�ra .NET, det vill s�ga en dator med ett operativsystem som Raspberry PI.
- **.NET nanoFramework** �r en gratis plattform med �ppen k�llkod som g�r att du kan skriva C#-applikationer f�r begr�nsade inb�ddade enheter.
- **Meadow** av Wilderness Labs ger dig .NET p� inb�ddade enheter f�r industriell IoT och bygger produktionsklassade l�sningar med .NET.
## Kodhantering
Det rekommenderas starkt att anv�nda ett k�llkontrollsystem online f�r att hantera din kod och andra tillg�ngar i din applikation.

St�d f�r GitHub �r inbyggt i Visual Studio, vilket g�r GitHub till ett rekommenderat val. F�rutom kodhantering har GitHub m�nga andra anv�ndbara funktioner f�r att bygga och distribuera din app, dokumentation, hantering av problem, projektplaner etc. GitHub �r gratis att anv�nda f�r de flesta hobbyprojekt.

## Prestanda
.NET-applikationer kan ha h�g prestanda, �ven j�mf�rt med andra tekniker. 
Ett exempel p� oberoende prestandatester finns p� [TechEmpower](https://www.techempower.com/benchmarks/#section=data-r22).
Prestanda �r i fokus, och .NET har m�nga prestandaf�rb�ttringar med varje utg�va.
Du f�r prestandaf�rb�ttringar bara genom att uppdatera din app till senaste .NET-versionen.

Du m�ste fr�ga dig sj�lv: Hur kritisk �r min applikation? Att skriva din kod l�tt att f�rst� �r att f�redra. Optimering b�r endast till�mpas vid flaskhalsar och endast n�r resultatet kan m�tas.
F�r att m�ta prestandan f�r din kod och effekten p� kod�ndringar du g�r, anv�nd [BenchMark .NET](https://github.com/dotnet/BenchmarkDotNet).

## L�s mer

- **[.NET](https://dotnet.microsoft.com/)**
- **[IoT med .NET](https://dotnet.microsoft.com/en-us/apps/iot)**
- **[Visual Studio alla versioner](https://visualstudio.microsoft.com/)**
- **[Entity Framework](https://docs.microsoft.com/en-us/ef/)**
- **[Dapper](https://dapper-tutorial.net/dapper)**
- **[gPRC med .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/)**
- **[GitHub](https://github.com/)**
- **[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)**
- **[SQL Server Database projects](https://docs.microsoft.com/en-us/visualstudio/data-tools/creating-and-managing-databases-and-data-tier-applications-in-visual-studio)**
- **[BenchMark .NET](https://github.com/dotnet/BenchmarkDotNet)**