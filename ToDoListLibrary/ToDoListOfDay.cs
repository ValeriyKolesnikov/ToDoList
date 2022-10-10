using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListLibrary.Exceptions;
using ToDoListLibrary.Validators;

namespace ToDoListLibrary
{
    public class ToDoListOfDay
    {
        private List<ToDo> _toDoList;
        private List<IValidator> _validators;
        public event Action<string>? Notify;

        public ToDoListOfDay()
        {
            _toDoList = new List<ToDo>();
            _validators = new List<IValidator>();
            //if (File.Exists(FileNameDataSet()))
            //    this.Load();
        }

        public List<ToDo> GetAll() => _toDoList;

        private void Validate(ToDo value)
        {
            foreach (IValidator validator in _validators)
            {
                if (!validator.IsValid(value, out string errorMessage))
                    throw new ValidateException(value, errorMessage);
            }
        }

        public ToDo? Read(string name)
        {
            return _toDoList.SingleOrDefault(human => human.Name.Equals(name, StringComparison.CurrentCulture));
        }

        public void AddToDo(ToDo item)
        {
            Validate(item);
            foreach (ToDo toDo in _toDoList)
                if (toDo.Equals(item))
                {
                    throw new ExistingToDoException(toDo);
                }
            _toDoList.Add(item);
            //this.Save();
            Notify?.Invoke($"Добавлено новое дело: {item}");
        }

        public void Delete(string name)
        {
            var toDo = Read(name);
            if (toDo is not null)
            {
                _toDoList.RemoveAll(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
                //this.Save();
                Notify?.Invoke($"Удалено дело: {toDo.Name}");
            }
            else throw new NotFoundToDoException(name);
            if (Read(name) is not null)
                throw new DeleteException(toDo);
        }

        public void Update(ToDo item)
        {
            for (int i = 0; i < _toDoList.Count; i++)
                if (_toDoList[i].Equals(item))
                {
                    _toDoList[i] = item;
                   //this.Save();
                    Notify?.Invoke($"Обновлено дело: {item.Name}");
                }
        }

        private static string FileNameDataSet() => Path.GetFullPath("ToDoListDataset.json");
        private void Save()
        {
            var json = JsonConvert.SerializeObject(_toDoList, Formatting.Indented);
            File.WriteAllText(FileNameDataSet(), json);
        }

        private void Load()
        {
            var json = File.ReadAllText(FileNameDataSet());
            var loadedList = JsonConvert.DeserializeObject<List<ToDo>>(json);
            if (loadedList != null)
                _toDoList.AddRange(loadedList);
        }

        public override string? ToString()
        {
            var builder = new StringBuilder();
            foreach(var item in _toDoList)
                builder.AppendLine(item.ToString());
            return builder.ToString();
        }
    }
}
