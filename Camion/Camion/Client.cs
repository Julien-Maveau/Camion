using Supvinci;
using System;

namespace Gestion { 
public class Client
{
    public Guid Id { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public DateTime Birthdate { get; set; }
    public int PhoneNumb { get; set; }
    public string Email { get; set; }
        public int Age
        {
            get { return Calculator.CalculateAge(Birthdate); }
        }
    }

}