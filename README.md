# LinkedIn-Style Professional Profile Site

A modern, responsive Single Page Application (SPA) with LinkedIn-inspired design for your professional profile, experience showcase, and blog.

## Features

- ✨ LinkedIn-style professional layout
- 🖼️ Profile picture and banner image support
- 📱 Fully responsive design
- 💼 Experience timeline with detailed achievements
- 🎓 Education and certifications sections
- 📝 Articles/blog section ready for content
- 🚴 Emergency medical info page (QR code accessible)
- ⚡ Fast, lightweight, and SEO-friendly

## Files Structure

```
├── index.html          # Main profile page (SPA)
├── medical.html        # Emergency medical information page
├── styles.css          # Professional LinkedIn-style stylesheet
├── script.js           # SPA navigation logic
├── images/             # Your images folder (create this)
│   ├── profile.jpg     # Your profile picture
│   ├── banner.jpg      # Your banner image (optional)
│   └── article1.jpg    # Article images (optional)
└── README.md          # This file
```

## Setup Instructions

### 1. Prepare Your Images

Create an `images` folder in your project directory:

```bash
mkdir images
```

**Profile Picture:**
- Recommended size: 400x400 pixels
- Format: JPG, PNG, or WebP
- Square aspect ratio works best
- Name it: `profile.jpg` (or update the HTML to match your filename)

**Banner Image (Optional):**
- Recommended size: 1584x396 pixels
- Format: JPG, PNG, or WebP
- Wide landscape format
- Name it: `banner.jpg`

**To add your banner:**
1. Add image to `images/banner.jpg`
2. In `index.html`, uncomment the banner image line:
```html
<!-- Remove the <!-- and --> to uncomment -->
<img src="images/banner.jpg" alt="Profile Banner" class="banner-image">
```

**Article Images (Optional):**
- Recommended size: 800x450 pixels (16:9 ratio)
- Name them: `article1.jpg`, `article2.jpg`, etc.

### 2. Image Optimization Tips

For best performance:

```bash
# Using ImageMagick to resize and optimize
convert input.jpg -resize 400x400^ -gravity center -extent 400x400 -quality 85 profile.jpg

# For banner
convert input.jpg -resize 1584x396^ -gravity center -extent 1584x396 -quality 85 banner.jpg
```

Or use online tools:
- https://squoosh.app/ (Google's image optimizer)
- https://tinypng.com/ (PNG compression)
- https://compressor.io/ (Multi-format)

### 3. Create GitHub Repository

1. Go to GitHub and create a new repository
2. Name it: `yourusername.github.io` (replace `yourusername` with your GitHub username)
3. Make it public
4. Don't initialize with README

### 4. Upload Files to GitHub

**Option A: Using Git (Command Line)**

```bash
# Navigate to your project folder
cd your-project-folder

# Initialize git
git init

# Add all files
git add .

# Commit files
git commit -m "Initial commit: Professional profile site"

# Add remote repository
git remote add origin https://github.com/yourusername/yourusername.github.io.git

# Push to GitHub
git branch -M main
git push -u origin main
```

**Option B: Using GitHub Web Interface**

1. Go to your repository
2. Click "Add file" → "Upload files"
3. Drag and drop all files including the `images` folder
4. Commit changes

### 5. Enable GitHub Pages

1. Go to your repository Settings
2. Navigate to "Pages" in the left sidebar
3. Under "Source", select "main" branch
4. Click "Save"
5. Your site will be live at: `https://yourusername.github.io`

### 6. Configure Custom Domain

1. In your domain registrar (where you bought your domain):
   - Add a CNAME record pointing to `yourusername.github.io`
   - OR add A records pointing to GitHub's IPs:
     - 185.199.108.153
     - 185.199.109.153
     - 185.199.110.153
     - 185.199.111.153

2. In your GitHub repository:
   - Go to Settings → Pages
   - Under "Custom domain", enter your domain name
   - Click "Save"
   - Enable "Enforce HTTPS" (wait a few minutes for SSL certificate)

3. Wait 24-48 hours for DNS propagation

## Customization Guide

### Update Personal Information

**1. Profile Header (Home Page):**

In `index.html`, update:
- `Your Name` → Your actual name
- `Software Architect | Azure Cloud Solutions Expert | Microservices Specialist` → Your headline
- `Your Location` → Your city/country
- `500+ connections` → Your actual network size (or remove)

**2. About Section:**

Update the about text to reflect your experience and focus areas.

**3. Skills & Expertise:**

Replace the example skills with your actual expertise. Keep the icon + title + description format.

**4. Experience Section:**

Replace with your actual work experience:
- Job titles
- Company names
- Dates
- Descriptions
- Key achievements

**5. Education & Certifications:**

Add your degrees and professional certifications.

**6. Contact Information:**

Update all contact links:
- Email address
- LinkedIn profile URL
- GitHub username
- Personal website

### Adding Profile Picture

1. Place your profile picture in `images/profile.jpg`
2. The site will automatically display it
3. If no image is found, a placeholder icon appears

### Adding Banner Image

1. Place your banner image in `images/banner.jpg`
2. In `index.html`, find this line in the profile-banner section:
```html
<!-- <img src="images/banner.jpg" alt="Profile Banner" class="banner-image"> -->
```
3. Remove the `<!--` and `-->` to uncomment it

### Color Customization

Update CSS variables in `styles.css`:

```css
:root {
    --primary-color: #0a66c2;    /* LinkedIn blue - change to your brand color */
    --secondary-color: #000000;   /* Text headings */
    --bg-color: #f3f2ef;         /* Page background */
}
```

### Adding Articles/Blog Posts

**Method 1: Static HTML (Simple)**

Edit the articles-grid section in `index.html` and add more article cards:

```html
<div class="article-card">
    <div class="article-image-placeholder">
        <img src="images/article1.jpg" alt="Article title">
    </div>
    <div class="article-content">
        <h3>Your Article Title</h3>
        <p class="article-meta">Published on January 26, 2026 • 5 min read</p>
        <p>Article excerpt goes here...</p>
        <a href="article-detail.html">Read more →</a>
    </div>
</div>
```

**Method 2: Dynamic Loading (Advanced)**

Create a `blog-posts.js` file:

```javascript
const articles = [
    {
        title: "Understanding Microservices Patterns",
        date: "2026-01-23",
        readTime: "8 min read",
        excerpt: "Deep dive into common microservices patterns...",
        image: "images/article1.jpg",
        link: "article1.html"
    }
];
```

Then modify the blog page to load articles dynamically.

**Method 3: Use Jekyll (GitHub Pages Native)**

GitHub Pages supports Jekyll out of the box:

1. Add a `_config.yml` file
2. Create a `_posts` folder for blog posts
3. Write posts in Markdown
4. Jekyll will generate your blog automatically

## Generating QR Code for Medical Page

### Method 1: Online QR Code Generator

1. Go to:
   - https://www.qr-code-generator.com/
   - https://www.qrcode-monkey.com/
   
2. Enter your medical page URL:
   - `https://yourdomain.com/medical.html`

3. Customize:
   - Add red/white colors for medical theme
   - Add frame with "EMERGENCY INFO"
   - Add medical cross icon

4. Download high-resolution (300 DPI minimum)

5. Print on weatherproof vinyl stickers

### Method 2: Command Line

```bash
# Install qrencode
sudo apt-get install qrencode  # Ubuntu/Debian
brew install qrencode           # macOS

# Generate QR code
qrencode -o medical-qr.png -s 10 "https://yourdomain.com/medical.html"
```

### Method 3: Python Script

```python
import qrcode

qr = qrcode.QRCode(version=1, box_size=10, border=4)
qr.add_data("https://yourdomain.com/medical.html")
qr.make(fit=True)

img = qr.make_image(fill_color="red", back_color="white")
img.save("medical-qr.png")
```

## Professional Image Tips

### Profile Picture Best Practices

✅ **Do:**
- Use a professional headshot
- Smile and look approachable
- Use good lighting
- Wear professional attire
- Use a neutral background
- Face the camera directly
- Show shoulders and head

❌ **Don't:**
- Use group photos
- Use overly casual photos
- Use low-quality/blurry images
- Use heavily filtered photos
- Crop from full-body shots

### Banner Image Ideas

- Cityscape of where you work/live
- Tech conference photos
- Abstract technology patterns
- Your work environment
- Gradient designs matching your brand colors
- Mountains/landscapes (professional)

### Where to Get Free Professional Images

- **Unsplash** (unsplash.com) - High-quality free photos
- **Pexels** (pexels.com) - Free stock photos
- **Canva** (canva.com) - Create custom banners
- **Remove.bg** - Remove background from profile pictures

## Testing Your Site Locally

### Option 1: Simple HTTP Server (Python)

```bash
# Python 3
python -m http.server 8000

# Then visit: http://localhost:8000
```

### Option 2: Live Server (VS Code Extension)

1. Install "Live Server" extension in VS Code
2. Right-click `index.html`
3. Select "Open with Live Server"

### Option 3: Node.js

```bash
npx http-server
```

## Responsive Testing

Test your site on different screen sizes:

1. **Desktop**: 1920x1080, 1366x768
2. **Tablet**: 768x1024 (iPad)
3. **Mobile**: 375x667 (iPhone), 360x740 (Android)

Use Chrome DevTools (F12) → Toggle device toolbar (Ctrl+Shift+M)

## SEO Optimization

### Update Meta Tags

In `index.html`, update the `<head>` section:

```html
<meta name="description" content="Your professional description here">
<meta property="og:title" content="Your Name - Software Architect">
<meta property="og:description" content="Azure Cloud Solutions Expert specializing in microservices">
<meta property="og:image" content="https://yourdomain.com/images/profile.jpg">
<meta property="og:url" content="https://yourdomain.com">
<meta name="twitter:card" content="summary_large_image">
```

### Add Structured Data

Add JSON-LD schema for better SEO:

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org/",
  "@type": "Person",
  "name": "Your Name",
  "jobTitle": "Software Architect",
  "url": "https://yourdomain.com",
  "sameAs": [
    "https://linkedin.com/in/yourprofile",
    "https://github.com/yourusername"
  ]
}
</script>
```

## Performance Optimization

1. **Optimize Images**: Use WebP format when possible
2. **Lazy Loading**: Add `loading="lazy"` to images
3. **Minify CSS/JS**: Use online minifiers before deploying
4. **CDN**: GitHub Pages serves from CDN automatically
5. **Caching**: GitHub Pages handles this automatically

## Security & Privacy

### Medical Page Considerations

⚠️ **Important:**
- Only include info for emergency responders
- Don't include full medical history
- Don't include SSN or insurance card photos
- Review and update every 6 months
- Consider password protection for sensitive info

### Password Protecting Medical Page

GitHub Pages doesn't support server-side authentication, but you can:

1. Use JavaScript-based protection (basic):
```javascript
const password = prompt("Enter password:");
if (password !== "your-password") {
    window.location.href = "index.html";
}
```

2. Use a third-party service like Netlify (supports password protection)

## Maintenance Checklist

- [ ] Update profile picture yearly
- [ ] Update experience as you change jobs
- [ ] Add new certifications when earned
- [ ] Review medical info every 6 months
- [ ] Add new articles/blog posts regularly
- [ ] Update skills as you learn new technologies
- [ ] Check all links work (contact info, social media)
- [ ] Test site on mobile devices
- [ ] Review and update meta descriptions

## Troubleshooting

**Images not showing:**
- Check file paths are correct
- Ensure images are in `images/` folder
- Verify image file extensions match (case-sensitive)
- Clear browser cache

**Site not updating:**
- GitHub Pages can take 5-10 minutes to deploy
- Clear browser cache
- Check GitHub Actions for build errors

**Custom domain not working:**
- Wait 24-48 hours for DNS propagation
- Verify DNS records are correct
- Check HTTPS is enabled in GitHub Pages settings

## Support & Resources

- **GitHub Pages Documentation**: https://docs.github.com/pages
- **LinkedIn Design Guidelines**: For inspiration
- **Chrome DevTools**: For testing and debugging
- **W3C Validator**: Validate your HTML

## License

Feel free to use this template for your personal professional website.

---

**Pro Tip**: Update your site regularly to keep it fresh and relevant. Add articles to establish thought leadership in your field!
