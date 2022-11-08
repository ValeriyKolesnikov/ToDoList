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
        #region Приватные поля
        private Dictionary<DateTime, List<ToDo>> _toDoListMap;
        private readonly DateTime _today; 
        #endregion
        public string UserName { get; set; }

        public ToDoListRepository(string userName)
        {
            _today = DateTime.Today;
            UserName = userName;
            _toDoListMap = new Dictionary<DateTime, List<ToDo>>();
            if (File.Exists(FileNameDataSet()))
                this.Load();        
        }

        /// <summary>
        /// Метод возвращает содержимое словаря со списками дел в формате "только для чтения"
        /// </summary>
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
        /// Метод добавляет дело в список дел на заданную дату
        /// </summary>
        /// <exception cref="ExistingToDoException"></exception>
        public void AddToDo(ToDo item, DateTime date)
        {
            AttributValidate(item);
            List<ToDo> list;
            if (_toDoListMap.ContainsKey(date))
            {
                list = _toDoListMap[date];
                foreach (ToDo toDo in list)
                    if (toDo.Equals(item))
                    {
                        throw new ExistingToDoException(toDo);
                    }
                list.Add(item);
                SortList(list);
            }
            else
            {
                list = new List<ToDo>() { item };
                _toDoListMap[date] = list;
            }
            this.Save();
        }

        /// <summary>
        /// Метод добавляет заданный список дел на заданную дату
        /// </summary>
        public void AddList(DateTime date, List<ToDo> list)
        {
            _toDoListMap[date] = list;
            SortList(list);
            this.Save();
        }

        /// <summary>
        /// Метод создает список дел на сегодня на основе вчерашнего дня
        /// </summary>
        public void AddListAsYesterday()
        {
            var yesterday = _today.AddDays(-1);
            AddList(_today, new List<ToDo>(_toDoListMap[yesterday]));
            OpenAll();
            this.Save();
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
            var list = GetToDoListToday();
            var toDo = Read(name);
            list.RemoveAll(toDo => toDo.Name.Equals(name, StringComparison.CurrentCulture));
            SortList(list);
            this.Save();
        }

        /// <summary>
        /// Метод удаляет список дел на заданную дату
        /// </summary>
        public void DeleteList(DateTime date)
        {
            _toDoListMap.Remove(date);
            this.Save();
        }

        /// <summary>
        /// Метод изменяет статус дела 
        /// </summary>
        public void ChangeStatus(string name)
        {
            var status = Read(name).Status;
            if (status == ToDoStatus.OPEN)
            {
                Read(name).Status = ToDoStatus.CLOSED;
            }
            else if (status == ToDoStatus.CLOSED)
            {
                Read(name).Status = ToDoStatus.OPEN;
            }
            this.Save();
        }

        /// <summary>
        /// Метод помечает дело как "Не буду выполнять"
        /// </summary>
        public void CancelToDo(string name)
        {
            Read(name).Status = ToDoStatus.NO;            
            this.Save();
        }

        /// <summary>
        /// Метод закрывает все открытые дела
        /// </summary>
        public void CloseAll()
        {
            var listToday = GetToDoListToday();
            foreach (ToDo item in listToday)
            if (item.Status == ToDoStatus.OPEN) 
                    ChangeStatus(item.Name);
            this.Save();
        }
#region Приватные методы
        /// <summary>
        /// Метод присваивает статус "Открыто" всем делам в списке
        /// </summary>
        private void OpenAll()
        {
            var listToday = GetToDoListToday();
            foreach (ToDo item in listToday)
                if (item.Status != ToDoStatus.OPEN)
                    item.Status = ToDoStatus.OPEN;
            this.Save();
        }

        /// <summary>
        /// Метод осуществляет валидацию по атрибутам
        /// </summary>
        /// <exception cref="ValidateException"></exception>
        private static void AttributValidate(ToDo value)
        {
            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(value, context, results, true))
            {
                foreach (ValidationResult error in results)
                {
                    throw new ValidateException(value, error.ErrorMessage!);
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

        private static void SortList(List<ToDo> list)
        {
            list.Sort();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Number = i + 1;
            }
        }

        /// <summary>
        /// Метод возвращает абсолютный путь файла с данными репозитория
        /// </summary>
        private string FileNameDataSet() => Path.GetFullPath($"{UserName}.json");
        
        /// <summary>
        /// Метод сериализует данные репозитория в json-файл 
        /// </summary>
        private void Save()
        {
            var json = JsonConvert.SerializeObject(_toDoListMap, Formatting.Indented);
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
        #endregion
    }
}