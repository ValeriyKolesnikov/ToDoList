using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Exceptions
{
    public class NotFoundToDoException : Exception
    {
        public NotFoundToDoException(string name) => Console.WriteLine($"Ошибка: Дело \"{name}\" не найдено в списке");
    }
}
