# Dharmacorpus website

Redesign of [dharmacorpus.in](https://www.dharmacorpus.in) — the website of Dharmacorpus, a legal practice in Dharmapuri, Tamil Nadu.

ASP.NET Core MVC (.NET 10) · Tailwind CSS v4 · Flowbite · jQuery · server-rendered HTML for Google and AI crawlers.

## Pages

| URL | Page |
| --- | --- |
| `/` | Home: situation finder, practice areas, process, courts, advocates, FAQs |
| `/about` | About the practice |
| `/practice-areas` | All 11 practice areas with filter |
| `/practice-areas/{slug}` | One page per practice area (11) |
| `/team` | Advocates |
| `/team/{slug}` | One profile per advocate (4) |
| `/contact` | Office details and enquiry form |
| `/disclaimer`, `/privacy-policy`, `/terms-of-use` | Legal pages |
| `/sitemap.xml`, `/robots.txt`, `/llms.txt` | Generated for search engines and AI crawlers |

Existing URLs from the current site are kept, so search rankings carry over.

## Run it locally

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js 20+.

```bash
cd src/Dharmacorpus.Web
npm ci              # first time only
dotnet run          # builds Tailwind CSS automatically, then starts the site
```

The build runs `npm run build`, which compiles `Styles/app.css` into `wwwroot/css/site.css` and copies jQuery, Flowbite and the fonts into `wwwroot`. While styling, `npm run watch:css` rebuilds CSS on every save. Pass `-p:SkipClientBuild=true` to skip the npm step.

## Editing content

Text lives in JSON files, not in the code:

- `Content/en/site.json` — name, phone, email, address, opening hours, headline numbers
- `Content/en/practice-areas.json` — the 11 practice areas (summary, scope, "who this is for", related areas)
- `Content/en/team.json` — advocates (to add a photo, put it in `wwwroot/img/team/` and set `"photo": "/img/team/name.jpg"`)
- `Content/en/faqs.json` — questions on the home page

Restart the app after editing. The street address and opening hours in `site.json` are empty until the practice confirms them; once filled they appear in the structured data automatically.

## Enquiry form email

Enquiries are emailed to the office and not stored on the server. Until SMTP is configured, the form tells visitors to call or email instead, so nothing is lost silently.

Never commit the password. Set it as environment variables on the server:

```
Smtp__Host=smtp.example.com
Smtp__Port=587
Smtp__UserName=...
Smtp__Password=...
Smtp__From=website@dharmacorpus.in
Smtp__To=contact@dharmacorpus.in
```

or locally with `dotnet user-secrets set "Smtp:Password" "..."`.

## Going live

- Set `Site__CanonicalHost=www.dharmacorpus.in` so the bare domain redirects to `www`.
- Serve over HTTPS (HSTS is enabled outside Development).
- `Dockerfile` builds a production image: `docker build -t dharmacorpus . && docker run -p 8080:8080 dharmacorpus`. It also runs on Azure App Service, IIS (`dotnet publish -c Release`) or any host that runs .NET 10.
- After launch: submit `https://www.dharmacorpus.in/sitemap.xml` in Google Search Console and Bing Webmaster Tools, and link the Google Business Profile.

## Security

- Strict Content-Security-Policy (`'self'` only — every script, style and font is self-hosted), plus `X-Frame-Options`, `nosniff`, `Referrer-Policy`, `Permissions-Policy`, HSTS.
- Enquiry form: anti-forgery token, server-side validation, honeypot field, rate limit of 5 submissions per 10 minutes per IP, mail-header injection guarded.
- No database, no stored personal data, no secrets in the repository.
- `npm audit` reports 4 low-severity advisories in a build-time dependency of Flowbite (`deepmerge` via its datepicker's Rollup plugin). That code is not shipped to browsers; the only fix offered is downgrading Flowbite to v2.

## Compliance

The site follows Bar Council of India Rule 36: a first-visit disclaimer, no testimonials, no claims of results or rankings, and only factual particulars about the advocates. Page copy should be approved by the advocates before launch.

## Project layout

```
src/Dharmacorpus.Web/
├─ Controllers/      Home, PracticeAreas, Team, Contact, Legal, Seo, Error
├─ Models/           content records, SeoMeta, EnquiryForm
├─ Services/         ContentService (JSON), SeoService (JSON-LD), EnquiryService (SMTP)
├─ Infrastructure/   security headers, canonical host redirect, icons, options
├─ Views/            Razor pages and shared partials
├─ Content/en/       site content (JSON)
├─ Styles/app.css    Tailwind entry and "Navy & Ivory" design tokens
└─ wwwroot/          js/site.js, favicon; build output in css/, lib/, fonts/
```
