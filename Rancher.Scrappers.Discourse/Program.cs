using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Rancher.Scrappers.Discourse
{
    internal class Program
    {
        private static string _apiUsername;
        private static string _apiKey;
        private static string _discourseServer;
        private static Logger _logger;
        private static HttpClient _client;
        private static Uri _uri;

        private static void Main(string[] args)
        {
            // Configure Structured Logging
            ConfigureLogging();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Starting Rancher Forums Log Scrapper.");

            ConfigureCancelKeyPress(_logger);

            GatherEnvironmentVariables();

            ConfigureHttpClient();

            ScrapeDiscourse();

            while (true)
            {
                _logger.Info("Will scrape again in an hour.");
                Task.Delay(TimeSpan.FromHours(1)).ContinueWith((_ignored) => { ScrapeDiscourse(); });
            }
        }

        private static void ConfigureHttpClient()
        {
            _logger.Info("Configuring HTTP Client.");
            var builder = new UriBuilder
            {
                Scheme = "https",
                Host = _discourseServer,
                Path = $"/admin/users/list/{"active"}.json"
            };
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["api_key"] = _apiKey;
            query["api_username"] = _apiUsername;
            builder.Query = query.ToString();
            _client = new HttpClient();
            _uri = builder.Uri;
        }

        private static void GatherEnvironmentVariables()
        {
            _logger.Info("Grabbing environment variables.");
            try
            {
                _logger.Info("Grabbing api_username.");
                _apiUsername = Environment.GetEnvironmentVariable("api_username");
                _logger.Info("Grabbing api_key.");
                _apiKey = Environment.GetEnvironmentVariable("api_key");
                _logger.Info("Grabbing discourse_server");
                _discourseServer = Environment.GetEnvironmentVariable("discourse_server");
            }
            catch (Exception e)
            {
                _logger.Error(e,
                    "There was a problem grabbing environment variables.  The variables required are 'api_username', 'api_key', and 'discourse_server'");
                throw;
            }
        }

        private static void ConfigureCancelKeyPress(Logger logger)
        {
            // Handle Ctrl+C
            Console.CancelKeyPress += delegate
            {
                logger.Info("Stopping Rancher Forums Log Scrapper.");
                logger.Info("Flushing NLog Log Manager.");
                LogManager.Shutdown();
                Environment.Exit(0);
            };
        }

        private static void ScrapeDiscourse()
        {
            _logger.Info("Scrapping Discourse at {url}, with username {username}", _discourseServer, _apiUsername);
            var stopWatch = Stopwatch.StartNew();
            var response = _client.GetAsync(_uri).Result;

            try
            {
                response.EnsureSuccessStatusCode();
                var users = JsonConvert.DeserializeObject<List<User>>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "There was a problem while attempting to scrape the forums at {url}.",
                    _uri.AbsolutePath);
                throw;
            }

            stopWatch.Stop();

            _logger.Info("Rancher Forums scraped in {elapsedSeconds} seconds.", stopWatch.Elapsed.TotalSeconds);
        }



        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = new JsonLayout
            {
                IncludeAllProperties = true,
                Attributes =
                {
                    new JsonAttribute("time", new SimpleLayout("${longdate}")),
                    new JsonAttribute("level", new SimpleLayout("${level}")),
                    new JsonAttribute("message", new SimpleLayout("${message}")),
                    new JsonAttribute("eventProperties", encode: false,
                        layout: new JsonLayout {IncludeAllProperties = true, MaxRecursionLimit = 2})
                }
            };
            config.AddRuleForAllLevels(consoleTarget, "*");
            LogManager.Configuration = config;
        }
    }
}