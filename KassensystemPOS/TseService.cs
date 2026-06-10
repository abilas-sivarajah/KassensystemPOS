using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace KassensystemPOS
{
    /// <summary>
    /// Schnittstelle für eine Technische Sicherheitseinrichtung (TSE).
    /// Hier würde später eine zertifizierte Cloud-TSE (z. B. fiskaly) angebunden.
    /// Die App ruft nur diese Schnittstelle auf – die konkrete Umsetzung ist
    /// austauschbar, ohne den Verkaufsablauf zu ändern.
    /// </summary>
    public interface ITseService
    {
        TseSignatur Signiere(string vorgangsDaten);
    }

    /// <summary>Das Ergebnis einer TSE-Signatur (kommt auf den Bon, real auch als QR-Code).</summary>
    public class TseSignatur
    {
        public long     TransaktionsNummer { get; set; }
        public long     SignaturZaehler    { get; set; }
        public DateTime Zeitstempel        { get; set; }
        public string   Signatur           { get; set; }
    }

    /// <summary>
    /// MOCK-TSE – erzeugt NUR Platzhalter-Signaturen zur Demonstration der Architektur.
    /// NICHT zertifiziert, NICHT für den Echtbetrieb geeignet. Wird später durch eine
    /// echte Cloud-TSE ersetzt. Zeigt aber das Prinzip: fortlaufender Signaturzähler +
    /// kryptografische Signatur über die Vorgangsdaten.
    /// </summary>
    public class MockTseService : ITseService
    {
        private static long _zaehler = 0;

        public TseSignatur Signiere(string vorgangsDaten)
        {
            long zaehler = Interlocked.Increment(ref _zaehler);
            DateTime jetzt = DateTime.Now;
            string basis = $"{vorgangsDaten}|{zaehler}|{jetzt:o}";

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(basis));
                return new TseSignatur
                {
                    TransaktionsNummer = zaehler,
                    SignaturZaehler    = zaehler,
                    Zeitstempel        = jetzt,
                    Signatur           = Convert.ToBase64String(hash)
                };
            }
        }
    }
}
