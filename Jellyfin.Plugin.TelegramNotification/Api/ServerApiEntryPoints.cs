using System;
using System.Linq;
using System.Threading;
using Jellyfin.Plugin.TelegramNotification.Configuration;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Services;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TelegramNotification.Api
{
    [Route("/Notification/Telegram/Test/{UserID}", "POST", Summary = "Tests Telegram")]
    public class TestNotification : IReturnVoid
    {
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string UserID { get; set; }
    }

    public class ServerApiEndpoints : IService
    {
        private readonly IUserManager _userManager;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public ServerApiEndpoints(
            IUserManager userManager,
            ILogger logger,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer)
        {
            _userManager = userManager;
            _logger = logger;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }
        private TeleGramOptions GetOptions(String userID)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.JellyfinUserId, userID, StringComparison.OrdinalIgnoreCase));
        }

        public Task Post(TestNotification request)
        {
            return new Notifier(_logger, _httpClient, _jsonSerializer).SendNotification(new UserNotification
            {
                Date = DateTime.UtcNow,
                Description = "This is a test notification from Jellyfin Server",
                Level = MediaBrowser.Model.Notifications.NotificationLevel.Normal,
                Name = "Jellyfin: Test Notification",
                User = _userManager.GetUserById(request.UserID)
            }, CancellationToken.None);
        }
    }
}
