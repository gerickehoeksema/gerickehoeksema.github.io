# Clean URL Routing Guide

This guide explains how to make `/medical` work without the `.html` extension, future-proofing your QR codes for technology stack migrations.

## 🎯 The Problem

You want QR codes to point to:
- `https://yourdomain.com/medical`

Instead of:
- `https://yourdomain.com/medical.html`

This way, if you migrate to Next.js, React, or any other framework later, you can keep the same QR code URLs.

## ✅ Solutions by Hosting Platform

### 1. **GitHub Pages** (Your Current Setup)

GitHub Pages doesn't support URL rewriting out of the box, but we've implemented a clever workaround:

**How it works:**
- Created a `404.html` that intercepts `/medical` requests
- JavaScript automatically redirects `/medical` → `/medical.html`
- Happens instantly, users won't notice

**Files needed:**
- ✅ `404.html` (already included)
- ✅ `script.js` with medical route handler (already included)

**Limitations:**
- There's a brief flash as it redirects through 404 page
- Not true URL rewriting (but works perfectly for QR codes)

**GitHub Pages Setup:**
```bash
# Just deploy normally - 404.html handles everything
git add .
git commit -m "Add clean URL routing"
git push origin main
```

**Testing on GitHub Pages:**
- Both `yourdomain.com/medical` and `yourdomain.com/medical.html` will work
- QR codes can use the clean URL: `yourdomain.com/medical`

---

### 2. **Apache / cPanel / Shared Hosting** (Recommended for Production)

Most traditional web hosts (like Bluehost, HostGator, GoDaddy) use Apache.

**Files included:**
- ✅ `.htaccess` (already created)

**How it works:**
- Apache reads `.htaccess` rules
- Automatically adds `.html` extension
- True URL rewriting (no redirect, instant)

**Setup:**
1. Upload `.htaccess` to your web root
2. Ensure mod_rewrite is enabled (usually is by default)
3. Both `/medical` and `/medical.html` work instantly

**Testing:**
```bash
# .htaccess should be in the same directory as index.html
ls -la
# Should see: .htaccess index.html medical.html etc.
```

**Note:** `.htaccess` files are hidden on Unix systems (starts with dot). Make sure to upload it!

---

### 3. **Netlify** (Easiest & Best Option)

Netlify is a free modern hosting platform with excellent routing support.

**Files included:**
- ✅ `_redirects` (already created)

**How it works:**
- Netlify reads `_redirects` file
- Handles routing natively
- Supports SPA routing perfectly

**Setup:**
1. Sign up for free at netlify.com
2. Connect your GitHub repository
3. Deploy automatically
4. Add custom domain

**Netlify Setup Steps:**
```bash
# 1. Push to GitHub
git add .
git commit -m "Add Netlify routing"
git push

# 2. On Netlify dashboard:
# - "Add new site" → "Import from Git"
# - Select your repository
# - Build settings: leave blank (static site)
# - Deploy!
```

**Netlify Advantages:**
- Free SSL certificate
- Automatic deployments from GitHub
- True URL rewriting (no redirects)
- Better performance (global CDN)
- Form handling, serverless functions

---

### 4. **Vercel** (Alternative to Netlify)

Similar to Netlify, great for modern web apps.

**Create `vercel.json`:**
```json
{
  "cleanUrls": true,
  "trailingSlash": false,
  "redirects": [
    {
      "source": "/medical",
      "destination": "/medical.html"
    }
  ]
}
```

**Setup:**
1. Install Vercel CLI: `npm i -g vercel`
2. Deploy: `vercel`
3. Follow prompts to connect GitHub

---

### 5. **Cloudflare Pages** (Another Alternative)

Free hosting with Cloudflare's global network.

**Create `_redirects`:** (same as Netlify)
```
/medical  /medical.html  200
```

**Setup:**
1. Sign up at pages.cloudflare.com
2. Connect GitHub repository
3. Deploy automatically

---

## 🚀 Migration Scenarios

### Scenario 1: Stay on GitHub Pages (Simple)

**Current setup works:**
- Deploy `404.html` (already included)
- QR codes use: `yourdomain.com/medical`
- Works immediately, no config needed

**Pros:**
- Free
- Simple
- Works with custom domains

**Cons:**
- Brief 404 redirect (usually unnoticeable)
- Not true URL rewriting

---

### Scenario 2: Move to Netlify (Recommended)

**Best for:**
- Clean URLs without hacks
- Future React/Next.js migrations
- Better performance

**Migration steps:**
1. Keep same GitHub repository
2. Connect Netlify to your GitHub
3. Point your domain DNS to Netlify
4. Everything works instantly with `_redirects`

**Zero QR code changes needed!**

---

### Scenario 3: Traditional Web Host (cPanel)

**If you prefer traditional hosting:**

1. Download all files from GitHub
2. Upload to web host via FTP/cPanel
3. Upload `.htaccess` file
4. Point domain to hosting

**Apache handles clean URLs automatically**

---

### Scenario 4: Future Framework Migration

When you eventually migrate to React, Next.js, or another framework:

**The beauty of clean URLs:**

```javascript
// Next.js pages/medical.js
export default function Medical() {
  return <MedicalInfo />
}
// Automatically available at /medical
```

```javascript
// React Router
<Route path="/medical" element={<Medical />} />
```

**Your QR codes continue to work - no reprinting needed!**

---

## 📱 QR Code Best Practices

### What to encode in QR:

✅ **Good:**
```
https://yourdomain.com/medical
```

❌ **Avoid:**
```
https://yourdomain.com/medical.html
```

### QR Code Generation:

```bash
# Using qrencode (Linux/Mac)
qrencode -o medical-qr.png -s 10 "https://yourdomain.com/medical"

# Python
import qrcode
qr = qrcode.QRCode(version=1, box_size=10, border=4)
qr.add_data("https://yourdomain.com/medical")
qr.make(fit=True)
img = qr.make_image(fill_color="red", back_color="white")
img.save("medical-qr.png")
```

---

## 🧪 Testing Your Setup

### Test Checklist:

1. **Test with .html extension:**
   ```
   https://yourdomain.com/medical.html
   ```
   Should work ✅

2. **Test without extension:**
   ```
   https://yourdomain.com/medical
   ```
   Should redirect/rewrite to medical.html ✅

3. **Test with trailing slash:**
   ```
   https://yourdomain.com/medical/
   ```
   Should work ✅

4. **Test QR code:**
   - Generate QR with clean URL
   - Scan with phone
   - Should load medical page ✅

### Browser Testing:

```javascript
// Open browser console (F12) and test:
fetch('https://yourdomain.com/medical')
  .then(r => console.log('Status:', r.status, r.url))
  
// Should show: Status: 200 (and final URL)
```

---

## 🔄 Current Status Summary

**Files you have:**

| File | Purpose | Platform |
|------|---------|----------|
| `404.html` | GitHub Pages routing | GitHub Pages |
| `.htaccess` | Clean URLs | Apache/cPanel |
| `_redirects` | Clean URLs | Netlify/Cloudflare |
| `script.js` | Fallback handler | All platforms |

**What works now:**
- ✅ GitHub Pages: `/medical` redirects via 404 handler
- ✅ Apache: `/medical` rewrites via .htaccess
- ✅ Netlify: `/medical` routes via _redirects
- ✅ All platforms: `/medical.html` works directly

**Your QR codes are future-proof! 🎉**

---

## 🚨 Troubleshooting

### Issue: /medical shows 404

**GitHub Pages:**
- Check `404.html` is uploaded
- Wait 5-10 minutes for deployment
- Clear browser cache

**Apache:**
- Check `.htaccess` is uploaded (it's hidden!)
- Verify mod_rewrite is enabled
- Check file permissions (644)

**Netlify:**
- Check `_redirects` is in root directory
- Redeploy site
- Check deploy logs

### Issue: Redirect loop

- Check for conflicting rules in `.htaccess`
- Verify you're not redirecting `/medical.html` → `/medical` → `/medical.html`
- Our configs avoid this

### Issue: Works locally but not online

- Local testing doesn't reflect server behavior
- Use browser network tab to debug
- Check actual HTTP headers

---

## 📚 Additional Resources

- **GitHub Pages**: https://docs.github.com/pages
- **Netlify Redirects**: https://docs.netlify.com/routing/redirects/
- **Apache mod_rewrite**: https://httpd.apache.org/docs/current/mod/mod_rewrite.html
- **Testing Tool**: https://httpstatus.io/

---

## 🎯 Recommendation

**For your use case (QR codes on bicycle stickers):**

1. **Start with GitHub Pages** (free, simple, works now)
2. **Print QR codes** with clean URL: `yourdomain.com/medical`
3. **Later migrate to Netlify** if you want better routing
4. **QR codes never need reprinting!**

**Best long-term solution:**
- Netlify (free tier is generous)
- Clean URLs work natively
- Easy framework migrations later
- Better performance

---

**Bottom line:** All your routing needs are covered. Use GitHub Pages now, migrate later if needed, QR codes always work! 🚴‍♂️
