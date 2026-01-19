# Учебный проект Тестирующая система

Микросервисная система на .NET для автоматизированного тестирования программного кода.

## Архитектура
Система построена на базе микросервисов, взаимодействующих через REST API и брокер сообщений RabbitMQ. Nginx является API Gateway который проксирует запросы.

## Используемые технологии:
- Dotnet 9.0
- ASP.NET
- EF CORE
- Dapper
- JWT
- Xunit
- Docker.DotNet
- RabbitMQ
- Docker
- Nginx
- Grafana


### Сервисы: 
* **Nginx (Gateway):** http://localhost
* **Auth Service:** — Авторизация и пользователи
* **Classrooms Service:** Управление проектами и аудиториями
* **Task Service:** Управление задачами
* **Package Service:** Обработка решений заданий
* **Run Worker:** Выполнение кода (Docker-out-of-Docker)
* **RabbitMQ:** http://localhost:15672 (guest/guest) — Очереди сообщений
* **PostgreSQL:** localhost:5432 — База данных (DB: TestSystem)
* **pgAdmin:** http://localhost:5050 (admin@gmail.com) — Управление БД
* **Prometheus:** http://localhost:9090 — Сбор метрик
* **Grafana:** http://localhost:3000 (admin/admin) — Визуализация метрик

## Запуск проекта
Для сборки и запуска всех сервисов нужен Docker и Docker Compose. выполните команду в корне проекта:

```bash
docker-compose up --build -d
```
