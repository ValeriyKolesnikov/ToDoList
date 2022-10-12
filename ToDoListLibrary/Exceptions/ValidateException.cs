using System.Runtime.Serialization;

namespace ToDoListLibrary.Exceptions
{
    public class ValidateException : Exception
    {
        public ValidateException(ToDo toDo, string errorMessage)
        {
            Console.WriteLine($"Ошибка валидации: Дело \"{toDo.Name}\" : {errorMessage}");
        }
    }
}