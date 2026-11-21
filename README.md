# gerickehoeksema.co.za

Personal profile site for Gericke Hoeksema, hosted on GitHub Pages at <https://gerickehoeksema.co.za>.

## How the site works

The site is plain HTML, CSS and JavaScript, plus a small C# **site builder** that renders parts of it from data files in `content/`.

```
content/*.json, content/articles/*.md ─┐
                                       ├─▶ build/SiteBuilder ─▶ dist/  (the finished site)
index.html, styles.css, images/, ... ──┘
```

- **Hand-written files** (`index.html`, `styles.css`, `script.js`, `medical/`, images, PDFs) are copied into `dist/` as they are.
- **Content** (profile and page metadata, links, Intro, Skills, About, Competencies, Certifications, Projects, Education, Articles) lives in `content/`. The builder renders it into the marked regions of `index.html` and into one page per article. `index.html` is now the layout: navigation, images, the icon sprite and the order of the sections.
- **Visibility rules** decide what gets published: drafts are never written to `dist/`, scheduled items appear on their date, unlisted items are reachable by link only.

Every push to `main` is tested, built and deployed to GitHub Pages by GitHub Actions, and `main` is rebuilt nightly. See [Deploying with GitHub Actions](#deploying-with-github-actions).
`index.html` still holds hand-written fallback markup between its build markers, so [rolling back](#rolling-back) to serving the branch keeps working.

## Repository layout

```
index.html            Profile: Home, About, Experience, Projects, Articles (hash-based pages).
                      <!-- build:name --> ... <!-- /build:name --> regions are filled by the builder.
styles.css            All styles, in @layers; colour tokens are light-dark() pairs
script.js             Page switching, theme toggle, "Copy link" on articles
medical/index.html    Emergency medical page for the QR stickers (self-contained, see below)
medical.html          Old address; redirects to /medical/ without JavaScript
404.html              Not-found page, with a /medical safety net
robots.txt            Keeps /medical out of search engines
sitemap.xml           Only used if Pages is rolled back to serve the branch; the builder generates its own
CNAME                 Custom domain, only used if Pages is rolled back (Actions deploys take it from Settings)
images/               Banner, avatar and share image (JPEG + WebP)
certifications/       Certificate PDFs

content/              Everything the builder renders (prose in Markdown, lists in JSON)
  profile.json          Name, job title, headline, location; drives <title>, description, share card, JSON-LD
  links.json            Buttons under the profile header (Email, LinkedIn, ...)
  intro.md              Home page Intro
  skills.json           Experience page Skills & Expertise tiles
  about.md              About page Professional Background text
  competencies.json     About page Core Competencies groups
  certifications.json   Certificates
  projects.json         Project cards
  education.json        Education
  articles/<id>/        One folder per article: index.md + assets (e.g. og.jpg)

build/
  SiteBuilder/          .NET 10 console app: content -> dist/
  SiteBuilder.Tests/    xUnit tests (visibility rules, loader, end-to-end builds)
  SiteBuilder.slnx

.github/workflows/build.yml   CI: test + build on every push; dist/ uploaded as an artifact
global.json                   Runs `dotnet test` on Microsoft Testing Platform (needed for xUnit v3 on .NET 10)
PLAN-v3.md                    Improvement plan
```

## Editing content

| Section | Where to edit |
|---|---|
| Name, job title, headline, location; page title, search description, share card, JSON-LD | `content/profile.json` |
| Buttons under the header (Email, LinkedIn, GitHub, Instagram, Strava, chess.com, Goodreads) | `content/links.json` |
| Intro (Home) | `content/intro.md` |
| Skills & Expertise (Experience page) | `content/skills.json` |
| Professional Background (About) | `content/about.md` |
| Core Competencies (About) | `content/competencies.json` |
| Certifications | `content/certifications.json` (+ the PDF in `certifications/`) |
| Education | `content/education.json` |
| Projects (finished projects only; profiles such as GitHub go in `links.json`) | `content/projects.json` |
| Articles | `content/articles/<id>/index.md` |
| Footer year | Automatic: the year of the build |
| Layout, navigation, images, banner and avatar alt text, icon sprite | `index.html` |

The rule of thumb: **prose is Markdown, lists are JSON.** `profile.json` is required; any other file can be missing (that section is then empty).

After editing, run the tests and a build (see [Building and previewing](#building-and-previewing)). CI does the same on every push.

### Visibility fields

Every item, in JSON or in article front matter, can use the same fields (PLAN-v3 §2):

| Field | Values | Effect |
|---|---|---|
| `id` | lowercase, unique per file | Required in JSON. For articles it is the folder name. |
| `visibility` | `public` (default), `unlisted`, `draft` | Drafts are never written to `dist/`. Unlisted items are built but left out of lists and the sitemap. |
| `publishFrom` | `yyyy-MM-dd` | Hidden until that date. Needs a rebuild on or after the date (the deploy workflow below rebuilds nightly). |
| `lens` | `["work"]`, `["life"]`, `["work", "life"]` | Ready for the Work / Life switch (PLAN-v3 §3.1). Defaults to work. |
| `featured` | `true` / `false` | Reserved for the home page. |

Parsing is strict: an unknown field fails the build, so a typo like `visiblity: draft` can't accidentally publish a draft.

### Examples

**The profile** (`content/profile.json`). The job title appears in the page title, the share card title and the JSON-LD, so changing positioning is one edit:

```json
{
  "name": "Gericke Hoeksema",
  "givenName": "Gericke",
  "familyName": "Hoeksema",
  "jobTitle": "Software Architect",
  "headline": "Software Developer - Cyclist - Chess player - Xbox gamer - Not yet a surfer",
  "location": { "locality": "Jeffreys Bay", "country": "South Africa", "countryCode": "ZA" },
  "description": "Shown in search results",
  "shareDescription": "Shown on LinkedIn / WhatsApp share cards",
  "image": "images/profile-600.webp",
  "knowsAbout": ["C#", ".NET", "Microsoft Azure"],
  "sameAs": ["https://www.linkedin.com/in/...", "https://github.com/gerickehoeksema"]
}
```

**A header button** (`content/links.json`, shown in file order). `primary: true` makes it the amber button; `brand` colours the icon. A `#` link (a placeholder until you have the URL) stays on the page without opening a new tab; set `"visibility": "draft"` to hide a button instead.

```json
{ "id": "strava", "label": "Strava", "url": "https://www.strava.com/athletes/3057211", "icon": "strava", "brand": "strava" }
```

**A skill** (`content/skills.json`): `{ "id": "azure", "title": "Azure Cloud", "description": "Cloud architecture & solutions", "icon": "cloud" }`

**A competency group** (`content/competencies.json`): `{ "id": "architecture", "title": "Architecture", "items": ["Microservices and monolith decomposition", "..."] }`

**An education item** (`content/education.json`): `{ "id": "cti", "school": "CTI Education Group", "qualification": "Software Development and IT Studies", "start": 2008, "end": 2009 }`. Leave out `start` for a single year.

**Intro and About** (`content/intro.md`, `content/about.md`): plain Markdown paragraphs, no front matter. Links and **bold** work.

**A role** (`content/experience.json`, newest first is automatic; leave out `end` while current):

```json
{
  "id": "nml",
  "role": "Senior Software Developer · Technical & Team Lead",
  "company": "New media Labs (NML)",
  "start": "2021-08",
  "summary": "Technical lead and senior developer ...",
  "highlights": ["Led the migration from a monolithic application ..."],
  "lens": ["work"]
}
```

**A certificate** (`content/certifications.json`, shown in file order). Write the PDF path as it is on disk; the builder encodes spaces and `#`. A full `https://` link to a verification page also works. A local PDF that doesn't exist fails the build.

```json
{ "id": "azure-fundamentals", "title": "Microsoft Certified: Azure Fundamentals", "issuer": "Microsoft", "link": "certifications/Azure Fundamentals.pdf" }
```

**A project** (`content/projects.json`, shown in file order). `icon` is an id from the sprite in `index.html`; `brand` is optional and colours the icon with a `brand-*` class.

```json
{ "id": "taxyou", "title": "TaxYou", "description": "...", "url": "https://www.taxyou.co.za", "linkText": "Visit project", "icon": "receipt" }
```

**An article** (`content/articles/<id>/index.md`): YAML front matter, then Markdown. Inline HTML is allowed (the diagram in the first article is inline SVG). `{.class}` on the line before a block adds a CSS class.

```markdown
---
title: "Breaking up a monolith: what I'd keep, and what I'd do differently"
description: "One sentence for search results and share cards."
lede: "Optional intro paragraph under the title."
kicker: Architecture
icon: boxes
date: 2026-12-01
lens: [work]
visibility: draft
keywords: [microservices, .NET, Azure]
---

Body in **Markdown**.
```

Put a 1200×630 `og.jpg` next to `index.md` for the LinkedIn/WhatsApp preview card; without one, the site-wide `images/og-image.jpg` is used.

### Publishing an article

1. Replace every `<mark class="todo">` placeholder. The build refuses to publish an article that still has one.
2. In the front matter, set `visibility: public` and the real `date` (or `publishFrom` to schedule it).
3. Preview it (`--preview`, below), then open a PR. CI builds and tests it; the `site` artifact on the run holds the result.
4. After it is live, paste the URL into LinkedIn's [Post Inspector](https://www.linkedin.com/post-inspector/) so the preview card is fresh.

The draft banner, `noindex`, the article list, reading time and `sitemap.xml` are all handled by the builder.

## Building and previewing

Needs the [.NET 10 SDK](https://dotnet.microsoft.com/download). Run from the repository root:

```bash
dotnet test --solution build/SiteBuilder.slnx                            # run the tests
dotnet run --project build/SiteBuilder                                    # build the live site into dist/
dotnet run --project build/SiteBuilder -- --preview --out dist-preview   # also render drafts and scheduled items
python3 -m http.server 8000 --directory dist                             # http://localhost:8000
```

Options: `--root <dir>`, `--out <dir>`, `--today yyyy-MM-dd` (pretend it's another day, to check scheduled items), `--preview`.
Never deploy a `--preview` build: it contains drafts. `dist/` and `dist-*/` are git-ignored.

### How the builder works

The build is a pipeline of small steps, each one class in `build/SiteBuilder/Pipeline/Steps/`:

| Step | What it does |
|---|---|
| `LoadContent` | Reads `content/` into typed records. Strict: unknown fields, bad dates and duplicate ids fail with the file name. |
| `ApplyVisibility` | Filters content once, using the rules in `build/SiteBuilder/Specifications/` (`NotDraft`, `PublishedBy`, `Unlisted`, `InLens`), composed into a `VisibilityPolicy`. |
| `PublishGuard` | Fails if a published article still has `<mark class="todo">`. |
| `CheckLocalLinks` | Fails if a certificate points at a PDF that doesn't exist. |
| `CopyStaticFiles` | Empties `dist/` and copies the hand-written site, leaving out `content/`, `build/`, `.github/`, Markdown files and the root `sitemap.xml`. |
| `RenderArticles` | Writes `/articles/<id>/index.html` and copies the article's assets. |
| `RenderHome` | Fills the `<!-- build:... -->` regions in `dist/index.html`. |
| `WriteSitemap` | Writes `sitemap.xml` with the home page and every listed article. |

A new step (image optimisation, EXIF stripping, a Strava fetch) is one new class added to `BuildPipeline.Default()`.
Templates are C# raw strings in `build/SiteBuilder/Rendering/`, so a typo in a template is a compile error.

## Continuous integration

`.github/workflows/build.yml` runs on every push and pull request, and nightly:

1. `dotnet test`
2. `dotnet run --project build/SiteBuilder`
3. Checks that no `class="todo"` reached `dist/` and that the emergency page is in the build
4. Uploads `dist/` as the Pages artifact (`github-pages` on the run), so you can download and inspect exactly what would be published
5. **On `main` only:** deploys it to GitHub Pages

## Deploying with GitHub Actions

The live site is deployed by the workflow. These are the settings it depends on, for reference or for setting up a fresh copy of the repository.

### 1. Pages source

1. On GitHub, open the repository's **Settings → Pages**.
2. Under **Build and deployment → Source**, choose **GitHub Actions** (instead of *Deploy from a branch*).
3. Leave **Custom domain** as `gerickehoeksema.co.za` and keep **Enforce HTTPS** on. With Actions deployments the custom domain comes from this setting; the `CNAME` file is no longer used (it's harmless to keep).

GitHub creates (or reuses) an environment called **github-pages**. Under **Settings → Environments → github-pages**, check that **Deployment branches** only allows `main`, so a feature branch can never deploy.

> The repository root is no longer served directly, so the live site only changes when a deploy passes. A failing test or a broken content file stops the deploy and leaves the previous version live.

### 2. The deploy job

Already in `.github/workflows/build.yml`:

```yaml
name: Build and deploy site

on:
  push:
  pull_request:
  workflow_dispatch:
  schedule:
    - cron: "17 3 * * *"   # nightly (03:17 UTC), so publishFrom dates go live without a push

permissions:
  contents: read

# One deployment at a time; a newer run waits instead of cancelling a live deploy.
concurrency:
  group: pages-${{ github.ref }}
  cancel-in-progress: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Test
        run: dotnet test --solution build/SiteBuilder.slnx --configuration Release

      - name: Build site
        run: dotnet run --project build/SiteBuilder --configuration Release

      - name: Check no draft reached dist/
        run: |
          if grep -rl 'class="todo"' dist; then
            echo "::error::Draft placeholders found in dist/"
            exit 1
          fi

      - name: Check the emergency page is in the build
        run: |
          test -f dist/medical/index.html
          test -f dist/medical.html

      - uses: actions/upload-pages-artifact@v3
        with:
          path: dist/

  deploy:
    # Only main deploys; pull requests and other branches just build and test.
    if: github.ref == 'refs/heads/main' && github.event_name != 'pull_request'
    needs: build
    runs-on: ubuntu-latest
    permissions:
      pages: write      # to deploy to Pages
      id-token: write   # to prove the deployment comes from this workflow
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - id: deployment
        uses: actions/deploy-pages@v4
```

What it does:

- `upload-pages-artifact` packages `dist/` in the format Pages expects.
- The `deploy` job runs only for `main`, needs the build and tests to pass, and gets the extra `pages` and `id-token` permissions only for itself.
- The nightly `schedule` rebuilds `main`, which is what makes `publishFrom` dates appear on time.
- The extra check fails the deploy if the emergency page is ever missing from the build.

### 3. Check the deployment

After merging to `main`:

1. **Actions** tab: the run shows `build` then `deploy`, with the site URL on the deploy job.
2. Open <https://gerickehoeksema.co.za> and check the Experience (Skills, Certifications, Education) and Projects pages.
3. Check that all three emergency addresses still work: `/medical`, `/medical/` and `/medical.html`.
4. Check that a made-up address shows the 404 page.

### Rolling back

Set **Settings → Pages → Source** back to **Deploy from a branch** (`main`, `/ (root)`). The site is then served from the repository root again, which still works because `index.html` keeps its fallback markup. That safety net is why the fallback markup should stay until the Actions deployment has been running for a while.

### After the switch (optional clean-up)

Once you're happy with deploying from Actions:

- The fallback markup between the `<!-- build:... -->` markers can be reduced to a short comment, so content only lives in `content/`.
- The root `sitemap.xml` and `CNAME` can be removed (the builder writes the sitemap; the domain comes from Settings).

## Emergency medical page

The QR stickers on the bikes and helmets point to this page, so treat its URL as permanent.

- `/medical`, `/medical/` and `/medical.html` all reach it. `medical/index.html` is served directly, and `medical.html` uses a meta refresh, so neither depends on JavaScript.
- The builder copies both files into `dist/` unchanged, and the test suite checks that `medical/index.html` is in the build.
- The page has its styles inlined and loads no external files, so it works on a weak signal.
- It's marked `noindex` and blocked in `robots.txt`, so search engines skip it. It is still public to anyone with the URL, by design.
- **This repository is public**, so the page source (and its git history) can be read on GitHub.
- Update the contents in `medical/index.html` only. `medical.html` is just a redirect.

## Icons

`index.html` has one inline SVG sprite at the top of `<body>` (article pages have their own small sprite in `build/SiteBuilder/Rendering/ArticlePage.cs`). Use an icon with:

```html
<svg class="icon" aria-hidden="true"><use href="#i-linkedin"/></svg>
```

- UI icons come from [Lucide](https://lucide.dev) (ISC). Brand icons come from [Simple Icons](https://simpleicons.org) (CC0); LinkedIn was taken from v9 because later versions dropped it.
- Brand colours are set with a `brand-*` class (`brand-linkedin`, `brand-strava`, ...) in `styles.css`.

## Updating images

Keep the JPEG and generate matching WebP versions at the same names and widths, for example with Python and Pillow:

```python
from PIL import Image
im = Image.open("images/banner-adventure.jpg").convert("RGB")
for w in (900, 1792):
    im.resize((w, round(im.height * w / im.width)), Image.LANCZOS) \
      .save(f"images/banner-adventure-{w}.webp", quality=80, method=6)
```

## Running the hand-written site without building

To see the fallback markup (what a rollback would serve), look at the repository as-is:

```bash
python3 -m http.server 8000
# http://localhost:8000
```

Use a local server rather than opening the file directly, so `/medical/` and the absolute links resolve. This shows the fallback markup, not the content files, and never shows articles.
