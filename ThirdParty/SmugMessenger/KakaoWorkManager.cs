using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace SmugMessenger
{
    public class KakaoWorkManager : IMessenger
    {
        private ILogger<KakaoWorkManager> _logger;
        private HttpClient _httpClient;

        public KakaoWorkManager(ILogger<KakaoWorkManager> logger)
        {
            _logger = logger;
        }

        public void InitMessenger(MessengerConfig msgConfig)
        {
            _logger.LogInformation("KakaoWorkManager initialized.");

            try
            {
                var key = msgConfig.KakaoAPIKey;
                if (key == null) throw new Exception("카카오 API 키 정보가 없어요");

                //KakaoLists = jsonConfig.GetJArray<KakaoMessageConfig>("KakaoMessages");
                //if (KakaoLists == null || KakaoLists == null) throw new Exception("Json에서 메세지 찾을 수 없음.");

                _httpClient = new();
                _httpClient.BaseAddress = new Uri("https://api.kakaowork.com");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KakaoWorkManager initialization failed.");
                throw;
            }

            // Initialize Kakao Work Messenger here
            // This is a placeholder for the actual implementation
            // You can add your own logic to initialize the Kakao Work Messenger
            // For example, setting up API keys, configuring settings, etc.

            // MessengerConfig.json is a placeholder for the actual configuration file
            // You can replace it with the actual file name or path
            // string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MessengerConfig.json");
            // if (File.Exists(configFilePath))
            // {
            //     string json = File.ReadAllText(configFilePath);
            //     var config = JsonConvert.DeserializeObject<MessengerConfig>(json);
            // Use the config object to initialize the messenger

            // msgConfig.KakaoAPIKey
        }

        public void SimpleMessage()
        {
        }
    }

    public class KakaoMessage
    {
        public string Text { get; set; }
        //public string[] Buttons { get; set; }
        //public string[] Images { get; set; }
        //public string[] Files { get; set; }
        //public string[] Links { get; set; }
        //public string[] Mentions { get; set; }
        //public string[] QuickReplies { get; set; }
        //public string[] CardButtons { get; set; }
        //public string[] CardImages { get; set; }
        //public string[] CardFiles { get; set; }
        //public string[] CardLinks { get; set; }
        //public string[] CardMentions { get; set; }
        //public string[] CardQuickReplies { get; set; }
    }
}