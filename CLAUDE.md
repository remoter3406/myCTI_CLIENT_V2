# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Überblick

myCTI ist ein Windows-Tray-Client (C# WinForms, .NET Framework 4.8, `myCTI2.csproj` → `myCTI.exe`) für einen myCTI-Telefonie-Server (CTI). Er zeigt Anruf-Popups an, wählt Nummern und stellt die Web-Oberfläche des Servers in einem eingebetteten WebView2-Browser dar. UI-Texte, Kommentare und Bezeichner sind überwiegend deutsch.

## Build

Csproj im alten Format (nicht SDK-Stil) mit `packages.config` – mit Visual Studio oder MSBuild bauen, nicht mit `dotnet build`.

MSBuild liegt nicht im PATH, sondern unter `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild myCTI_CLIENT.sln /restore /p:Configuration=Debug   # Ausgabe: bin\Debug\myCTI.exe
& $msbuild myCTI_CLIENT.sln /restore /p:Configuration=Release

# Tests (nach dem Build)
dotnet test myCTI.Tests\myCTI.Tests.csproj --no-build
dotnet test myCTI.Tests\myCTI.Tests.csproj --no-build --filter "FullyQualifiedName~LostCallTests"   # einzelne Klasse/Methode
```

Einzige NuGet-Abhängigkeit des Hauptprojekts ist Microsoft.Web.WebView2 (wird nach `packages\` wiederhergestellt; fehlt es, bricht der Build mit der Meldung „NuGet-Paket fehlt“ ab). `myCTI.Tests` ist ein SDK-Projekt (net48, MSTest) und testet die reinen Parser in `CtiProtocol` sowie Helfer aus `Common.cs`; `internal`-Member sind per `InternalsVisibleTo` sichtbar. Es gibt keinen Linter. `git` ist in dieser Umgebung nicht im PATH.

Neue `.cs`-Dateien müssen explizit in `myCTI2.csproj` eingetragen werden (Forms brauchen die `DependentUpon`-Einträge für `.designer.cs`/`.resx`).

## Architektur

**Einstiegspunkt (`Program.cs`)** – Einzelinstanz-Logik:
- Läuft bereits ein `myCTI`-Prozess, schickt die neue Instanz nur `CSTA7<arg>` per UDP an `127.0.0.1:31312` und beendet sich (so lassen externe Aufrufer/URL-Handler den laufenden Client navigieren).
- Liegt die Exe nicht in einem `\cti\`-Ordner, liest sie Serveradresse/Protokoll aus der eingebetteten `token`-Ressource (`Properties/Resources.resx`, Platzhalter `FKtokenFKstartFK###…FKtokenFKendeFK`, den der Server beim Download in die Binärdatei einsetzt), schreibt sie in die Registry, kopiert sich nach `%USERPROFILE%\Documents\CTI\myCTI.exe` und startet von dort neu (Registry-Flag `copyStart`).
- Andernfalls wird `TrayContext` gestartet.

**`TrayContext` (ApplicationContext) ist die Zentrale.** Er besitzt alle Forms und verdrahtet ihre Events:
- `CTIMain` (`CT`) – Hauptfenster mit WebView2 für Serverseiten (`Monitor.PL`, Kontakte, …), Wählfeld und Buttons. Löst Events aus (`DialNumber`, `ReleaseButton`, `ZeitplanButton`, …), die TrayContext in HTTP-Aufrufe an den Server umsetzt.
- `CTIausgabe` ×3 (Array `AU[0..2]`) – Anruf-Popups. Bis zu drei gleichzeitige Anrufe; `belegt[0..2]` ordnet eine Anruf-ID einem Popup zu (gleiche ID → gleiches Popup, sonst erstes freies). `ausgabe(text)` zerlegt den Anrufdatensatz über `CtiProtocol.ParseCall` (`<cmd><x><ziel>|<anrufer>|<for>|<id>|<bild>`, ab Index 2) und leitet ihn zusätzlich an UDP-Port 31311 weiter.
- `ShutDownDummy` (`SD`) – unsichtbare Form, die das Herunterfahren von Windows abfängt und globale Hotkeys registriert (Strg+<konfigurierte Taste> = markierten Text über die Zwischenablage wählen, Strg+Leertaste = Popups ausblenden).
- `Settings`, `InputBox` – Konfigurationsdialoge. `Settings` bekommt die `CtiServer`-Instanz von TrayContext übergeben.

**Gemeinsame Bausteine:**
- `CtiServer` – einziger HTTP-Zugang zum Server (`Get`, `GetReg`, `SetReg`), hält Server/Ssl/User/Password/DefCred und `WebOK`.
- `CtiProtocol` – reine Parser ohne UI (Anrufdatensatz, Blink-Flag, verpasste Anrufe, Wählnummer, URL-Kodierung für dial/deflect). Neue Protokoll-Logik hierher legen und in `myCTI.Tests` testen.
- `LocalReg` (in `Common.cs`) – lokale Einstellungen unter `HKCU\SOFTWARE\AB Server`.

**Eingehend: UDP-Listener auf Port 31312** (`myUdpClient`, Hintergrund-Thread). Es werden nur Pakete von localhost und von der aufgelösten Server-IP angenommen, andere werden verworfen und in `error.txt` geloggt (`setServer` bei Serverwechsel aufrufen). `udpClient_UDP_receive` setzt Teilstücke zusammen und reicht die Nachricht per `BeginInvoke` an `ReceiveUDP` im UI-Thread weiter. Nachrichten beginnen mit `CSTA`, gefolgt von einer Befehlsziffer:
- `5` – Tray-Icon blinken lassen (verpasster Anruf), gesteuert über ein Pipe-Feld
- `7` – CT zu einer URL navigieren (Präfix `2` = URL im Hintergrund abrufen und Ergebnis in die Registry unter `VB and VBA Program Settings\AB Server\General\Result` schreiben, genutzt von VBA-/Office-Integrationen)
- `8` – `Documents\CTI\myStartGPO.exe` starten
- alles andere – Anruf-Event, wird an ein Popup geleitet
- Pakete mit genau 60001 Bytes sind Teilstücke; sie werden aneinandergehängt, bis ein kürzeres Paket kommt.

Der 500-ms-Timer `myTimer` prüft etwa alle 10 s im Hintergrund per TCP-Verbindung zum Webserver-Port, ob der Server erreichbar ist (Tray-Icon wird bei Fehler rot), und meldet Änderungen des RDP-Clientnamens. Blockierende Serveraufrufe gehören nicht in den UI-Thread, sondern in den Thread-Pool, mit Rückkehr über `uiInvoker.BeginInvoke`.

**Ausgehend: HTTP zum Server** über `CtiServer.Get(path)` → `{ssl}://{server}/{path}`. Liefert `SetUDP?status=off` eine Benutzer-ID, wird mit den Windows-Standardanmeldedaten (NTLM-SSO) gearbeitet, sonst NTLM mit gespeichertem Benutzer/Passwort. Zertifikatsfehler werden immer akzeptiert (`CtiServer.AcceptAllCertificates()` einmalig beim Start, in WebView2 per `AlwaysAllow`). URL-Parameter mit `TOOL.UrlEnc` kodieren (ANSI); dial/deflect nutzen `CtiProtocol.UrlEncodeDial`. Wichtige Endpunkte: `SetUDP?status=on|off|locked|unlocked`, `cti.pl?dial=|button=|deflect=|zeitplan=`, `sendudp?cmd=…`, `action?cmd=getreg|setreg&var=…` (serverseitige Registry, in TrayContext gekapselt durch `RegLoadServer`/`RegSaveServer` mit Schlüsseln unter `CTI\userN\CTI…`), `css/<style>.wav` für Benachrichtigungstöne. Achtung: `Settings` sendet `setreg`-Werte UTF-8-kodiert, TrayContext ANSI. Das ist historisch so gewachsen und ohne Abstimmung mit dem Server nicht zu ändern.

**Lokaler Zustand** liegt in `HKCU\SOFTWARE\AB Server` (`server`, `ssl`, `user`, `password` im Klartext, Fenster-/Popup-Positionen, `DoubleClickLink`, `keycodeRelease`, …), Zugriff über `LocalReg`. Logs: `TOOL.debugx` → `trace.txt`, `TOOL.errorx` → `error.txt` neben der Exe. Beide sind threadsicher, schreiben Datum und Uhrzeit, werden beim Start gelöscht und ab 5 MB nach `*.old` verschoben. Nicht direkt `File.AppendAllText` verwenden.

## Konventionen und Stolperfallen

- `Common.cs` definiert eine Hilfsklasse `VB`, die VB-Stringfunktionen nachbildet, aber **`VB.Mid(s, start)` / `VB.Mid(s, start, len)` zählen ab 0** (dünne Wrapper um `Substring`), anders als echtes VB. `VB.Val` liefert bei Parse-Fehlern 0. Der Code wurde aus VB.NET portiert und nutzt diese Helfer durchgängig – diesen Stil beibehalten.
- Quelldateien sind UTF-8 mit BOM und CRLF; beim Schreiben per Skript beibehalten.
- Strings werden für UDP und HTTP mit `Encoding.Default` (ANSI-Codepage) konvertiert; Umlaute werden in `convertUTF2HTML` manuell URL-kodiert.
- Fehlerbehandlung: breites `try/catch`, das nach `error.txt` loggt und weiterläuft; die App darf im Tray nicht abstürzen.
- Die an den Server gemeldete Version ist der Fenstertitel von `CTIMain` (`CT.Text`).
