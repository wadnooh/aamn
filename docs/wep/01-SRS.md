# Software Requirements Specification (SRS)  
## Wadnooh Software & Computer

**Version:** 1.0 (Phase-1 aligned)  
**Date:** 2026-08-02  
**Status:** Living document — Phase 1 shipped on ASP.NET Core + static wwwroot; enterprise stack deferred.

---

### 1. Vision

**Wadnooh Software & Computer** is an Arabic Engineering Digital Ecosystem — not a course marketplace alone.

Pillars: **Education · Research · Innovation · Jobs**  
Tagline: **Code • Repair • Connect • Support**

The portal serves students, faculty, practicing engineers, training bodies, and industry with specialty gateways, an academy (LMS-lite), digital library, projects hub, **OSH/HSE center**, Engineering Copilot (AI), and progressive modules (labs, research, forum, jobs, store) marked “soon” until built.

### 2. Goals

1. Provide a bilingual (AR/EN, RTL-first) institutional portal with Nile/Kush identity.  
2. Offer specialty portals (22+ including Petroleum, Chemical, Aviation, Mining, Biomedical, OSH).  
3. Deliver a first-class **Occupational Safety & Health** center with courses, templates metadata, and legal external aggregators.  
4. Support study/AI assistance for engineering + risk analysis without hosting copyrighted full texts.  
5. Document an enterprise target architecture (Next.js + Django + Postgres + Redis + Docker + Cloudflare) without forcing migration in Phase 1.

### 3. Users & roles (scaffold → full RBAC in Phase 2)

| Role | Needs |
|------|--------|
| Guest | Browse catalog, OSH center, library links, search |
| Student | Enroll (local progress), AI, portfolio shell |
| Teacher / TA | Teach/grade affordances (UI scaffold) |
| Author | Content authorship (Phase 2 CMS) |
| Supervisor / SystemAdmin | Admin shell, catalog oversight |
| Industry / Employer | Jobs listings (Phase 3) |
| Researcher | Research center (Phase 2+) |

### 4. Functional requirements (selected)

| ID | Requirement | Phase |
|----|-------------|-------|
| F-01 | Home, Academy workspace, Specialties, Courses, Projects, Library, News, Events, About/Guide, Contact (newsletter) | 1 |
| F-02 | Nav IA includes Virtual Labs, Research, Forum, Jobs, Engineering Store as **soon** shells | 1 |
| F-03 | Specialty gateway tabs: Roadmap, Courses, Labs, Books, Research, Projects, Downloads, Jobs, Discussion, Exams | 1 |
| F-04 | Dedicated `osh.html` OSH/HSE center covering management, risk, hazards, PPE, PTW, incidents, inspection, emergency, sectors, courses, templates, official sources | 1 |
| F-05 | JSON catalogs + optional `/api/catalog/{resource}` including `osh-sources` | 1 |
| F-06 | Engineering Copilot `/api/ai/study` + `/api/ai/books` (Wikipedia/Open Library; OSH query map) | 1 |
| F-07 | Certificates placeholder (local completion) | 1 |
| F-08 | Full LMS, exam engine, verified certs, forums, notifications | 2 |
| F-09 | Virtual labs, jobs marketplace, mobile apps, deep university APIs | 3 |
| F-10 | Copyright-safe linking only — no pirated PDFs | All |

### 5. Non-functional requirements

- **NFR-01 Performance:** Static pages + JSON load under typical shared hosting; API via tunnel/CORS for Hostinger static front.  
- **NFR-02 i18n:** AR default RTL; EN LTR; shared string table.  
- **NFR-03 Accessibility:** Semantic headings, keyboard-usable nav; improve WCAG in Phase 2.  
- **NFR-04 Security:** JWT auth for membership paths; no secrets in static pack; admin scaffold only.  
- **NFR-05 Legal:** Aggregator stores summaries + official URLs + `lastRefreshed`; no full copyrighted republication.  
- **NFR-06 Maintainability:** Catalog JSON editable; docs under `docs/wep/`.

### 6. Modules (blueprint mapping)

1. Portal shell & IA  
2. Academy (courses, paths, progress)  
3. Specialty gateways  
4. **OSH/HSE center** + sources aggregator  
5. Digital library  
6. Projects / innovation hub  
7. Engineering Copilot  
8. News & events  
9. Certificates (placeholder → verified)  
10. Research center (soon)  
11. Forum (soon)  
12. Jobs (soon)  
13. Virtual labs (soon)  
14. Engineering store (soon)  
15. Admin shell  

### 7. Out of scope (Phase 1)

Virtual labs runtime, blockchain certificates, GraphQL, production MFA/SSO, Redis/Elasticsearch, native mobile apps, full CMS authoring.

### 8. Constraints & assumptions

- Phase 1 runtime remains **ASP.NET Core + wwwroot** on local/tunnel; static site on Hostinger (`wadnooh.com`) with `API_BASE` → Cloudflare tunnel.  
- Enterprise rewrite is planned, not executed in this pass.

### 9. Acceptance (Phase 1)

- [x] Branding Portal + tagline live on home  
- [x] OSH center + specialty `osh`/`hse`  
- [x] Expanded specialties list  
- [x] Aggregator JSON with official links  
- [x] Docs pack under `docs/wep/`  
- [x] Deployable to wadnooh.com with working AI/catalog API
