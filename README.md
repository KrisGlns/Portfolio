# Portfolio — Christos Galanis

A personal portfolio site for a .NET developer, built end to end in C#: a **Blazor WebAssembly**
front end, an **ASP.NET Core minimal API** behind the contact form, and a shared class library that
holds the resume as strongly-typed data.

> Live site: https://krisglns.github.io/Portfolio/

<!-- 📖 **[PROJECT-GUIDE.md](PROJECT-GUIDE.md)** — the full tech stack, local setup, and a step-by-step
walkthrough for publishing the site for free on GitHub Pages. -->

## Why it's built this way

The site has to do two things that pull in opposite directions: be a free-to-host static page, and
show that I write real .NET. So the resume lives in `Portfolio.Shared` as compiled-in data, the API
serves it over `GET /api/resume`, and the client falls back to the compiled copy whenever the API is
unreachable. The result deploys to GitHub Pages as pure static files and still works — the only
feature that needs the backend is the contact form, which hides itself and offers a mailto instead.

## What the site does

- **One page**: a hero plus the five sections the nav tracks — about, experience timeline, education, skills, contact — all driven from `ResumeData`.
- **Light/dark theme** persisted in `localStorage`, applied by an inline script before first paint so there is no flash of the wrong palette.
- **Scroll-reveal and scroll-spy** via `IntersectionObserver`; both stand down under `prefers-reduced-motion`.
- **CV download** straight from `wwwroot/files/`.
- **Working contact form** — client and server validate the same annotated model, a honeypot absorbs bots, and a successful send replaces the form with a confirmation that echoes back the address the reply will go to.
- **Degrades to a static page.** With no API configured the site still renders in full from the compiled-in resume data and offers a mailto card instead of the form.

## Solution layout

```
Portfolio.slnx
├─ .github/workflows/        ci.yml (build) + deploy-pages.yml (publish the site)
├─ render.yaml               Render Blueprint — the whole API service, declared
├─ .dockerignore             keeps bin/obj and the Blazor project out of the build context
└─ src/
   ├─ Portfolio.Shared       Resume model + data, contact DTOs. Referenced by both ends.
   ├─ Portfolio.Api          ASP.NET Core minimal API: /api/resume, /api/health, /api/contact
   └─ Portfolio.Web          Blazor WebAssembly standalone app (the site itself)
```

| Project | Notable pieces |
| --- | --- |
| `Portfolio.Shared` | `ResumeData.Current` — the single source of truth for every word on the site. `ContactRequest` carries the DataAnnotations used for validation on **both** sides of the wire. |
| `Portfolio.Api` | Endpoint groups in `Endpoints/`, a `ResendEmailSender` that posts to Resend's REST API with no SDK, per-IP fixed-window rate limiting, a form honeypot, configurable CORS, OpenAPI in development. |
| `Portfolio.Web` | `PortfolioApiClient` with graceful degradation, `ThemeService` over JS interop, one global stylesheet (`wwwroot/css/app.css`), and ~120 lines of vanilla JS in `wwwroot/js/site.js` for scroll-reveal, scroll-spy, API warm-up and the contact card's height lock. |

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

To actually send mail locally, supply a Resend key through user secrets rather than a file:

```bash
dotnet user-secrets set "Resend:ApiKey" "re_..." --project src/Portfolio.Api
```

Without a key the endpoint returns `503` and the form says so plainly — it never accepts a message
it cannot deliver.

### Running the API in Docker

```bash
docker build -f src/Portfolio.Api/Dockerfile -t portfolio-api .
docker run -p 8080:8080 -e Resend__ApiKey=re_... portfolio-api
```

No volume: the API writes nothing to disk.

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

**The API → Render, via the blueprint.** `render.yaml` describes the whole service, so
Render → New → Blueprint → this repo is the entire setup. Supply `Resend__ApiKey` when prompted; it
is never committed. Then:

1. Set `Api.BaseUrl` in `src/Portfolio.Web/wwwroot/appsettings.json` to the Render URL.
2. Push — the contact form appears on the next Pages deploy.

The API is **stateless**: messages go straight out through Resend and nothing touches disk, which
suits a free tier with an ephemeral filesystem. If Resend rejects a message the API logs it in full
and returns `502`, so nothing is ever silently dropped.

The container sets `DOTNET_hostBuilder__reloadConfigOnChange=false`. Config is baked into the image
and cannot change at runtime, so the file watchers .NET would otherwise start are pure cost — and
each one consumes an inotify instance, a limit shared across containers that small hosts do run out
of.

Free instances sleep after ~15 minutes. The site pings `/api/health` once when the contact section
scrolls into view, so the container wakes while the visitor is still typing.

## Configuration reference

| Setting | Where | Default | Purpose |
| --- | --- | --- | --- |
| `Api:BaseUrl` | `Portfolio.Web/wwwroot/appsettings*.json` | empty | API root. Empty ⇒ fully static, contact form hidden. |
| `Cors:AllowedOrigins` | `Portfolio.Api/appsettings*.json` | empty | Allowed browser origins. Empty ⇒ any origin (fine locally, tighten in production). |
| `Resend:ApiKey` | **environment / user secrets only** | empty | Resend API key. Never commit it. Empty ⇒ `/api/contact` returns 503. |
| `Resend:From` | `Portfolio.Api/appsettings.json` | `Portfolio <onboarding@resend.dev>` | Sender. The sandbox domain needs no DNS setup. |
| `Resend:To` | `Portfolio.Api/appsettings.json` | your address | Recipient. Must be the Resend account's own address while using the sandbox sender. |
| `Resend:SubjectPrefix` | `Portfolio.Api/appsettings.json` | `Portfolio` | Prepended to the subject line so form mail is easy to filter. |
| `Resend:BaseUrl` | `Portfolio.Api/appsettings.json` | `https://api.resend.com/` | Override only to test against a stub or an API-compatible provider. |

Any of these can be supplied as an environment variable with `__` in place of `:` —
`Resend__ApiKey`, `Cors__AllowedOrigins__0` — which is how the Render service is configured.

## Built with

.NET 10 · Blazor WebAssembly · ASP.NET Core Minimal APIs · vanilla CSS (no framework) · GitHub Actions
