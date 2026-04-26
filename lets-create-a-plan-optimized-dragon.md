# Plan: Profile Site — LinkedIn-Inspired Dark Theme + Medical Page + Custom Domain

## Context

Gericke wants a personal profile site hosted on GitHub Pages at `gerickehoeksema.co.za`. The primary goals are:
1. A clean, professional profile page (LinkedIn-inspired layout, dark theme already in place).
2. A medical info page excluded from all indexing — accessible via QR code sticker on bikes for emergencies.

The codebase (`gerickehoeksema.github.io-feat/`) already has a working foundation: SPA routing, a medical page with the correct noindex/noarchive meta tags, robots.txt exclusions, and real personal medical data already populated. The work is mostly refinement and wiring up the domain.

---

## What Already Exists (no changes needed)

- `medical.html` — **completely untouched**. It has its own distinct emergency styling (red badge, dark blue background, amber warning box) that intentionally looks different from the profile site. Real data already populated. `noindex/nofollow/noarchive` meta tags, no-cache headers, and print-friendly styles are all in place.
- `robots.txt` — already disallows `/medical` and `/medical.html`.
- `404.html` — GitHub Pages SPA routing handler, already handles `/medical` → `/medical.html`.
- `_redirects` — Netlify routing (not needed for GitHub Pages but harmless).
- `.htaccess` — Apache config (not used on GitHub Pages but harmless).
- `script.js` — SPA navigation + `/medical` route handler already works.
- `images/` — `profile.jpg` and `banner.jpg` already present.

---

## Changes Required

### 1. Add `CNAME` file (new file)
**File:** `gerickehoeksema.github.io-feat/CNAME`  
**Content:** `gerickehoeksema.co.za`  
GitHub Pages reads this file to bind the custom domain. Without it, the custom domain won't work.

> After pushing, in GitHub repo → Settings → Pages → verify the domain is set and "Enforce HTTPS" is enabled. DNS: add a CNAME record pointing `gerickehoeksema.co.za` → `gerickehoeksema.github.io` with the domain registrar.

---

### 2. LinkedIn-inspired layout refinements (`styles.css`)

Keep the existing dark color palette. Refine the layout/feel to match LinkedIn's professional aesthetic:

**Navbar:**
- Keep dark background but tighten to `52px` height (LinkedIn nav is compact).
- Use a subtle bottom border (`1px solid #1e293b`) instead of a glow/shadow.
- Nav links: remove glow effects, use clean underline-on-active indicator (a `3px` blue bottom border on the active link, like LinkedIn's tab indicator).

**Profile header card:**
- Banner height: `200px` (already correct).
- Profile picture: keep circular, add a `4px white border` ring — signature LinkedIn look.
- Name: `24px`, bold, `--text-primary`.
- Headline: `16px`, `--text-secondary`.
- Location: `14px`, `--text-muted` with pin icon inline.
- "Connect" / "More" button group: primary button filled blue (`--primary-blue`), secondary button outlined.

**Section cards:**
- Add a consistent `border: 1px solid var(--border-color)` on all cards.
- Section title: `18px`, bold, with a thin `3px` left blue border accent (LinkedIn section headers).
- Consistent `24px` padding inside cards.
- Remove glow/shadow effects — replace with a single subtle `box-shadow: 0 1px 3px rgba(0,0,0,0.3)`.

**Skills/competencies grid:**
- Keep 2-column auto-fit grid.
- Each card: pill/chip style tags for sub-items instead of paragraph text.

**Experience timeline:**
- Keep the vertical timeline but make the dot markers solid circles (LinkedIn uses company logo circles — we use solid blue dots).
- Role title: bold `16px`. Company: `14px --text-secondary`. Dates: `12px --text-muted`.

**Typography cleanup:**
- Use a consistent type scale: `24/18/16/14/12px`.
- Reduce decorative glow text effects to zero — LinkedIn is clean/flat.

---

### 3. Personal content in `index.html`

The HTML still has placeholder text. Replace with Gericke's real info (known from medical.html and context):

- **Name:** Gericke Hoeksema
- **Location:** South Africa
- **Headline, skills, experience, certifications:** need to be filled in — Gericke to provide, or we fill with best-guess from context (Software Architect / Azure / .NET based on existing placeholder copy) and he edits.
- **Contact links:** email, LinkedIn URL, GitHub username — Gericke to provide.

> Plan: fill in what's known, leave clear `<!-- TODO: -->` markers for anything unknown.

---

## Files to Modify

| File | Change |
|------|--------|
| `gerickehoeksema.github.io-feat/CNAME` | **Create** — `gerickehoeksema.co.za` |
| `gerickehoeksema.github.io-feat/styles.css` | LinkedIn-inspired layout refinements (navbar, cards, typography) |
| `gerickehoeksema.github.io-feat/index.html` | Replace placeholder text with Gericke's real info |

**Not touching:**
- `medical.html` — intentionally kept with its own emergency style (red/dark/amber), completely separate from the profile look
- `script.js` — routing works correctly
- `404.html` — GitHub Pages handler works correctly
- `robots.txt` — already excludes medical page

---

## QR Code (post-deploy action — not code)

After the site is live at `gerickehoeksema.co.za`, generate the QR code pointing to:
```
https://gerickehoeksema.co.za/medical
```
The `/medical` clean URL is already handled by `404.html` → redirects to `medical.html`. This is the URL to encode in the QR code for the bike stickers.

---

## Verification

1. Serve locally with `python -m http.server 8000` — check all 5 nav sections load.
2. Navigate to `http://localhost:8000/medical.html` — verify emergency info displays correctly.
3. Confirm `robots.txt` at `/robots.txt` blocks `/medical`.
4. After GitHub Pages deploy: visit `https://gerickehoeksema.co.za` and `https://gerickehoeksema.co.za/medical`.
5. Confirm medical page does NOT appear in Google search results (check `site:gerickehoeksema.co.za/medical`).
