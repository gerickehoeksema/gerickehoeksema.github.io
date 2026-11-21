# gerickehoeksema.co.za

Personal profile site for Gericke Hoeksema, hosted on GitHub Pages at <https://gerickehoeksema.co.za>.

Plain HTML, CSS and JavaScript with no build step (yet; see [`PLAN-v3.md`](PLAN-v3.md)).

## Structure

```
index.html            Profile: Home, About, Experience, Projects, Articles (hash-based pages)
styles.css            All styles, in @layers; colour tokens are light-dark() pairs
script.js             Page switching between the sections in index.html
articles/<slug>/       One folder per article: index.html + og.jpg (1200x630 share image)
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

## Publishing an article

Articles are plain pages at `/articles/<slug>/` until the Phase 2 builder takes over (the URLs stay the same).

1. Replace every `<mark class="todo">` with real text (`grep -n 'class="todo"' articles/*/index.html` must return nothing).
2. Remove the `draft-banner` paragraph and the `<meta name="robots" content="noindex">` tag.
3. Set the real date in `<time datetime>` and `datePublished` in the JSON-LD.
4. Add the URL to `sitemap.xml`.
5. After it is live, paste the URL into LinkedIn's [Post Inspector](https://www.linkedin.com/post-inspector/) so the preview card is fresh.

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
