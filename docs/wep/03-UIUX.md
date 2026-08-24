# UI/UX Design Brief — Wadnooh Software & Computer

## Brand & principles

- **Name:** Wadnooh Software & Computer  
- **Tagline:** Code • Repair • Connect • Support  
- **Palette:** Nile deep `#053d47`, Nile `#0a5f6c`, Kush gold `#c4a35a`, stone surfaces; OSH accent `#b45309` (restrained, professional — not generic purple AI).  
- **Type:** El Messiri (display) + IBM Plex Sans Arabic (body).  
- **Composition:** Brand-first hero; one job per section; cards only for interaction containers; full-bleed atmospheric gradient (not flat white).  
- **Motion:** Subtle fade/slide on panels; avoid noise.

## Sitemap (Phase 1)

```
Home
├── Unified search
├── OSH highlight → /osh.html
├── Specialty grid → /specialty.html?dept={slug}
├── Stats / Courses / Paths / Articles / Projects
├── Q-bank teasers / Events / Video shell
├── Success / Partners / News / FAQ / Newsletter
└── Academy workspace (#learn)
    ├── Courses · Paths · Library · Projects · Universities
    ├── My dashboard · Portfolio · AI Copilot · Membership
Specialty portals (/specialty.html)
OSH Center (/osh.html)
Guide / About (/guide.html)
Admin shell (/admin.html)
Soon (nav only): Labs · Research · Forum · Jobs · Store
```

## Key screens

| Screen | Purpose | Notes |
|--------|---------|-------|
| Home | Portal positioning | Brand hero + OSH band + dept grid |
| Academy LMS-lite | Learn & progress | Local progress keys |
| Specialty gateway | Per-dept IA | Roadmap…Discussion tabs; soon badges |
| OSH center | Full HSE curriculum outline | Sources aggregator + templates metadata |
| AI Copilot | Study/risk/code | Tunnel API |
| Guide | Onboarding | Links to OSH & specialties |
| Admin | Catalog read shell | Phase 2 CMS |

## AR / EN & RTL

- Default AR `dir=rtl`; EN flips `dir=ltr`.  
- Prefer logical CSS (`margin-inline-start`, `border-inline-start`) on new UI.  
- Strings via `data-i18n` / `I18N` map in `wadnooh-eng.js`.  
- Avoid mirrored icons that break meaning; keep safety/amber accents identical in both locales.

## Content rules

- External materials: **title + short Arabic/English summary + official URL** only.  
- Templates: metadata / placeholders until Phase 2 file generation.  
- Never fake live labs, blockchain certs, or full job boards.
