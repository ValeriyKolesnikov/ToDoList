
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ToDoListLibrary
{
    public class ToDo : IComparable<ToDo>
    {
        [JsonConstructor]
        public ToDo(string name, TimeOnly startTime, ToDoStatus status)
        {
            Name = name;             
            StartTime = startTime;
            Status = status;
            Number = 1;
        }

        public ToDo(string name, TimeOnly startTime) : this(name, startTime, ToDoStatus.OPEN) { }

        [Range(1,50, ErrorMessage = "Порядковый номер должен входить в диапазон [1..50)")]
        public int Number { get; set; }
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Наименование должно содержать от 1 до 20 символов")]
        public string Name { get; init; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly StartTime { get; init; }
        public ToDoStatus Status { get; set; }

        /// <summary>
        /// Метод возвращает список свойств класса в формате List
        /// </summary>
        /// <returns></returns>
        
        private List<string> GetInfo()
        {
            return new List<string> {
            $"\n{PrintStatus(this)}",            
            $"{Number}. ",
            $"{StartTime.ToString("HH:mm")} | ",
            $"{Name}"};
        }

        /// <summary>
        /// Метод возвращает статус выполнения в формате string
        /// </summary>

        private string PrintStatus(ToDo toDo)
        {
            if (toDo.Status == ToDoStatus.CLOSED)
                return "[ + ]";
            if (toDo.Status == ToDoStatus.NO)
                return "[no]";
            return "[    ]";
        }

        public override string ToString() => string.Join("\t", this.GetInfo());

        public int CompareTo(ToDo other)
        {
            if (this.StartTime.Equals(other.StartTime))
                return this.Name.CompareTo(other.Name);
            return StartTime.CompareTo(other.StartTime);
        }

        public override bool Equals(object? obj)
        {
            return obj is ToDo todo &&
                   Name == todo.Name &&
                   StartTime.Equals(todo.StartTime);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, StartTime);
        }
    }
}
