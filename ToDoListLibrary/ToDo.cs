
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ToDoListLibrary
{
    public class ToDo : IComparable<ToDo>, ICloneable
    {
        public ToDo(string name, TimeOnly startTime)
        {
            Name = name;             
            StartTime = startTime;
            Status = ToDoStatus.OPEN;
        }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "Наименование должно содержать от 1 до 20 символов")]
        public string Name { get; init; }
        [JsonConverter(typeof(TimeOnlyConverter))]
        public TimeOnly StartTime { get; init; }
        public ToDoStatus Status { get; private set; }

        /// <summary>
        /// Метод возвращает список свойств класса в формате List
        /// </summary>
        /// <returns></returns>
        private List<string> GetInfo()
        {
            return new List<string> {
            $"\nДело:",
            $"{Name}",
            $"Время: {StartTime}",
            $"Статус: {GetStatus(this)}"};
        }

        /// <summary>
        /// Метод изменяет свойство "Status"
        /// </summary>
        public void ChangeStatus()
        {
            if (Status == ToDoStatus.OPEN)
                Status = ToDoStatus.CLOSED;
            else Status = ToDoStatus.OPEN;
        }

        /// <summary>
        /// Метод возвращает статус выполнения в формате string
        /// </summary>
        private string GetStatus(ToDo toDo)
        {
            if (toDo.Status == ToDoStatus.CLOSED)
                return "Закрыто";
            return "Открыто";
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

        public object Clone() => MemberwiseClone();
    }
}
