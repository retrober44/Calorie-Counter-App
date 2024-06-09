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
            _logger.LogInformation("Received user data: {@UserData}", userData);

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
                _logger.LogInformation("User data saved successfully.");

                return Ok(new { message = "User data saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user data.");
                return StatusCode(500, new { message = "There was an error saving the user data." });
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
            return NotFound(new { message = "User data not found." });
        }

        var lines = System.IO.File.ReadAllLines(filePath);
        if (lines.Length < 2)
        {
            return NotFound(new { message = "No user data entries found." });
        }

        var lastEntry = lines.Last();
        var data = lastEntry.Split(',');

        var userData = new UserData
        {
            Username = username,
            BirthDate = DateTime.ParseExact(data[1], "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
            Weight = double.Parse(data[3], CultureInfo.InvariantCulture),
            Height = int.Parse(data[4], CultureInfo.InvariantCulture),
            Gender = data[5],
            PalWert = double.Parse(data[6], CultureInfo.InvariantCulture)
        };

        return Ok(userData);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving user data.");
        return StatusCode(500, new { message = "There was an error retrieving the user data." });
    }
}

    }
}
