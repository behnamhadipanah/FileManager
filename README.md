# FileManager

Multi-tenant file storage API built with ASP.NET Core (.NET 10). Applications register once, receive a token, and manage folders/files in isolation. Images convert to WebP and videos to WebM via background workers. Object storage uses RustFS (S3-compatible); metadata lives in SQL Server.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (2022 recommended)
- [RustFS](https://github.com/rustfs/rustfs) (or any S3-compatible endpoint)
- [FFmpeg](https://ffmpeg.org/) on `PATH` (required for video conversion and thumbnails)
- Docker (optional, for compose-based setup)

Local Kootam NuGet packages ship under `./nugets` and are wired through `nuget.config`.

## Solution layout

| Project | Role |
|---------|------|
| `FileManager.Api` | HTTP API, auth filters, Scalar docs |
| `FileManager.Application` | CQRS handlers, validators, services |
| `FileManager.Contracts` | Request/response DTOs |
| `FileManager.Domain` | Aggregates, value objects, policies |
| `FileManager.Infrastructure` | SQL Server, RustFS, converters, workers |
| `tests/*` | Unit and integration tests |

## Configuration

Defaults live in `src/FileManager.Api/appsettings.json`:

| Section | Purpose |
|---------|---------|
| `SqlServer` | Connection string + `AutoMigrate` (runs migrations and seeds admin on startup) |
| `RustFs` | S3 endpoint, credentials, bucket names |
| `Jwt` | Signing key, issuer, audience, token lifetimes |

Override with environment variables using `__` nesting, for example:

```bash
export SqlServer__ConnectionString="Server=127.0.0.1,1533;Database=FileManager;User Id=sa;Password=...;TrustServerCertificate=True;"
export RustFs__ServiceUrl="http://localhost:9100"
export RustFs__AccessKey="admin"
export RustFs__SecretKey="qwe123!@#"
```

## Run locally

1. Start SQL Server and RustFS so they match `appsettings.json` (default ports `1533` and `9100`).
2. Ensure FFmpeg is installed and available on `PATH`.
3. Restore and run:

```bash
dotnet restore FileManager.slnx
dotnet run --project src/FileManager.Api/FileManager.Api.csproj
```

API: `http://localhost:5066`  
Scalar (Development): `http://localhost:5066/scalar/v1`

On Windows you can also use:

```powershell
./scripts/run.ps1    # start in background
./scripts/stop.ps1   # stop
```

When `SqlServer:AutoMigrate` is `true`, the API migrates the database and seeds the admin user:

| Email | Password |
|-------|----------|
| `admin@admin.com` | `admin` |

## Run with Docker Compose

From the repository root:

```bash
docker compose -f docker/docker-compose.yml up --build
```

This starts SQL Server, RustFS, Elasticsearch, and the API on `http://localhost:8080`.

Compose credentials differ from local `appsettings.json` — see `docker/docker-compose.yml` for ports and secrets.

## Authentication

### Application clients

Register an application, store the returned token, and send it on token-scoped routes:

```http
X-Application-Token: <token-from-registration>
```

Token-scoped routes use the `/api/application/...` prefix (current app metadata, folders, trash, upload limits).

Routes under `/api/applications/{applicationId}/...` take the application id in the path (files, folders, trash, upload limits). Keep the token secret; regenerate via manage API or `POST /api/application/regenerate-token`.

### Admin (Manage) UI

JWT auth for `/api/manage/...` endpoints.

```http
POST /api/manage/auth/login
Content-Type: application/json

{
  "email": "admin@admin.com",
  "password": "admin",
  "rememberMe": false
}
```

Tokens are returned in the response body and set as HTTP-only cookies (`fm_access_token`, `fm_refresh_token`). Use `POST /api/manage/auth/refresh` and `POST /api/manage/auth/logout` as needed.

## Typical client flow

### 1. Register an application

```http
POST /api/applications/register
Content-Type: application/json

{
  "applicationName": "MyApp",
  "minImageSizeKilobytes": 1,
  "maxImageSizeKilobytes": 5120,
  "minVideoSizeKilobytes": 1,
  "maxVideoSizeKilobytes": 102400,
  "minDocumentSizeKilobytes": 1,
  "maxDocumentSizeKilobytes": 20480
}
```

Response includes `applicationId`, `token`, and `rootFolderId`.

### 2. Create a folder

```http
POST /api/applications/{applicationId}/folders
Content-Type: application/json

{
  "name": "Photos",
  "parentFolderBusinessId": null
}
```

### 3. Upload a file

Uploads are queued and return `202 Accepted` with `UploadStatus` = `Pending` (0). Poll `GET` until status is `Completed` (2). Max request body: 500 MB.

```http
POST /api/applications/{applicationId}/files
Content-Type: multipart/form-data

file: <binary>
parentFolderBusinessId: <optional-guid>
```

Batch upload: `POST /api/applications/{applicationId}/files/batch`.

### 4. Download a file

Application clients:

```http
GET /api/applications/{applicationId}/files/{fileBusinessId}/download
```

Admin (JWT):

```http
GET /api/manage/applications/{applicationId}/files/{fileBusinessId}/download
```

Both return the file bytes with `Content-Disposition: attachment` using the stored file name. The file must have `UploadStatus` = Completed. The application download also rejects soft-deleted files.

For inline preview without forcing a download, use the manage content endpoint:

```http
GET /api/manage/applications/{applicationId}/files/{fileBusinessId}/content
GET /api/manage/applications/{applicationId}/files/{fileBusinessId}/thumbnail
```

### 5. Browse with application token

```http
GET /api/application
X-Application-Token: <token>

GET /api/application/folders/contents
X-Application-Token: <token>

GET /api/application/upload-limits
X-Application-Token: <token>
```

### 6. Soft delete / trash / restore

```http
DELETE /api/applications/{applicationId}/files/{fileBusinessId}
POST   /api/applications/{applicationId}/files/{fileBusinessId}/restore
DELETE /api/applications/{applicationId}/files/{fileBusinessId}/permanent

GET    /api/applications/{applicationId}/trash/items
POST   /api/applications/{applicationId}/trash/items
POST   /api/applications/{applicationId}/trash/items/{trashItemId}/restore
DELETE /api/applications/{applicationId}/trash/items/{trashItemId}
```

## API overview

### Public / application-scoped

| Method | Path | Notes |
|--------|------|-------|
| `POST` | `/api/applications/register` | Register tenant app |
| `GET` | `/api/application` | Current app (`X-Application-Token`) |
| `POST` | `/api/application/regenerate-token` | Rotate token |
| `GET` | `/api/application/folders/...` | Browse folders by token |
| `GET` | `/api/application/trash/items` | Browse trash by token |
| `GET` | `/api/application/upload-limits` | Limits by token |
| `*` | `/api/applications/{id}/folders` | Folder CRUD |
| `*` | `/api/applications/{id}/files` | Upload, get, download, delete, restore |
| `GET` | `/api/applications/{id}/files/{fileBusinessId}/download` | Download file attachment |
| `*` | `/api/applications/{id}/trash` | Trash operations |
| `GET` | `/api/applications/{id}/upload-limits` | Limits by application id |

### Manage (JWT)

| Method | Path | Notes |
|--------|------|-------|
| `POST` | `/api/manage/auth/login` | Login |
| `POST` | `/api/manage/auth/refresh` | Refresh tokens |
| `POST` | `/api/manage/auth/logout` | Logout |
| `GET` | `/api/manage/applications` | List applications |
| `GET` | `/api/manage/applications/{id}/upload-limits` | Limits |
| `POST` | `/api/manage/applications/{id}/regenerate-token` | Rotate token |
| `GET` | `/api/manage/applications/{id}/folders/...` | Browse folders |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}` | File metadata |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/content` | Inline file stream (preview) |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/thumbnail` | Thumbnail WebP stream |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/download` | Download file attachment |
| `GET` | `/api/manage/applications/{id}/trash/items` | Browse trash |

Full interactive docs: Scalar at `/scalar/v1` when running in Development.

## Tests

```bash
dotnet test FileManager.slnx
```

## Media processing notes

- Images (except SVG) convert to WebP in the background.
- Videos convert to WebM (`libvpx` + `libvorbis`); thumbnails are extracted with FFmpeg then saved as WebP.
- Upload status lifecycle: `Pending` → processing → `Completed` (or failed). Poll the file resource after upload.
