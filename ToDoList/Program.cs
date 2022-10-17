
using ToDoList;


var service = new ToDoListConsoleService();

try
{      
    service.WorkingWithToDoRepository();
}

catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine();
