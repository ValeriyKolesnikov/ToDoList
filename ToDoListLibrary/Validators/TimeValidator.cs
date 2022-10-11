using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Validators
{
    public class TimeValidator : IValidator
    {
        public bool IsValid(ToDo value, out string errorMessage)
        {
            if (TimeOnly.TryParse(value.StartTime.ToString(), out _))
            {
                errorMessage = "";
                return true;
            }
            errorMessage = "Время должно быть в формате \"HH:mm\"";
            return false;
        }
    }
}
