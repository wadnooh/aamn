# Implementation Plan — Wadnooh Software & Computer

## Stack strategy

| Layer | Phase 1 (now) | Phase 2–3 (enterprise target) |
|-------|---------------|-------------------------------|
| Web UI | Static HTML/CSS/JS in ASP.NET `wwwroot` | **Next.js** (App Router), AR/EN |
| API / domain | ASP.NET Core minimal APIs + AI study | **Django** (DRF) or keep .NET BFF — decision gate Week 1 of P2 |
| Data | JSON catalogs + SQLite/Identity | **PostgreSQL** |
| Cache / jobs | None | **Redis**, Celery/Hangfire |
| Deploy | Hostinger static + Cloudflare quick tunnel for API | **Docker** + Cloudflare (named tunnel / Workers) |
| Search | Client catalog filter | Postgres FTS / OpenSearch later |

> Do **not** migrate to Next.js/Django in the current pass; document only.

## Phase 1 — Current portal (weeks 0–2, largely done)

| Week | Milestone |
|------|-----------|
| 0–1 | Portal rebrand, IA/nav, specialty expansion, OSH center, aggregator JSON, Copilot OSH tune |
| 1–2 | Hostinger deploy + tunnel `API_BASE`, verify OSH live, docs pack (`docs/wep`) |
| Buffer | Content enrichment (more library/projects), admin polish |

**Exit criteria:** wadnooh.com shows Portal branding; `/osh.html` live; AI books/study via tunnel; SRS/ERD/UIUX/plan published.

## Phase 2 — LMS & engagement (≈ 8–12 weeks)

1. Auth RBAC complete (7 roles)  
2. Server-side enrollments, progress, assessments  
3. Forum per specialty; file uploads for templates (user-owned)  
4. Verified certificates (non-blockchain)  
5. Research center MVP (metadata + links)  
6. Begin Next.js front **or** progressive enhancement of wwwroot (choose one)

## Phase 3 — Ecosystem (≈ 12–16 weeks)

1. Virtual labs integrations (external tools first)  
2. Jobs board + employer accounts  
3. Engineering store (digital goods / toolkits)  
4. Mobile (PWA first)  
5. University SSO/API integrations  
6. Hardening: observability, CDN, named Cloudflare tunnel

## Risks

| Risk | Mitigation |
|------|------------|
| Quick tunnel URL rotates | `refresh-hostinger-api.ps1` + watchdog; plan named tunnel |
| Copyright claims | Links+summaries only; `lastRefreshed`; no PDF hosting |
| Scope creep (fake P2/P3) | Explicit soon badges; SRS out-of-scope list |
| Dual-stack rewrite cost | Keep JSON schemas close to ERD; migrate catalogs → Postgres ETL |
| AI quality/cost | Wikipedia+Open Library fallback; optional OpenAI key |

## Decision log

- 2026-08-02: Remain on ASP.NET+wwwroot for Phase 1 live portal; enterprise stack documented as target.
