using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            // Benutzername eingeben oder neu erstellen
            Console.Write("Bitte geben Sie Ihren Benutzernamen ein (falls vorhanden). Andernfalls drücken Sie Enter: ");
            string benutzername = Console.ReadLine().ToLower();
            string dateiPfad = $"{benutzername}-gewichtsdaten.csv";

            double groesse = 0;
            DateTime geburtsdatum = DateTime.MinValue;
            string geschlecht = "";
            double palWert = 1.4; // Standard-PAL-Wert
            bool neuerBenutzer = false;
            string vorname = "";

            if (string.IsNullOrWhiteSpace(benutzername))
            {
                Console.WriteLine("Kein Benutzername gefunden.");
                Console.Write("Bitte geben Sie Ihren Vornamen ein: ");
                vorname = Console.ReadLine();

                Console.Write("Bitte erstellen Sie einen neuen Benutzernamen: ");
                benutzername = Console.ReadLine().ToLower();

                if (string.IsNullOrWhiteSpace(vorname) || string.IsNullOrWhiteSpace(benutzername))
                {
                    Console.WriteLine("Ungültiger Vorname oder Benutzername eingegeben. Programm wird beendet.");
                    Console.WriteLine("Drücken Sie die Eingabetaste, um das Programm zu beenden.");
                    Console.ReadLine();
                    return;
                }

                dateiPfad = $"{benutzername}-gewichtsdaten.csv";
                neuerBenutzer = true;

                // Abfrage der notwendigen Daten für neuen Benutzer
                Console.Write("Bitte geben Sie Ihre Größe in cm ein: ");
                groesse = Convert.ToDouble(Console.ReadLine());

                Console.Write("Bitte geben Sie Ihr Geburtsdatum ein (TT.MM.JJJJ): ");
                geburtsdatum = DateTime.ParseExact(Console.ReadLine(), "dd.MM.yyyy", null);

                Console.Write("Bitte geben Sie Ihr Geschlecht (m/w) ein: ");
                geschlecht = Console.ReadLine().ToLower();
            }

            if (!File.Exists(dateiPfad))
            {
                File.WriteAllText(dateiPfad, "Benutzername,Vorname,Datum,Geburtsdatum,Gewicht (kg),Größe (cm),Alter,Geschlecht,BMI,Grundumsatz,Täglicher Kalorienverbrauch,Zielkalorien,PAL-Wert\n");
            }
            else if (!neuerBenutzer)
            {
                // Lesen der vorhandenen Daten
                List<string> zeilen = File.ReadAllLines(dateiPfad).ToList();
                bool benutzerGefunden = false;

                for (int i = 1; i < zeilen.Count; i++)
                {
                    string[] spalten = zeilen[i].Split(',');

                    if (spalten.Length < 13)
                    {
                        Console.WriteLine($"Warnung: Zeile {i + 1} in der CSV-Datei ist unvollständig und wird übersprungen.");
                        continue;
                    }

                    if (spalten[0] == benutzername)
                    {
                        // Benutzer gefunden, Daten laden
                        vorname = spalten[1];
                        groesse = Convert.ToDouble(spalten[5]);
                        geburtsdatum = DateTime.ParseExact(spalten[3], "dd.MM.yyyy", null);
                        geschlecht = spalten[7];
                        palWert = Convert.ToDouble(spalten[12]);
                        benutzerGefunden = true;
                        break;
                    }
                }

                if (!benutzerGefunden)
                {
                    Console.WriteLine("Benutzername nicht gefunden. Programm wird beendet.");
                    Console.WriteLine("Drücken Sie die Eingabetaste, um das Programm zu beenden.");
                    Console.ReadLine();
                    return;
                }
            }

            // Debug-Ausgabe zur Überprüfung der geladenen Daten
            Console.WriteLine($"Benutzername: {benutzername}");
            Console.WriteLine($"Vorname: {vorname}");
            Console.WriteLine($"Größe: {groesse} cm");
            Console.WriteLine($"Geburtsdatum: {geburtsdatum:dd.MM.yyyy}");
            Console.WriteLine($"Geschlecht: {geschlecht}");
            Console.WriteLine($"PAL-Wert: {palWert}");

            // Eingabe des Gewichts
            Console.Write("Bitte geben Sie Ihr Gewicht in kg ein: ");
            double gewicht = Convert.ToDouble(Console.ReadLine());

            // Option zum Ändern des PAL-Werts
            Console.Write("Möchten Sie Ihren PAL-Wert ändern? (j/n): ");
            string palAntwort = Console.ReadLine().ToLower();

            if (palAntwort == "j")
            {
                Console.WriteLine("Bitte wählen Sie Ihren PAL-Wert aus der folgenden Liste:");
                Console.WriteLine("1.2 - Sitzend oder liegend (keine oder kaum Bewegung)");
                Console.WriteLine("1.4 - 1.5 - Geringe Aktivität (z.B. überwiegend sitzende Tätigkeit)");
                Console.WriteLine("1.6 - 1.7 - Mäßige Aktivität (z.B. gehende oder stehende Tätigkeit)");
                Console.WriteLine("1.8 - 1.9 - Aktive Tätigkeit (z.B. körperlich anstrengende Arbeit)");
                Console.WriteLine("2.0 - 2.5 - Sehr hohe Aktivität (z.B. schwere körperliche Arbeit)");
                Console.WriteLine();
                Console.Write("Bitte geben Sie Ihren PAL-Wert ein: ");
                palWert = Convert.ToDouble(Console.ReadLine());
            }

            // Berechnung des Alters
            int alter = DateTime.Now.Year - geburtsdatum.Year;
            if (DateTime.Now.DayOfYear < geburtsdatum.DayOfYear)
                alter--;

            // Berechnung des BMI
            double groesseInMetern = groesse / 100;
            double bmi = gewicht / (groesseInMetern * groesseInMetern);
            Console.WriteLine();
            Console.WriteLine($"Ihr Body Mass Index (BMI) ist: {bmi:F2}");
            Console.WriteLine();

            // Bewertung des BMI
            if (bmi < 18.5)
            {
                Console.WriteLine("Sie haben Untergewicht.");
            }
            else if (bmi >= 18.5 && bmi < 24.9)
            {
                Console.WriteLine("Sie haben Normalgewicht.");
            }
            else if (bmi >= 25 && bmi < 29.9)
            {
                Console.WriteLine("Sie haben Übergewicht.");
            }
            else
            {
                Console.WriteLine("Sie haben Adipositas.");
            }

            // Hinweis für Personen mit hoher Muskelmasse
            Console.WriteLine("Hinweis: Wenn Sie viel Muskelmasse haben, wie z.B. bei Kraftsportlern, kann der BMI aufgrund des höheren Muskelgewichts abweichen und weniger aussagekräftig sein.");

            // Berechnung des Grundumsatzes (Basal Metabolic Rate - BMR)
            double grundumsatz;
            if (geschlecht == "m")
            {
                grundumsatz = 66.47 + (13.75 * gewicht) + (5.003 * groesse) - (6.755 * alter);
            }
            else if (geschlecht == "w")
            {
                grundumsatz = 655.1 + (9.563 * gewicht) + (1.850 * groesse) - (4.676 * alter);
            }
            else
            {
                Console.WriteLine("Ungültiges Geschlecht eingegeben.");
                Console.WriteLine("Drücken Sie die Eingabetaste, um das Programm zu beenden.");
                Console.ReadLine();
                return;
            }
            Console.WriteLine($"Ihr Grundumsatz ist: {grundumsatz:F2} Kalorien/Tag");
            Console.WriteLine();

            double taeglicherKalorienverbrauch = grundumsatz * palWert;
            double zielkalorien = taeglicherKalorienverbrauch - 500;
            Console.WriteLine();
            Console.WriteLine($"Ihr täglicher Kalorienverbrauch ist: {taeglicherKalorienverbrauch:F2} Kalorien/Tag");
            Console.WriteLine($"Um in einer Woche ein halbes Kilo Fett zu verlieren, sollten Sie täglich {zielkalorien:F2} Kalorien zu sich nehmen.");

            // Datum für die aktuelle Eintragung im deutschen Format
            string datum = DateTime.Now.ToString("dd.MM.yyyy");

            // Lesen der vorhandenen Daten
            List<string> zeilenAlt = File.ReadAllLines(dateiPfad).ToList();
            List<string> zeilenNeu = new List<string>(zeilenAlt);

            // Prüfen, ob der Benutzer für den aktuellen Tag bereits Daten hat
            bool eintragVorhanden = false;
            for (int i = 1; i < zeilenNeu.Count; i++)
            {
                string[] spalten = zeilenNeu[i].Split(',');
                if (spalten.Length < 13)
                {
                    continue; // Zeile überspringen, wenn sie nicht genügend Daten enthält
                }

                if (spalten[0] == benutzername && spalten[2] == datum)
                {
                    // Eintrag aktualisieren
                    zeilenNeu[i] = $"{benutzername},{vorname},{datum},{geburtsdatum:dd.MM.yyyy},{gewicht},{groesse},{alter},{geschlecht},{bmi:F2},{grundumsatz:F2},{taeglicherKalorienverbrauch:F2},{zielkalorien:F2},{palWert}";
                    eintragVorhanden = true;
                    break;
                }
            }

            if (!eintragVorhanden)
            {
                // Neuen Eintrag hinzufügen
                string neueZeile = $"{benutzername},{vorname},{datum},{geburtsdatum:dd.MM.yyyy},{gewicht},{groesse},{alter},{geschlecht},{bmi:F2},{grundumsatz:F2},{taeglicherKalorienverbrauch:F2},{zielkalorien:F2},{palWert}";
                zeilenNeu.Add(neueZeile);
            }

            // Daten in die CSV-Datei schreiben
            File.WriteAllLines(dateiPfad, zeilenNeu);

            Console.WriteLine();
            Console.WriteLine("Daten wurden erfolgreich gespeichert.");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Eingabefehler: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Fehler beim Zugriff auf die Datei: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Drücken Sie die Eingabetaste, um das Programm zu beenden.");
            Console.ReadLine();
        }
    }
}