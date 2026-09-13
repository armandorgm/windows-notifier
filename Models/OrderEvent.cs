using System;
using System.Text.Json.Serialization;

namespace WindowsNotifier.Models
{
    public class PositionState
    {
        [JsonPropertyName("size")]
        public double Size { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; } = "FLAT"; // LONG, SHORT, FLAT
    }

    public class OrderEvent
    {
        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = "ORDER_UPDATE"; // ORDER_UPDATE, POSITION_UPDATE

        [JsonPropertyName("instance_id")]
        public int InstanceId { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("order_status")]
        public string OrderStatus { get; set; } = "NEW"; // NEW, PARTIALLY_FILLED, FILLED, CANCELED, EXPIRED

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("qty")]
        public double Qty { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; } = "BUY"; // BUY, SELL

        [JsonPropertyName("position_before")]
        public PositionState PositionBefore { get; set; } = new();

        [JsonPropertyName("position_after")]
        public PositionState PositionAfter { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AppSettings
    {
        public string DbType { get; set; } = "SQLite"; // SQLite
        public string ConnectionString { get; set; } = "Data Source=../backend/binance_tracker_v3.db;";
        public int Port { get; set; } = 50005; // Puerto TCP local para recibir eventos
    }
}
