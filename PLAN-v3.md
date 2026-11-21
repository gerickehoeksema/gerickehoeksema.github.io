# Profile v3: improvement plan

**Goal:** turn the site into the bridge between Gericke-the-architect and Gericke-the-cyclist / chess player / Jimny explorer, with **full control over what is shown, to whom, and when**.

Status: draft for review. Phase 0 (hygiene) and Phase 1 (visual refresh: palette, SVG icons, typography, light mode) are done and have been removed from this plan. Phase 2 is in a test run: the C# builder renders Experience, Certifications, Projects and Articles into `dist/` and CI builds it, but GitHub Pages still serves the repository root.

---

## 1. Where the site is today

| Area | Current state | Problem |
|---|---|---|
| Content | Experience, Certifications, Projects and Articles come from `content/` (Phase 2 test run). The profile header, Intro, Skills, About and Education are still hard-coded in `index.html`. | Most changes still mean editing HTML by hand, and those sections can't be switched off without deleting them. |
| Personal vs. work | Personal life is two sentences in "Intro". Everything else is CV. | The site reads as a CV with a nice banner, not a bridge between the two. |
| Emergency page | `medical/index.html`, reached from the QR sticker on the bikes and helmets. Served without JavaScript at `/medical`, `/medical/` and `/medical.html`. | Nothing guards that URL against a future change, and the repo is public, so the source and its git history are readable on GitHub. See §4.2. |

---

## 2. Core idea: content as data, with visibility you control

Separate **what** is on the site from **how** it is rendered.

```
content/
  profile.json      name, headline, location, avatar, banner, links
  sections.json     ordered list of sections with on/off and audience
  experience.json   jobs (each one can be hidden)
  projects.json
  certifications.json
  life/
    cycling.json    bikes, favourite routes, Strava widget config
    chess.json      chess.com username, which stats to show
    jimny.json      trips / gallery
    reading.json    books (manual or Goodreads RSS)
```

Every item gets the same small visibility contract. You'll know this as a specification pattern applied to content:

```jsonc
{
  "id": "jimny-baviaanskloof-2026",
  "title": "Baviaanskloof in the Jimny",
  "lens": ["life"],            // "work", "life", or both
  "visibility": "public",      // "public" | "unlisted" | "draft"
  "publishFrom": "2026-12-01", // optional: scheduled reveal
  "featured": true
}
```

- **`lens`:** which side of you an item belongs to. It drives the "Work / Life / All" toggle (§3.1).
- **`visibility`:**
  - `public` is rendered and listed.
  - `unlisted` is rendered only via a direct link, never in lists.
  - `draft` is **never published**. It's filtered out at build time, so it never reaches the deployed site.
- **`publishFrom`:** schedules content without anyone having to remember to flip a switch.

**Important:** filtering hidden items in browser JavaScript is *not* control. The JSON would still ship to every visitor. Real control means filtering at **build time**, which is why §5 introduces a build step.

---

## 3. Site structure

### 3.1 The "lens" switch (the bridge)
A three-way segmented control under the profile header:

```
[ 💼 Work ]  [ 🌄 All ]  [ 🚴 Life ]
```

- **All** (default): an interleaved timeline of career milestones, certifications, rides, trips, and chess milestones.
- **Work:** what a recruiter or client wants. Experience, skills, certifications, CV download, LinkedIn.
- **Life:** cycling, the Jimny, chess, reading, Jeffreys Bay.
- The headline and the banner change with the lens. The work lens can use a cleaner crop, the life lens the full adventure scene.
- The lens is kept in the URL (`/?lens=work`), so you can send a recruiter a link that opens straight in work mode.

### 3.2 Page structure

| Page | Content |
|---|---|
| **Home** | Header, lens switch, "Now" card (what I'm working on / riding / reading this month), featured items. |
| **Work** | Merge today's About + Experience + Projects + Certifications into one well-structured page, with an architecture-focused "How I design systems" section (patterns, ADRs, Azure reference diagrams). |
| **Life** | Cycling (Strava stats), Chess (live rating), Jimny trips (gallery and map), Reading. Life cards get their own look (warmer borders, a subtle paper texture) so work and life are distinct but clearly the same site. |
| **Writing** | Articles for both lenses. Markdown files rendered at build time. The first article is drafted in `content/articles/`. |

---

## 4. Code quality

### 4.1 Structure
- `styles.css` is already organised into `@layer reset, tokens, base, layout, components, utilities`. Once the build step exists, split each layer into its own file and bundle at build time.
- Render components from data: one template per content type (experience item, project card, life card) instead of copy-pasted HTML blocks.
- Fix the inconsistent title: `<title>` says "Software Architect" while the experience section says "Senior Software Developer". Pick one positioning and drive both (plus the Open Graph and JSON-LD job title) from `profile.json`.

### 4.2 Emergency page (QR sticker)
The QR stickers on the bikes and helmets are printed, so **`/medical` is a contract that must never break**, and the page must open instantly for a stranger with no login.
- **Protect it with a test.** Add a build test and a scheduled GitHub Actions check that fails if `/medical`, `/medical/` or `/medical.html` stops returning 200 with the expected content.
- **Show only what a first responder needs:** name, emergency contacts, blood type, allergies, conditions, medication, and medical aid. Leave out ID numbers and home address.

---

## 5. Architecture options for the build step

All three options give build-time filtering. Pick based on how much you want to own.

### Option A: Jekyll (native to GitHub Pages)
- Content in `_data/*.yml` and `_posts/*.md`, with Liquid templates.
- No CI needed. GitHub Pages builds it for you.
- Downside: Ruby/Liquid, which is not your stack, and its logic is limited.

### Option B: a small C# static-site builder (recommended)
A `.NET 10` (LTS) console app in `/build` that you fully own:

```
build/
  SiteBuilder.csproj
  Content/ContentLoader.cs      // reads content/*.json + markdown
  Content/IVisibilityRule.cs    // Specification pattern
    PublishedRule, NotDraftRule, ScheduledRule, LensRule
  Rendering/*.cs                // C# raw-string templates per component (compile-time checked; RazorLight optional later)
  Pipeline/IBuildStep.cs        // Pipeline / Chain of Responsibility
    LoadContent -> ApplyVisibility -> FetchIntegrations -> Render -> OptimiseImages -> Write
  Integrations/
    IActivityProvider.cs        // Strategy + Adapter
      StravaActivityProvider, ChessComStatsProvider, GoodreadsRssProvider
```

- **Specification pattern** for visibility rules. They're composable and unit-testable (`new NotDraft().And(new PublishedBefore(now))`).
- **Pipeline** for build steps, so new steps (image optimisation, sitemap) slot in without touching existing ones.
- **Strategy / Adapter** for third-party data. Swapping Goodreads for StoryGraph is one new class.
- **GitHub Actions** runs `dotnet run --project build`, then `actions/deploy-pages`. A nightly cron rebuild keeps Strava and chess stats fresh without exposing API secrets to the browser.
- The tests become your safety net: "a draft item is never written to `/dist`".

### Option C: Option B deployed to Azure Static Web Apps
The same builder, but deployed to **Azure Static Web Apps (Free tier)** instead of GitHub Pages:
- **Private repository support.** Drafts and the CV source no longer sit in a public repo. The medical page stays **publicly served** for the QR code, but its source is no longer browsable on GitHub. The old data stays in the existing git history unless that history is rewritten.
- **Built-in auth and role-based routes** (`staticwebapp.config.json`). A `friends` lens can sit behind a login or an invite-only role for family. `/medical` is explicitly excluded and always anonymous.
- An Azure Functions API (C#) for the Strava OAuth token refresh, instead of a build-time secret.
- Custom domain `gerickehoeksema.co.za` with a free managed certificate.
- PR preview environments: every PR gets its own URL to review before merging.

**Recommendation:** build **Option B now** on GitHub Pages, keeping the code free of hosting-specific logic, then move to **Option C** once keeping drafts and source private justifies it. The move is mostly a different deploy step in the workflow.

---

## 6. Integrations (life lens)

| Source | How | Notes |
|---|---|---|
| **chess.com** | Public API `api.chess.com/pub/player/gerickehoeksema/stats`. No key needed. | Show rapid/blitz/daily rating and puzzle rating. Fetch at build time or in the browser. |
| **Strava** | Strava API with OAuth refresh token stored as a GitHub/Azure secret, fetched at build time. | Show year-to-date distance and elevation, plus recent rides. **You choose which activities appear** (for example only public ones, with start/end hidden so your home location isn't revealed). |
| **Goodreads** | Shelf RSS feed (the API is closed). | Currently-reading and recently-read. |
| **Jimny trips** | Markdown + photos in `content/life/jimny/`, optional GPX → static map. | Fully under your control. |

**Privacy defaults:** never render exact GPS start points, always strip EXIF location from uploaded photos (add this as a build step), and have each integration opt in per item where possible.

---

## 7. Suggested phases

| Phase | Scope | Size |
|---|---|---|
| **2. Content as data** | Test run done: `build/SiteBuilder` (.NET 10) with Specification-based visibility rules, a step pipeline, 50 tests and a CI workflow; Experience, Certifications, Projects and Articles come from `content/`. Remaining: move the profile header and links, Skills, About and Education to content, then deploy `dist/` with `actions/deploy-pages`. | Medium |
| **3. The bridge** | Lens switch, Home "Now" card, Life page (with its own card style), merged Work page. | Medium |
| **4. Integrations** | chess.com, Strava, Goodreads, Jimny trips, EXIF stripping. | Medium |
| **5. Azure SWA** | Private repo, auth-protected friends lens (medical stays public), PR previews, Functions for Strava OAuth. | Medium |

---

## 8. Decisions for you

1. **Positioning:** "Software Architect" or "Senior Developer / Tech Lead, working towards architect"? The headline, `<title>`, and meta description should agree.
2. ~~**Build stack**~~: decided, the C# builder (Option B) on GitHub Pages.
3. **Medical page content:** it stays public for the QR code. Should anything currently on it be trimmed (for example ID number or address)?
4. **Default lens** for first-time visitors: All, or Work?
5. **Strava detail level:** totals only, or individual rides with maps (start/finish trimmed)?
6. **Goodreads link:** the button is still `href="#"`. Which URL should it use?
