using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListLibrary;
using ToDoListLibrary.Exceptions;

namespace ToDoList
{
    internal class ToDoListConsoleService
    {
        private string? name;
        private TimeOnly time;
        private DateTime _today;
        public ToDoListRepository Repository { get; set; }

        public ToDoListConsoleService()
        {
            Repository = new ToDoListRepository("kolesnik1988");
            _today = DateTime.Today;
        }

        public void WorkingWithToDoRepository()
        {           
            Repository.Notify += (message) => Console.WriteLine(message);
            Repository.Notify += WritingToFile;
            string command;
            List<ToDo> list;
            do
            {
                PrintMenuToDoRepository();
                command = Console.ReadLine();
                switch (command)
                {
                    case "past":
                        Create(_today.AddDays(-1));
                        break;
                    case "create":
                        Create(_today);
                        break;
                    case "copy":
                        Repository.AddListAsYesterday();
                        break;
                    case "print":
                        PrintAll(_today);
                        break;
                    case "old":
                        var date = InputDate();                       
                        PrintAll(date);
                        break;
                    case "delete":
                        name = InputName();
                        Repository.Delete(name);
                        break;
                    case "add":
                        Add();
                        break;
                    case "status":
                        name = InputName();
                        Repository.ChangeStatus(name);
                        break;
                    case "close":
                        Repository.CloseAll();
                        break;
                    case "exit":
                        break;
                    default:
                        throw new InputException();
                }
            }
            while (command != "exit");
        }

        private void Add()
        {
            InputConstruсtor();
            Repository.AddToDo(new ToDo(name!, time!));
        }

        private void Add(List<ToDo> list)
        {
            InputConstruсtor();           
            Repository.AddToDoInList(new ToDo(name!, time!), list);
        }

        private void Create(DateTime date)
        {
            var listToDo = new List<ToDo>();
            string command;
            while (true)
            {
                Console.WriteLine("Добавить новое дело в список? y/n");
                command = Console.ReadLine();
                if (command == "y")
                    Add(listToDo);
                else if (command == "n")
                    break;
                else Console.WriteLine("Такой команды не существует! Повторите ввод:");
            }
            Repository.AddList(date, listToDo);
        }

        private void InputConstruсtor()
        {
            name = InputName();
            time = InputTime();
        }

        private string InputName()
        {
            Console.WriteLine("Введите наименование дела:");
            return Console.ReadLine();
        }

        private DateTime InputDate()
        {
            while (true)
            {
                Console.WriteLine("Введите дату в формате \"dd.MM.yyyy\"");
                var input = Console.ReadLine();
                if (DateTime.TryParse(input, out DateTime date))
                    return date;
                Console.WriteLine("Неверный формат даты");
            }
        }

        private static TimeOnly InputTime()
        {
            while (true)
            {
                Console.WriteLine("Введите время в формате \"HH:mm\"");
                var input = Console.ReadLine();
                if (TimeOnly.TryParse(input, out TimeOnly time))
                    return time;
                Console.WriteLine("Неверный формат времени");
            }
        }

        private static void WritingToFile(string message)
        {
            string filePath = @"log_test.txt";
            File.AppendAllText(filePath, $"{DateTime.Now} {message}{Environment.NewLine}");
        }

        private void PrintAll(DateTime date)
        {
            var toDoList = Repository.GetList(date);
            if (toDoList.Count() == 0)
                Console.WriteLine($"Cписок дел пуст\n");
            else
            {
                foreach (ToDo toDo in toDoList)
                {
                    Console.Write(toDo);
                }
                Console.WriteLine();
            }
        }

        private static void PrintMenuToDoRepository()
        {
            Console.WriteLine("\n" + "create - создать список дел на сегодня\n" +
                          "copy - создать список дел на основе предыдущего дня\n" +
                          "print - вывести список дел на сегодня\n" +
                          "old - вывести список дел из архива\n" +
                          "add - добавить дело в список\n" +
                          "delete - удалить дело из списка\n" +
                          "status - поменять стаус дела\n" +
                          "close - закрыть все дела\n" +
                          "exit - выход\n");
        }
    }
}
