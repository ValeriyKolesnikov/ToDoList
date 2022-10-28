
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ToDoListLibrary;
using System.Timers;

namespace ToDoListBot
{
    /// <summary>
    /// Класс-сервис работы со списком дел через телеграмм-бот
    /// </summary>
    internal class TelegramBotService
    {
        #region Приватные поля
        private Dictionary<long, Marker> _markers;
        private static Dictionary<long, ToDoListRepository>? _repos;
        private Dictionary<long, string> _names;
        private string _menu;
        private static Marker _marker;
        private static DateTime _today;
        private ToDoListRepository? _repository;
        private static string? _list;
        private static List<ToDo>? _toDoList;        
        private static string? _name;
        private static TimeOnly _startTime;
        private static int _number;
        private static System.Timers.Timer? _aTimer;
        private static ITelegramBotClient? _botClient;
        private static long _key;
        const string Number = "Введите порядковый номер дела";
        #endregion

        public TelegramBotService(ITelegramBotClient botClient)
        {
            _today = DateTime.Today;
            _menu = System.IO.File.Exists(@"botMenu.txt") ? System.IO.File.ReadAllText(@"botMenu.txt") : "Список команд не найден";
            _markers = new();
            _repos = new();
            _names = new();
            _toDoList = new();
            _botClient = botClient;
        }


        /// <summary>
        /// Метод для работы с репозиторием списков дел с помощью бота
        /// </summary>
        public void WorkingWithRepository(ITelegramBotClient bot)
        {
            SetTimer();
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadKey();
        }

        /// <summary>
        /// Метод - обработчик ошибок
        /// </summary>
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.ToString());
            botClient.SendTextMessageAsync(_key, "Возникла ошибка");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Метод - обработчик входных данных
        /// </summary>
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            Console.WriteLine(JsonConvert.SerializeObject(update, Formatting.Indented));
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                _key = update.Message.Chat.Id;
                InitializeFields(update);
                switch (_marker)
                {
                    case Marker.IS_MENU:
                        await SelectMenu(update, botClient);
                        break;
                    case Marker.IS_ADDED_TODO:
                        await AddToDo(update, botClient);                        
                        break;
                    case Marker.IS_NAME_INPUT:
                        await InputName(update, botClient);
                        break;
                    case Marker.IS_TIME_INPUT:
                        await InputTime(update, botClient);
                        break;
                    case Marker.IS_DATE_INPUT:
                        await InputDate(update, botClient);
                        break;
                    case Marker.IS_CHANGE_STATUS:
                        await ChangeStatus(update, botClient);
                        break;
                    case Marker.IS_DELETED:
                        await DeleteToDo(update, botClient);
                        break;
                    case Marker.IS_CANCELLED:
                        await CancelToDo(update, botClient);
                        break;
                    default: 
                        return;
                }
                _markers[_key] = _marker;
            }
            return;
        }


        /// <summary>
        /// Метод приводит поля в соответсвие с текущим пользователем
        /// </summary>
        private void InitializeFields(Update update)
        {
            if (_markers.ContainsKey(_key) && _repos!.ContainsKey(_key))
            {
                _marker = _markers[_key];
                _repository = _repos[_key];
                _toDoList = _repository.GetList(_today).ToList();
                if (_names.ContainsKey(_key))
                    _name = _names[_key];
            }
            else
            {
                _marker = Marker.IS_MENU;
                _markers.Add(_key, _marker);
                var userName = update.Message?.Chat.Username;
                if (userName == null)
                    userName = _key.ToString();
                _repository = new ToDoListRepository(userName);
                _repos!.Add(_key, _repository);
            }
        }

        /// <summary>
        /// Метод возвращает отформатированный список дел на заданную дату для вывода в чате
        /// </summary>
        private static string GetList(DateTime date, ToDoListRepository repo)
        {
            var list = string.Join(' ', repo.GetList(date));
            if (string.IsNullOrEmpty(list))
                return "Cписок пуст";
            return list;
        }

        /// <summary>
        /// Метод для добавления нового дела в список
        /// </summary>
        private async Task AddToDo(Update update, ITelegramBotClient botClient)
        {
            string message = update?.Message?.Text!;
            if (message.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(_key, "Введите название дела");
                _marker = Marker.IS_NAME_INPUT;
                return;
            } 
            else if (message.Equals("n", StringComparison.InvariantCultureIgnoreCase))
            {
                _marker = Marker.IS_MENU;
                _repository!.AddList(_today, _toDoList!);
                await botClient.SendTextMessageAsync(_key, "Добавлен (обновлен) список дел");
                return;
            }
            else 
                await botClient.SendTextMessageAsync(_key, "Неверный ввод. Повторите ввод");
            return;
        }

        /// <summary>
        /// Метод для работы со списком основных команд в меню бота
        /// </summary>
        private async Task SelectMenu(Update update, ITelegramBotClient botClient)
        {
            if (update!.Message!.Text!.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                                new KeyboardButton[] { "/start", "/print","/addlist", "/add", "/copy" },
                                new KeyboardButton[] { "/old", "/status", "/allclose", "/notodo", "/del" }
                            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(_key, _menu, replyMarkup: keyboard);
                return;
            }
            if (update.Message.Text.Equals("/print", StringComparison.InvariantCultureIgnoreCase))
            {
                _list = GetList(DateTime.Today, _repository!);
                await botClient.SendTextMessageAsync(_key, _list);
                return;
            }
            if (update.Message.Text.Equals("/addlist", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, "Создан список дел.\nДобавить новое дело в список ? y / n");
                _toDoList = new();
                _marker = Marker.IS_ADDED_TODO;
                return;
            }
            if (update.Message.Text.Equals("/add", StringComparison.InvariantCultureIgnoreCase))
            {
                _toDoList = _repository!.GetList(_today).ToList();
                await botClient.SendTextMessageAsync(update.Message.Chat, "Введите наименование дела");
                _marker = Marker.IS_NAME_INPUT;
                return;
            }
            if (update.Message.Text.Equals("/old", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, "Введите дату в формате \"dd.MM.yyyy\"");
                _marker = Marker.IS_DATE_INPUT;
                return;
            }
            if (update.Message.Text.Equals("/copy", StringComparison.InvariantCultureIgnoreCase))
            {
                _repository!.AddListAsYesterday();
                await botClient.SendTextMessageAsync(update.Message.Chat, "Добавлен список дел на сегодня");
                return;
            }
            if (update.Message.Text.Equals("/allclose", StringComparison.InvariantCultureIgnoreCase))
            {
                _repository!.CloseAll();
                await botClient.SendTextMessageAsync(update.Message.Chat, "Cписок дел на сегодня закрыт");
                return;
            }
            if (update.Message.Text.Equals("/status", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, Number);
                _marker = Marker.IS_CHANGE_STATUS;
                return;
            }
            if (update.Message.Text.Equals("/del", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, Number);
                _marker = Marker.IS_DELETED;
                return;
            }
            if (update.Message.Text.Equals("/cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, Number);
                _marker = Marker.IS_CANCELLED;
                return;
            }
        }

        /// <summary>
        /// Метод для ввода названия дела через бот
        /// </summary>
        private async Task InputName(Update update, ITelegramBotClient botClient)
        {
            _name = update.Message?.Text;
            await botClient.SendTextMessageAsync(_key, "Введите время начала в формате \"HH:mm\"");
            _marker = Marker.IS_TIME_INPUT;
            _names[_key] = _name!;
            return;
        }

        /// <summary>
        /// Метод для ввода времени начала дела через бот
        /// </summary>
        private async Task InputTime(Update update, ITelegramBotClient botClient)
        {
            var input = update.Message?.Text;
            if (TimeOnly.TryParseExact(input, "HH:mm", out TimeOnly time))
            {
                _startTime = time;
                _toDoList!.Add(new ToDo(_name!, _startTime));
                _repository!.AddToDo(new ToDo(_name!, _startTime));
                _marker = Marker.IS_ADDED_TODO;
                await botClient.SendTextMessageAsync(_key, "Дело добавлено. Добавить новое дело в список y / n?");
            }
            else
                await botClient.SendTextMessageAsync(_key, "Неверный формат времени\nВведите время начала в формате \"HH:mm\"");
            return;
        }

        /// <summary>
        /// Метод для ввода даты через бот
        /// </summary>
        private async Task InputDate(Update update, ITelegramBotClient botClient)
        {
            var input = update.Message?.Text;
            if (DateTime.TryParse(input, out DateTime date))
            {
                string list = GetList(date, _repository!);
                await botClient.SendTextMessageAsync(_key, list);
                _marker = Marker.IS_MENU;
            }
            else
                await botClient.SendTextMessageAsync(_key, "Неверный формат даты\nВведите дату в формате \"dd.MM.yyyy\"");
            return;
        }

        /// <summary>
        /// Метод для ввода порядкого номера через бот
        /// </summary>
        private static async Task InputNumber(Update update, ITelegramBotClient botClient)
        {
            var input = update.Message?.Text;
            if (int.TryParse(input, out _number) && _number <= _toDoList!.Count && _number >= 0)
            {
                return;
            }
            else
                await botClient.SendTextMessageAsync(_key, "Нет дела с таким порядковым номером. Введите другой номер");
            return;
        }

        /// <summary>
        /// Метод для изменения статуса дела
        /// </summary>
        private async Task ChangeStatus(Update update, ITelegramBotClient botClient)
        {
            await InputNumber(update, botClient);
            if (_number > _toDoList!.Count())
                return;
            var name = _toDoList![_number - 1].Name;
            _repository!.ChangeStatus(name);
            await botClient.SendTextMessageAsync(_key, $"Изменен статус дела \"{name}\".");
            _marker = Marker.IS_MENU;
        }
        
        /// <summary>
         /// Метод удаляет дело из списка
         /// </summary>
        private async Task DeleteToDo(Update update, ITelegramBotClient botClient)
        {
            await InputNumber(update, botClient);
            if (_number > _toDoList!.Count())
                return;
            var name = _toDoList![_number - 1].Name;
            _repository!.Delete(name);
            await botClient.SendTextMessageAsync(_key, $"Удалено дело \"{name}\".");
            _marker = Marker.IS_MENU;
        }

        /// <summary>
        /// Метод помечает дело как "Не буду выполнять"
        /// </summary>
        private async Task CancelToDo(Update update, ITelegramBotClient botClient)
        {
            await InputNumber(update, botClient);
            if (_number > _toDoList!.Count())
                return;
            var name = _toDoList![_number - 1].Name;
            _repository!.CancelToDo(name);
            await botClient.SendTextMessageAsync(_key, $"Дело \"{name}\" помечено как \"Не буду выполнять\".");
            _marker = Marker.IS_MENU;
        }

        #region Методы для создания уведомлений
        private static void SetTimer()
        {
            _aTimer = new System.Timers.Timer(60000);
            _aTimer.Elapsed += OnTimedEvent!;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var timeNow = DateTime.Now;
            var date = DateOnly.FromDateTime(timeNow);
            var timeSpan = TimeSpan.FromMinutes(1);
            IEnumerable<ToDo> list;
            foreach (var repo in _repos!)
            {
                list = repo.Value.GetList(_today);
                foreach (ToDo toDo in list)
                {
                    var timeDifference = e.SignalTime - date.ToDateTime(toDo.StartTime);
                    if (timeDifference < timeSpan && timeDifference > TimeSpan.Zero)
                        _botClient!.SendTextMessageAsync(repo.Key, $"Напоминание: {toDo.StartTime} {toDo.Name}");
                }
            }
        }
        #endregion
    }
}
