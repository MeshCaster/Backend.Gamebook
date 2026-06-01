# GameBook — Business & Third-Party Setup Guide

> Every external account, permission, key, and approval you need before going live.

---

## 1. Flitt (Payments)

**What it does:** Processes card payments (Visa/Mastercard), Apple Pay, Google Pay, refunds, subscriptions. Natively supports Georgian businesses and GEL currency.

**Docs:** https://docs.flitt.com/ | **Support:** support@flitt.com | **(032) 500 01 02**

### Steps
1. **Register as a merchant** at https://flitt.com/ → "Get Started"
   - Flitt supports businesses registered in Georgia, Uzbekistan, Moldova, Armenia
   - You will need:
     - Legal business name and registration documents
     - Bank account details for GEL payouts
     - Business owner identity verification
     - Website or app description
2. **Get your test credentials** from Flitt Merchant Portal:
   - `merchant_id` — your unique merchant identifier (e.g. `1549901`)
   - `payment_key` — secret key used to generate SHA1 signatures (test env uses `"test"`)
3. **Configure server callback URL** in Merchant Portal:
   - URL: `https://your-api-domain.com/v1/webhooks/flitt`
   - Flitt sends order status updates (approved, declined, refunded) to this URL
4. **Test with sandbox:**
   - Use test `merchant_id` and `payment_key` provided by Flitt
   - API endpoint: `https://pay.flitt.com/api/checkout/token` (same for test and live)
   - Test cards are provided in Flitt documentation
5. **Enable Apple Pay:**
   - Register Apple Pay merchant ID in Apple Developer portal (see section 3)
   - Request Flitt to enable Apple Pay on your merchant account
   - Configure Apple Pay certificate in Flitt Merchant Portal
6. **Enable Google Pay (requires approval):**
   1. Request Flitt to enable Google Pay on your merchant account
   2. Build with test merchant ID using `ENVIRONMENT_TEST`
   3. Request Google production access via Google Pay Business Console
   4. Submit app to Google for review
   5. Upon Google approval, request Flitt to activate live Google Pay credentials
   6. Switch to `ENVIRONMENT_PRODUCTION` in your app
   7. Submit production build with live merchant ID
7. **Go live:**
   - Request Flitt to switch your account from test to production
   - Replace test `merchant_id` and `payment_key` with live values

### Keys for backend `.env`
```
FLITT_MERCHANT_ID=1549901
FLITT_PAYMENT_KEY=your-payment-key
```

### Keys for mobile app
```
EXPO_PUBLIC_FLITT_MERCHANT_ID=1549901
```

### How authentication works
Flitt uses SHA1 signature-based authentication (not API keys):
- Sort all request parameters alphabetically
- Concatenate `payment_key` + sorted parameter values
- SHA1 hash the result → this is the `signature` parameter
- Every request and callback includes a `signature` for verification

### API integration pattern
```
Backend creates order → POST /api/checkout/token → receives token
Mobile app uses token with Flitt React Native SDK → user completes payment
Flitt sends callback to server_callback_url → backend updates booking status
```

### Pricing
- Contact Flitt for Georgia-specific rates (typically competitive with regional providers)
- GEL currency natively supported — no conversion fees

### Approval timeline
- Test credentials: contact Flitt support, typically within a few business days
- Live activation: after integration review by Flitt team

---

## 2. Supabase (Authentication & User Database)

**What it does:** Handles user sign-up, sign-in, JWT tokens, OAuth providers.

### Steps
1. **Create a Supabase project** at https://supabase.com/dashboard
2. **Copy your project credentials** from Settings → API:
   - Project URL → `SUPABASE_URL`
   - `anon` public key → `SUPABASE_ANON_KEY`
   - JWT secret → `SUPABASE_JWT_SECRET` (Settings → API → JWT Settings)
3. **Enable authentication providers** in Authentication → Providers:
   - **Email** — enabled by default, configure "Confirm email" toggle
   - **Google OAuth:**
     - Create OAuth credentials in Google Cloud Console (see section 6)
     - Paste Client ID and Client Secret into Supabase
   - **Apple Sign-In:**
     - Requires Apple Developer account (see section 3)
     - Create a Services ID and configure redirect URL
     - Paste credentials into Supabase
4. **Configure redirect URLs** in Authentication → URL Configuration:
   - Add `gamebook://auth-callback` (your app's deep link scheme)
5. **Optional — Supabase Storage** for user avatars:
   - Create a `avatars` bucket with public read access
   - Set max file size policy (e.g. 2MB)

### Keys for backend `.env`
```
SUPABASE_URL=https://your-project-id.supabase.co
SUPABASE_JWT_SECRET=your-jwt-secret
SUPABASE_ANON_KEY=eyJhbGciOi...
```

### Keys for mobile app
```
EXPO_PUBLIC_SUPABASE_URL=https://your-project-id.supabase.co
EXPO_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOi...
```

### Pricing
- Free tier: 50,000 monthly active users, 1GB database, 1GB storage
- Pro plan ($25/mo): 100K MAU, 8GB database, 100GB storage

### Approval timeline
- Instant — no approval needed

---

## 3. Apple Developer Program

**What it does:** Required for iOS App Store submission, Apple Sign-In, Apple Pay, push notifications (APNs).

### Steps
1. **Enroll** at https://developer.apple.com/programs/enroll/
   - Individual ($99/year) or Organization ($99/year, requires D-U-N-S number)
   - Organization enrollment requires a D-U-N-S number — apply free at https://developer.apple.com/enroll/duns-lookup/
2. **Create an App ID** in Certificates, Identifiers & Profiles:
   - Bundle ID: `com.gamebook.mobile`
   - Enable capabilities: Push Notifications, Sign In with Apple, Apple Pay (for Flitt)
3. **Create an APNs Key** (for push notifications):
   - Certificates, Identifiers & Profiles → Keys → Create a key
   - Enable "Apple Push Notifications service (APNs)"
   - Download the `.p8` key file (you can only download it once)
   - Upload this key to your Expo dashboard (see section 5)
4. **Configure Sign In with Apple:**
   - Create a Services ID for web-based OAuth (used by Supabase)
   - Configure the redirect URL from Supabase
5. **Apple Pay Merchant ID:**
   - Register a Merchant ID (e.g. `merchant.com.gamebook`)
   - Create a Payment Processing Certificate
   - Upload to Flitt Merchant Portal (request Apple Pay enablement from Flitt support)

### Costs
- $99/year for the developer program

### Approval timeline
- Individual: 24–48 hours
- Organization: 1–4 weeks (D-U-N-S verification)

---

## 4. Google Play Developer Account

**What it does:** Required for Android app distribution on Google Play Store.

### Steps
1. **Register** at https://play.google.com/console/signup
   - One-time $25 registration fee
   - Requires Google account and identity verification
2. **Create your app** in Play Console:
   - Package name: `com.gamebook.mobile`
   - Complete the store listing (title, description, screenshots, icon)
3. **Content rating** — complete the IARC questionnaire
4. **Data safety form** — declare what data you collect:
   - Email address (authentication)
   - Payment info (processed by Flitt, not stored by you)
   - Location (for nearby venues)
   - Name and avatar (profile)
5. **Set up Google Cloud project** for FCM (see section 6)

### Costs
- $25 one-time registration

### Approval timeline
- Account: 1–2 days (identity verification)
- First app review: 3–7 days

---

## 5. Expo (Build & Push Notifications)

**What it does:** Builds native iOS/Android binaries, manages push notification delivery, OTA updates.

### Steps
1. **Create an Expo account** at https://expo.dev/signup (free)
2. **Install EAS CLI:** `npm install -g eas-cli && eas login`
3. **Initialize EAS Build:**
   - Run `eas build:configure` in the project directory
   - This creates `eas.json` with build profiles
4. **Configure push notifications:**
   - **iOS:** Upload your APNs `.p8` key file:
     - Expo dashboard → Project → Credentials → iOS → Push Key
     - Or run `eas credentials` and follow prompts
   - **Android:** Upload FCM credentials:
     - Expo dashboard → Project → Credentials → Android → FCM V1 Service Account Key
     - Get this JSON key from Google Cloud Console (see section 6)
5. **Build your first binary:**
   - `eas build --platform ios` (requires Apple Developer account linked)
   - `eas build --platform android`
6. **Submit to stores:**
   - `eas submit --platform ios`
   - `eas submit --platform android`

### Keys
No env keys needed — Expo uses its own credential management.

### Pricing
- Free tier: 30 iOS + 15 Android builds/month, 1000 OTA updates
- Production plan ($99/mo): unlimited builds, 50K updates

### Approval timeline
- Instant — no approval needed

---

## 6. Google Cloud Console (OAuth + FCM)

**What it does:** Provides Google Sign-In OAuth credentials and Firebase Cloud Messaging for Android push notifications.

### Steps for Google OAuth (used by Supabase)
1. Go to https://console.cloud.google.com
2. Create a new project (e.g. "GameBook")
3. APIs & Services → OAuth consent screen:
   - User type: External
   - App name: GameBook
   - Authorized domains: your Supabase project domain
4. APIs & Services → Credentials → Create OAuth Client ID:
   - Application type: Web application
   - Authorized redirect URI: copy from Supabase Google provider settings
5. Copy Client ID and Client Secret → paste into Supabase dashboard

### Steps for FCM (Android push notifications)
1. Go to https://console.firebase.google.com
2. Create a Firebase project (link to your Google Cloud project)
3. Add an Android app with package `com.gamebook.mobile`
4. Download `google-services.json` → place in project root
5. Project Settings → Service Accounts → Generate new private key
6. Upload this JSON key to Expo dashboard as FCM V1 credentials

### Steps for Google Maps (venue map feature)
1. Google Cloud Console → APIs & Services → Enable "Maps SDK for Android" and "Maps SDK for iOS"
2. Create an API key, restrict to your app's bundle ID / package name
3. Add to `app.json`:
   ```json
   {
     "expo": {
       "ios": { "config": { "googleMapsApiKey": "AIza..." } },
       "android": { "config": { "googleMaps": { "apiKey": "AIza..." } } }
     }
   }
   ```

### Pricing
- OAuth: free
- FCM: free (up to billions of messages)
- Maps SDK: $200/mo free credit, then $7 per 1000 requests

### Approval timeline
- Instant for development (unverified app)
- OAuth consent screen verification: 2–6 weeks (required when you exceed 100 users)

---

## 7. Domain & Hosting (Backend)

**What it does:** Hosts the ASP.NET Core API and database in production.

### Recommended options
| Provider | Approach | Estimated cost |
|----------|----------|---------------|
| Railway | Docker deploy from GitHub | $5–20/mo |
| Fly.io | Docker deploy, auto-scaling | $5–15/mo |
| Azure App Service | .NET native, managed Postgres | $15–50/mo |
| DigitalOcean App Platform | Docker + managed DB | $12–30/mo |
| Self-hosted VPS (Hetzner) | Full control, cheapest | $5–10/mo |

### Steps (general)
1. **Register a domain** (e.g. `api.gamebook.ge`) — any registrar
2. **Set up managed PostgreSQL** (or self-host via Docker Compose)
3. **Set up managed Redis** (or self-host, needed for SignalR scaling)
4. **Deploy the Docker image** built from `infra/Dockerfile`
5. **Configure TLS/HTTPS** (Let's Encrypt or cloud provider)
6. **Set environment variables** (all keys from `.env.example`)
7. **Run database migration** on first deploy: `dotnet ef database update`
8. **Configure DNS** to point your domain to the deployment

---

## 8. Legal & Compliance

### Required before launch
- [ ] **Privacy Policy** — must disclose:
  - Data collected (email, name, location, payment info)
  - How payment data is handled (Flitt processes, you don't store card numbers)
  - Push notification opt-in/opt-out
  - Data retention and deletion policy
  - Third parties receiving data (Flitt, Supabase, Expo, Google)
- [ ] **Terms of Service** — must cover:
  - Booking and cancellation policy
  - Refund policy
  - User responsibilities
  - Limitation of liability
- [ ] **Cookie/tracking disclosure** — if using analytics
- [ ] **GDPR compliance** (if serving EU users):
  - Right to data export
  - Right to deletion (`DELETE /v1/users/me`)
  - Data processing agreements with Supabase and Flitt

### Georgian business requirements
- [ ] Business registration with Revenue Service
- [ ] Tax registration (if applicable for digital services)
- [ ] Consumer protection compliance for booking services

---

## 9. Partner Venue Agreements

### What you need from each venue
- [ ] Venue name, address, coordinates (for map)
- [ ] Station inventory (types, specs, hourly rates)
- [ ] Operating hours
- [ ] Logo and cover photos (high-res)
- [ ] Agreement on commission structure (% per booking)
- [ ] Agreement on cancellation/refund policy
- [ ] Point of contact for technical integration (QR scanner setup)
- [ ] Wi-Fi requirements for QR check-in device

### QR Check-In System
- [ ] Decide on QR scanner approach:
  - Tablet at reception running a simple web app that validates QR codes via API
  - Or staff uses a mobile app to scan
- [ ] Backend endpoint needed: `POST /v1/bookings/{id}/check-in` (not yet built)

---

## Summary: Account Setup Checklist

| Service | Required for | Cost | Setup time |
|---------|-------------|------|------------|
| Flitt | Payments | Contact for rates | Few business days |
| Supabase | Auth + user DB | Free–$25/mo | Instant |
| Apple Developer | iOS + Apple Sign-In + APNs | $99/year | 1–4 weeks |
| Google Play Console | Android distribution | $25 one-time | 1–2 days |
| Expo | Builds + push delivery | Free–$99/mo | Instant |
| Google Cloud | OAuth + FCM + Maps | Free–$7/1K map loads | Instant (verification 2–6 weeks) |
| Domain registrar | API hosting | $10–15/year | Instant |
| Hosting provider | Backend + DB + Redis | $10–50/mo | 1 day |

### Recommended order
1. Supabase (instant, unblocks all auth development)
2. Flitt test credentials (contact support, unblocks payment development)
3. Google Cloud project (instant, unblocks OAuth + Maps)
4. Expo account (instant, unblocks builds)
5. Apple Developer enrollment (start early — can take weeks for organizations)
6. Google Play registration (start early — identity verification)
7. Domain + hosting (when ready for staging deployment)
8. Flitt live activation (when ready for real payments)
9. Google Pay production approval (submit after integration is tested)
10. Legal documents (before store submission)
11. Venue partner agreements (before launch)
