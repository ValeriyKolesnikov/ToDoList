using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListBot
{
    /// <summary>
    /// Enum содержит маркеры, указывающие на какой стадии 
    /// работы с ботом находится пользователь
    /// </summary>
    internal enum Marker
    {
        IS_MENU,
        IS_ADDED_TODO,
        IS_NAME_INPUT,
        IS_TIME_INPUT,
        IS_DATE_INPUT,
        IS_CHANGE_STATUS,
        IS_DELETED,
        IS_CANCELLED
    }
}
