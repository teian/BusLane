# BusLane.Transport.RabbitMQ

An implementation of the that connects to a [RabbitMQ](https://www.rabbitmq.com/)
broker.

## Configuration

The configuration used to create a consumer or producer reads the follow configuration keys:

```json
{
  "Messaging": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Usage

