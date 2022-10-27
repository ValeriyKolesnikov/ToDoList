
using Telegram.Bot;
using ToDoListBot;


var token = "5617761400:AAHwY1vz-0tHsjcsHUNfBdV_YD5UnOeRemE";
var botClient = new TelegramBotClient(token);
var bot = await botClient.GetMeAsync();
var botService = new TelegramBotService(botClient);

try
{
    Console.WriteLine($"Запущен телеграм-бот @{bot.Username}");
    botService.WorkingWithRepository(botClient);
}

catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
