using Microsoft.Extensions.Logging;

namespace SmugMessenger
{
    public class KakaoWorkManager : IMessenger
    {
        private ILogger<KakaoWorkManager> _logger;

        public KakaoWorkManager(ILogger<KakaoWorkManager> logger)
        {
            _logger = logger;
        }

        public void InitMessenger()
        {
            _logger.LogInformation("KakaoWorkManager initialized.");
            // Initialize Kakao Work Messenger here
            // This is a placeholder for the actual implementation
            // You can add your own logic to initialize the Kakao Work Messenger
            // For example, setting up API keys, configuring settings, etc.
        }
    }
}