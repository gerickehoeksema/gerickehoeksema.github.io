# gerickehoeksema.co.za

Personal profile site for Gericke Hoeksema, hosted on GitHub Pages at <https://gerickehoeksema.co.za>.

Plain HTML, CSS and JavaScript, plus a small C# site builder that renders content from `content/` (Phase 2 of [`PLAN-v3.md`](PLAN-v3.md), currently a test run: the live site is still served straight from the repository root).

## Structure

```
index.html            Profile: Home, About, Experience, Projects, Articles (hash-based pages).
                      <!-- build:name --> regions are filled from content/ by the builder.
styles.css            All styles, in @layers; colour tokens are light-dark() pairs
script.js             Page switching between the sections in index.html
medical/index.html    Emergency medical page for the QR stickers (self-contained, see below)
medical.html          Old address; redirects to /medical/ without JavaScript
404.html              Not-found page, with a /medical safety net
robots.txt            Keeps /medical out of search engines
sitemap.xml
CNAME                 Custom domain for GitHub Pages
images/
  banner-adventure.jpg            Banner (JPEG fallback)
  banner-adventure-{900,1792}.webp
  profile.jpg                     Avatar (JPEG fallback)
  profile-{320,600}.webp
  og-image.jpg                    1200x630 social sharing card
certifications/       Certificate PDFs
content/
  experience.json     Experience timeline
  articles/<id>/      One folder per article: index.md (front matter + Markdown) and assets (og.jpg)
build/
  SiteBuilder/        .NET 10 console app: content -> dist/
  SiteBuilder.Tests/  xUnit tests (visibility rules, loader, end-to-end builds)
.github/workflows/    CI: test + build on every push, dist/ uploaded as an artifact
```

## Emergency medical page

The QR stickers on the bikes and helmets point to this page, so treat its URL as permanent.

- `/medical`, `/medical/` and `/medical.html` all reach it. GitHub Pages serves `medical/index.html` directly, and `medical.html` uses a meta refresh, so neither depends on JavaScript.
- The page has its styles inlined and loads no external files, so it works on a weak signal.
- It's marked `noindex` and blocked in `robots.txt`, so search engines skip it. It is still public to anyone with the URL, by design.
- **This repository is public**, so the page source (and its git history) can be read on GitHub.
- Update the contents in `medical/index.html` only. `medical.html` is just a redirect.

## Icons

`index.html` has one inline SVG sprite at the top of `<body>`. Use an icon with:

```html
<svg class="icon" aria-hidden="true"><use href="#i-linkedin"/></svg>
```

- UI icons come from [Lucide](https://lucide.dev) (ISC). Brand icons come from [Simple Icons](https://simpleicons.org) (CC0); LinkedIn was taken from v9 because later versions dropped it.
- Brand colours are set per button with a `brand-*` class (`brand-linkedin`, `brand-strava`, ...) in `styles.css`.

## Building the site

Needs the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test --solution build/SiteBuilder.slnx               # run the tests
dotnet run --project build/SiteBuilder                       # build the live site into dist/
dotnet run --project build/SiteBuilder -- --preview --out dist-preview   # include drafts, for review only
cd dist && python3 -m http.server 8000                       # look at it
```

How it works: `LoadContent -> ApplyVisibility -> PublishGuard -> CopyStaticFiles -> RenderArticles -> RenderHome -> WriteSitemap`.
Each step is one class in `build/SiteBuilder/Pipeline/Steps/`; visibility rules are small specifications in `build/SiteBuilder/Specifications/`.

### Content visibility

Every item has the same fields (see PLAN-v3 §2):

| Field | Values | Effect |
|---|---|---|
| `visibility` | `public` (default), `unlisted`, `draft` | Drafts are never written to `dist/`. Unlisted items are built but left out of lists and the sitemap. |
| `publishFrom` | `yyyy-MM-dd` | Hidden until that date. A nightly rebuild (once deploying from CI) makes it appear. |
| `lens` | `[work]`, `[life]`, `[work, life]` | Ready for the Work / Life switch. |

Unknown fields are an error, so a typo like `visiblity: draft` fails the build instead of publishing a draft.

## Publishing an article

1. Replace every `<mark class="todo">` in `content/articles/<id>/index.md`. The build refuses to publish an article that still has one.
2. In the front matter, set `visibility: public` and the real `date`.
3. Open a PR. CI builds and tests it; download the `site` artifact to check the result.
4. After it is live, paste the URL into LinkedIn's [Post Inspector](https://www.linkedin.com/post-inspector/) so the preview card is fresh.

The draft banner, `noindex`, the article list and `sitemap.xml` are all handled by the builder.

## Updating images

Keep the JPEG and generate matching WebP versions at the same names and widths, for example with Python and Pillow:

```python
from PIL import Image
im = Image.open("images/banner-adventure.jpg").convert("RGB")
for w in (900, 1792):
    im.resize((w, round(im.height * w / im.width)), Image.LANCZOS) \
      .save(f"images/banner-adventure-{w}.webp", quality=80, method=6)
```

## Running locally

```bash
python3 -m http.server 8000
# http://localhost:8000
```

Use a local server rather than opening the file directly, so `/medical/` and the absolute links resolve.
