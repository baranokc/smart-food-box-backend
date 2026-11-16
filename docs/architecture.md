## 1. High-Level System Architecture

```mermaid
flowchart LR
    subgraph IoT["IoT Hardware Layer"]
        BOX["Smart Food Box\n(Temperature, Humidity, Weight Sensors)"]
    end

    subgraph Broker["Messaging Layer (Docker)"]
        MQTT["Eclipse Mosquitto\nMQTT Broker (Port 1883)"]
    end

    subgraph Backend["Application Layer (.NET 10 Web API)"]
        WORKER["MqttTelemetryWorker\n(BackgroundService)"]
        SERVICE["Telemetry & Alert\nEvaluation Service"]
        API["REST API Controllers\n(JSON over HTTP)"]
    end

    subgraph Storage["Persistence Layer (Docker)"]
        DB[("PostgreSQL 16\nDatabase (Port 5433)")]
    end

    subgraph Clients["Client Layer"]
        WEB["Web Dashboard"]
        MOBILE["Mobile App"]
    end

    BOX -- "Publish JSON\nsmartbox/{deviceCode}/telemetry" --> MQTT
    MQTT -- "Subscribe\nsmartbox/+/telemetry" --> WORKER
    WORKER --> SERVICE
    SERVICE -- "EF Core\nRead / Write" --> DB
    DB -- "Query Data" --> API
    API <--> WEB
    API <--> MOBILE
```
