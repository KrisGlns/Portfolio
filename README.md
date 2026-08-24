# Portfolio — Christos Galanis

A personal portfolio site for a .NET developer, built end to end in C#: a **Blazor WebAssembly**
front end, an **ASP.NET Core minimal API** behind the contact form, and a shared class library that
holds the resume as strongly-typed data.

> Live site: _add your GitHub Pages URL here once the first deploy finishes_

<!-- 📖 **[PROJECT-GUIDE.md](PROJECT-GUIDE.md)** — the full tech stack, local setup, and a step-by-step
walkthrough for publishing the site for free on GitHub Pages. -->

## Why it's built this way

The site has to do two things that pull in opposite directions: be a free-to-host static page, and
show that I write real .NET. So the resume lives in `Portfolio.Shared` as compiled-in data, the API
serves it over `GET /api/resume`, and the client falls back to the compiled copy whenever the API is
unreachable. The result deploys to GitHub Pages as pure static files and still works — the only
feature that needs the backend is the contact form, which hides itself and offers a mailto instead.

## Solution layout

```
Portfolio.slnx
├─ src/Portfolio.Shared      Resume model + data, contact DTOs. Referenced by both ends.
├─ src/Portfolio.Api         ASP.NET Core minimal API: /api/resume, /api/contact, /api/health
└─ src/Portfolio.Web         Blazor WebAssembly standalone app (the site itself)
```

| Project | Notable pieces |
| --- | --- |
| `Portfolio.Shared` | `ResumeData.Current` — the single source of truth for every word on the site. `ContactRequest` carries the DataAnnotations used for validation on **both** sides of the wire. |
| `Portfolio.Api` | Endpoint groups in `Endpoints/`, a file-backed `ContactMessageStore` (JSON Lines), per-IP fixed-window rate limiting, configurable CORS, OpenAPI in development. |
| `Portfolio.Web` | `PortfolioApiClient` with graceful degradation, `ThemeService` over JS interop, one global stylesheet (`wwwroot/css/app.css`), scroll-reveal and scroll-spy via `IntersectionObserver` in `wwwroot/js/site.js`. |

## Running it locally

```bash
# Terminal 1 — the API (http://localhost:5221)
dotnet run --project src/Portfolio.Api

# Terminal 2 — the site (http://localhost:5122)
dotnet run --project src/Portfolio.Web
```

`src/Portfolio.Web/wwwroot/appsettings.Development.json` already points the client at
`http://localhost:5221/`, and the API's development CORS policy already allows the client's ports.

Skip the API entirely if you only care about the site — it renders the same content from
`ResumeData` and swaps the contact form for a mailto card.

### Running the API in Docker

```bash
docker build -f src/Portfolio.Api/Dockerfile -t portfolio-api .
docker run -p 8080:8080 -v portfolio-data:/app/data portfolio-api
```

## Editing the content

Everything the site displays comes from
[`src/Portfolio.Shared/ResumeData.cs`](src/Portfolio.Shared/ResumeData.cs) — experience, education,
skills and their meter levels, the hero stats, and the contact links. Change it there and both the
API and the client pick it up; no markup to touch.

The downloadable CV is `src/Portfolio.Web/wwwroot/files/Christos-Galanis-CV.pdf`. Replace the file
and keep the name, or update `Profile.CvUrl`.

## Deployment

**The site → GitHub Pages.** `.github/workflows/deploy-pages.yml` publishes the Blazor app on every
push to `main`. It rewrites `<base href>` to the repository sub-path, copies `index.html` to
`404.html` so deep links work without an SPA fallback, and drops a `.nojekyll` file so the
`_framework` directory survives. Enable it once under **Settings → Pages → Source → GitHub Actions**.

**The API → anywhere that runs a container.** Azure App Service, Render, Fly.io, a VPS. Then:

1. Set `Cors:AllowedOrigins` on the API to your Pages URL.
2. Set `Api.BaseUrl` in `src/Portfolio.Web/wwwroot/appsettings.json` to the API's public URL.
3. Push — the contact form appears on the next deploy.

Contact submissions are appended to `data/messages.jsonl` next to the API. Mount a volume if you
want them to outlive a redeploy, or swap `ContactMessageStore` for email or a database.

## Configuration reference

| Setting | Where | Default | Purpose |
| --- | --- | --- | --- |
| `Api:BaseUrl` | `Portfolio.Web/wwwroot/appsettings*.json` | empty | API root. Empty ⇒ fully static, contact form hidden. |
| `Cors:AllowedOrigins` | `Portfolio.Api/appsettings*.json` | empty | Allowed browser origins. Empty ⇒ any origin (fine locally, tighten in production). |
| `Contact:StorePath` | `Portfolio.Api/appsettings*.json` | `./data/messages.jsonl` | Where contact messages are appended. |

## Built with

.NET 10 · Blazor WebAssembly · ASP.NET Core Minimal APIs · vanilla CSS (no framework) · GitHub Actions
