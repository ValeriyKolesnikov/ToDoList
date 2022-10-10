using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Exceptions
{
    public class NotFoundFileException : Exception
    {
        public NotFoundFileException(string file)
        {
            Console.WriteLine($"Ошибка: Файл {file} не найден");
        }
    }
}
