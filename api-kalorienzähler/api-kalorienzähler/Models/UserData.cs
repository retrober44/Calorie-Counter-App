using System;
using System.Globalization;

namespace ApiKalorienzaehler.Models
{
    public class UserData
    {
        public string Username { get; set; }
        public string BirthDate { get; set; } // Format: "yyyy-MM-dd"
        public int Height { get; set; } // in cm
        public double Weight { get; set; } // in kg
        public string Gender { get; set; }
        public double PalWert { get; set; } = 1.4; // Default value

        public int Age
        {
            get
            {
                var birthDate = DateTime.ParseExact(BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var age = DateTime.Now.Year - birthDate.Year;
                if (birthDate > DateTime.Now.AddYears(-age)) age--;
                return age;
            }
        }

        public double BMI => Weight / Math.Pow(Height / 100.0, 2);

        public double Grundumsatz
        {
            get
            {
                return Gender == "m"
                    ? 66.47 + (13.75 * Weight) + (5.003 * Height) - (6.755 * Age)
                    : 655.1 + (9.563 * Weight) + (1.850 * Height) - (4.676 * Age);
            }
        }

        public double TaeglicherKalorienverbrauch => Grundumsatz * PalWert;

        public double Zielkalorien => TaeglicherKalorienverbrauch - 500;
    }
}
