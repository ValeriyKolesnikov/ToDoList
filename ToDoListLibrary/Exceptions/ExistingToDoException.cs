using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Exceptions
{
    public class ExistingToDoException : Exception
    {
        public ExistingToDoException(ToDo toDo) => Console.WriteLine($"Ошибка: Дело \"{toDo.Name}\" уже есть в списке");
    }
}
