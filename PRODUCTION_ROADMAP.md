# GameBook — Production Roadmap

> What remains before the app can serve real customers.

---

## 1. Authentication & User Management

### Done
- [x] Supabase JWT validation on backend
- [x] `useAuth` hook with session management on frontend
- [x] API client auto-attaches JWT to all requests
- [x] `/v1/users/me` GET/PUT endpoints

### TODO
- [ ] Supabase project: enable Email + Google + Apple OAuth providers
- [ ] Frontend: build sign-up / sign-in screens (email + social)
- [ ] Frontend: add "Forgot password" flow (Supabase `resetPasswordForEmail`)
- [ ] Frontend: persist session with `react-native-mmkv` (currently in-memory only)
- [ ] Backend: user profile creation on first login (upsert from Supabase claims)
- [ ] Backend: add `DELETE /v1/users/me` for GDPR account deletion
- [ ] Rate-limit auth endpoints (already configured in Program.cs, verify thresholds)

---

## 2. Payments (Flitt)

### Done
- [x] Backend payment service scaffold (needs rewrite from Stripe to Flitt)
- [x] Backend `/v1/payments/intent` endpoint (needs rewrite for Flitt order flow)
- [x] Backend `/v1/webhooks/stripe` endpoint (needs rename + rewrite for Flitt callbacks)
- [x] Frontend `lib/payments/stripe.ts` (needs rewrite with `@flittpayments/react-native-flitt`)

### TODO — Backend Migration (Stripe → Flitt)
- [ ] **Register as Flitt merchant** and obtain test credentials (see `BUSINESS_SETUP.md`)
- [ ] Replace NuGet `Stripe.net` with Flitt C# SDK (`FlittSDK` NuGet package)
- [ ] Rewrite `StripePaymentService` → `FlittPaymentService`:
  - Use `merchant_id` + `payment_key` for SHA1 signature auth
  - Create orders via `POST https://pay.flitt.com/api/checkout/token` (returns a token for mobile SDK)
  - Handle refunds/reversals via Flitt API
- [ ] Rename `WebhookEndpoints.cs` → update `/v1/webhooks/stripe` to `/v1/webhooks/flitt`
- [ ] Implement Flitt `server_callback_url` handler — verify signature, update `Payment` + `Booking` status
- [ ] Update `PaymentProvider` enum: replace `Stripe` with `Flitt`
- [ ] Update `.env.example` with `FLITT_MERCHANT_ID` and `FLITT_PAYMENT_KEY`
- [ ] Update `Program.cs` DI registration for new payment service

### TODO — Frontend Migration (Stripe → Flitt)
- [ ] Remove `@stripe/stripe-react-native` from `package.json`
- [ ] Install `@flittpayments/react-native-flitt` (`npm install @flittpayments/react-native-flitt --save`)
- [ ] Rewrite `lib/payments/stripe.ts` → `lib/payments/flitt.ts` using Flitt React Native SDK
- [ ] Build the payment screen (`app/booking/payment.tsx`) with Flitt checkout token flow
- [ ] Android: add `play-services-wallet` dependency and wallet metadata to `AndroidManifest.xml`
- [ ] iOS: configure Apple Pay merchant ID in Xcode Signing & Capabilities
- [ ] Handle payment errors and show user-friendly messages
- [ ] Test full payment flow end-to-end with Flitt test merchant credentials

### TODO — Apple Pay & Google Pay via Flitt
- [ ] Apple Pay: register merchant ID in Apple Developer portal, create Payment Processing Certificate, configure in Flitt merchant portal
- [ ] Google Pay: request Flitt to enable Google Pay on your merchant account, build with `ENVIRONMENT_TEST`, submit to Google for production approval, then switch to `ENVIRONMENT_PRODUCTION`

---

## 3. Push Notifications

### Done
- [x] Backend `ExpoPushService` sends to Expo push endpoint
- [x] Backend `BookingReminderJob` sends reminders 30 min before booking
- [x] Domain: `PushToken` entity with platform kind (iOS/Android)

### TODO
- [ ] Frontend: install `expo-notifications` and request permission on startup
- [ ] Frontend: register push token and POST to backend (`POST /v1/users/me/push-token` — endpoint not yet created)
- [ ] Backend: add `POST /v1/users/me/push-token` and `DELETE /v1/users/me/push-token` endpoints
- [ ] Backend: send push on booking confirmation, cancellation, friend request, invite
- [ ] For production builds: configure APNs key in Expo dashboard (iOS) and FCM credentials (Android) — see `BUSINESS_SETUP.md`
- [ ] Handle notification tap → deep link to relevant screen (booking ticket, friend request, etc.)
- [ ] Add notification preferences (user can toggle categories on/off)

---

## 4. Booking Flow (End-to-End)

### Done
- [x] Backend: `CreateBookingHandler` with overlap check, pricing, QR generation
- [x] Backend: `CancelBookingHandler` with cancellation fee logic
- [x] Backend: `GetVenueAvailability` generates hourly slots
- [x] Backend: `PricingService` with cancellation policy (free < 2h, 50% late fee)
- [x] Frontend: `app/booking/[venueId].tsx` placeholder
- [x] Frontend: `lib/store/bookingDraft.ts` Zustand store

### TODO
- [ ] Frontend: build the booking wizard — date picker → time slot grid → station selector → player count → confirm
- [ ] Frontend: integrate `StationRow` component for station selection
- [ ] Frontend: call `POST /v1/bookings` → then `POST /v1/payments/create-order` → then Flitt checkout
- [ ] Frontend: build ticket screen (`app/booking/ticket/[id].tsx`) with QR code from `react-native-qrcode-svg`
- [ ] Frontend: add booking cancellation UI with confirmation dialog
- [ ] Backend: add GIST exclusion constraint migration for time-range overlap on `bookings` table
- [ ] Backend: send SignalR `SlotUpdated` event when a booking is created/cancelled
- [ ] Add booking modification (change date/time/station within policy window)

---

## 5. Real-Time Availability (SignalR)

### Done
- [x] Backend: `VenueHub` with join/leave group by slug
- [x] Backend: `UserHub` with authenticated user groups
- [x] Backend: Redis backplane configured (optional)
- [x] Frontend: `useVenueAvailability` hook connecting to `/hubs/venue`

### TODO
- [ ] Backend: broadcast `SlotUpdated` from `CreateBookingHandler` and `CancelBookingHandler`
- [ ] Frontend: reflect real-time slot changes on venue detail and booking screens
- [ ] Add connection status indicator (connected/reconnecting/disconnected)
- [ ] Test with multiple concurrent users

---

## 6. Venue Map

### Done
- [x] `react-native-maps` installed
- [x] Backend: venues have `Latitude`/`Longitude` (NetTopologySuite `Point`)
- [x] Backend: `GET /v1/venues` supports lat/lng query params

### TODO
- [ ] Frontend: implement `app/(tabs)/map.tsx` with `MapView` and venue markers
- [ ] Add marker callout showing venue name, rating, "View" button
- [ ] Add user location permission request (`expo-location`)
- [ ] Sort venues by distance from user
- [ ] For Google Maps on iOS: add API key to `app.json` under `ios.config.googleMapsApiKey`

---

## 7. Reviews

### Done
- [x] Backend: `GET /v1/reviews/venue/{venueSlug}` and `POST /v1/reviews`
- [x] Frontend: `app/reviews/[venueSlug].tsx` placeholder
- [x] Frontend: API client functions in `lib/api/reviews.ts`

### TODO
- [ ] Frontend: build reviews list with `ReviewItem` component
- [ ] Frontend: build "Write a Review" form (star rating + comment)
- [ ] Backend: restrict reviews to users who have a completed booking at the venue
- [ ] Backend: prevent duplicate reviews per user per venue
- [ ] Add average rating recalculation on new review

---

## 8. Friends & Squad

### Done
- [x] Backend: `GET /v1/friends`, `POST /v1/friends/request`, `POST /v1/friends/{id}/accept`
- [x] Domain: `Friend` and `Invite` entities
- [x] Frontend: `app/(tabs)/squad.tsx` placeholder
- [x] Frontend: API client in `lib/api/friends.ts`

### TODO
- [ ] Frontend: build friends list with search/add functionality
- [ ] Frontend: friend request accept/reject UI
- [ ] Backend: add `POST /v1/bookings/{id}/invite` to invite friends to a session
- [ ] Frontend: invite friends during booking flow
- [ ] Push notification on friend request and invite
- [ ] Add "remove friend" functionality

---

## 9. Profile & Wallet

### Done
- [x] Backend: `GET /v1/users/me`, `PUT /v1/users/me`
- [x] Domain: `Wallet` entity with `Money` value object
- [x] Frontend: `app/(tabs)/profile.tsx` with CutBox layout

### TODO
- [ ] Frontend: connect profile screen to real API data
- [ ] Frontend: avatar upload (use Supabase Storage or separate S3 bucket)
- [ ] Frontend: display wallet balance
- [ ] Backend: wallet top-up via Flitt
- [ ] Backend: wallet-based payment option (pay from balance)
- [ ] Frontend: booking history on profile screen

---

## 10. Background Jobs

### Done
- [x] `NoShowSweeper` — marks confirmed bookings as no-show after 15 min
- [x] `BookingReminderJob` — push notification 30 min before booking

### TODO
- [ ] Add error handling and retry logic with exponential backoff
- [ ] Add dead-letter logging for failed push notifications
- [ ] Consider moving to a proper job scheduler (Hangfire or Quartz.NET) for reliability
- [ ] Add job for automatic review request (24h after completed booking)
- [ ] Add job for expired wallet transaction cleanup

---

## 11. Infrastructure & DevOps

### Done
- [x] `docker-compose.dev.yml` — Postgres 16 + Redis 7
- [x] `Dockerfile` — multi-stage .NET 9 build
- [x] Serilog console logging
- [x] NSwag/Swagger API docs

### TODO
- [ ] Create production `docker-compose.prod.yml` with proper resource limits
- [ ] Set up CI/CD pipeline (GitHub Actions recommended):
  - `dotnet test` on PR
  - Build + push Docker image on merge to main
  - EAS Build trigger for mobile
- [ ] Add health check endpoint for load balancer (basic one exists at `/healthz`)
- [ ] Set up structured logging sink (Seq, Datadog, or CloudWatch)
- [ ] Add error tracking (Sentry for both backend and frontend)
- [ ] Set up database backups (pg_dump cron or managed Postgres)
- [ ] Configure HTTPS/TLS termination (reverse proxy or cloud load balancer)
- [ ] Set up staging environment matching production

---

## 12. App Store Submission

### TODO
- [ ] Configure EAS Build (`eas.json`) for iOS and Android
- [ ] Create Apple Developer account and App Store Connect entry
- [ ] Create Google Play Developer account and Play Console entry
- [ ] App icons (1024x1024 for iOS, adaptive icon for Android)
- [ ] Splash screen (replace default Expo splash)
- [ ] Screenshots for App Store (6.7", 6.5", 5.5") and Play Store
- [ ] Privacy policy URL (required by both stores)
- [ ] Terms of service URL
- [ ] App Store review notes (test account credentials, Flitt test mode explanation)
- [ ] Android: `google-services.json` for FCM push notifications
- [ ] iOS: APNs key uploaded to Expo dashboard
- [ ] Set up EAS Update for OTA updates post-launch

---

## 13. Testing

### Done
- [x] 23 unit tests (pricing, overlap, cancellation)
- [x] 5 architecture tests (layer dependency enforcement)
- [x] Integration test scaffold with WebApplicationFactory

### TODO
- [ ] Integration tests: booking creation, payment flow, auth-protected endpoints
- [ ] Frontend: add component tests with React Native Testing Library
- [ ] Frontend: add E2E tests with Detox or Maestro
- [ ] Load testing: simulate concurrent bookings for overlap constraint validation
- [ ] Security: test JWT expiration handling, unauthorized access, SQL injection

---

## 14. Security Hardening

### TODO
- [ ] Enable CORS with specific origin whitelist (currently allows all in dev)
- [ ] Verify rate limiter thresholds for all endpoints
- [ ] Add request size limits
- [ ] Sanitize user input (display names, review comments) against XSS
- [ ] Ensure Flitt callback signature verification is active before go-live
- [ ] Audit all endpoints for proper authorization checks
- [ ] Add API versioning strategy for future breaking changes
- [ ] Enable HTTPS-only cookies and security headers
