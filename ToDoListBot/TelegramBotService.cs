
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ToDoListLibrary;

namespace ToDoListBot
{
    internal class TelegramBotService
    {
        private static string _list;
        private static List<ToDo> _toDoList;
        private static string _menu;
        private static ToDoListRepository _repository;
        private static Marker _marker;
        private static ToDo _todo;
        private static string _name;
        private static TimeOnly _startTime;
        private static DateTime _today;

        public TelegramBotService()
        {
            _today = DateTime.Today;
            _repository = new ToDoListRepository();
            _list = GetList(DateTime.Today);
            _menu = System.IO.File.ReadAllText(@"botMenu.txt");
            _marker = Marker.IS_MENU;
            _toDoList = new List<ToDo>();
        }

        private static string GetList(DateTime date)
        {
            var list = string.Join(Environment.NewLine, _repository.GetList(date));
            if (string.IsNullOrEmpty(list))
                return "Cписок пуст";
            return list;
        }

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

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }


        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            Console.WriteLine(JsonConvert.SerializeObject(update, Formatting.Indented));
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
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
                    default: 
                        return;
                }
//                await botClient.SendTextMessageAsync(update.Message.Chat, "Введите команду");
//                return;
            }
            return;
        }

        private static async Task AddToDo(Update update, ITelegramBotClient botClient)
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
                _list = GetList(DateTime.Today);
                await botClient.SendTextMessageAsync(update.Message.Chat, "Добавлен (обновлен) список дел");
                return;
            }
        }

        private static async Task SelectMenu(Update update, ITelegramBotClient botClient)
        {
            if (update.Message.Text.Equals("/start", StringComparison.InvariantCultureIgnoreCase))
            {
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                                new KeyboardButton[] {"/start", "/print"},
                                new KeyboardButton[] {"/addlist", "/add", "/old"}
                            })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(update.Message.Chat, _menu, replyMarkup: keyboard);
                return;
            }
            if (update.Message.Text.Equals("/print", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, _list);
                return;
            }
            if (update.Message.Text.Equals("/addlist", StringComparison.InvariantCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(update.Message.Chat, "Создан список дел.\nДобавить новое дело в список ? y / n");
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
        }

        private static async Task InputName(Update update, ITelegramBotClient botClient)
        {
            _name = update.Message.Text;
            await botClient.SendTextMessageAsync(update.Message.Chat, "Введите время начала в формате \"HH:mm\"");
            _marker = Marker.IS_TIME_INPUT;
            return;
        }

        private static async Task InputTime(Update update, ITelegramBotClient botClient)
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
    }
}
