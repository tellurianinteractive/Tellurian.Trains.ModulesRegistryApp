## Frachtkunden und Frachten
Ein wichtiger Teil einer Fahrplansitzung sind Frachten.
Die Frachten können auf zwei Arten gesteuert werden:
- Waggons in festen Turnus, die nach einem Fahrplan verkehren.
- Durch Frachtbriefe kontrollierte Wagenladungen.

### Frachtbriefe
In der **Modulregistrierung** können Frachtbriefe auf zwei Arten erstellt werden:
- Frachtbriefe, die von Besprechung zu Besprechung wiederverwendet werden können.
- Frachtbriefe für Fracht zwischen den Güterbahnhöfen, die sich in einem bestimmten Treffen befinden.

Die **Module Registry** hat grundsätzlich die gleichen Funktionen wie die *Yellow Pages*, jedoch mit einigen Unterschieden.

### Frachtkunden
Im **Modulregister** tragen Sie Frachtkunden und Frachtströme ein, für die Waren versendet und empfangen werden.
Sie können Frachtkunden und Warenströme für Ihren eigenen Modulbahnhof erfassen,
sondern auch für *Externe Stationen*.

Eine *Externe Station* ist eine reale Station, die mit historisch korrekten Daten eingegeben werden muss
über den Bahnhof, seine Kunden und Güterströme.

### So entstehen Frachtbriefe
Im Gegensatz zu den *Gelben Seiten* können Sie im **Modulregister** Frachtbriefe nicht manuell selbst erstellen.
Stattdessen erstellt die Anwendung Frachtbriefe, indem sie den Frachtfluss der sendenden und empfangenden Frachtkunden abgleicht.
Der Abgleich erfolgt anhand der folgenden Daten:
- **Thema** für die sendende und empfangende Station muss übereinstimmen. Dies verhindert z.B. Amerikanische und europäische Themen sind gemischt.
- **Maßstab**, um die Entstehung von Frachten zwischen Modulen unterschiedlicher Größe zu vermeiden. Externe Stationen haben keinen Maßstab und werden daher unabhängig vom Maßstab angepasst.
- **Beladungsart** muss übereinstimmen. Dies wird mit der internen ID der Ladung abgeglichen und nicht mit dem Namen. Ein benutzerdefinierter Lasttypname *hat* keinen Einfluss auf die Übereinstimmung.
- **Jahre** müssen sich überschneiden: die Zeiträume für *Warenart*, *Absende- und Empfangsstation und Frachtzuführung*.
Das bedeutet, dass Frachtbriefe historisch korrekt erstellt werden.

### Frachtbriefe anpassen und drucken
Die erstellten Frachtbriefe können auf verschiedene Arten angepasst werden:
- Nummer zum Ausdrucken.
- Wenn Sie eine bestimmte Anzahl oder eine bestimmte Anzahl pro Betriebstag drucken möchten.
- Wenn Sie einen Rückfrachtbrief für ein leeres Wagen wünschen.

Für abfahrende Waggonladungen können Sie auch Leerwagenbestellungen ausdrucken, die Sie am Schattenbahnhof aufgeben.