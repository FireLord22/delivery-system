DeliverySystem — production-ready серверная система на основе ASP.NET Core Web API с поддержкой контейнеризации, кэширования, мониторинга и CI/CD. В комплекте — графическое WPF-приложение для работы с данными через REST API с разграничением ролей пользователя и администратора.
Цель лабораторной работы - разработать и развернуть полноценную серверную систему с использованием современного стека технологий, включающего контейнеризацию, реляционную базу данных, кэширование, мониторинг и автоматизацию сборки, а также разработать графическое клиентское приложение на C# с применением делегатов и событий.

Задачи
  Изучить принципы контейнеризации с использованием Docker и Docker Compose
  Разработать RESTful API на платформе ASP.NET Core для предметной области (вариант 18)
  Развернуть PostgreSQL в контейнере и подключить через Entity Framework Core
  Реализовать кэширование часто запрашиваемых данных с использованием Redis
  Настроить обратный прокси Nginx для маршрутизации входящих запросов
  Подключить систему мониторинга на основе Prometheus и Grafana
  Создать пайплайн непрерывной интеграции в GitHub Actions
  Разработать WPF-клиент с применением делегатов и событий для взаимодействия с API

Технологический стек

<img width="735" height="671" alt="image" src="https://github.com/user-attachments/assets/c7c911cf-2e17-4c51-817c-57d224385010" />

Структура проекта

<img width="201" height="737" alt="VLJRQi9047qN_8Tz5rFw1ugnKAWWNjhdDHdZw6Odx6wy8FttRgmaEsbIa83dp9oPd9dCn3ffN9chuS1AvOQ3-t0Wn7rJFOKOYHGKdi7SnlPc7UGlJtmaeWpTeTpLm4XSo9psYYwsmKOYmUXMYJ-pugVSjC0WSqRQ6L8A3CjTokGaCx0D7IKsu6SAGRj-cag3NhAFNr7f-iJ5WloRTtNJrK" src="https://github.com/user-attachments/assets/24e1539a-73a4-4399-aa70-bdf6a3bc7127" />

Требования
  Docker Desktop
  .NET SDK 8.0 (для запуска WPF-клиента)

Swagger UI http://localhost/swagger
Grafana http://localhost:3000
Prometheus http://localhost:9090
Метрики API http://localhost:8080/metrics

user user123 Просмотр и создание посылок
admin admin123 Полный доступ

Рубцов Э.Д.
ТвГТУ — Б.ИВТ.ПРОМ.23.01
