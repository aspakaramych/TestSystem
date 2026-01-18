Микросервисная система на .NET для автоматизированного тестирования программного кода.

## Архитектура
Система построена на базе микросервисов, взаимодействующих через REST API и брокер сообщений RabbitMQ.

### Сервисы и порты:
* **Nginx (Gateway):** http://localhost (Порт 80)
* **Auth Service:** http://localhost:8080 — Авторизация и пользователи
* **Classrooms Service:** Управление проектами и аудиториями
* **Task Service:** Управление задачами
* **Package Service:** Обработка пакетов заданий
* **Run Worker:** Выполнение кода (Docker-out-of-Docker)
* **RabbitMQ:** http://localhost:15672 (guest/guest) — Очереди сообщений
* **PostgreSQL:** localhost:5432 — База данных (DB: TestSystem)
* **pgAdmin:** http://localhost:5050 (admin@gmail.com) — Управление БД
* **Prometheus:** http://localhost:9090 — Сбор метрик
* **Grafana:** http://localhost:3000 (admin/admin) — Визуализация метрик

## Предварительные требования
1.  Установленный Docker и Docker Compose.

## Запуск проекта
Для сборки и запуска всех сервисов выполните команду в корне проекта:

```bash
docker-compose up --build -d
