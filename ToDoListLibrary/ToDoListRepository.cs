using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using ToDoListLibrary.Exceptions;

namespace ToDoListLibrary
{
    /// <summary>
    /// Класс-репозиторий. Хранит списки дел на определенную дату в формате Dictionary, где
    /// key - дата в формате  string, value - список дел в формате List<ToDo>
    /// </summary>
    public class ToDoListRepository
    {
        #region Приватные поля и константы
        private Dictionary<DateTime, List<ToDo>> _toDoListMap;
        private DateTime _today; 
        #endregion

        public ToDoListRepository()
        {
            _today = DateTime.Today;
            if (File.Exists(FileNameDataSet()))
                this.Load();
            else 
                _toDoListMap = new Dictionary<DateTime, List<ToDo>>();
        }        

        /// <summary>
        /// Метод осуществляет валидацию по атрибутам
        /// </summary>
        /// <exception cref="ValidateException"></exception>
        private void AttributValidate(ToDo value)
        {
            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(value, context, results, true))
            {
                foreach (ValidationResult error in results)
                {
                    throw new ValidateException(value, error.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// Метод возвращает список дел на сегодня.
        /// Если список дел отсутствует, то сначала создает новый пустой список
        /// </summary>
        private List<ToDo> GetToDoListToday()
        {
            if (!_toDoListMap.ContainsKey(_today))
                _toDoListMap[_today] = new List<ToDo>();
            return _toDoListMap[_today];
        }

        public event Action<string>? Notify;
        public IReadOnlyDictionary<DateTime, List<ToDo>> GetMap() => _toDoListMap;

        /// <summary>
        /// Метод возвращает список дел на заданную дату в формате IEnumerable
        /// </summary>
        public IEnumerable<ToDo> GetList(DateTime date)
        {
            if (_toDoListMap.ContainsKey(date))
                return _toDoListMap[date];
            return Enumerable.Empty<ToDo>();
        }
        
        /// <summary>
        /// Метод добавляет заданное дело в заданный список дел
        /// </summary>
        public void AddToDoInList(ToDo toDo, List<ToDo> list)
        {
            AttributValidate(toDo);
            list.Add(toDo);
        }

        /// <summary>
        /// Метод добавляет заданный список дел на заданную дату
        /// </summary>
        public void AddList(DateTime date, List<ToDo> list)
        {
            _toDoListMap[date] = list;
            list.Sort();
            this.Save();
            Notify?.Invoke($"Добавлен новый список дел на {date.Date}");
        }

        /// <summary>
        /// Метод создает список дел на сегодня на основе вчерашнего дня
        /// </summary>
        public void AddListAsYesterday()
        {
            var yesterday = _today.AddDays(-1);
            AddList(_today, new List<ToDo>(_toDoListMap[yesterday]));
            Notify?.Invoke($"Добавлен новый список дел на {_today.Date}");
        }

        /// <summary>
        /// Метод добавляет дело в список дел на сегодня
        /// </summary>
        /// <exception cref="ExistingToDoException"></exception>
        public void AddToDo(ToDo item)
        {
            AttributValidate(item);
            List<ToDo> listToday;
            if (_toDoListMap.ContainsKey(_today))
            {
                listToday = _toDoListMap[_today];
                foreach (ToDo toDo in listToday)
                    if (toDo.Equals(item))
                    {
                        throw new ExistingToDoException(toDo);
                    }
                listToday.Add(item);
                listToday.Sort();
            } else
            {
                listToday = new List<ToDo>() {item};
                _toDoListMap[_today] = listToday;
            }
            this.Save();
            Notify?.Invoke($"Добавлено новое дело: {item}");
        }

        /// <summary>
        /// Метод возвращает дело из списка дел на сегодня по названию
        /// </summary>
        /// <exception cref="NotFoundToDoException"></exception>
        public ToDo Read(string name)
        {
            var toDo = GetToDoListToday()
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
            GetToDoListToday().RemoveAll(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            this.Save();
            Notify?.Invoke($"Удалено дело: {toDo.Name}");
        }

        /// <summary>
        /// Метод вносит изменения в заданное дело из списка дел на сегодня
        /// </summary>
        /// <exception cref="NotFoundToDoException"></exception>
        public void Update(ToDo item, ToDo updateItem)
        {
            AttributValidate(item);
            AttributValidate(updateItem);
            var listToday = GetToDoListToday();
            for (int i = 0; i < listToday.Count; i++)
            {
                if (listToday[i].Equals(item))
                {
                    listToday[i] = updateItem;
                    this.Save();
                    Notify?.Invoke($"Обновлено дело: {item.Name}");
                    return;
                }                
            }
            throw new NotFoundToDoException(item.Name);
        }

        /// <summary>
        /// Метод удаляет список дел на заданную дату
        /// </summary>
        public void DeleteList(DateTime date)
        {
            _toDoListMap.Remove(date);
            this.Save();
            Notify?.Invoke($"Удален список дел на {date}");
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
            var listToday = GetToDoListToday();
            foreach (ToDo item in listToday)
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
            var loadedList = JsonConvert.DeserializeObject<Dictionary<DateTime, List<ToDo>>>(json);
            if (loadedList != null)
                _toDoListMap = new Dictionary<DateTime, List<ToDo>>(loadedList);
            else _toDoListMap = new Dictionary<DateTime, List<ToDo>>();
        }
    }
}