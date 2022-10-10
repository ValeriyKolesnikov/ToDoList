using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListLibrary.Exceptions
{
    public class DeleteException : Exception
    {
        public DeleteException(ToDo toDo) => Console.WriteLine($"Ошибка удаления: Дело  \"{toDo.Name}\" по-прежнему в списке");
    }
}
