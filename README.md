# Document Management System

Team project for the Software Engineering 3 course.

## Team

- Mahdi Abbasi
- Marko Lakic
- Kenan Sabic

## Requirements

- .NET SDK 10
- Docker with Docker Compose

Docker Compose runs the frontend, API, PostgreSQL, RabbitMQ and a placeholder OCR worker together. The frontend is available on host port **4200**, the API on host port **5176**, PostgreSQL on host port **5433**, and the RabbitMQ management UI on **15672**. Port 80 remains available for other local services such as PNW.

In Development mode, the API creates a demo account on startup if it does not already exist: **admin** / **admin123**. 

## Run with Docker Compose

From the repository root, set a JWT signing secret of at least 32 characters, then start the containers:

```bash
export JWT_SECRET="$(openssl rand -hex 32)"
docker compose up --build
```

Generate a secret with `openssl rand -hex 32`, then paste it after `DSM_JWT_SECRET=` in `.env`. Keep the same value between runs. The `.env` file is ignored by Git; only the empty `.env.example` template is committed.

Start the containers with `docker compose up --build`. Compose reads `.env` automatically and passes `DSM_JWT_SECRET` to the API as `Jwt__SecretKey`. An exported `DSM_JWT_SECRET` overrides the value in `.env`. Compose reports an error if the secret is missing or empty; the API requires at least 32 characters.

The frontend is at `http://localhost:4200`, the API is at `http://localhost:5176` and Swagger UI is at `http://localhost:5176/swagger`. Run `docker compose down` to stop the containers; the database volume remains available for the next run.

## Run the API locally

From the repository root:

1. Create `c#/server/Api/appsettings.Local.json` from the example.
2. In the new file, replace the PostgreSQL password placeholder with the password in `docker-compose.yml` (`dsmpassword` for the provided development setup). Replace the JWT secret placeholder with a secret of at least 32 characters. The local settings file is ignored by Git.
3. Ensure the repository-root `.env` file is configured as described above, then start PostgreSQL:
4. Start PostgreSQL and RabbitMQ:

   ```bash
   docker compose up -d postgres rabbitmq
   ```

4. Start the API in Development mode:

   ```bash
   dotnet run --project 'c#/server/Api/Api.csproj' --launch-profile http
   ```

Use `c#/server/Api/server.http` for example requests: register or log in, copy the returned JWT into `@token`, and copy created document and folder IDs into the variables at the top. Stop the Compose API before running the API locally because both use port 5176.

Run the tests with:

```bash
dotnet test 'c#/server/server.sln'
```

## API overview

- `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `PUT /api/auth/me`
- `GET`, `POST`, `PUT`, and `DELETE` under `/api/documents`; `GET /api/documents/search?term=...`
- `GET`, `POST`, `PUT`, and `DELETE` under `/api/folders`

Document and folder requests require `Authorization: Bearer <token>`. A folder can have a `containingFolder` ID to place it under another folder owned by the same user. A document can also have a `containingFolder` ID for one of its owner's folders, or `null` to remain outside folders. Document requests currently store metadata, including `fileName`.

RabbitMQ and placeholder OCR worker

After a document is saved, the API publishes a JSON message with `messageId`, `documentId` and `fileName` to the durable `ocr-jobs` queue through RabbitMQ's default exchange. The worker logs receipt and acknowledges the message manually. Actual file upload, file storage and OCR are added in a later sprint.

RabbitMQ's management UI is at `http://localhost:15672` (development login: `dsmuser` / `dsmpassword`). API and worker settings can be overridden with `RabbitMQ__HostName`, `RabbitMQ__Port`, `RabbitMQ__UserName`, `RabbitMQ__Password` and `RabbitMQ__QueueName`. Defaults connect to `localhost:5672`; Compose sets the host to `rabbitmq`.