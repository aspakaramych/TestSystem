Микросервисная система на .NET для автоматизированного тестирования программного кода.

## Архитектура
Система построена на базе микросервисов, взаимодействующих через REST API и брокер сообщений RabbitMQ.

### Сервисы и порты:
* **Nginx (Gateway):** http://localhost (Порт 80)
* **Auth Service:** — Авторизация и пользователи
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
```

## Auth Service
Эндпоинты аутентификации. Работают без предварительной проверки прав.

### POST `/api/Auth/login`
**Описание:** Вход в систему.
**Request Body:**
| Property | Type | Required |
| -------- | ---- | -------- |
| `email` | string | yes |
| `password` | string | yes |

### POST `/api/Auth/register`
**Описание:** Регистрация.
**Request Body:**
| Property | Type | Required |
| -------- | ---- | -------- |
| `email` | string | yes |
| `password` | string | yes |

---

## Classrooms Service 

### GET `/api/Classrooms/classrooms`
**Access:** `User` | **Headers:** `Authorization`
**Responses:** `200 OK` (List of classrooms)

### POST `/api/Classrooms/classrooms`
**Access:** `Admin` | **Headers:** `Authorization`
**Request Body (`ClassRoomCreateRequest`):**
| Property | Type | Required |
| -------- | ---- | -------- |
| `title` | string | yes |

---

## Task Service 

### GET `/api/TaskService/{classroomId}/tasks`
**Описание:** Получение списка задач для конкретного класса.
**Parameters:**
- `classroomId` (path, string) — ID класса.
- `page` / `pageSize` (query, int) — Пагинация.
**Access:** `User` | **Responses:** `200 OK`.

### POST `/api/TaskService/{classroomId}/tasks`
**Описание:** Создание новой задачи.
**Access:** `Admin Only`
**Parameters:** `classroomId` (path, string).
**Request Body (`TaskRequest`):**
| Property | Type | Req | Description |
| :--- | :--- | :---: | :--- |
| `title` | string | ✅ | Название задачи |
| `description` | string | ✅ | Описание |
| `inputSample` | string | ❌ | Пример входных данных |
| `outputSample` | string | ❌ | Пример выходных данных |
| `tests` | string | ❌ | Код тестов |

Тесты должны быть вида: "[{ \"in\": \"\", \"out\": \"Hello world\" }, ...]"

### GET `/api/TaskService/{classroomId}/tasks/{id}`
**Описание:** Получение детальной информации по конкретной задаче.
**Parameters:** `classroomId` (path), `id` (path).
**Access:** `User` | **Responses:** `200 OK`.

---

## Package Service

### POST `/api/Package/task/{taskId}/send/code`
**Описание:** Отправка кода решения задачи на проверку.
**Access:** `Authorized User`
**Parameters:** `taskId` (path, string).
**Request Body (`PackageRequest`):**
| Property | Type | Req | Description |
| :--- | :--- | :---: | :--- |
| `language` | string | ✅ | Язык программирования |
| `code` | string | ✅ | Исходный код решения |

### GET `/api/Package/packages`
**Описание:** Получение списка отправленных пакетов (решений).
**Access:** `Authorized User`
**Parameters:** `page` / `pageSize` (query, int).