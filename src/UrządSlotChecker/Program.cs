using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PuppeteerSharp;
using Newtonsoft.Json;

namespace UrządSlotChecker
{
    class Program
    {
        private static readonly Config _config;
        private static readonly string[] _lackOfSlotsMessages;

        static Program()
        {
            var configStr = File.ReadAllText("Config.json");
            _config = JsonConvert.DeserializeObject<Config>(configStr);

            _lackOfSlotsMessages = new[]
            {
                _config.LackOfSlotsMessage1,
                _config.LackOfSlotsMessage2
            };
        }

        static async Task Main(string[] args)
        {
            // var browserFetcher = new BrowserFetcher();
            // await browserFetcher.DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                ExecutablePath = _config.BrowserPath
            });
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = _config.PageWidth,
                Height = _config.PageHeight
            });

            while (true)
            {
                try
                {
                    await CheckAsync(page);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.Now} {e}");
                }

                await Task.Delay(_config.CheckIntervalInSeconds * 1000);
            }
        }

        private static async Task CheckAsync(IPage page)
        {
            await page.GoToAsync(_config.URL);

            var loadingOptions = new WaitForSelectorOptions
            {
                Visible = true
            };

            var visitTypeSelector = $"#Operacja2 div:nth-of-type({_config.VisitTypeNumber}) button";
            await page.WaitForSelectorAsync(visitTypeSelector, loadingOptions);
            await Task.Delay(100);
            var visitType = await page.QuerySelectorAsync(visitTypeSelector);
            await visitType.ClickAsync();

            var nextButtonSelector = "#form-wizard .wizard-card-footer button";
            await page.WaitForSelectorAsync(nextButtonSelector, loadingOptions);
            await Task.Delay(100);
            var nextButton = await page.QuerySelectorAsync(nextButtonSelector);
            await nextButton.ClickAsync();

            var errorMessageSelector = "#Dataiczas3 h5";
            await page.WaitForSelectorAsync(errorMessageSelector, loadingOptions);
            await Task.Delay(1000);
            var messageHandle = await page.QuerySelectorAsync(errorMessageSelector);
            if (messageHandle != null)
            {
                var messageTextHandle = await messageHandle.GetPropertyAsync("innerText");
                var message = (await messageTextHandle.JsonValueAsync())?.ToString();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (_lackOfSlotsMessages.All(m => !message.Contains(m)))
                    {
                        var dateTime = DateTime.Now;
                        Alert(dateTime);
                        Directory.CreateDirectory(_config.ScreenshotsDir);
                        await page.ScreenshotAsync(
                            $"{_config.ScreenshotsDir}/{dateTime.Year}.{dateTime.Month}.{dateTime.Day}.png");
                    }
                }
            }
        }

        private static void Alert(DateTime dateTime)
        {
            var fromAddress = new MailAddress(_config.FromEmail);
            var toAddress = new MailAddress(_config.ToEmail);

            var smtp = new SmtpClient
            {
                Host = _config.SmtpHost,
                Port = _config.SmtpPort,
                EnableSsl = _config.SmtpEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _config.FromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = _config.EmailSubject,
                Body = _config.EmailContent
            })
            {
                smtp.Send(message);
            }

            Console.WriteLine($"{dateTime} Alert!");
        }
    }
}
