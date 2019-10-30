using System.Collections.Generic;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.TelegramNotification.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TelegramNotification
{
    public class Notifier : INotificationService
    {
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;

        public Notifier(ILogger logger, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && IsValid(options) && options.Enabled;
        }

        private TeleGramOptions GetOptions(User user)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.JellyfinUserId, user.Id.ToString("N"), StringComparison.OrdinalIgnoreCase));
        }

        public string Name
        {
            get { return Plugin.Instance.Name; }
        }

        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            var parameters = new Dictionary<string, string>
            {
                {"chat_id", options.ChatID},
                {"text", $"*{request.Name}*\n{request.Description}"},
                {"parse_mode", "Markdown" }
            };

            _logger.LogDebug("TeleGram to Token : {0} - {1} - {2}", options.BotToken, options.ChatID, request.Name);

            var _httpRequest = new HttpRequestOptions
            {
                Url = "https://api.telegram.org/bot" + options.BotToken + "/sendMessage",
                RequestContentType = "application/json",
                BufferContent = false,
                RequestContent = _jsonSerializer.SerializeToString(parameters),
                CancellationToken = cancellationToken,
                LogErrorResponseBody = true,
                EnableKeepAlive = false
            };

            await _httpClient.Post(_httpRequest).ConfigureAwait(false);
        }

        private bool IsValid(TeleGramOptions options)
        {
            return !string.IsNullOrEmpty(options.ChatID) &&
                !string.IsNullOrEmpty(options.BotToken);
        }
    }
}
