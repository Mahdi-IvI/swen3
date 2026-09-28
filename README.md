# Document Management System

Team project for the Software Engineering 3 course.

## Team

- Mahdi Abbasi
- Marko Lakic
- Kenan Sabic

## Requirements

- .NET SDK 10
- Docker with Docker Compose

The API runs locally and PostgreSQL runs in Docker. The database is available on host port **5433**.

## Run locally

From the repository root:

1. Create `c#/server/Api/appsettings.Local.json` from the example.
2. In the new file, replace the PostgreSQL password placeholder with the password in `docker-compose.yml` (`dsmpassword` for the provided development setup). Replace the JWT secret placeholder with a secret of at least 32 characters. The local settings file is ignored by Git.
3. Start PostgreSQL:

   ```bash
   docker compose up -d postgres
   ```

4. Start the API in Development mode:

   ```bash
   dotnet run --project 'c#/server/Api/Api.csproj' --launch-profile http
   ```

The API is at `http://localhost:5176`. Swagger UI is at `http://localhost:5176/swagger`. Use `c#/server/Api/server.http` for example requests: register or log in, copy the returned JWT into `@token`, and copy created document and folder IDs into the variables at the top.

Run the tests with:

```bash
dotnet test 'c#/server/server.sln'
```

## API overview

- `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `PUT /api/auth/me`
- `GET`, `POST`, `PUT`, and `DELETE` under `/api/documents`; `GET /api/documents/search?term=...`
- `GET`, `POST`, `PUT`, and `DELETE` under `/api/folders`

Document and folder requests require `Authorization: Bearer <token>`. A folder can have a `containingFolder` ID to place it under another folder owned by the same user. A document can also have a `containingFolder` ID for one of its owner's folders, or `null` to remain outside folders. Document requests currently store metadata, including `fileName`.