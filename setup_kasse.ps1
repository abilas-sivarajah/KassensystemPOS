# ============================================================
#  KassensystemPOS - komplettes Datenbank-Setup in EINEM Skript
#  Legt die Datenbank 'Kasse' an und erstellt alle Tabellen.
#
#  Ausfuehren (kein Admin noetig):
#    1. Start -> "PowerShell" -> oeffnen
#    2. Diesen Befehl einfuegen:
#       powershell -ExecutionPolicy Bypass -File "C:\Users\abila\Desktop\KassensystemPOS\setup_kasse.ps1"
#
#  Vorteil: Verbindet sich direkt mit der richtigen Datenbank.
#  Kein \c-Reconnect -> Tabellen koennen NIE in der falschen DB landen.
# ============================================================

$ErrorActionPreference = "Stop"

# --- Einstellungen (bei Bedarf anpassen) ---
$PgBin    = "C:\Program Files\PostgreSQL\17\bin"
$User     = "postgres"
$Passwort = "postgres"          # muss zur App.config passen
$DbName   = "Kasse"
# -------------------------------------------

$psql = Join-Path $PgBin "psql.exe"
if (-not (Test-Path $psql)) {
    Write-Host "FEHLER: psql.exe nicht gefunden unter $psql" -ForegroundColor Red
    Write-Host "Passe oben die Variable \$PgBin an deine PostgreSQL-Version an." -ForegroundColor Yellow
    Read-Host "Enter zum Beenden"; exit 1
}
$env:PGPASSWORD = $Passwort

try {
    # --- 1. Verbindung / Passwort testen ---
    Write-Host "1) Teste Verbindung zu PostgreSQL ..." -ForegroundColor Cyan
    & $psql -U $User -d postgres -c "SELECT 1;" | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Verbindung fehlgeschlagen. Stimmt das Passwort '$Passwort' fuer Benutzer '$User'?"
    }

    # --- 2. (Aufraeumen) Falsch in 'postgres' angelegte Tabellen entfernen ---
    Write-Host "2) Raeume eventuell falsch angelegte Tabellen aus 'postgres' ..." -ForegroundColor Cyan
    & $psql -U $User -d postgres -c 'DROP TABLE IF EXISTS public."Transaktionen", public."Abrechnungen", public."Artikel", public."Kategorieliste", public."UnternnehmensDaten" CASCADE;' | Out-Null

    # --- 3. Datenbank 'Kasse' anlegen, falls noch nicht vorhanden ---
    Write-Host "3) Pruefe/erstelle Datenbank '$DbName' ..." -ForegroundColor Cyan
    $vorhanden = (& $psql -U $User -d postgres -t -A -c "SELECT 1 FROM pg_database WHERE datname='$DbName';").Trim()
    if ($vorhanden -eq "1") {
        Write-Host "   -> Datenbank '$DbName' existiert bereits, wird weiterverwendet." -ForegroundColor DarkGray
    } else {
        & $psql -U $User -d postgres -c "CREATE DATABASE ""$DbName"" WITH ENCODING='UTF8' LC_COLLATE='C' LC_CTYPE='C' TEMPLATE=template0;"
        if ($LASTEXITCODE -ne 0) { throw "CREATE DATABASE fehlgeschlagen." }
        Write-Host "   -> Datenbank '$DbName' neu erstellt." -ForegroundColor Green
    }

    # --- 4. Tabellen DIREKT in 'Kasse' anlegen (kein Reconnect!) ---
    Write-Host "4) Erstelle Tabellen in '$DbName' ..." -ForegroundColor Cyan
    $sql = @'
-- Bei Fehler sofort abbrechen
\set ON_ERROR_STOP on

CREATE TABLE IF NOT EXISTS public."Kategorieliste" (
    "Kategorie_Nr"  INTEGER         NOT NULL,
    "Kategoriename" VARCHAR(255)    NULL,
    CONSTRAINT "PK_Kategorieliste" PRIMARY KEY ("Kategorie_Nr")
);

CREATE TABLE IF NOT EXISTS public."Artikel" (
    "Artikelnr"   INTEGER         NOT NULL,
    "Artikelbez"  VARCHAR(255)    NULL,
    "Nettopreis"  NUMERIC(18, 2)  NULL,
    "Kategorie"   INTEGER         NULL,
    "Steuersatz"  INTEGER         NULL,
    CONSTRAINT "PK_Artikel" PRIMARY KEY ("Artikelnr"),
    CONSTRAINT "FK_Artikel_Kategorieliste" FOREIGN KEY ("Kategorie")
        REFERENCES public."Kategorieliste" ("Kategorie_Nr")
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public."Abrechnungen" (
    "AbrechnungsID"    SERIAL          NOT NULL,
    "SollBestand"      NUMERIC(18, 2)  NULL,
    "AbrechnungsDatum" TIMESTAMP       NULL,
    CONSTRAINT "PK_Abrechnungen" PRIMARY KEY ("AbrechnungsID")
);

CREATE TABLE IF NOT EXISTS public."Transaktionen" (
    "RechnungsID"       SERIAL          NOT NULL,
    "NettoGesamtBetrag" NUMERIC(18, 2)  NULL,
    "Steuer"            NUMERIC(18, 2)  NULL,
    "BruttoBetrag"      NUMERIC(18, 2)  NULL,
    "RechnungsDatum"    TIMESTAMP       NULL,
    "AbrechnungsID"     INTEGER         NULL,
    CONSTRAINT "PK_Transaktionen" PRIMARY KEY ("RechnungsID"),
    CONSTRAINT "FK_Transaktionen_Abrechnungen" FOREIGN KEY ("AbrechnungsID")
        REFERENCES public."Abrechnungen" ("AbrechnungsID")
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS public."UnternnehmensDaten" (
    "UnternehmensID" SERIAL          NOT NULL,
    "Name"           VARCHAR(255)    NULL,
    "Strasse"        VARCHAR(255)    NULL,
    "Hausnummer"     INTEGER         NULL,
    "PLZ"            INTEGER         NULL,
    "Ort"            VARCHAR(255)    NULL,
    "Tel"            BIGINT          NULL,
    "Steuernummer"   BIGINT          NULL,
    CONSTRAINT "PK_UnternnehmensDaten" PRIMARY KEY ("UnternehmensID")
);

-- Beispiel-Kategorien (nur falls noch nicht vorhanden)
INSERT INTO public."Kategorieliste" ("Kategorie_Nr", "Kategoriename") VALUES
    (1, 'Getraenke'),
    (2, 'Speisen'),
    (3, 'Sonstiges')
ON CONFLICT ("Kategorie_Nr") DO NOTHING;
'@
    $sql | & $psql -U $User -d $DbName -f -
    if ($LASTEXITCODE -ne 0) { throw "Tabellen konnten nicht erstellt werden." }

    # --- 5. Kontrolle ---
    Write-Host ""
    Write-Host "5) Kontrolle - Tabellen in '$DbName':" -ForegroundColor Cyan
    & $psql -U $User -d $DbName -c '\dt'

    Write-Host ""
    Write-Host "FERTIG. Datenbank '$DbName' ist eingerichtet. Du kannst die App jetzt starten." -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "FEHLER: $_" -ForegroundColor Red
}
finally {
    $env:PGPASSWORD = $null
    Read-Host "Enter zum Beenden"
}
