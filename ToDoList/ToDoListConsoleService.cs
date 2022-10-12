using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListLibrary;
using ToDoListLibrary.Exceptions;

namespace ToDoList
{
    internal static class ToDoListConsoleService
    {
        static string? name;
        static TimeOnly time;
        static string? description;
        static DateTime today = DateTime.Today;
        public static void WorkingWithToDoRepository(ToDoListRepository repo)
        {           
            repo.Notify += (message) => Console.WriteLine(message);
            repo.Notify += WritingToFile;
            string command;
            List<ToDo> list;
            do
            {
                PrintMenuToDoRepository();
                command = Console.ReadLine();
                switch (command)
                {
                    case "past":
                        Create(repo, today.AddDays(-1));
                        break;
                    case "create":
                        Create(repo, today);
                        break;
                    case "copy":
                        repo.AddListAsYesterday();
                        break;
                    case "print":
                        PrintAll(today, repo);
                        break;
                    case "old":
                        var date = InputDate();                       
                        PrintAll(DateTime.Parse(date), repo);
                        break;
                    case "delete":
                        name = InputName();
                        repo.Delete(name);
                        break;
                    case "add":
                        Add(repo);
                        break;
                    case "update":
                        UpdateToDo(repo);
                        break;
                    case "status":
                        name = InputName();
                        repo.ChangeStatus(name);
                        break;
                    case "close":
                        repo.CloseAll();
                        break;
                    case "exit":
                        break;
                    default:
                        throw new InputException();
                }
            }
            while (command != "exit");
        }

        private static void Add(ToDoListRepository repo)
        {
            InputConstruсtor();
            repo.AddToDo(new ToDo(name!, time!, description!));
        }

        private static void Add(ToDoListRepository repo, List<ToDo> list)
        {
            InputConstruсtor();           
            repo.AddToDoInList(new ToDo(name!, time!, description!), list);
        }

        private static void Create(ToDoListRepository repo, DateTime date)
        {
            var listToDo = new List<ToDo>();
            string command;
            while (true)
            {
                Console.WriteLine("Добавить новое дело в список? y/n");
                command = Console.ReadLine();
                if (command == "y")
                    Add(repo, listToDo);
                else if (command == "n")
                    break;
                else Console.WriteLine("Такой команды не существует! Повторите ввод:");
            }
            repo.AddList(date, listToDo);
        }

        private static void UpdateToDo(ToDoListRepository repo)
        {
            InputName();
            var toDo = repo.Read(name!);
            Console.WriteLine("Введите новые параметры");
            if (toDo == null)
                throw new NotFoundToDoException(name!);
            repo.Update(toDo, new ToDo(name!, time!, description!));
        }

        private static void InputConstruсtor()
        {
            name = InputName();
            time = TimeOnly.Parse(InputTime());
            description = InputDescription();
        }

        private static string InputName()
        {
            Console.WriteLine("Введите наименование дела:");
            return Console.ReadLine();
        }

        private static string InputDescription()
        {
            Console.WriteLine("Введите описание дела:");
            return Console.ReadLine();
        }        

        private static string InputDate()
        {
            while (true)
            {
                Console.WriteLine("Введите дату в формате \"dd.MM.yyyy\"");
                var date = Console.ReadLine();
                if (DateOnly.TryParse(date, out _))
                    return date;
                Console.WriteLine("Неверный формат даты");
            }
        }

        private static string InputTime()
        {
            Console.WriteLine("Введите время в формате \"HH:mm\"");
            return Console.ReadLine();
        }

        private static void WritingToFile(string message)
        {
            string filePath = @"log_test.txt";
            File.AppendAllText(filePath, $"{DateTime.Now} {message}{Environment.NewLine}");
        }

        private static void PrintAll(DateTime date, ToDoListRepository repo)
        {
            var toDoList = repo.GetList(date);
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
                          "update - обновить дело\n" +
                          "status - поменять стаус дела\n" +
                          "close - закрыть все дела\n" +
                          "exit - выход\n");
        }
    }
}
