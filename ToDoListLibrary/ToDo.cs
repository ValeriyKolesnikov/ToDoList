using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ToDoListLibrary
{
    public class ToDo : IComparable<ToDo>
    {
        public ToDo(string name, TimeOnly startTime, string description)
        {
            Name = name;             
            StartTime = startTime;
            Description = description;
            Status = ToDoStatus.OPEN;
        }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "Наименование должно содержать от 1 до 20 символов")]
        public string Name { get; set; }
        public TimeOnly StartTime { get; set; }
        public ToDoStatus Status { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Описание должно содержать до 100 символов")]
        public string Description { get; set; }

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
            $"Описание: {Description}",
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
    }
}
