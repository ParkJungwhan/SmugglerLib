using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;

namespace TestWindow.Views
{
    public class DashBoardWindowModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();

        public DashBoardWindowModel()
        {
            InitializeData();
            StartTimer();
        }

        #region Properties

        private DateTime _currentTime = DateTime.Now;

        public DateTime CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(); }
        }

        private double _cpuUsage = 0;

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuColor));
            }
        }

        private double _memoryUsage = 0;

        public double MemoryUsage
        {
            get => _memoryUsage;
            set
            {
                _memoryUsage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryColor));
            }
        }

        private double _diskUsage = 0;

        public double DiskUsage
        {
            get => _diskUsage;
            set
            {
                _diskUsage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiskColor));
            }
        }

        public Color CpuColor => GetColorByUsage(CpuUsage);
        public Color MemoryColor => GetColorByUsage(MemoryUsage);
        public Color DiskColor => GetColorByUsage(DiskUsage);

        public int CpuCoreCount { get; set; } = 8;
        public string MemoryInfo { get; set; } = "16GB / 32GB";
        public string DiskInfo { get; set; } = "512GB / 1TB";
        public string NetworkDownload { get; set; } = "125.3 MB/s";
        public string NetworkUpload { get; set; } = "45.7 MB/s";
        public string NetworkAdapter { get; set; } = "Intel Ethernet";

        public ObservableCollection<ServerInfo> ServerList { get; set; }
        public ObservableCollection<AlertInfo> RecentAlerts { get; set; }

        #endregion Properties

        private void InitializeData()
        {
            // 서버 목록 초기화
            ServerList = new ObservableCollection<ServerInfo>
            {
                new ServerInfo { ServerName = "Web-Server-01", IpAddress = "192.168.1.101", Status = "Online", ResponseTime = "15ms", CpuUsage = 25.5, MemoryUsage = 68.2, LastCheck = DateTime.Now },
                new ServerInfo { ServerName = "DB-Server-01", IpAddress = "192.168.1.102", Status = "Online", ResponseTime = "8ms", CpuUsage = 45.8, MemoryUsage = 78.9, LastCheck = DateTime.Now },
                new ServerInfo { ServerName = "API-Server-01", IpAddress = "192.168.1.103", Status = "Warning", ResponseTime = "142ms", CpuUsage = 89.2, MemoryUsage = 65.1, LastCheck = DateTime.Now },
                new ServerInfo { ServerName = "Cache-Server-01", IpAddress = "192.168.1.104", Status = "Online", ResponseTime = "5ms", CpuUsage = 12.3, MemoryUsage = 34.7, LastCheck = DateTime.Now },
                new ServerInfo { ServerName = "File-Server-01", IpAddress = "192.168.1.105", Status = "Offline", ResponseTime = "Timeout", CpuUsage = 0, MemoryUsage = 0, LastCheck = DateTime.Now.AddMinutes(-5) },
                new ServerInfo { ServerName = "Mail-Server-01", IpAddress = "192.168.1.106", Status = "Online", ResponseTime = "22ms", CpuUsage = 33.6, MemoryUsage = 52.8, LastCheck = DateTime.Now }
            };

            // 알림 목록 초기화
            RecentAlerts = new ObservableCollection<AlertInfo>
            {
                new AlertInfo { Title = "높은 CPU 사용률", Message = "API-Server-01에서 CPU 사용률이 90%를 초과했습니다.", Severity = "Critical", Timestamp = DateTime.Now.AddMinutes(-2) },
                new AlertInfo { Title = "서버 연결 실패", Message = "File-Server-01에 연결할 수 없습니다.", Severity = "Critical", Timestamp = DateTime.Now.AddMinutes(-5) },
                new AlertInfo { Title = "메모리 사용률 경고", Message = "DB-Server-01의 메모리 사용률이 75%를 초과했습니다.", Severity = "Warning", Timestamp = DateTime.Now.AddMinutes(-8) },
                new AlertInfo { Title = "디스크 공간 부족", Message = "Web-Server-01의 디스크 공간이 85%를 초과했습니다.", Severity = "Warning", Timestamp = DateTime.Now.AddMinutes(-12) },
                new AlertInfo { Title = "네트워크 지연", Message = "API-Server-01의 응답 시간이 100ms를 초과했습니다.", Severity = "Info", Timestamp = DateTime.Now.AddMinutes(-15) }
            };
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 현재 시간 업데이트
            CurrentTime = DateTime.Now;

            // 시뮬레이션된 메트릭 업데이트
            CpuUsage = Math.Max(0, Math.Min(100, CpuUsage + _random.Next(-5, 6)));
            MemoryUsage = Math.Max(0, Math.Min(100, MemoryUsage + _random.Next(-3, 4)));
            DiskUsage = Math.Max(0, Math.Min(100, DiskUsage + _random.Next(-2, 3)));

            // 네트워크 속도 시뮬레이션
            NetworkDownload = $"{_random.Next(80, 200)}.{_random.Next(0, 10)} MB/s";
            NetworkUpload = $"{_random.Next(20, 80)}.{_random.Next(0, 10)} MB/s";
            OnPropertyChanged(nameof(NetworkDownload));
            OnPropertyChanged(nameof(NetworkUpload));

            // 서버 상태 업데이트
            foreach (var server in ServerList)
            {
                server.CpuUsage = Math.Max(0, Math.Min(100, server.CpuUsage + _random.Next(-10, 11)));
                server.MemoryUsage = Math.Max(0, Math.Min(100, server.MemoryUsage + _random.Next(-5, 6)));
                server.LastCheck = DateTime.Now;

                if (server.Status != "Offline")
                {
                    server.ResponseTime = $"{_random.Next(5, 50)}ms";
                }
            }
        }

        private Color GetColorByUsage(double usage)
        {
            if (usage < 50) return Colors.LimeGreen;      // 안전
            if (usage < 75) return Colors.Orange;         // 주의
            return Colors.Red;                            // 위험
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServerInfo : INotifyPropertyChanged
    {
        public string ServerName { get; set; }
        public string IpAddress { get; set; }

        private string _status;

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public Brush StatusColor
        {
            get
            {
                return Status switch
                {
                    "Online" => new SolidColorBrush(Colors.LimeGreen),
                    "Warning" => new SolidColorBrush(Colors.Orange),
                    "Offline" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
        }

        private string _responseTime;

        public string ResponseTime
        {
            get => _responseTime;
            set { _responseTime = value; OnPropertyChanged(); }
        }

        private double _cpuUsage;

        public double CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(); }
        }

        private double _memoryUsage;

        public double MemoryUsage
        {
            get => _memoryUsage;
            set { _memoryUsage = value; OnPropertyChanged(); }
        }

        private DateTime _lastCheck;

        public DateTime LastCheck
        {
            get => _lastCheck;
            set { _lastCheck = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AlertInfo
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime Timestamp { get; set; }

        public Brush SeverityColor
        {
            get
            {
                return Severity switch
                {
                    "Critical" => new SolidColorBrush(Colors.Red),
                    "Warning" => new SolidColorBrush(Colors.Orange),
                    "Info" => new SolidColorBrush(Colors.CornflowerBlue),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
        }
    }
}