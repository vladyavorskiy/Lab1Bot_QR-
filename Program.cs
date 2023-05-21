using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.PortableExecutable;
using System;
using Telegram.Bot.Requests.Abstractions;

using Flurl;
using Flurl.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using static System.Net.WebRequestMethods;
using Telegram.Bot.Types.InputFiles;
using System.Linq;
using System.Threading.Tasks;
using Svg;
using System.Drawing.Imaging;
using Telegram.Bot.Types.Passport;
using System.Xml;
using Telegram.Bot.Args;

namespace ConsoleAppi
{

    internal class Program
    {
        static string site;
        static string color;
        static string er;
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient("6108067259:AAHTogeQhZnmPNGPRrru1ucPG272GLoYSas");


            using CancellationTokenSource cts = new();
         
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

             

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                

                var keyboard = new ReplyKeyboardMarkup(new[]
        {
                    // Создание первой строки кнопок
                    new[]
                    {
                        new KeyboardButton("Создать")
                       
                    },
                    // Создание второй строки кнопок
                    new[]
                    {
                        new KeyboardButton("Цвет"),
                        new KeyboardButton("Исправление ошибок")
                    }
                });
                // Отправка сообщения с кнопками


              

                var sentMessage = update.Message;


                if (sentMessage.Text != null)
                {
                    if (sentMessage.Text.Contains("http://") && sentMessage.Text.Contains(".") || sentMessage.Text.Contains("https://") && sentMessage.Text.Contains("."))
                    {
                        site = sentMessage.Text;
                        await botClient.SendTextMessageAsync(chatId, "Выберите настройки:", replyMarkup: keyboard);
                        color = "#000000";
                        er = "M";
                    }

                    else if (update.Message.Text == "Создать")
                    {
                        await botClient.SendTextMessageAsync(sentMessage.Chat.Id, "Это ссылка, получай QR-код");

                        var code = await $"https://online-qr.ru/api/qr-codes".WithOAuthBearerToken("a1f9c40b50957f1f596f99bb0f310dd4").PostUrlEncodedAsync(new { name = "qr", type = "url", url = site, foreground_type = "color", foreground_color = color, ecc = er }).ReceiveJson();
                        var id = code.data.id;

                        var result = await $"https://online-qr.ru/api/qr-codes/{id}".WithOAuthBearerToken("a1f9c40b50957f1f596f99bb0f310dd4").AppendPathSegment("").GetJsonAsync<Root>();

                        var filename = Path.GetRandomFileName();
                        //await $"{result.data.qr_code_logo}".DownloadFileAsync(filename);
                        var t = await $"{result.data.qr_code_logo}".GetStringAsync();
                        var x = new XmlDocument();
                        x.LoadXml(t);
                        var svgDocument = SvgDocument.Open(x);
                        using (var smallBitmap = svgDocument.Draw())
                        {
                            var width = smallBitmap.Width;
                            var height = smallBitmap.Height;

                            using (var bitmap = svgDocument.Draw(width, height))//I render again
                            {
                                bitmap.Save(filename + ".png", ImageFormat.Png);
                            }
                        }

                        var fileStream = System.IO.File.Open(filename + ".png", FileMode.Open);
                        await botClient.SendDocumentAsync(sentMessage.Chat.Id, new InputMedia(fileStream, "name"));

                    }

                    else if (update.Message.Text == "Исправление ошибок")
                    {
                        // Создание новой клавиатуры с подзначениями
                        var subKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Низкий"), new KeyboardButton("Средний"),new KeyboardButton("Высокий"),new KeyboardButton("Лучший") },
                            new[] { new KeyboardButton("Назад") }
                        });


                        // Изменение клавиатуры в уже отправленном сообщении
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: subKeyboard);
                    }
                    else if (update.Message.Text == "Назад")
                    {
                        // Возврат к первоначальной клавиатуре
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }

                    else if (update.Message.Text == "Низкий")
                    {
                        er = "L";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }

                    else if (update.Message.Text == "Средний")
                    {
                        er = "M";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }

                    else if (update.Message.Text == "Высокий")
                    {
                        er = "Q";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }

                    else if (update.Message.Text == "Лучший")
                    {
                        er = "H";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }


                    else if (update.Message.Text == "Цвет")
                    {
                        // Создание новой клавиатуры с подзначениями
                        var subKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[] { new KeyboardButton("Красный"), new KeyboardButton("Синий") },
                            new[] { new KeyboardButton("Назад") }
                        });


                        // Изменение клавиатуры в уже отправленном сообщении
                        await botClient.SendTextMessageAsync(chatId, "Выберите цвет:", replyMarkup: subKeyboard);
                    }
                    else if (update.Message.Text == "Назад")
                    {
                        // Возврат к первоначальной клавиатуре
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }


                    else if (update.Message.Text == "Красный")
                    {
                        color = "#FF0000";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }

                    else if (update.Message.Text == "Синий")
                    {
                        color = "#1100FF";
                        await botClient.SendTextMessageAsync(chatId, "Выберите значение:", replyMarkup: keyboard);
                    }



                }
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }

        public class Data
        {
            public int id { get; set; }
            public int link_id { get; set; }
            public int project_id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string qr_code { get; set; }
            public object qr_code_logo { get; set; }
            public Settings settings { get; set; }
            public string last_datetime { get; set; }
            public string datetime { get; set; }
        }

        public class Root
        {
            public Data data { get; set; }
        }

        public class Settings
        {
            public string foreground_type { get; set; }
            public string foreground_color { get; set; }
            public string background_color { get; set; }
            public bool custom_eyes_color { get; set; }
            public int qr_code_logo_size { get; set; }
            public int size { get; set; }
            public int margin { get; set; }
            public string ecc { get; set; }
            public string url { get; set; }
        }
    }
}