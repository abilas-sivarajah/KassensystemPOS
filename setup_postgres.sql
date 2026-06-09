-- PostgreSQL Setup-Skript fuer KassensystemPOS
-- Ausfuehren als: psql -U postgres -f setup_postgres.sql

-- Datenbank erstellen
CREATE DATABASE "Kasse"
    WITH ENCODING = 'UTF8'
    LC_COLLATE = 'German_Germany.1252'
    LC_CTYPE = 'German_Germany.1252'
    TEMPLATE = template0;

-- Mit der Datenbank verbinden
\c Kasse

-- ============================================================
-- Tabelle: Kategorieliste
-- ============================================================
CREATE TABLE public."Kategorieliste" (
    "Kategorie_Nr"  INTEGER         NOT NULL,
    "Kategoriename" VARCHAR(255)    NULL,
    CONSTRAINT "PK_Kategorieliste" PRIMARY KEY ("Kategorie_Nr")
);

-- ============================================================
-- Tabelle: Artikel
-- ============================================================
CREATE TABLE public."Artikel" (
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

-- ============================================================
-- Tabelle: Abrechnungen
-- ============================================================
CREATE TABLE public."Abrechnungen" (
    "AbrechnungsID"   SERIAL          NOT NULL,
    "SollBestand"     NUMERIC(18, 2)  NULL,
    "AbrechnungsDatum" TIMESTAMP      NULL,
    CONSTRAINT "PK_Abrechnungen" PRIMARY KEY ("AbrechnungsID")
);

-- ============================================================
-- Tabelle: Transaktionen
-- ============================================================
CREATE TABLE public."Transaktionen" (
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

-- ============================================================
-- Tabelle: UnternnehmensDaten
-- ============================================================
CREATE TABLE public."UnternnehmensDaten" (
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

-- ============================================================
-- Beispiel-Kategorien einfuegen (optional)
-- ============================================================
INSERT INTO public."Kategorieliste" ("Kategorie_Nr", "Kategoriename") VALUES
(1, 'Getraenke'),
(2, 'Speisen'),
(3, 'Sonstiges');
