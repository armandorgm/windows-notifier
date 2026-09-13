import socket
import json
import time

def send_mock_events():
    port = 50005
    print(f"Connecting to Windows Notifier on port {port}...")
    
    # Intentar conectar con reintentos
    s = None
    for i in range(5):
        try:
            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            s.connect(('127.0.0.1', port))
            print("Connected successfully!")
            break
        except Exception as e:
            print(f"Connection attempt {i+1} failed: {e}. Retrying in 2 seconds...")
            time.sleep(2)
            
    if not s:
        print("Could not connect to the C# application overlay.")
        return

    # 1. Simular colocación de una orden ENTRY (NEW)
    print("Sending ENTRY placed (NEW) event...")
    entry_event = {
        "event_type": "ORDER_UPDATE",
        "instance_id": 104,
        "symbol": "BTCUSDT",
        "order_id": "1111111",
        "order_status": "NEW",
        "reason": "MACD Golden Cross Signal Detected",
        "price": 60000.0,
        "qty": 1.0,
        "side": "BUY",
        "position_before": {"size": 0.0, "direction": "FLAT"},
        "position_after": {"size": 0.0, "direction": "FLAT"}
    }
    s.sendall((json.dumps(entry_event) + "\n").encode('utf-8'))
    time.sleep(0.5)

    # 2. Simular ejecución parcial de la orden ENTRY (PARTIALLY_FILLED)
    print("Sending ENTRY partially filled event...")
    partial_event = {
        "event_type": "ORDER_UPDATE",
        "instance_id": 104,
        "symbol": "BTCUSDT",
        "order_id": "1111111",
        "order_status": "PARTIALLY_FILLED",
        "reason": "MACD Golden Cross Signal Detected",
        "price": 60000.0,
        "qty": 1.0,
        "side": "BUY",
        "position_before": {"size": 0.0, "direction": "FLAT"},
        "position_after": {"size": 0.4, "direction": "LONG"}
    }
    s.sendall((json.dumps(partial_event) + "\n").encode('utf-8'))
    time.sleep(0.5)

    # 3. Simular ejecución completa de la orden ENTRY (FILLED)
    print("Sending ENTRY filled event...")
    filled_event = {
        "event_type": "ORDER_UPDATE",
        "instance_id": 104,
        "symbol": "BTCUSDT",
        "order_id": "1111111",
        "order_status": "FILLED",
        "reason": "MACD Golden Cross Signal Detected",
        "price": 60000.0,
        "qty": 1.0,
        "side": "BUY",
        "position_before": {"size": 0.4, "direction": "LONG"},
        "position_after": {"size": 1.0, "direction": "LONG"}
    }
    s.sendall((json.dumps(filled_event) + "\n").encode('utf-8'))
    time.sleep(3)

    # 4. Simular colocación de la orden de salida Take Profit (TP_PLACED / NEW)
    print("Sending Take Profit (NEW) event...")
    tp_event = {
        "event_type": "ORDER_UPDATE",
        "instance_id": 104,
        "symbol": "BTCUSDT",
        "order_id": "2222222",
        "order_status": "NEW",
        "reason": "Take Profit Target Placed (TP)",
        "price": 61500.0,
        "qty": 1.0,
        "side": "SELL",
        "position_before": {"size": 1.0, "direction": "LONG"},
        "position_after": {"size": 1.0, "direction": "LONG"}
    }
    s.sendall((json.dumps(tp_event) + "\n").encode('utf-8'))
    time.sleep(4)

    # 5. Simular ejecución del TP (FILLED y reducción de posición flotante a FLAT)
    print("Sending Take Profit FILLED event...")
    tp_filled_event = {
        "event_type": "ORDER_UPDATE",
        "instance_id": 104,
        "symbol": "BTCUSDT",
        "order_id": "2222222",
        "order_status": "FILLED",
        "reason": "Take Profit Target Reached (TP)",
        "price": 61500.0,
        "qty": 1.0,
        "side": "SELL",
        "position_before": {"size": 1.0, "direction": "LONG"},
        "position_after": {"size": 0.0, "direction": "FLAT"}
    }
    s.sendall((json.dumps(tp_filled_event) + "\n").encode('utf-8'))
    
    s.close()
    print("Finished sending simulated mock events!")

if __name__ == "__main__":
    send_mock_events()
