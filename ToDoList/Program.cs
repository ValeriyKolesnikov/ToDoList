
using ToDoListLibrary;
using ToDoList;
using ToDoListLibrary.Exceptions;

var repo = new ToDoListRepository();
var today = DateOnly.FromDateTime(DateTime.Now).ToString();

try
{      
    ToDoListConsoleService.WorkingWithToDoRepository(repo);
}

catch(InputException ex)
{
    Console.WriteLine(ex.Message);
}

catch (NotFoundToDoException ex)
{
    Console.WriteLine(ex.Message);
}

catch (DeleteException ex)
{
    Console.WriteLine(ex.Message);
}

catch (ExistingToDoException ex)
{
    Console.WriteLine(ex.Message);
}

catch (ValidateException ex)
{
    Console.WriteLine(ex.Message);
}

catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine();
