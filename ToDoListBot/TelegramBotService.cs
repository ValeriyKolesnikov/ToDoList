
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ToDoListLibrary;

namespace ToDoListBot
{
    /// <summary>
    /// Класс-сервис работы со списком дел через телеграмм-бот
    /// </summary>
    internal class TelegramBotService
    {
        #region Приватные поля
        private Dictionary<long, Marker> _markers;
        private Dictionary<long, ToDoListRepository> _repos;
        private Dictionary<long, string> _names;
        private Dictionary<long, List<ToDo>> _toDoLists;
        private static string _menu;
        private static Marker _marker;
        private static DateTime _today;
        private ToDoListRepository _repository;
        private string _list;
        private static List<ToDo> _toDoList;        
        private static string _name;
        private static TimeOnly _startTime;
        #endregion

        public TelegramBotService()
        {
            _today = DateTime.Today;
            _menu = System.IO.File.ReadAllText(@"botMenu.txt");
            _markers = new();
            _repos = new();
            _toDoLists = new();
            _names = new();
            _toDoList = new();
        }

        /// <summary>
        /// Метод возвращает отформатированный список дел на заданную дату для вывода в чате
        /// </summary>

        private string GetList(DateTime date, ToDoListRepository repo)
        {
            var list = string.Join(Environment.NewLine, repo.GetList(date));
            if (string.IsNullOrEmpty(list))
                return "Cписок пуст";
            return list;
        }

        /// <summary>
        /// Метод для работы с репозиторием списков дел с помощью бота
        /// </summary>
        /// <param name="bot"></param>
        public void WorkingWithRepository(ITelegramBotClient bot)
        {
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
                var key = update.Message.Chat.Id;
                await InitializeFields(update, key);
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
                    default: 
                        return;
                }
                _markers[key] = _marker;
            }
            return;
        }
              
        /// <summary>
        /// Метод для добавления нового дела в список
        /// </summary>
        private async Task AddToDo(Update update, ITelegramBotClient botClient)
        {
            if (update.Message.Text.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, "Введите название дела");
                _marker = Marker.IS_NAME_INPUT;
                return;
            }
            if (update.Message.Text.Equals("n", StringComparison.InvariantCultureIgnoreCase))
            {
                _marker = Marker.IS_MENU;
                _repository.AddList(_today, _toDoList);
                await botClient.SendTextMessageAsync(update.Message.Chat, "Добавлен (обновлен) список дел");
                return;
            }
        }

        /// <summary>
        /// Метод для работы со списком основных команд в меню бота
        /// </summary>
        private async Task SelectMenu(Update update, ITelegramBotClient botClient)
        {
            if (update.Message.Text.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                                new KeyboardButton[] {"/start", "/print","/addlist"},
                                new KeyboardButton[] {"/copy", "/add", "/old"}
                            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(update.Message.Chat, _menu, replyMarkup: keyboard);
                return;
            }
            if (update.Message.Text.Equals("/print", StringComparison.InvariantCultureIgnoreCase))
            {
                _list = GetList(DateTime.Today, _repository);
                await botClient.SendTextMessageAsync(update.Message.Chat, _list);
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
                _toDoList = _repository.GetList(_today).ToList();
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
                _repository.AddListAsYesterday();
                await botClient.SendTextMessageAsync(update.Message.Chat, "Добавлен список дел на сегодня");

                return;
            }

        }

        /// <summary>
        /// Метод для ввода названия дела через бот
        /// </summary>
        private async Task InputName(Update update, ITelegramBotClient botClient)
        {
            _name = update.Message.Text;
            await botClient.SendTextMessageAsync(update.Message.Chat, "Введите время начала в формате \"HH:mm\"");
            _marker = Marker.IS_TIME_INPUT;
            return;
        }

        /// <summary>
        /// Метод для ввода времени начала дела через бот
        /// </summary>
        private async Task InputTime(Update update, ITelegramBotClient botClient)
        {
            var input = update.Message.Text;
            if (TimeOnly.TryParse(input, out TimeOnly time))
            {
                _startTime = time;
                _toDoList.Add(new ToDo(_name, _startTime));
                _marker = Marker.IS_ADDED_TODO;
                await botClient.SendTextMessageAsync(update.Message.Chat, "Дело добавлено. Добавить новое дело в список y / n?");
            }
            else
                await botClient.SendTextMessageAsync(update.Message.Chat, "Неверный формат времени\nВведите время начала в формате \"HH:mm\"");
            return;
        }

        /// <summary>
        /// Метод для ввода даты через бот
        /// </summary>
        private async Task InputDate(Update update, ITelegramBotClient botClient)
        {
            var input = update.Message.Text;
            if (DateTime.TryParse(input, out DateTime date))
            {
                string list = GetList(date, _repository);
                await botClient.SendTextMessageAsync(update.Message.Chat, list);
                _marker = Marker.IS_MENU;
            }
            else
                await botClient.SendTextMessageAsync(update.Message.Chat, "Неверный формат даты\nВведите дату в формате \"dd.MM.yyyy\"");
            return;
        }

        /// <summary>
        /// Метод приводит поля в соответсвие с текущим пользователем
        /// </summary>
        private async Task InitializeFields(Update update, long key)
        {
            if (_markers.ContainsKey(key))
            {
                _marker = _markers[key];
                _repository = _repos[key];
                if (_toDoLists.ContainsKey(key))
                    _toDoList = _toDoLists[key];
                if (_names.ContainsKey(key))
                    _name = _names[key];
            }
            else
            {
                _marker = Marker.IS_MENU;
                _markers.Add(key, _marker);
                var userName = update.Message.Chat.Username;
                if (userName == null)
                    userName = key.ToString();
                _repository = new ToDoListRepository(userName);
                _repos.Add(key, _repository);
            }
        }
    }
}
