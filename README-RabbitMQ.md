# PSP RabbitMQ Setup Guide

This project now uses a persistent RabbitMQ setup that runs independently of the Aspire AppHost.

## Quick Start

### 1. Start RabbitMQ (Required before running the app)
```bash
# Start RabbitMQ container
./rabbitmq.sh start
```

### 2. Run the Application
```bash
# Start the Aspire AppHost (from AppHost directory)
cd src/Orchestration/PSP.AppHost.App
dotnet run
```

### 3. Access Services
- **Aspire Dashboard**: https://localhost:17274
- **RabbitMQ Management UI**: http://localhost:15672 (admin/admin123)
- **Application Logs**: `./Logs/` directory

## RabbitMQ Management Commands

```bash
# Start RabbitMQ
./rabbitmq.sh start

# Check status
./rabbitmq.sh status

# View logs
./rabbitmq.sh logs

# Restart RabbitMQ
./rabbitmq.sh restart

# Stop RabbitMQ
./rabbitmq.sh stop

# Clean up (removes all data)
./rabbitmq.sh clean
```

## Benefits of This Setup

### ✅ **Persistent Storage**
- RabbitMQ data survives container restarts
- Messages are not lost when AppHost stops
- Queue configurations persist

### ✅ **Independent Operation**
- RabbitMQ runs independently of AppHost
- Can restart application without losing messages
- Better for development and debugging

### ✅ **Consistent Configuration**
- Fixed credentials: `admin/admin123`
- Fixed ports: `5672` (AMQP), `15672` (Management)
- Reliable connection strings

### ✅ **Development Workflow**
1. Start RabbitMQ once: `./rabbitmq.sh start`
2. Develop and restart AppHost as needed
3. RabbitMQ keeps running in background
4. Stop RabbitMQ when done: `./rabbitmq.sh stop`

## Connection Details

- **Host**: localhost
- **Port**: 5672 (AMQP)
- **Virtual Host**: /
- **Username**: admin
- **Password**: admin123
- **Management UI**: http://localhost:15672

## Troubleshooting

### RabbitMQ Won't Start
```bash
# Check if Docker Desktop is running
docker info

# Check for port conflicts
lsof -i :5672
lsof -i :15672

# View detailed logs
./rabbitmq.sh logs
```

### Connection Issues
```bash
# Verify RabbitMQ is running
./rabbitmq.sh status

# Check if AppHost can connect
# Look for MassTransit connection logs in application logs
```

### Reset Everything
```bash
# Stop and clean RabbitMQ (removes all data)
./rabbitmq.sh clean

# Start fresh
./rabbitmq.sh start
```

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Aspire Host   │    │   RabbitMQ       │    │   Worker        │
│                 │    │   (Docker)       │    │   Services      │
│   Port: 17274   │◄──►│   Port: 5672     │◄──►│                 │
│                 │    │   Mgmt: 15672    │    │   Consumers     │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌──────────────────┐
                       │   Persistent     │
                       │   Storage        │
                       │   (Volumes)      │
                       └──────────────────┘
```

This setup provides a robust, development-friendly messaging infrastructure that maintains state across application restarts.
