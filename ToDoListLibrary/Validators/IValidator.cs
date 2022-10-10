using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Validators
{
    public interface IValidator
    {
        public bool IsValid(ToDo value, out string errorMessage);
    }
}
