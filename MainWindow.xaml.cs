using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WindowsNotifier.Models;
using WindowsNotifier.Services;

namespace WindowsNotifier
{
    public partial class MainWindow : Window
    {
        private LocalReceiverService? _receiver;
        private DatabaseService? _dbService;
        private AppSettings _settings = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            ConfigureOverlayPosition();
        }

        private void LoadSettings()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null) _settings = loaded;
                }
                catch { }
            }
            _dbService = new DatabaseService(_settings);
        }

        private void ConfigureOverlayPosition()
        {
            // Posicionar la ventana arriba a la derecha de la pantalla principal
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = screenWidth - this.Width - 10;
            this.Top = 40; // 40px debajo del borde superior
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _receiver = new LocalReceiverService(_settings.Port);
            _receiver.OnOrderEventReceived += OnNewOrderEvent;
            _receiver.Start();
        }

        private void OnNewOrderEvent(OrderEvent orderEvent)
        {
            // Ejecutar en el hilo de la UI (WPF)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Actualizar Textos
                TitleText.Text = $"{orderEvent.Side} | {orderEvent.Symbol} ({orderEvent.OrderStatus})";
                InstanceText.Text = $"INSTANCIA #{orderEvent.InstanceId}";
                ReasonText.Text = $"Razón: {orderEvent.Reason}";

                // Configurar color de estado
                switch (orderEvent.OrderStatus)
                {
                    case "NEW":
                        StatusIndicator.Background = new SolidColorBrush(Color.FromRgb(255, 204, 0)); // Amarillo
                        break;
                    case "PARTIALLY_FILLED":
                        StatusIndicator.Background = new SolidColorBrush(Color.FromRgb(0, 150, 255)); // Azul
                        break;
                    case "FILLED":
                        StatusIndicator.Background = new SolidColorBrush(Color.FromRgb(0, 255, 100)); // Verde
                        break;
                    default:
                        StatusIndicator.Background = new SolidColorBrush(Color.FromRgb(150, 150, 150)); // Gris
                        break;
                }

                // Si es un Take Profit, buscar el precio de entrada en la Base de Datos para calcular la ganancia a percibir
                var isTp = orderEvent.Reason.Contains("TP", StringComparison.OrdinalIgnoreCase) || 
                           orderEvent.Reason.Contains("TAKE_PROFIT", StringComparison.OrdinalIgnoreCase);

                if (isTp && _dbService != null)
                {
                    double? entryPrice = _dbService.GetEntryPriceForTakeProfit(orderEvent.OrderId, string.Empty);
                    if (entryPrice.HasValue && entryPrice.Value > 0)
                    {
                        double diff = orderEvent.Price - entryPrice.Value;
                        // Si era un Short, la ganancia es inversa
                        if (orderEvent.Side.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                        {
                            diff = entryPrice.Value - orderEvent.Price;
                        }
                        double profit = diff * orderEvent.Qty;
                        EstimatedProfitText.Text = $"${profit:F2} USD";
                        ProfitContainer.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ProfitContainer.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    ProfitContainer.Visibility = Visibility.Collapsed;
                }

                // Actualizar barra de posición
                PositionVisualizer.UpdatePosition(orderEvent);

                // Ejecutar animación de entrada (FadeIn)
                var fadeIn = (Storyboard)FindResource("FadeInStoryboard");
                fadeIn.Begin(NotificationCard);

                // Auto-ocultar la notificación después de 15 segundos si no se reciben nuevas actualizaciones
                AutoDismissNotification(15000);
            }));
        }

        private int _currentDismissId = 0;
        private async void AutoDismissNotification(int delayMs)
        {
            int localId = ++_currentDismissId;
            await Task.Delay(delayMs);
            if (localId == _currentDismissId)
            {
                var fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
                fadeOut.Begin(NotificationCard);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _receiver?.Stop();
        }
    }
}