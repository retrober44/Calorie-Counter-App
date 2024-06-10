using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ApiKalorienzaehler.Models;

namespace ApiKalorienzaehler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDataController : ControllerBase
    {
        private readonly ILogger<UserDataController> _logger;

        public UserDataController(ILogger<UserDataController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult SaveUserData([FromBody] UserData userData)
        {
            _logger.LogInformation("Benutzerdaten empfangen: {@UserData}", userData);

            try
            {
                var directoryPath = Path.Combine("UserData");
                Directory.CreateDirectory(directoryPath);

                var filePath = Path.Combine(directoryPath, $"{userData.Username}-gewichtsdaten.csv");
                var fileExists = System.IO.File.Exists(filePath);
                var lines = new List<string>();

                if (fileExists)
                {
                    lines = System.IO.File.ReadAllLines(filePath).ToList();
                }
                else
                {
                    lines.Add("Datum,Geburtsdatum,Alter,Gewicht,BMI,Grundumsatz,PAL-Wert,Täglicher Kalorienverbrauch,Zielkalorien");
                }

                // Convert birth date to dd.MM.yyyy format
                var birthDate = DateTime.ParseExact(userData.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var formattedBirthDate = birthDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

                var newEntry = string.Format(CultureInfo.InvariantCulture,
                    "{0:dd.MM.yyyy},{1},{2},{3:F2},{4:F2},{5:F2},{6:F2},{7:F2},{8:F2}",
                    DateTime.Now,
                    formattedBirthDate, // Use the formatted birth date here
                    userData.Age,
                    userData.Weight,
                    userData.BMI,
                    userData.Grundumsatz,
                    userData.PalWert,
                    userData.TaeglicherKalorienverbrauch,
                    userData.Zielkalorien
                );

                var existingEntryIndex = lines.FindIndex(line => line.StartsWith($"{DateTime.Now:dd.MM.yyyy}"));
                if (existingEntryIndex >= 0)
                {
                    lines[existingEntryIndex] = newEntry;
                }
                else
                {
                    lines.Add(newEntry);
                }

                System.IO.File.WriteAllLines(filePath, lines);
                _logger.LogInformation("Benutzerdaten erfolgreich gespeichert.");

                return Ok(new { message = "Benutzerdaten erfolgreich gespeichert." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern der Benutzerdaten.");
                return StatusCode(500, new { message = "Es gab einen Fehler beim Speichern der Benutzerdaten." });
            }
        }

        [HttpGet("{username}")]
        public IActionResult GetUserData(string username)
        {
            try
            {
                var directoryPath = Path.Combine("UserData");
                var filePath = Path.Combine(directoryPath, $"{username}-gewichtsdaten.csv");

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning($"Benutzerdaten für {username} nicht gefunden.");
                    return NotFound(new { message = "Benutzerdaten nicht gefunden." });
                }

                var lines = System.IO.File.ReadAllLines(filePath);
                if (lines.Length < 2)
                {
                    _logger.LogWarning($"Keine Benutzereinträge für {username} gefunden.");
                    return NotFound(new { message = "Keine Benutzereinträge gefunden." });
                }

                var lastEntry = lines.Last();
                var data = lastEntry.Split(',');

                // Log the raw data for debugging
                _logger.LogInformation($"Gelesene Daten für {username}: {string.Join(", ", data)}");

                if (data.Length < 9)
                {
                    _logger.LogError("Die CSV-Datei hat nicht die erwartete Anzahl von Spalten.");
                    return StatusCode(500, new { message = "Die CSV-Datei hat nicht die erwartete Anzahl von Spalten." });
                }

                var userData = new UserData
                {
                    Username = username,
                    BirthDate = DateTime.ParseExact(data[1], "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                    Height = int.Parse(data[2], CultureInfo.InvariantCulture),
                    Weight = double.Parse(data[3], CultureInfo.InvariantCulture),
                    Gender = data[4],
                    PalWert = double.Parse(data[6], CultureInfo.InvariantCulture)
                };

                return Ok(userData);
            }
            catch (FormatException fe)
            {
                _logger.LogError(fe, "Fehler beim Formatieren der Daten.");
                return StatusCode(500, new { message = "Fehler beim Formatieren der Daten." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Benutzerdaten.");
                return StatusCode(500, new { message = "Es gab einen Fehler beim Abrufen der Benutzerdaten." });
            }
        }
    }
}
