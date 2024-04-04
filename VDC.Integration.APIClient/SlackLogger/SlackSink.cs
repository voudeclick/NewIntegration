using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace VDC.Integration.APIClient.SlackLogger
{
    public class SlackSink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;
        private readonly Uri _webhookUrl;
        private readonly string _channel;
        private readonly string _username;

        public SlackSink(string webhookUrl, string channel, string username, ITextFormatter formatter)
        {
            _formatter = formatter;
            _webhookUrl = new Uri(webhookUrl);
            _channel = channel;
            _username = username;
        }

        public void Emit(LogEvent logEvent)
        {
            var buffer = new StringWriter(new StringBuilder(256));
            _formatter.Format(logEvent, buffer);
            string message = buffer.ToString();
            Payload payload = new Payload()
            {
                Channel = _channel,
                Username = _username,
                Text = message
            };
            string payloadJson = JsonConvert.SerializeObject(payload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;
                var response = client.UploadValues(_webhookUrl, "POST", data);
            }
        }
    }

    public static class SlackSinkExtensions
    {
        const string DefaultConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        public static LoggerConfiguration SlackSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string webhookUrl,
                  string channel,
                  string username,
                  string outputTemplate = DefaultConsoleOutputTemplate,
                  IFormatProvider formatProvider = null,
                  LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            var formatter = new Serilog.Formatting.Display.MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return loggerConfiguration.Sink(new SlackSink(webhookUrl, channel, username, formatter), restrictedToMinimumLevel);
        }
    }

    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
