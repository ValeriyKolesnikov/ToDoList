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
        #region Приватные поля и константы
        private Dictionary<DateOnly, List<ToDo>> _toDoListMap;
        private List<IValidator> _validators;
 //       [JsonProperty]
        [JsonConverter(typeof(DateOnlyCoverter))]
        private DateOnly _today; 
        private List<ToDo> _listToday;
        private static string path;
        private const string WORKER_PATH = "ToDoListMapDataset.json";
        private const string TEST_PATH = "ToDoListMapDatasetTest.json";
        #endregion

        public ToDoListRepository(ProjectType type)
        {
            _toDoListMap = new Dictionary<DateOnly, List<ToDo>>();
            _validators = new List<IValidator>();
            this.RegisterValidator(new TimeValidator());
            _today = DateOnly.FromDateTime(DateTime.Now);
            path = type == ProjectType.WORKER ? WORKER_PATH : TEST_PATH;
            if (File.Exists(FileNameDataSet()))
                this.Load();
            _listToday = GetToDoListToday();
        }

        public ToDoListRepository() : this(ProjectType.WORKER) { }

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
            if (_toDoListMap.ContainsKey(date))
                return _toDoListMap[date];
            return Enumerable.Empty<ToDo>();
        }

        /// <summary>
        /// Метод возвращает список дел на сегодня.
        /// Если список дел отсутствует, то создает новый пустой список
        /// </summary>
        private List<ToDo> GetToDoListToday()
        {
            if (!_toDoListMap.ContainsKey(_today))
            _toDoListMap[_today] = new List<ToDo>();
            return _toDoListMap[_today];
        }
        
        /// <summary>
        /// Метод добавляет заданный список дел на заданную дату
        /// </summary>
        public void AddToDoList(DateOnly date, List<ToDo> list)
        {
            _toDoListMap[date] = list;
            list.Sort();
            this.Save();
            Notify?.Invoke($"Добавлен новый список дел на {date}");
        }

        /// <summary>
        /// Метод создает список дел на сегодня на основе вчерашнего дня
        /// </summary>
        public void CreateToDoListAsYesterday()
        {
            var yesterday = _today.AddDays(-1);
            AddToDoList(_today, new List<ToDo>(_toDoListMap[yesterday]));
            this.Save();
            Notify?.Invoke($"Добавлен новый список дел на {_today}");
        }

        /// <summary>
        /// Метод добавляет дело в существующий список дел на сегодня
        /// </summary>
        /// <exception cref="ExistingToDoException"></exception>
        public void AddToDo(ToDo item)
        {
            Validate(item);
            var listToday = _toDoListMap[_today];
            foreach (ToDo toDo in listToday)
                if (toDo.Equals(item))
                {
                    throw new ExistingToDoException(toDo);
                }
            listToday.Add(item);
            listToday.Sort();
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
            Validate(item);
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
        private static string FileNameDataSet() => Path.GetFullPath(path);
        
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
            var loadedList = JsonConvert.DeserializeObject<Dictionary<DateOnly, List<ToDo>>>(json);
            if (loadedList != null)
                _toDoListMap = new Dictionary<DateOnly, List<ToDo>>(loadedList);
        }
    }
}
