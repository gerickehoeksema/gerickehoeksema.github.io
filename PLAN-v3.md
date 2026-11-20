# Profile v3: improvement plan

**Goal:** turn the site into the bridge between Gericke-the-architect and Gericke-the-cyclist / chess player / Jimny explorer, with **full control over what is shown, to whom, and when**.

Status: draft for review. Nothing in this plan has been implemented yet.

---

## 1. Where the site is today

| Area | Current state | Problem |
|---|---|---|
| Content | Hardcoded in `index.html` (426 lines). | Every change means editing HTML by hand. You can't switch a section off without deleting it. |
| Personal vs. work | Personal life is two sentences in "Intro". Everything else is CV. | The site reads as a CV with a nice banner, not a bridge between the two. |
| Navigation | Hash-based SPA in `script.js`. | Bugs, see §4.1. |
| Visual identity | LinkedIn-style dark cards, emoji icons, illustrated banner and avatar. | The illustrated art and the generic corporate cards don't match yet. |
| Privacy | `medical.html` holds real medical data and is excluded only by `robots.txt` and `noindex`. | The repo is public, so anyone can read that data on GitHub. `noindex` doesn't make it private. |
| Leftovers | `banner.svg`, `banner.jpg`, `_redirects` (Netlify), `ROUTING_GUIDE.md`, `IMAGES_SETUP.md`, `lets-create-a-plan-optimized-dragon.md`, and a `/medical` handler in `script.js` that never runs on GitHub Pages. | Noise, and it confuses future you about how the site actually works. |

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

- **`lens`:** which side of you an item belongs to. It drives the "Work / Life / All" toggle (§3.2).
- **`visibility`:**
  - `public` is rendered and listed.
  - `unlisted` is rendered only via a direct link, never in lists.
  - `draft` is **never published**. It's filtered out at build time, so it never reaches the deployed site.
- **`publishFrom`:** schedules content without anyone having to remember to flip a switch.

**Important:** filtering hidden items in browser JavaScript is *not* control. The JSON would still ship to every visitor. Real control means filtering at **build time**, which is why §5 introduces a build step.

---

## 3. Visual direction

### 3.1 Art direction: lean into the illustration
The new banner and avatar give the site a strong, warm, comic-illustration identity. Make the rest of the UI match:
- **Palette from the art:** sunset amber (`#F2A541`), sky blue (`#3B82C4`), pine green (`#2F5D3A`), and dusty trail beige (`#E9D8B4`) as accents on the existing dark base. Define them as CSS custom properties.
- **Replace emoji icons** (💻 ☁️ 🔧 🏆 📝) with one consistent inline-SVG icon set, for example Lucide. Emoji render differently on every OS and look off-brand.
- **Typography:** one display face with some character for headings (for example *Space Grotesk*) and keep *Inter* for body text.
- **Cards:** softer, warmer borders and a subtle paper texture on life cards, so work and life are visually distinct but clearly the same site.
- **Light mode:** add a `prefers-color-scheme` light theme plus a manual toggle that remembers the choice.

### 3.2 The "lens" switch (the bridge)
A three-way segmented control under the profile header:

```
[ 💼 Work ]  [ 🌄 All ]  [ 🚴 Life ]
```

- **All** (default): an interleaved timeline of career milestones, certifications, rides, trips, and chess milestones.
- **Work:** what a recruiter or client wants. Experience, skills, certifications, CV download, LinkedIn.
- **Life:** cycling, the Jimny, chess, reading, Jeffreys Bay.
- The headline and the banner change with the lens. The work lens can use a cleaner crop, the life lens the full adventure scene.
- The lens is kept in the URL (`/?lens=work`), so you can send a recruiter a link that opens straight in work mode.

### 3.3 Page structure

| Page | Content |
|---|---|
| **Home** | Header, lens switch, "Now" card (what I'm working on / riding / reading this month), featured items. |
| **Work** | Merge today's About + Experience + Projects + Certifications into one well-structured page, with an architecture-focused "How I design systems" section (patterns, ADRs, Azure reference diagrams). |
| **Life** | Cycling (Strava stats), Chess (live rating), Jimny trips (gallery and map), Reading. |
| **Writing** | Articles for both lenses. Markdown files rendered at build time. Drop the "Coming soon" placeholder until the first article exists. |

### 3.4 Polish
- Fix the name overlapping the banner on desktop. Either give it a text-shadow / gradient scrim, or move it fully below the banner.
- Responsive images: serve WebP/AVIF in a few sizes through `<picture>`. The banner is currently a single 278 KB JPEG.
- Add a skip link, `aria-current` on the active nav link, visible focus states, and respect `prefers-reduced-motion` (the fade animation ignores it today).

---

## 4. Code quality

### 4.1 Bugs in the current `script.js`
1. **Double handlers.** Nav links get one click listener, and the generic `a[href^="#"]` listener *also* matches them. Each click calls `showPage` twice and pushes two history entries.
2. **History corruption.** `popstate` calls `showPage`, which calls `pushState` again. Pressing Back pushes a new entry, so Forward stops working.
3. **`load` also pushes state**, which adds a duplicate history entry on every page load.
4. **Dead medical handler.** On GitHub Pages, `/medical` is served by `404.html`, never `index.html`, so this code never runs. Delete it.
5. The fade-in style is injected with JS. It belongs in `styles.css`.

### 4.2 Structure
- Split the 935-line `styles.css` into layers (`tokens.css`, `base.css`, `components/*.css`, `pages/*.css`) using `@layer`, bundled at build time.
- Render components from data: one template per content type (experience item, project card, life card) instead of copy-pasted HTML blocks.
- Add SEO and sharing metadata: Open Graph / Twitter cards (use the banner as `og:image`), a `Person` JSON-LD schema linking LinkedIn, GitHub, Strava, and chess.com, a `sitemap.xml`, and a canonical URL.
- Fix the inconsistent title: `<title>` says "Software Architect" while the experience section says "Senior Software Developer". Pick one positioning and drive both from `profile.json`.

### 4.3 Clean up
Delete `banner.svg`, the old `banner.jpg`, `_redirects`, `ROUTING_GUIDE.md`, `IMAGES_SETUP.md`, and `lets-create-a-plan-optimized-dragon.md`. Fold anything still relevant into the README.

---

## 5. Architecture options for the build step

All three options give build-time filtering. Pick based on how much you want to own.

### Option A: Jekyll (native to GitHub Pages)
- Content in `_data/*.yml` and `_posts/*.md`, with Liquid templates.
- No CI needed. GitHub Pages builds it for you.
- Downside: Ruby/Liquid, which is not your stack, and its logic is limited.

### Option B: a small C# static-site builder (recommended)
A `.NET 9` console app in `/build` that you fully own:

```
build/
  SiteBuilder.csproj
  Content/ContentLoader.cs      // reads content/*.json + markdown
  Content/IVisibilityRule.cs    // Specification pattern
    PublishedRule, NotDraftRule, ScheduledRule, LensRule
  Rendering/RazorRenderer.cs    // RazorLight templates per component
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
- **Private repository support.** Drafts, the medical data, and the CV source no longer sit in a public repo. This is the only option that solves the privacy issue in §1.
- **Built-in auth and role-based routes** (`staticwebapp.config.json`). A `friends` lens or a protected `/medical` page can sit behind a login, or behind an invite-only role for family.
- An Azure Functions API (C#) for the Strava OAuth token refresh, instead of a build-time secret.
- Custom domain `gerickehoeksema.co.za` with a free managed certificate.
- PR preview environments: every PR gets its own URL to review before merging.

**Recommendation:** build **Option B now** on GitHub Pages, keeping the code free of hosting-specific logic, then move to **Option C** once the medical-data and drafts privacy need justifies it. The move is mostly a different deploy step in the workflow.

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
| **0. Hygiene** | Fix the `script.js` bugs (§4.1), delete leftovers, add OG/JSON-LD metadata, responsive images, accessibility fixes. | Small, and it can ship on its own. |
| **1. Visual refresh** | Palette tokens, SVG icons, typography, name/banner overlap, light mode. | Small–medium |
| **2. Content as data** | Move content to `content/*.json` and build the C# site builder with visibility rules and tests. Deploy via GitHub Actions. | Medium |
| **3. The bridge** | Lens switch, Home "Now" card, Life page, merged Work page. | Medium |
| **4. Integrations** | chess.com, Strava, Goodreads, Jimny trips, EXIF stripping. | Medium |
| **5. Azure SWA** | Private repo, auth-protected routes (medical, friends), PR previews, Functions for Strava OAuth. | Medium |

---

## 8. Decisions for you

1. **Positioning:** "Software Architect" or "Senior Developer / Tech Lead, working towards architect"? The headline, `<title>`, and meta description should agree.
2. **Build stack:** the C# builder (Option B, recommended), or Jekyll to stay zero-CI?
3. **Medical page:** keep it public by URL, put it behind auth (Option C), or keep only a minimal emergency-contact version public?
4. **Default lens** for first-time visitors: All, or Work?
5. **Strava detail level:** totals only, or individual rides with maps (start/finish trimmed)?
6. **Goodreads link:** the button is still `href="#"`. Which URL should it use?
