using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmugControl.Views
{
    /// <summary>
    /// WidgetServerView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WidgetServerView : UserControl
    {
        public WidgetServerView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(WidgetServerView), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty ServerStateProperty =
            DependencyProperty.Register(nameof(ServerState), typeof(ServerState), typeof(WidgetServerView), new PropertyMetadata(ServerState.Normal));

        public ServerState ServerState
        {
            get => (ServerState)GetValue(ServerStateProperty);
            set => SetValue(ServerStateProperty, value);
        }

        public static readonly DependencyProperty ServerTypeProperty =
           DependencyProperty.Register(nameof(ServerType), typeof(ServerType), typeof(WidgetServerView), new PropertyMetadata(ServerType.GameServer));

        public ServerType ServerType
        {
            get => (ServerType)GetValue(ServerTypeProperty);
            set => SetValue(ServerTypeProperty, value);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSelected = !IsSelected;
        }
    }

    public enum ServerType
    {
        GameServer,
        LogServer,
        LoginServer,
        MessengerServer,
        BillServer,
    }

    public enum ServerState
    {
        Normal,
        Warning,
        Error
    }
}