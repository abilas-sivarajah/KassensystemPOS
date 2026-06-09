using System;
using System.Collections.Generic;
using System.Configuration;
using Npgsql;

namespace KassensystemPOS
{
    public static class DB
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["KasseContext"].ConnectionString;

        // ── Kategorieliste ──────────────────────────────────────────
        public static List<Kategorieliste> GetAlleKategorien()
        {
            var list = new List<Kategorieliste>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Kategorie_Nr\", \"Kategoriename\" FROM public.\"Kategorieliste\"", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Kategorieliste
                        {
                            Kategorie_Nr = r.GetInt32(0),
                            Kategoriename = r.IsDBNull(1) ? null : r.GetString(1)
                        });
            }
            return list;
        }

        public static Kategorieliste GetKategorieByName(string name)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Kategorie_Nr\", \"Kategoriename\" FROM public.\"Kategorieliste\" WHERE \"Kategoriename\" = @n", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read())
                            return new Kategorieliste
                            {
                                Kategorie_Nr = r.GetInt32(0),
                                Kategoriename = r.IsDBNull(1) ? null : r.GetString(1)
                            };
                }
            }
            return null;
        }

        // ── Artikel ─────────────────────────────────────────────────
        public static List<Artikel> GetAlleArtikel()
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\"", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(LeseArtikel(r));
            }
            return list;
        }

        public static List<Artikel> GetArtikelByKategorie(int kategorieNr)
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Kategorie\" = @k", conn))
                {
                    cmd.Parameters.AddWithValue("k", kategorieNr);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(LeseArtikel(r));
                }
            }
            return list;
        }

        public static Artikel GetArtikelByNummer(int nr)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Artikelnr\" = @nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", nr);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) return LeseArtikel(r);
                }
            }
            return null;
        }

        public static List<Artikel> GetArtikelByName(string name)
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Artikelbez\" = @n", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(LeseArtikel(r));
                }
            }
            return list;
        }

        public static void ArtikelHinzufuegen(Artikel a)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Artikel\" (\"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\") VALUES (@nr, @bez, @preis, @kat, @steuer)", conn))
                {
                    cmd.Parameters.AddWithValue("nr", a.Artikelnr);
                    cmd.Parameters.AddWithValue("bez", (object)a.Artikelbez ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("preis", (object)a.Nettopreis ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("kat", (object)a.Kategorie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer", (object)a.Steuersatz ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ArtikelAktualisieren(Artikel a)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE public.\"Artikel\" SET \"Artikelbez\"=@bez, \"Nettopreis\"=@preis, \"Kategorie\"=@kat, \"Steuersatz\"=@steuer WHERE \"Artikelnr\"=@nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", a.Artikelnr);
                    cmd.Parameters.AddWithValue("bez", (object)a.Artikelbez ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("preis", (object)a.Nettopreis ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("kat", (object)a.Kategorie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer", (object)a.Steuersatz ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ArtikelLoeschen(int artikelnr)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM public.\"Artikel\" WHERE \"Artikelnr\" = @nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", artikelnr);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static Artikel LeseArtikel(NpgsqlDataReader r) => new Artikel
        {
            Artikelnr   = r.GetInt32(0),
            Artikelbez  = r.IsDBNull(1) ? null : r.GetString(1),
            Nettopreis  = r.IsDBNull(2) ? (decimal?)null : r.GetDecimal(2),
            Kategorie   = r.IsDBNull(3) ? (int?)null    : r.GetInt32(3),
            Steuersatz  = r.IsDBNull(4) ? (int?)null    : r.GetInt32(4)
        };

        // ── Transaktionen ────────────────────────────────────────────
        public static List<Transaktionen> GetAlleTransaktionen()
        {
            var list = new List<Transaktionen>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"RechnungsID\", \"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"AbrechnungsID\" FROM public.\"Transaktionen\"", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(LeseTransaktion(r));
            }
            return list;
        }

        public static List<Transaktionen> GetOffeneTransaktionen()
        {
            var list = new List<Transaktionen>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"RechnungsID\", \"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"AbrechnungsID\" FROM public.\"Transaktionen\" WHERE \"AbrechnungsID\" IS NULL", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(LeseTransaktion(r));
            }
            return list;
        }

        public static int TransaktionHinzufuegen(Transaktionen t)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Transaktionen\" (\"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\") VALUES (@netto, @steuer, @brutto, @datum) RETURNING \"RechnungsID\"", conn))
                {
                    cmd.Parameters.AddWithValue("netto",  (object)t.NettoGesamtBetrag ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer", (object)t.Steuer            ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("brutto", (object)t.BruttoBetrag      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("datum",  (object)t.RechnungsDatum    ?? DBNull.Value);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public static void TransaktionenAbrechnen(int abrechnungsID, List<int> rechnungsIDs)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                foreach (var id in rechnungsIDs)
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE public.\"Transaktionen\" SET \"AbrechnungsID\"=@abr WHERE \"RechnungsID\"=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("abr", abrechnungsID);
                        cmd.Parameters.AddWithValue("id",  id);
                        cmd.ExecuteNonQuery();
                    }
            }
        }

        private static Transaktionen LeseTransaktion(NpgsqlDataReader r) => new Transaktionen
        {
            RechnungsID       = r.GetInt32(0),
            NettoGesamtBetrag = r.IsDBNull(1) ? (decimal?)null  : r.GetDecimal(1),
            Steuer            = r.IsDBNull(2) ? (decimal?)null  : r.GetDecimal(2),
            BruttoBetrag      = r.IsDBNull(3) ? (decimal?)null  : r.GetDecimal(3),
            RechnungsDatum    = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4),
            AbrechnungsID     = r.IsDBNull(5) ? (int?)null      : r.GetInt32(5)
        };

        // ── Abrechnungen ─────────────────────────────────────────────
        public static int AbrechnungHinzufuegen(Abrechnungen a)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Abrechnungen\" (\"SollBestand\", \"AbrechnungsDatum\") VALUES (@soll, @datum) RETURNING \"AbrechnungsID\"", conn))
                {
                    cmd.Parameters.AddWithValue("soll",  (object)a.SollBestand      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("datum", (object)a.AbrechnungsDatum ?? DBNull.Value);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        // ── UnternnehmensDaten ───────────────────────────────────────
        public static UnternnehmensDaten GetUnternehmensDaten()
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"UnternehmensID\", \"Name\", \"Strasse\", \"Hausnummer\", \"PLZ\", \"Ort\", \"Tel\", \"Steuernummer\" FROM public.\"UnternnehmensDaten\" LIMIT 1", conn))
                using (var r = cmd.ExecuteReader())
                    if (r.Read())
                        return new UnternnehmensDaten
                        {
                            UnternehmensID = r.GetInt32(0),
                            Name           = r.IsDBNull(1) ? null       : r.GetString(1),
                            Strasse        = r.IsDBNull(2) ? null       : r.GetString(2),
                            Hausnummer     = r.IsDBNull(3) ? (int?)null  : r.GetInt32(3),
                            PLZ            = r.IsDBNull(4) ? (int?)null  : r.GetInt32(4),
                            Ort            = r.IsDBNull(5) ? null       : r.GetString(5),
                            Tel            = r.IsDBNull(6) ? (long?)null : r.GetInt64(6),
                            Steuernummer   = r.IsDBNull(7) ? (long?)null : r.GetInt64(7)
                        };
            }
            return null;
        }

        public static int GetUnternehmensDatenAnzahl()
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM public.\"UnternnehmensDaten\"", conn))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void UnternehmensDatenHinzufuegen(UnternnehmensDaten u)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"UnternnehmensDaten\" (\"Name\",\"Strasse\",\"Hausnummer\",\"PLZ\",\"Ort\",\"Tel\",\"Steuernummer\") VALUES (@name,@str,@hnr,@plz,@ort,@tel,@steuernr)", conn))
                {
                    cmd.Parameters.AddWithValue("name",     (object)u.Name         ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("str",      (object)u.Strasse      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("hnr",      (object)u.Hausnummer   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("plz",      (object)u.PLZ          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ort",      (object)u.Ort          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tel",      (object)u.Tel          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuernr", (object)u.Steuernummer ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UnternehmensDatenAktualisieren(UnternnehmensDaten u)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE public.\"UnternnehmensDaten\" SET \"Name\"=@name,\"Strasse\"=@str,\"Hausnummer\"=@hnr,\"PLZ\"=@plz,\"Ort\"=@ort,\"Tel\"=@tel,\"Steuernummer\"=@steuernr WHERE \"UnternehmensID\"=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id",       u.UnternehmensID);
                    cmd.Parameters.AddWithValue("name",     (object)u.Name         ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("str",      (object)u.Strasse      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("hnr",      (object)u.Hausnummer   ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("plz",      (object)u.PLZ          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("ort",      (object)u.Ort          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tel",      (object)u.Tel          ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuernr", (object)u.Steuernummer ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AlleUnternehmensDatenLoeschen()
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM public.\"UnternnehmensDaten\"", conn))
                    cmd.ExecuteNonQuery();
            }
        }
    }
}
