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
    /// <summary>
    /// Класс-репозиторий. Хранит списки дел на определенную дату в формате Dictionary
    /// Key - Дата в формате  string, value - список дел в формате List<ToDo>
    /// </summary>
    public class ToDoListRepository
    {
        #region Приватные поля
        private Dictionary<string, List<ToDo>> _toDoListMap;
        private List<IValidator> _validators;
        private DateOnly _today; 
        private string _todayAsString;
        private List<ToDo> _listToday;
        #endregion
       
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

        public event Action<string>? Notify;
        /// <summary>
        /// Метод регистрирует валидатор (добавляет в список валидаторов репозитория)
        /// </summary>
        private void RegisterValidator(IValidator validator) => _validators.Add(validator);

        /// <summary>
        /// Метод осуществляет валидацию по всем валидаторам
        /// </summary>
        /// <exception cref="ValidateException"></exception>
        private void Validate(ToDo value)
        {
            foreach (IValidator validator in _validators)
            {
                if (!validator.IsValid(value, out string errorMessage))
                    throw new ValidateException(value, errorMessage);
            }
        }

        /// <summary>
        /// Метод возвращает список дел на заданную дату в формате IEnumerable
        /// </summary>
        public IEnumerable<ToDo> GetList(DateOnly date)
        {
            var key = date.ToString();
            if (_toDoListMap.ContainsKey(key))
                return _toDoListMap[key];
            return Enumerable.Empty<ToDo>();
        }

        /// <summary>
        /// Метод создает список дел на заданную дату
        /// </summary>
        /// <param name="date"></param>
        /// <param name="list"></param>
        public void CreateToDoList(string date, List<ToDo> list)
        {
            if (_toDoListMap.ContainsKey(date))
                return;
            _toDoListMap[date] = list;
            list.Sort();
            Save();
            Notify?.Invoke($"Добавлен новый список дел на {date}");
        }

        /// <summary>
        /// Метод создает список дел на сегодня на основе вчерашнего дня
        /// </summary>
        public void CreateToDoListAsYesterday()
        {
            var yesterday = _today.AddDays(-1);
            CreateToDoList(_todayAsString, new List<ToDo>(_toDoListMap[yesterday.ToString()]));
        }

        /// <summary>
        /// Метод добавляет дело в существующий список дел на сегодня
        /// </summary>
        /// <exception cref="ExistingToDoException"></exception>
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

        /// <summary>
        /// Метод возвращает дело из списка дел на сегодня по названию
        /// </summary>
        /// <exception cref="NotFoundToDoException"></exception>
        public ToDo Read(string name)
        {
            var toDo = _listToday
                .SingleOrDefault(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            if (toDo == null)
                throw new NotFoundToDoException(name);
            return toDo;
        }

        /// <summary>
        /// Метод удаляет дело из списка дел на сегодня по названию
        /// </summary>
        public void Delete(string name)
        {
            var toDo = Read(name);
            _listToday.RemoveAll(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            this.Save();
            Notify?.Invoke($"Удалено дело: {toDo.Name}");
        }

        /// <summary>
        /// Метод вносит изменения в заданное дедло из списка дел на сегодня
        /// </summary>
        /// <exception cref="NotFoundToDoException"></exception>
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

        /// <summary>
        /// Метод изменяет статус выполнения дела на противоположный
        /// </summary>
        /// <param name="name"></param>
        public void ChangeStatus(string name)
        {         
            Read(name).ChangeStatus();
            this.Save();
            Notify?.Invoke($"Изменен статус дела: {name}");

        }

        /// <summary>
        /// Метод закрывает все открытые дела
        /// </summary>
        public void CloseAll()
        {
            foreach (ToDo item in _listToday)
            if (item.Status == ToDoStatus.OPEN) 
                    item.ChangeStatus();
            this.Save();
            Notify?.Invoke($"Все дела на сегодня закрыты");
        }

        /// <summary>
        /// Метод возвращает абсолютный путь файла с данными репозитория
        /// </summary>
        /// <returns></returns>
        private static string FileNameDataSet() => Path.GetFullPath("ToDoListMapDataset.json");
        
        /// <summary>
        /// Метод сериализует данные репозитория в json-файл 
        /// </summary>
        private void Save()
        {
            var json = JsonConvert.SerializeObject(_toDoListMap);
            File.WriteAllText(FileNameDataSet(), json);
        }

        /// <summary>
        /// Метод десериализует данные репозитория из json-файла
        /// </summary>
        private void Load()
        {
            var json = File.ReadAllText(FileNameDataSet());
            var loadedList = JsonConvert.DeserializeObject<Dictionary<string, List<ToDo>>>(json);
            if (loadedList != null)
                _toDoListMap = new Dictionary<string, List<ToDo>>(loadedList);
        }
    }
}
