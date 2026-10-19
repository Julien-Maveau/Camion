using System;

namespace Supvinci
{
    public class Calculator
    {
        public static int CalculateAge(DateTime birthday)
        {
            int age = DateTime.Now.Year - birthday.Year;
            // use the date of birth compared at the date of today to get the age.
            if (birthday.Date > DateTime.Now.AddYears(-age).Date)
            {
                age--;
            }

            return age;
        }
    }
}