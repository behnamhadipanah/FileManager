# FileManager

Multi-tenant file storage API built with ASP.NET Core (.NET 10). Applications register once, receive a token, and manage folders/files in isolation. Images convert to WebP and videos to WebM via background workers. Object storage uses RustFS (S3-compatible) with per-application buckets; metadata lives in SQL Server. API messages are localized (Persian and English) via the Kootam Translator.

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
| `FileManager.Api` | HTTP API, auth filters, Scalar docs, request localization |
| `FileManager.Application` | CQRS handlers, validators, services, translation behaviors |
| `FileManager.Contracts` | Request/response DTOs |
| `FileManager.Domain` | Aggregates, value objects, policies, domain message keys |
| `FileManager.Infrastructure` | SQL Server, RustFS, converters, workers, translator seed data |
| `tests/*` | Unit and integration tests |

## Configuration

Defaults live in `src/FileManager.Api/appsettings.json`:

| Section | Purpose |
|---------|---------|
| `SqlServer` | Connection string + `AutoMigrate` (runs migrations and seeds admin on startup) |
| `RustFs` | S3 endpoint, credentials, path-style addressing, public URL override |
| `Jwt` | Signing key, issuer, audience, token lifetimes |
| `Translator` | Default/fallback culture, SQL-backed translations table, cache reload interval |

### SqlServer

| Key | Description |
|-----|-------------|
| `ConnectionString` | Primary write connection |
| `ReadConnectionString` | Optional read replica (falls back to `ConnectionString` when null) |
| `AutoMigrate` | Run embedded SQL migrations and seed admin on startup |

### RustFs

| Key | Description |
|-----|-------------|
| `ServiceUrl` | S3 endpoint the API uses to read/write objects (may be an internal Docker hostname) |
| `PublicServiceUrl` | Browser-reachable base URL for `publicUrl` links. When empty, `ServiceUrl` is used (fine for local dev on the host) |
| `AccessKey` / `SecretKey` | S3 credentials |
| `ForcePathStyle` | Path-style addressing (`http://host/bucket/key`). Default `true` for RustFS |
| `FilesBucket` / `ThumbnailsBucket` | Legacy defaults in config; runtime storage uses per-application buckets (see below) |

### Translator

| Key | Description |
|-----|-------------|
| `DefaultCulture` | Default locale (`fa-IR`) |
| `FallbackCulture` | Fallback when a key is missing (`en-US`) |
| `AutoCreateSqlTable` | Create the `Translations` table on startup |
| `UseCaching` | Cache translations in memory |
| `ReloadDataIntervalInMinuts` | Cache reload interval |
| `TableName` / `SchemaName` | SQL table location (`dbo.Translations`) |

The translator reuses the `SqlServer:ConnectionString` when no dedicated connection is set. Domain validation and error messages (`domain.*` keys) are seeded in English and Persian on first run.

Override with environment variables using `__` nesting, for example:

```bash
export SqlServer__ConnectionString="Server=127.0.0.1,1533;Database=FileManager;User Id=sa;Password=...;TrustServerCertificate=True;"
export RustFs__ServiceUrl="http://localhost:9100"
export RustFs__PublicServiceUrl="http://localhost:9100"
export RustFs__AccessKey="admin"
export RustFs__SecretKey="qwe123!@#"
export Translator__DefaultCulture="en-US"
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
docker compose -f docker/docker-compose.yml up --build -d
docker compose -f docker/docker-compose.yml logs -f filemanager-api
docker compose -f docker/docker-compose.yml down
```

Stack services:

| Service | Host port | Notes |
|---------|-----------|-------|
| API | `8080` | Image installs FFmpeg + curl; auto-migrates DB and seeds admin |
| SQL Server | `1433` | SA password `Your_password123` |
| RustFS S3 API | `9000` | Access key `rustfsadmin` / secret `rustfsadmin123` |
| RustFS console | `9001` | Storage UI |

The API waits until SQL Server is healthy, then starts with:

- `SqlServer__AutoMigrate=true`
- `RustFs__ServiceUrl=http://rustfs:9000` (internal Docker network)
- `RustFs__PublicServiceUrl` set to a browser-reachable RustFS URL (host-mapped S3 port `9000` in compose)
- RustFS path-style addressing
- JWT settings matching `appsettings.json`

Build context is the repo root (`docker/FileManager.Api.Dockerfile`) and uses local packages from `./nugets`. `.dockerignore` keeps build context lean.

Compose credentials differ from local `appsettings.json` (ports `1533` / `9100`). Default seeded admin remains `admin@admin.com` / `admin`.

## Localization

Supported cultures: `fa-IR` (default), `en-US`.

Send the desired language on any request:

```http
Accept-Language: en-US
```

Validation errors, domain exceptions, and CQRS failure messages are translated through the Kootam Translator pipeline. Message keys use the `domain.*` prefix (for example `domain.file-not-found`). Defaults are seeded into SQL; you can extend or override entries in the `Translations` table.

Direct controller responses (such as download `404` bodies) are also localized.

## Object storage and buckets

Each registered application gets its own S3 buckets, isolated by application name and file type. Buckets are created automatically when an application registers and on first upload.

### Bucket naming

Application names are sanitized (lowercase, non-alphanumeric → `-`) and combined with a type suffix:

| File type | Files bucket | Thumbnails bucket |
|-----------|--------------|---------------------|
| Image | `{app}-images` | `{app}-images-thumbnail` |
| Video | `{app}-videos` | `{app}-videos-thumbnail` |
| Document | `{app}-documents` | — |
| Audio | `{app}-audio` | — |
| Archive | `{app}-archives` | — |
| Other | `{app}-files` | — |

Example: application `MyApp` stores images in `myapp-images` and thumbnails in `myapp-images-thumbnail`.

### Public read access

When a bucket is created, a read-only S3 policy is applied so objects can be fetched directly without going through the API. Completed file metadata includes a `publicUrl` field pointing at the object in RustFS.

Path-style URL format (default):

```
{PublicServiceUrl}/{bucket}/{objectKey}
```

Use `RustFs:PublicServiceUrl` when the API talks to RustFS on an internal hostname (Docker) but clients need a host-accessible link.

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

Response includes `applicationId`, `token`, and `rootFolderId`. S3 buckets for the application are provisioned at registration time.

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

When upload completes, the file resource includes `publicUrl` for direct RustFS access (when the bucket policy allows it).

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

Alternatively, use `publicUrl` from the file metadata for direct object storage access.

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
| `POST` | `/api/applications/register` | Register tenant app (provisions S3 buckets) |
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
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}` | File metadata (includes `publicUrl`) |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/content` | Inline file stream (preview) |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/thumbnail` | Thumbnail WebP stream |
| `GET` | `/api/manage/applications/{id}/files/{fileBusinessId}/download` | Download file attachment |
| `GET` | `/api/manage/applications/{id}/trash/items` | Browse trash |

Full interactive docs: Scalar at `/scalar/v1` when running in Development.

## File metadata (`StorageFileResponse`)

| Field | Description |
|-------|-------------|
| `businessId` | Stable file identifier |
| `name` / `mimeType` / `sizeBytes` / `contentHash` | File properties |
| `parentFolderBusinessId` | Parent folder (null for root) |
| `fileType` | Enum: Image, Video, Document, Audio, Archive, Unknown |
| `conversionStatus` / `uploadStatus` / `thumbnailStatus` | Processing state |
| `isDeleted` / `deletionTime` / `creationTime` | Lifecycle |
| `publicUrl` | Direct RustFS URL when upload is completed; null while pending |

## Tests

```bash
dotnet test FileManager.slnx
```

## Media processing notes

- Images (except SVG) convert to WebP in the background.
- Videos convert to WebM (`libvpx` + `libvorbis`); thumbnails are extracted with FFmpeg then saved as WebP.
- Upload status lifecycle: `Pending` → processing → `Completed` (or failed). Poll the file resource after upload.
- Thumbnails for images and videos are stored in a separate bucket per application (`{app}-{type}-thumbnail`).
