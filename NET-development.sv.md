# .NET-utveckling
*Av [Stefan Fjällemark](https://github.com/fjallemark), Tellurian Interactive AB, Sverige, april 2023*

Den här artikeln syftar till att beskriva teknikval vid utveckling av mjukvara i modernt .NET. I skrivande stund 2023 bör all ny .NET-utveckling använda .NET 7 eller senare.

## Nuvarande .NET
Nuvarande .NET är gratis och öppen källkod; rambiblioteken, exekveringsmotorerna och kompilatorerna. 
Du kan bygga alla typer av applikationer på nästan vilken plattform som helst. 
.NET körs även i webbläsaren i web assembly och på många enkelkortsdatorer. 
Detta gör .NET till en one-stop-shop där du kan återanvända mycket av dina .NET-kunskaper för alla typer av applikationer.

> ### Missuppfattningar om .NET
> När du söker på Internet och jämför .NET med andra plattformar och tekniker verkar det som att mycket av skrivningen om 
> .NET är föråldrad eller helt enkelt fel. [Denna YouTube-video av Nick Chapsas](https://youtube.com/watch?v=AFNujHJfMtU) ger några exempel.

> ### .NET Framework
> Dagens .NET är *inte* samma sak som .NET Framework, som endast är Windows och aldrig bör användas för nya applikationer. 
> Den senaste versionen av .NET Framework är 4.8, och den kommer att underhållas så länge som Windows stöds.

## Programspråk
Alla språk på .NET använder samma exekveringssystem och samma typsystem, 
så du kan vanligtvis kombinera bibliotek skrivna i vilket .NET-program som helst med varandra. 
Det finns några regler att följa för att få det att fungera.

- **C#** är det vanligaste språket som används i .NET-utveckling. 
Är ett språk i C-familjen och delar de flesta av sina egenskaper med programmeringsspråket Java.
- **F#** är ett funktionellt förstaspråk med ett pragmatiskt förhållningssätt. 
F# har en mycket mer kortfattad syntax, vilket gör att F#-program är betydligt mindre än motsvarande C#-program.
- **VB.NET** är den moderna varianten av programmeringsspråket BASIC. 
Den har nästan samma funktioner som C# och en enkel och lättförståelig syntax.

Det finns även andra språk som körs på .NET, till exempel  [Phyton](https://ironpython.net/) och [COBOL](https://portal.microfocus.com/s/article/KM000009164?language=en_US)..

## Programbibliotek
Oavsett vilket .NET-språk du använder kommer du att vara beroende av samma .NET-basbibliotek. 
Så mycket av kunskapen handlar om att få ut det mesta av den färdiga programvaran som Microsoft 
och andra biblioteksskribenter har att erbjuda:

- [**.NET Base Class Library**](https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview) (BCL) har en hel del inbyggd funktionalitet för nästan allt du vill skriva kod för.
- Bilbliotekshanteraren [**NuGet**](https://www.nuget.org/) är sättet du kan återanvända komponenter skrivna av andra eller dig själv.

## Utvecklarverktyg
Microsoft är känt för sina bra utvecklingsverktyg. 
Det här avsnittet täcker endast gratisverktyg för att utveckla .NET-applikationer.

- **Visual Studio Code** är nu en av de mest populära verktygen för att skriva programvara på nästan alla språk. 
Det är en plattformsoberoende applikation som körs på Windows, macOS och Linux. 
Anpassning görs genom att installera olika plugins för ett visst språk eller andra ändamål.
- **Visual Studio** har en community-utgåva som är gratis och mycket kapabel för utveckling av .NET-applikationer. 
Anpassning görs genom att välja att installera en eller flera utvecklingspaket. 
Det finns också många användbara gratis plugins att installera från Visual Studio Markeplace. 
Visual Studio är ett endast Windows-program, men det finns också en separat version för macOS.
- **SQL Server** har en gratis utvecklarutgåva som är en komplett version med vissa begränsningar. 
Det finns också en gratis expressutgåva som är idealisk för mindre applikationer. 
SQL Server har också en molnversion men ingen gratis nivå, den minsta SQL Server-molndatabasen kostar cirka 5 € per månad.
- **SQL Server Management Studio** är en gratis Windows-applikation för att skapa och hantera SQL Server-databas. 
Den fungerar med både lokala databaser och molndatabaser. Den kommer att installeras med SQL Server Developer Edition.
- **Visual Studio** har bra stöd för att bygga och underhålla databaser i **Microsoft SQL Server**. 
För det första har du liknande tillgång till databasen som med SQL Server Management Studio, 
och för det andra finns det en speciell databasprojekttyp, 
som du kan underhålla din SQL-kod med versionskontroll och distribuera uppdateringar och 
migrering till din befintliga databas eller skapa nya. .
- **Entity Framework Core** har också bra stöd för databasdefinitioner och inkrementella databasuppgraderingar. 
Den stöder de vanligaste databaserna.

## Användargränssnitt
Det finns många sätt att skapa applikationer med ett användargränssnitt med .NET. Huvudfaktorn är målplattformen du tänker bygga mjukvara för:

- **Endast Windows**: Du har för avsikt att utveckla ett program som endast ska köras på Windows-datorer.
- **Webb**: Applikationen ska vara tillgänglig i en webbläsare.
- **Cross-platform**: Din app bör köras på ioS, Andriod, macOs, Windows och/eller Linux.

### Användargränssnittsteknik
- **Windows Forms** är ett enkelt och snabbt sätt att skapa Windows-applikationer med enkla formulär.
- **Windows Presentation Foundations** förkortning WPF är ett mer avancerat sätt att skapa användargränssnitt endast på Windows.
- **ASP.NET** är ett ramverk för att skapa webbapplikationer. 
ASP.NET-applikationer kan vara värd och köras på alla plattformar som stöds av .NET, inklusive molnplattformar. 
Med Razor-syntaxen kan du skriva webbsidor som en blandning av HTML, CSS och C#.
- **Blazor** är ett speciellt webbramverk för ASP.NET. Du skriver komponenter med en blandning av HTML, CSS och C#. 
Du kan interagera med JavaScript och vilken kod som helst som kan kompileras till Web Assembly (C, Rust och andra). 
  Det finns många färdiga komponenter tillgängliga. Komponenterna skrivna i Blazor kan renderas i flera miljöer:
  - på en webbserver som uppdaterar webbanvändargränssnittet via websockets,
  - på en webbserver som skickar vanlig html till webbläsaren (ny i .NET 8),
  - inbäddad i vilken webbapplikation som helst med vilket ramverk som helst,
  - kör komponenten på webbsammansättning i webbläsaren, och
  - i en plattformsoberoende MAUI-applikation.
  - Med den kommande .NET 8 kommer du även att kunna blanda renderingslägen i samma applikation. Detta gör Blazor-komponenter till de mest återställbara användargränssnittskomponenterna i .NET.
- **Multiplatform Application User Interface** förkortat MAUI är den senaste teknologin som syftar till att göra det enklare att skapa appar som körs inbyggt på iOS, Android, macOs och Windows med det inbyggda användargränssnittet för var och en av dessa plattformar. MAUI är en utveckling av Xmamarin Forms. Du kan också bädda in Blazor-komponenter i en MAUI-applikation, eller skriva en MAUI-applikation enbart med Blazor-komponenter.

## Datalagring
Det finns flera typer av sätt att lagra data och komma åt den. De viktigaste typerna av databaser är:

- **Relationsdatabaser** som lagrar data i tabeller och som vanligtvis nås av SQL-frågespråket. Exempel på databaser är Microsoft SQL Server, Postgress, My SQL och SQL Lite.
- **Dokumentdatabaser** som lagrar hela strukturer av objekt i ett stycke som en hierakisk struktur. Exempel på databaser är Mongo DB och Azure Cosmos DB.
- **Molnlagring** är givetvis alla ovanstående typer av databaser men även andra alternativ som bloblagring eller nyckel/värdelagring.
- **Inbäddade databaser**, ett exempel är SQL Lite kompilerad till webassembly så att du kan köra den som en lokal lagring i din Blazor-webbapplikation.

Databaser gör din applikation mer komplex. Om kraven på datalagring och sökning är begränsad är det definitivt ett alternativ att bara lagra applikationsdata på disken i en enda fil eller flera filer.
Då kan man använda de inbyggda JSON- och XML-serialiseringsteknikerna för att skriva och läsa data till/från filer på disken. Detta är en robust teknik och anpassad för förändringar i dina datastrukturer.

### Dataåtkomstteknik
För att komma åt data i databaser krävs ett bibliotek för dataåtkomst.

- **ADO.NET** är det grundläggande biblioteket för all databasåtkomst i .NET. 
Den är ganska låg nivå men har hög prestanda, men du måste skriva mycket kod för att använda den. 
ADO.NET fungerar även för molndatabaser.
- **Dapper** är en enkel objektmappare för .NET och är praktiskt taget lika snabb som att använda en rå ADO.NET. 
Att använda Dapper minskar mängden kod att skriva, men ändå måste du hantera mycket av logiken i din kod, speciellt när du sparar data.
- **Entity Framework Core** är en objekt-relationell överbyggnad som gör det möjligt att arbeta med databaser. 
- Det eliminerar behovet av det mesta av dataåtkomstkoden som utvecklare vanligtvis behöver skriva. 
- Det stöder en mängd olika databaser och gör databasens typ  mer eller mindre transparent. 
Den stöder också databasmigrering och versionshantering.

## Datakommunikation
Du kan använda råa kommunikationsstandarder som TCP och UDP med sockets. 
Oftare kommer du att använda ett ramverk som bygger på dessa standarder.
- **ASP.NET** är också ett ramverk för att bygga webb-API:er baserade på HTTP-protokollet. ASP.NET stöder nästan alla funktioner du vill använda och är också mycket anpassningsbara.
- **gPRC** är ett modernt, högpresterande, lätt RPC-ramverk (Remote Procedure Call). gPRC är också plattformsoberoende och kompatibel med tjänster/klienter skrivna på alla andra språk på vilken annan plattform som helst. .NET har en mycket effektiv implementering och verktyg för att utveckla både gRPC klient- och serverapplikationer, .NET gRPC har även en funktion som gör gRPC API:er tillgängliga som webb-API, så du behöver inte implementera båda.

## Interoperabilitet
.NET är känt för interoperabilitet:
- Anropsfunktioner i Dynamic Link Libraries (DLL) kompilerade på ett icke .NET-språk.
- C++ interop för att kapsla in en inbyggd C++-klass och anropa denna från C# eller ett annat .NET-språk.
- Att exponera COM-komponenter för .NET så att .NET-kod kan anropa dessa det (endast Windows).
- Stöd för dynamiska objekt för interoperabilitet med språk som IronPython och IronRuby.
- Blazor interoperabilitet i webbläsaren med JavaScript och bibliotek på valfritt språk som kompileras till Web Assembly.

## Molnutveckling
Dagens .NET är skapat med molnet i åtanke. Du kan enkelt distribuera appar till molnet, och .NET har stöd på flera molnplattformar inklusive Microsoft Azure och Amazon Web Services (AWS).

Du får ofta en viss nivå av gratis cloud computing, vilket vanligtvis räcker för applikationer med begränsad användning. Det är också lätt att skala upp med måttliga kostnader.
Det enklaste sättet att distribuera till ett molnmiljö kallas mjukvara som en tjänst, där hanteringen av den underliggande infrastrukturen hanteras av molnleverantören.

.NET har också utmärkt stöd för distribution till containers och för att distribuera containers till orkestreringstjänster som Kubernetes.

## IoT-utveckling
Du kan bygga IoT-appar med C# och .NET som körs på Raspberry Pi, HummingBoard, BeagleBoard, Pine A64 och mer. Det finns tre vanliga tillvägagångssätt:
- **.NET IoT Libraries** är lämplig för datorer som kan köra .NET, det vill säga en dator med ett operativsystem som Raspberry PI.
- **.NET nanoFramework** är en gratis plattform med öppen källkod som gör att du kan skriva C#-applikationer för begränsade inbäddade enheter.
- **Meadow** av Wilderness Labs ger dig .NET på inbäddade enheter för industriell IoT och bygger produktionsklassade lösningar med .NET.
## Kodhantering
Det rekommenderas starkt att använda ett källkontrollsystem online för att hantera din kod och andra tillgångar i din applikation.

Stöd för GitHub är inbyggt i Visual Studio, vilket gör GitHub till ett rekommenderat val. Förutom kodhantering har GitHub många andra användbara funktioner för att bygga och distribuera din app, dokumentation, hantering av problem, projektplaner etc. GitHub är gratis att använda för de flesta hobbyprojekt.

## Prestanda
.NET-applikationer kan ha hög prestanda, även jämfört med andra tekniker. 
Ett exempel på oberoende prestandatester finns på [TechEmpower](https://www.techempower.com/benchmarks/#section=data-r21).
Prestanda är i fokus, och .NET har många prestandaförbättringar med varje utgåva.

Du måste fråga dig själv: Hur kritisk är min applikation? Att skriva din kod lätt att förstå är att föredra. Optimering bör endast tillämpas vid flaskhalsar och endast när resultatet kan mätas.
För att mäta prestandan för din kod och effekten på kodändringar du gör, använd [BenchMark .NET](https://github.com/dotnet/BenchmarkDotNet).

## Läs mer

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