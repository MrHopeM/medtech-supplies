<div align="center">

# MedTech Supplies — Medical Sourcing Platform

**A dark, motion-rich web app for a South African medical-supply sourcing company.**
Enquiry-first (no online prices), built on the .NET stack, and designed to stand out in a very traditional industry.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-5C2D91?logo=blazor&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-8-blueviolet)
![Azure](https://img.shields.io/badge/Azure-App%20Service%20ready-0078D4?logo=microsoftazure&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

### ▶ [View the live site — medtechsupplies.runasp.net](http://medtechsupplies.runasp.net)

👤 **Author:** [Hope Mphulwane](https://www.linkedin.com/in/hope-mphulwane-0a5943b0/)

> The live demo is currently served over **http** while the host provisions an SSL certificate — if your browser forces https and shows a connection error, try the `http://` address directly. First load cold-starts (~20s) on the free tier.

</div>

---

## Overview

MedTech Supplies is a medical-equipment **sourcing partner** — a middleman that buys from wholesalers and supplies hospitals, clinics and pharmacies across South Africa. A standard e-commerce store doesn't fit that B2B model, so this app takes an **enquiry-first** approach:

> Clients browse the catalogue → add products to an **enquiry basket** → request a quote. The internal team receives the enquiry (with items and contact details) and follows up with tailored pricing. **No prices are shown online.**

On top of a solid .NET foundation, the front end is a **dark, futuristic experience** with real motion — designed to make the brand memorable.

## ✨ Highlights

- **Animated particle-network hero** rendered on a `<canvas>`
- **Glassmorphism** UI, **3D tilt-and-glow** cards, scroll **parallax & reveals**
- **Rotating headline** and animated **stat counters**
- Custom **"AI assistant" chat widget** — typing indicator + context-aware canned replies
- **Enquiry basket** persisted in the browser, with a clean quote-request flow
- **Staff back-office**: a searchable enquiries inbox with one-click reply/call
- **Transactional emails** to both the client (receipt) and the team (dispatch alert), with branded HTML templates
- **Pluggable email providers** — SendGrid, Brevo (API) or plain SMTP, switchable via config
- Fully **responsive**, **accessible** (respects `prefers-reduced-motion`), SAHPRA-compliant branding

## 🧱 Tech stack

| Layer | Technology |
|---|---|
| UI / App | ASP.NET Core **8**, **Blazor Web App** (Interactive Server), C# |
| Data | **Entity Framework Core 8** — SQLite (dev) → **Azure SQL** (prod) via one config switch |
| Auth | Cookie authentication for the staff area |
| Email | Provider abstraction: **SendGrid** / **Brevo API** / **SMTP** + branded HTML templates |
| Front-end motion | Hand-written **CSS + vanilla JS** (particle canvas, tilt, parallax, reveals) — no UI framework |
| Hosting | **Azure App Service**–ready |
| Tooling | JetBrains Rider, .NET User-Secrets |

## 🏗️ Architecture

```
MedTechSupplies.Web/
├── Domain/            # Entities (Product, Category, Enquiry, EnquiryItem, AppUser)
├── Data/              # AppDbContext (IDbContextFactory) + seeder
├── Services/          # Catalog, Cart (enquiry basket), Enquiry, Auth
│   └── Email/         # IEmailSender → SendGrid / Brevo / SMTP / dev-outbox + templates
├── Components/
│   ├── Layout/        # Store (dark themed) + Admin (light) layouts
│   ├── Shared/        # ProductCard, Icon set, ChatWidget
│   └── Pages/         # Home, Catalog, Product, Enquiry list, Request-a-quote, Admin inbox
└── wwwroot/           # app.css (theme + motion), app.js (motion engine), SVG logo
```

Design notes: the dark theme is **scoped** to the storefront so the admin stays clean and readable; the data layer uses `IDbContextFactory` (the recommended Blazor pattern); money-free by design; email sending never blocks checkout and falls back to a local outbox in development.

## 🚀 Getting started

**Requires the .NET 8 SDK.**

```bash
git clone <your-repo-url>
cd Medtechsupplies
dotnet run --project MedTechSupplies.Web
```

Open **http://localhost:5137** — the database is created and seeded automatically.
Staff inbox: **/admin** (demo login `admin` / `admin123`).

## ✉️ Email configuration

Emails work out of the box in a **dev outbox** (written to `App_Data/outbox/` as HTML). To send for real, set one provider via user-secrets:

```bash
# Brevo (API) — or use "SendGrid" / "Smtp"
dotnet user-secrets set "Email:Provider" "Brevo"
dotnet user-secrets set "Email:BrevoApiKey" "<key>"
```

> Any provider requires a **verified sender / authenticated domain** — this is anti-spam law, not a code setting.

## ☁️ Deploy to Azure App Service

```bash
dotnet publish MedTechSupplies.Web -c Release
```

Create a .NET 8 App Service, deploy the publish output, then in **Configuration** set:
`Database__Provider=SqlServer`, `ConnectionStrings__AzureSql=<...>`, and your email keys.

## 🗺️ Roadmap

- Real AI in the chat widget (LLM-backed) + hand-off to a human
- Azure SQL + EF Core migrations for production
- Domain authentication for branded transactional email
- Analytics on enquiry conversion

## 👤 Author

**Hope (Kgaogelo) Mphulwane** — [LinkedIn](https://www.linkedin.com/in/hope-mphulwane-0a5943b0/)

## 📄 License

MIT — see [LICENSE](LICENSE).
