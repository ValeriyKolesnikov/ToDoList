using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListLibrary.Exceptions;
using ToDoListLibrary.Validators;

namespace ToDoListLibrary
{
    public class ToDoListRepository
    {
        private Dictionary<string, List<ToDo>> _toDoListMap;
        private List<IValidator> _validators;
        private DateOnly _today; 
        private string _todayAsString;
        private List<ToDo> _listToday;
        public event Action<string>? Notify;
        public ToDoListRepository()
        {
            _toDoListMap = new Dictionary<string, List<ToDo>>();
            _validators = new List<IValidator>();
            this.RegisterValidator(new TimeValidator());
            if (File.Exists(FileNameDataSet()))
                this.Load();
            _today = DateOnly.FromDateTime(DateTime.Now);
            _todayAsString = DateOnly.FromDateTime(DateTime.Now).ToString();
            _listToday = _toDoListMap[_todayAsString];
        }

        //public IEnumerable<ToDo> GetAll(DateOnly date) => _toDoListMap[date];
        public void RegisterValidator(IValidator validator) => _validators.Add(validator);

        private void Validate(ToDo value)
        {
            foreach (IValidator validator in _validators)
            {
                if (!validator.IsValid(value, out string errorMessage))
                    throw new ValidateException(value, errorMessage);
            }
        }

        public IEnumerable<ToDo> GetList(DateOnly date)
        {
            var key = date.ToString();
            if (_toDoListMap.ContainsKey(key))
                return _toDoListMap[key];
            return Enumerable.Empty<ToDo>();
        }

        public void CreateToDoList(string date, List<ToDo> list)
        {
            if (_toDoListMap.ContainsKey(date))
                return;
            _toDoListMap[date] = list;
            list.Sort();
            Save();
            Notify?.Invoke($"Добавлен новый список дел на {date}");
        }

        public void CreateToDoListAsYesterday()
        {
            var yesterday = _today.AddDays(-1);
            CreateToDoList(_todayAsString, new List<ToDo>(_toDoListMap[yesterday.ToString()]));
        }

        public void AddToDo(ToDo item)
        {
            Validate(item);
            foreach (ToDo toDo in _listToday)
                if (toDo.Equals(item))
                {
                    throw new ExistingToDoException(toDo);
                }
            _listToday.Add(item);
            _listToday.Sort();
            this.Save();
            Notify?.Invoke($"Добавлено новое дело: {item}");
        }

        public ToDo Read(string name)
        {
            var toDo = _listToday
                .SingleOrDefault(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            if (toDo == null)
                throw new NotFoundToDoException(name);
            return toDo;
        }

        public void Delete(string name)
        {
            var toDo = Read(name);
            _listToday.RemoveAll(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            this.Save();
            Notify?.Invoke($"Удалено дело: {toDo.Name}");
        }

        public void Update(ToDo item)
        {
            for (int i = 0; i < _listToday.Count; i++)
            {
                if (_listToday[i].Equals(item))
                {
                    _listToday[i] = item;
                    this.Save();
                    Notify?.Invoke($"Обновлено дело: {item.Name}");
                    return;
                }                
            }
            throw new NotFoundToDoException(item.Name);
        }

        public void ChangeStatus(string name)
        {         
            Read(name).ChangeStatus();
            this.Save();
            Notify?.Invoke($"Изменен статус дела: {name}");

        }

        public void CloseAll()
        {
            foreach (ToDo item in _listToday)
            if (item.Status == ToDoStatus.OPEN) 
                    item.ChangeStatus();
            this.Save();
            Notify?.Invoke($"Все дела на сегодня закрыты");
        }

        private static string FileNameDataSet() => Path.GetFullPath("ToDoListMapDataset.json");
        private void Save()
        {
            var json = JsonConvert.SerializeObject(_toDoListMap);
            File.WriteAllText(FileNameDataSet(), json);
        }

        private void Load()
        {
            var json = File.ReadAllText(FileNameDataSet());
            var loadedList = JsonConvert.DeserializeObject<Dictionary<string, List<ToDo>>>(json);
            if (loadedList != null)
                _toDoListMap = new Dictionary<string, List<ToDo>>(loadedList);
        }
    }
}
