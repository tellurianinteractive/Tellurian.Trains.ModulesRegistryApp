## Bereiten Sie Ihre Lok für das Modultreffen vor
Hier sind einige Richtlinien zur Vorbereitung Ihrer Lokomotiven, bevor Sie sie auf die Strecke bringen
bei Modultreffen.

### Vorbereitung vor dem Treffen
Dies sind die Dinge, die Sie zu Hause oder im Club erledigen können, bevor Sie zum Treff reisen.
 
#### Vorbereitung der Decodereinstellungen
Wenn Sie die Fähigkeit haben oder jemanden haben, der Ihnen helfen kann, gehen Sie wie folgt vor, um die Einstellungen Ihres Lok-DCC-Decoders anzupassen:
- CV 29 lesen. Bits sind von 0 bis 7 nummeriert.
  - Wenn Bit 2 gesetzt ist, zeigt dies den Analogbetrieb an. Dies muss ausgeschaltet werden, indem dieses Bit auf Null gesetzt wird. Dadurch wird verhindert, dass die Lok bei Ausfall eines Boosters auf Hochtouren rast.
  - Wenn Bit 3 gesetzt ist, zeigt dies an, dass RailCom eingeschaltet ist. Dies muss ausgeschaltet werden, indem dieses Bit auf Null gesetzt wird. Das macht DCC etwas effizienter.
  - Empfohlene Werte für CV 29 sind:
	- 0 ist eine Lok mit Kurzadresse 1-99 und 14 Fahrstufen.
	- 2 ist eine Lok mit Kurzadresse 1-99 und 28/126 Fahrstufen.
	- 32 ist eine Lok mit langer Adresse 128-9999 und 14 Fahrstufen.
	- 34 ist eine Lok mit langer Adresse 128-9999 und 28/126 Fahrstufen.
	- Für Lok in *Rückwärts*-Richtung addieren Sie 1 zu den obigen CV29-Werten.
- Wenn CV 19 > 0 ist, ist dies die Adresse, auf die die Lok antwortet. Es wird empfohlen, CV 19=0 zu setzen, um diese Adresse zu deaktivieren.
- Lokadresse Nummer 3 ist verboten. Dies ist die häufigste Standardadresse neuer Decoder.
- Lassen Sie Ihre Lok sanft starten und stoppen, idealerweise mit einer Geschwindigkeit, die proportional dazu ist, wie viel Sie den FRED-Knopf drehen. Mit einer wegspringenden Lok macht das Fahren keinen Spaß.
  - CV 3 steuert die Beschleunigung und Sie können jeden Wert verwenden, aber nicht zu lange.
  - CV 4 steuert die Verzögerung und Sie sollten einen kleinen Wert verwenden, damit die Lok auf einer kurzen Strecke angehalten werden kann.
- Höchstgeschwindigkeit der Lok sollte „sicher“ sein. Stellen Sie dazu den Wert CV 5 ein.

#### Vorbereitung der Lokmaschinerie
- Reinigen Sie die Räder und Stromkontaktmesser für einen optimalen Stromverbrauch.
- Wenn Ihre Lok Rutschschutzgummiringe hat, überprüfen Sie, ob diese in Ordnung sind. Lokomotiven mit Rädern für Rutschschutz-Gummiringe werden ohne diese Ringe nicht gut fahren.
- Testen Sie Ihre Lokomotiven vor dem Treffen, indem Sie sie auf einem Gleis fahren, nicht nur auf einem Prüfstand.
- Wenn Ihre Lok während des Treffens ausfällt, gibt es hoffentlich jemanden, der eine Ersatzlok zur Verfügung stellen kann. Bringen Sie also ein paar zusätzliche Loks mit.

#### Vorbereitung von Wendezug-Personenzügen
- Es wird empfohlen, dass die Lok und der Steuerwagen am anderen Ende synchronisierte Lichter haben, die sich mit der Zugrichtung ändern.
Das heißt, sie müssen die gleiche DCC-Adresse und die gleichen Scheinwerfer- und Hintergrundbeleuchtungsfunktionen haben. Diese Einstellungen werden in den Decodereinstellungen geändert.

#### Zubereitung von FRED
Der FRED-Regel ist der einzige, der bei FREMO-Treffen verwendet wird.
- Stellen Sie sicher, dass Ihr FRED mit Betreiber, Klasse und Inventarnummer gekennzeichnet ist, z. B. **DB MZ 1456**. Dies hilft Menschen, den richtigen FRED zu finden.
- Es wird auch empfohlen, wesentliche Lokfunktionen auf dem FRED-Aufkleber anzubringen. Mindestens Lichtfunktionen, Hupe und Motor sollten dokumentiert werden.
- Beachten Sie, dass Funktionen über F8 aufgrund von Einschränkungen in der verwendeten digitalen Zentrale manchmal bei FREMO-Meetings nicht funktionieren. Stellen Sie also alle wesentlichen Funktionen auf F1-F8 zur Verfügung.
- Auch wenn Sie keine FRED zu Ihren Loks haben, bereiten Sie einen FRED-Aufkleber vor.

#### Vorbereitung der Lokkarte
- Alle Loks, die planmäßige Züge fahren, müssen normalerweise eine Lokkarte haben.
- Eine Lokkarte muss eine transparente Tasche in Kreditkartengröße haben, wo der Lokfahrplan abgelegt wird.

### Bei der Versammlung
Sie **können** nicht einfach eine Lok mit einer nicht reservierten Adresse auf das Gleis stellen.
Bei Modultreffen müssen Lokadressen abgestimmt werden. Dazu gibt es im Wesentlichen zwei Möglichkeiten:
- **Reservierte FREMO-Lokadressen verwenden:** Sie haben von FREMO einen Satz Lokadressen erhalten, oder Sie müssen sich eine reservierte Adresse von einem leihen, der hat, oder
- **Adressreservierung pro Treff verwenden:** Reservieren Sie Ihre bestehende Lokadresse im Voraus oder am Veranstaltungsort, bevor Sie sie auf die Strecke stellen.
Wenn Ihre Adresse bereits anderweitig belegt ist, müssen Sie eine andere freie Adresse reservieren und die Adresse Ihrer Lok auf diese ändern.
