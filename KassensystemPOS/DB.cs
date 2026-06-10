using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;

namespace KassensystemPOS
{
    public static class DB
    {
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["KasseContext"].ConnectionString;

        // ── Kategorieliste ──────────────────────────────────────────
        public static async Task<List<Kategorieliste>> GetAlleKategorienAsync()
        {
            var list = new List<Kategorieliste>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Kategorie_Nr\", \"Kategoriename\" FROM public.\"Kategorieliste\"", conn))
                using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                    while (await r.ReadAsync())
                        list.Add(new Kategorieliste
                        {
                            Kategorie_Nr = r.GetInt32(0),
                            Kategoriename = r.IsDBNull(1) ? null : r.GetString(1)
                        });
            }
            return list;
        }

        public static async Task<Kategorieliste> GetKategorieByNameAsync(string name)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Kategorie_Nr\", \"Kategoriename\" FROM public.\"Kategorieliste\" WHERE \"Kategoriename\" = @n", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                        if (await r.ReadAsync())
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
        public static async Task<List<Artikel>> GetAlleArtikelAsync()
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\"", conn))
                using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                    while (await r.ReadAsync())
                        list.Add(LeseArtikel(r));
            }
            return list;
        }

        public static async Task<List<Artikel>> GetArtikelByKategorieAsync(int kategorieNr)
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Kategorie\" = @k", conn))
                {
                    cmd.Parameters.AddWithValue("k", kategorieNr);
                    using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                        while (await r.ReadAsync())
                            list.Add(LeseArtikel(r));
                }
            }
            return list;
        }

        public static async Task<Artikel> GetArtikelByNummerAsync(int nr)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Artikelnr\" = @nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", nr);
                    using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                        if (await r.ReadAsync()) return LeseArtikel(r);
                }
            }
            return null;
        }

        public static async Task<List<Artikel>> GetArtikelByNameAsync(string name)
        {
            var list = new List<Artikel>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\" FROM public.\"Artikel\" WHERE \"Artikelbez\" = @n", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                        while (await r.ReadAsync())
                            list.Add(LeseArtikel(r));
                }
            }
            return list;
        }

        public static async Task ArtikelHinzufuegenAsync(Artikel a)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Artikel\" (\"Artikelnr\", \"Artikelbez\", \"Nettopreis\", \"Kategorie\", \"Steuersatz\") VALUES (@nr, @bez, @preis, @kat, @steuer)", conn))
                {
                    cmd.Parameters.AddWithValue("nr", a.Artikelnr);
                    cmd.Parameters.AddWithValue("bez", (object)a.Artikelbez ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("preis", (object)a.Nettopreis ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("kat", (object)a.Kategorie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer", (object)a.Steuersatz ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task ArtikelAktualisierenAsync(Artikel a)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE public.\"Artikel\" SET \"Artikelbez\"=@bez, \"Nettopreis\"=@preis, \"Kategorie\"=@kat, \"Steuersatz\"=@steuer WHERE \"Artikelnr\"=@nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", a.Artikelnr);
                    cmd.Parameters.AddWithValue("bez", (object)a.Artikelbez ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("preis", (object)a.Nettopreis ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("kat", (object)a.Kategorie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer", (object)a.Steuersatz ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task ArtikelLoeschenAsync(int artikelnr)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "DELETE FROM public.\"Artikel\" WHERE \"Artikelnr\" = @nr", conn))
                {
                    cmd.Parameters.AddWithValue("nr", artikelnr);
                    await cmd.ExecuteNonQueryAsync();
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
        public static async Task<List<Transaktionen>> GetAlleTransaktionenAsync()
        {
            var list = new List<Transaktionen>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"RechnungsID\", \"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"AbrechnungsID\", \"StornoVon\", \"Zahlart\" FROM public.\"Transaktionen\"", conn))
                using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                    while (await r.ReadAsync())
                        list.Add(LeseTransaktion(r));
            }
            return list;
        }

        public static async Task<List<Transaktionen>> GetOffeneTransaktionenAsync()
        {
            var list = new List<Transaktionen>();
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"RechnungsID\", \"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"AbrechnungsID\", \"StornoVon\", \"Zahlart\" FROM public.\"Transaktionen\" WHERE \"AbrechnungsID\" IS NULL", conn))
                using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                    while (await r.ReadAsync())
                        list.Add(LeseTransaktion(r));
            }
            return list;
        }

        public static async Task<int> TransaktionHinzufuegenAsync(Transaktionen t)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Transaktionen\" (\"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"Zahlart\") VALUES (@netto, @steuer, @brutto, @datum, @zahlart) RETURNING \"RechnungsID\"", conn))
                {
                    cmd.Parameters.AddWithValue("netto",   (object)t.NettoGesamtBetrag ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("steuer",  (object)t.Steuer            ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("brutto",  (object)t.BruttoBetrag      ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("datum",   (object)t.RechnungsDatum    ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("zahlart", (object)t.Zahlart           ?? DBNull.Value);
                    return (int)(await cmd.ExecuteScalarAsync());
                }
            }
        }

        /// <summary>
        /// Storniert eine Transaktion durch eine GEGENBUCHUNG mit negativen Betraegen.
        /// Die Originalbuchung bleibt unveraendert erhalten – Pflicht fuer ein
        /// ordnungsgemaesses Kassensystem (Unveraenderbarkeit/GoBD).
        /// </summary>
        public static async Task<int> StorniereTransaktionAsync(Transaktionen original)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO public.\"Transaktionen\" (\"NettoGesamtBetrag\", \"Steuer\", \"BruttoBetrag\", \"RechnungsDatum\", \"StornoVon\", \"Zahlart\") VALUES (@netto, @steuer, @brutto, @datum, @storno, @zahlart) RETURNING \"RechnungsID\"", conn))
                {
                    cmd.Parameters.AddWithValue("netto",   -(original.NettoGesamtBetrag ?? 0m));
                    cmd.Parameters.AddWithValue("steuer",  -(original.Steuer            ?? 0m));
                    cmd.Parameters.AddWithValue("brutto",  -(original.BruttoBetrag      ?? 0m));
                    cmd.Parameters.AddWithValue("datum",   DateTime.Now);
                    cmd.Parameters.AddWithValue("storno",  original.RechnungsID);
                    cmd.Parameters.AddWithValue("zahlart", (object)original.Zahlart ?? DBNull.Value);
                    return (int)(await cmd.ExecuteScalarAsync());
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
            AbrechnungsID     = r.IsDBNull(5) ? (int?)null      : r.GetInt32(5),
            StornoVon         = r.IsDBNull(6) ? (int?)null      : r.GetInt32(6),
            Zahlart           = r.IsDBNull(7) ? null            : r.GetString(7)
        };

        // ── Abrechnungen ─────────────────────────────────────────────

        /// <summary>
        /// Legt eine Abrechnung an und ordnet ihr alle angegebenen Rechnungen zu –
        /// alles in EINER Transaktion. Bricht etwas ab, wird nichts gespeichert.
        /// </summary>
        public static async Task<int> AbrechnungDurchfuehrenAsync(decimal sollBestand, DateTime datum, List<int> rechnungsIDs)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    int abrID;
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO public.\"Abrechnungen\" (\"SollBestand\", \"AbrechnungsDatum\") VALUES (@soll, @datum) RETURNING \"AbrechnungsID\"", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("soll",  sollBestand);
                        cmd.Parameters.AddWithValue("datum", datum);
                        abrID = (int)(await cmd.ExecuteScalarAsync());
                    }

                    foreach (var id in rechnungsIDs)
                        using (var cmd = new NpgsqlCommand(
                            "UPDATE public.\"Transaktionen\" SET \"AbrechnungsID\"=@abr WHERE \"RechnungsID\"=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("abr", abrID);
                            cmd.Parameters.AddWithValue("id",  id);
                            await cmd.ExecuteNonQueryAsync();
                        }

                    tx.Commit();
                    return abrID;
                }
            }
        }

        // ── UnternnehmensDaten ───────────────────────────────────────
        public static async Task<UnternnehmensDaten> GetUnternehmensDatenAsync()
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand(
                    "SELECT \"UnternehmensID\", \"Name\", \"Strasse\", \"Hausnummer\", \"PLZ\", \"Ort\", \"Tel\", \"Steuernummer\" FROM public.\"UnternnehmensDaten\" LIMIT 1", conn))
                using (var r = (NpgsqlDataReader)(await cmd.ExecuteReaderAsync()))
                    if (await r.ReadAsync())
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

        public static async Task UnternehmensDatenHinzufuegenAsync(UnternnehmensDaten u)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
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
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task UnternehmensDatenAktualisierenAsync(UnternnehmensDaten u)
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
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
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task AlleUnternehmensDatenLoeschenAsync()
        {
            using (var conn = new NpgsqlConnection(ConnStr))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM public.\"UnternnehmensDaten\"", conn))
                    await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
