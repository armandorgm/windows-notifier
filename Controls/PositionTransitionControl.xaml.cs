using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WindowsNotifier.Models;

namespace WindowsNotifier.Controls
{
    public partial class PositionTransitionControl : UserControl
    {
        public PositionTransitionControl()
        {
            InitializeComponent();
        }

        public void UpdatePosition(OrderEvent orderEvent)
        {
            // Textos
            SymbolText.Text = $"{orderEvent.Symbol} | {orderEvent.Side} {orderEvent.OrderStatus}";
            BeforeSizeText.Text = $"Antes: {orderEvent.PositionBefore.Size} ({orderEvent.PositionBefore.Direction})";
            AfterSizeText.Text = $"Después: {orderEvent.PositionAfter.Size} ({orderEvent.PositionAfter.Direction})";
            FloatingText.Text = $"Posición Flotante Restante: {orderEvent.PositionAfter.Size} {orderEvent.PositionAfter.Direction}";

            // Determinar color de la barra (Verde para compra/aumento largo, Rojo para venta/reducción)
            var isReduction = orderEvent.PositionAfter.Size < orderEvent.PositionBefore.Size;

            if (orderEvent.OrderStatus == "CANCELED" || orderEvent.OrderStatus == "EXPIRED")
            {
                AfterBar.Fill = new SolidColorBrush(Color.FromRgb(100, 100, 100)); // Gris si se canceló
            }
            else if (isReduction)
            {
                AfterBar.Fill = new SolidColorBrush(Color.FromRgb(255, 75, 75)); // Rojo al reducir/cerrar parcial
            }
            else
            {
                AfterBar.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 204)); // Celeste/Verde al abrir/aumentar
            }

            // Animar el ancho de las barras de posición usando el contenedor de UI como referencia de tamaño
            double totalWidth = this.ActualWidth > 0 ? this.ActualWidth - 20 : 380;
            double maxExpectedSize = Math.Max(orderEvent.PositionBefore.Size, orderEvent.PositionAfter.Size);
            if (maxExpectedSize <= 0) maxExpectedSize = 1.0; // Evitar división por cero

            double beforeWidth = (orderEvent.PositionBefore.Size / maxExpectedSize) * totalWidth;
            double afterWidth = (orderEvent.PositionAfter.Size / maxExpectedSize) * totalWidth;

            // Animaciones suaves
            var beforeAnimation = new DoubleAnimation(beforeWidth, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var afterAnimation = new DoubleAnimation(afterWidth, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            BeforeBar.BeginAnimation(WidthProperty, beforeAnimation);
            AfterBar.BeginAnimation(WidthProperty, afterAnimation);
        }
    }
}
