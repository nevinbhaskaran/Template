#!/bin/bash

# PSP RabbitMQ Management Script
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$PROJECT_DIR/docker-compose.yml"

# Function to check if Docker is running
check_docker() {
    if ! docker info >/dev/null 2>&1; then
        echo "❌ Docker is not running!"
        echo "📱 Please start Docker Desktop first:"
        echo "   1. Open Docker Desktop application"
        echo "   2. Wait for it to start"
        echo "   3. Try again"
        exit 1
    fi
}

case "$1" in
    start)
        echo "🚀 Starting PSP RabbitMQ..."
        check_docker
        docker-compose -f "$COMPOSE_FILE" up -d
        if [ $? -eq 0 ]; then
            echo "✅ RabbitMQ started successfully!"
            echo "📊 Management UI: http://localhost:15672 (admin/admin123)"
            echo "🔌 AMQP Port: localhost:5672"
            echo ""
            echo "💡 Tip: Access the management UI to monitor queues and exchanges"
        else
            echo "❌ Failed to start RabbitMQ"
        fi
        ;;
    stop)
        echo "🛑 Stopping PSP RabbitMQ..."
        check_docker
        docker-compose -f "$COMPOSE_FILE" stop
        echo "✅ RabbitMQ stopped"
        ;;
    restart)
        echo "🔄 Restarting PSP RabbitMQ..."
        check_docker
        docker-compose -f "$COMPOSE_FILE" restart
        echo "✅ RabbitMQ restarted"
        ;;
    status)
        echo "📋 PSP RabbitMQ Status:"
        check_docker
        docker-compose -f "$COMPOSE_FILE" ps
        ;;
    logs)
        echo "📝 PSP RabbitMQ Logs:"
        check_docker
        docker-compose -f "$COMPOSE_FILE" logs -f rabbitmq
        ;;
    clean)
        echo "🧹 Cleaning up PSP RabbitMQ (removes data)..."
        check_docker
        read -p "This will delete all RabbitMQ data. Are you sure? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            docker-compose -f "$COMPOSE_FILE" down -v
            echo "✅ RabbitMQ cleaned up"
        else
            echo "❌ Operation cancelled"
        fi
        ;;
    check-docker)
        echo "🔍 Checking Docker status..."
        if docker info >/dev/null 2>&1; then
            echo "✅ Docker is running"
            docker --version
        else
            echo "❌ Docker is not running"
            echo "📱 Please start Docker Desktop"
        fi
        ;;
    *)
        echo "PSP RabbitMQ Management Script"
        echo ""
        echo "Usage: $0 {start|stop|restart|status|logs|clean|check-docker}"
        echo ""
        echo "Commands:"
        echo "  start        - Start RabbitMQ container"
        echo "  stop         - Stop RabbitMQ container"
        echo "  restart      - Restart RabbitMQ container"
        echo "  status       - Show RabbitMQ container status"
        echo "  logs         - Show RabbitMQ logs (real-time)"
        echo "  clean        - Stop and remove RabbitMQ container and data"
        echo "  check-docker - Check if Docker Desktop is running"
        echo ""
        echo "⚠️  Docker Desktop must be running before using this script"
        echo ""
        echo "After starting, access:"
        echo "  Management UI: http://localhost:15672 (admin/admin123)"
        echo "  AMQP Port: localhost:5672"
        exit 1
        ;;
esac
