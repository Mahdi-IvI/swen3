# Document Management System

Team project for the Software Engineering 3 course.

## Team

- Mahdi Abbasi
- Marko Lakic
- Kenan Sabic

## Requirements

- .NET SDK 10
- Docker with Docker Compose

Docker Compose runs the API and PostgreSQL together. The API is available on host port **5176** and PostgreSQL on host port **5433**.

## Run with Docker Compose

From the repository root, set a JWT signing secret of at least 32 characters, then start both containers:

```bash
export DSM_JWT_SECRET="$(openssl rand -hex 32)"
docker compose up --build
```

The API is at `http://localhost:5176` and Swagger UI is at `http://localhost:5176/swagger`. Run `docker compose down` to stop the containers; the database volume remains available for the next run.

## Run the API locally

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
