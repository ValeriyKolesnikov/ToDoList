using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Exceptions
{
    public class InputException : Exception
    {
        public InputException() => Console.WriteLine("Ошибка ввода. Такой команды не существует");
    }
}
