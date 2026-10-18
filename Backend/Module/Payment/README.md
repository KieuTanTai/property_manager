# Payment module (VNPAY gateway)

Standalone ASP.NET Core app that talks to the VNPAY payment gateway using
[`VNPAY.NET` 8.5.0](https://github.com/phanxuanquang/VNPAY.NET) and records every payment attempt in
its own table (`payment_transaction`).

It is the **system of record for payments**. Other modules stay untouched: `monthly_invoice`,
`receipt` and `account` are never written by this module. Reconciliation (marking an invoice paid,
creating a receipt) is left to a later step — consumers read payment data from here instead.

> API reference, code walkthrough, VNPAY-specific traps and how to verify a transaction live in
> [`API.md`](API.md). This file covers running, configuring and testing the module.

## Run

```bash
# schema (once, against the same DB the other modules use)
mysql -h 127.0.0.1 -P 3306 -u appuser -p ms_identity_test < docs/sql/payment/payment_schema.sql

dotnet run --project Backend/Module/Payment/Payment.csproj --launch-profile https
```

- URL: `https://localhost:7093` , http `http://localhost:5144`
- Swagger: `https://localhost:7093/swagger`
- Uses the **existing** `ms_identity_test` database and the shared connection string name
  `IdentityTest`. No new database; the module's compose file only builds the app, the DB keeps
  living in `docs/sql/compose.yaml`.
- The SDK may not be on `PATH`; `~/.dotnet/dotnet` works too.

## Docker

```bash
cp Backend/Module/Payment/.env.example Backend/Module/Payment/.env   # fill in the VNPAY values
docker compose -f Backend/Module/Payment/compose.yaml up --build
```

- Swagger: `https://localhost:7093/swagger` — the image ships a **dev-only self-signed**
  certificate, so use `curl -k` (or trust it) and expect a browser warning.
- Ports mirror the local `https` profile: `7093` → https, `5144` → http.
- The container serves HTTPS on purpose: `[RequireHttps]` would answer 403 for every client-facing
  endpoint if it only spoke http.
- The image listens on `8080`/`8081` internally and runs as the non-root `$APP_UID` user, so it
  never needs privileges.
- The build context is `Backend/` (not the module folder) because `Payment.csproj` references
  `../../Shared/Shared.csproj`. `Backend/.dockerignore` keeps `appsettings*.json` and
  `paymenttestinfo.txt` out of the image.

Reaching the database from the container:

| Situation | `IDENTITY_TEST_CONNECTION` |
|---|---|
| DB from `docs/sql/compose.yaml` (default) | `server=host.docker.internal;port=3306;...` — the compose file adds `host-gateway` |
| Payment attached to the DB's network | `server=property_mariadb;port=3306;...` (network `sql_default`) |

For production, mount a real certificate over `/https/payment.pfx` and set
`ASPNETCORE_Kestrel__Certificates__Default__Path` / `__Password` accordingly, or terminate TLS at
the proxy and let the app read `X-Forwarded-Proto` (already supported).

## Configuration

| Setting | Required | Default |
|---|---|---|
| `ConnectionStrings:IdentityTest` | yes | – |
| `VNPAY:TmnCode` | yes | – |
| `VNPAY:HashSecret` | yes | – |
| `VNPAY:CallbackUrl` | yes | – |
| `VNPAY:BaseUrl` | no | `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html` |
| `VNPAY:Version` | no | `2.1.0` |
| `VNPAY:OrderType` | no | `other` |

`TmnCode`, `HashSecret` and `CallbackUrl` are validated at startup — the app throws when missing.

### How to supply them

`appsettings*.json` is gitignored. Pick one of these:

**1. user-secrets — for local `dotnet run`**

```bash
dotnet user-secrets set "VNPAY:TmnCode" "<vnp_TmnCode>" \
  --project Backend/Module/Payment/Payment.csproj
# ...same for ConnectionStrings:IdentityTest, VNPAY:HashSecret, VNPAY:CallbackUrl
```

**2. environment variables — containers, CI, scripts**

Same keys, with `:` written as `__`:

```bash
export ConnectionStrings__IdentityTest="server=127.0.0.1;port=3306;database=ms_identity_test;user=appuser;password=apppass"
export VNPAY__TmnCode="<vnp_TmnCode>"
export VNPAY__HashSecret="<vnp_HashSecret>"
export VNPAY__CallbackUrl="https://<public-host>/api/payment/return"

dotnet run --project Backend/Module/Payment/Payment.csproj --launch-profile https
```

**3. `.env` — for docker compose**

Copy `.env.example` to `.env` (gitignored) and fill it in; compose interpolates it into the
container's environment (`VNPAY_TMN_CODE` → `VNPAY__TmnCode`). Keys that are required use
`${VAR:?message}`, so compose fails fast with a readable message instead of the app throwing at
startup.

`appsettings.Local.json` also still works if you prefer a file. Normal ASP.NET Core precedence
applies: environment variables override `appsettings.json`, user-secrets only apply in Development.

**Never commit `TmnCode` / `HashSecret`.** `paymenttestinfo.txt` holds sandbox credentials and is
gitignored (scratch, not source); rotate those values before real use.

## Endpoints (`/api/payment`)

| Method | Route | Purpose |
|---|---|---|
| POST | `create-url` | Create a payment attempt and get the VNPAY redirect URL |
| GET | `ipn` | VNPAY server-to-server notification (settles the attempt) |
| GET | `return` | Browser redirect target, read-only |
| GET | `{paymentTransactionId:guid}` | One attempt |
| GET | `by-txn-ref/{txnRef}` | One attempt by VNPAY transaction reference |
| GET | `by-invoice/{invoiceId:guid}` | Every attempt for an invoice |
| GET | `by-invoice/{invoiceId:guid}/status` | Minimal paid/not-paid answer for consumers |
| GET | `by-account/{accountId:guid}` | Paged attempts of an account |
| GET | `status/{status}` | Paged attempts by status (`Pending\|Succeeded\|Failed\|Expired\|NeedsReview`) |

Enum values travel as strings (`JsonStringEnumConverter`), like every other module.

## Flow

```
client ──POST create-url──────────────► Payment : creates `pending` row, returns VNPAY url
client ──redirect to VNPAY url────────► VNPAY sandbox
VNPAY  ──GET /api/payment/ipn─────────► Payment : verifies signature + amount,
                                                 settles Succeeded / Failed / NeedsReview
                                                 (or leaves it Pending if VNPAY is mid-flight),
                                                 returns {"RspCode":"00"}
browser──GET /api/payment/return──────► Payment : read-only status of the attempt
```

`create-url` stores the row **before** returning, so an IPN always finds its transaction.

### Behaviour worth knowing

- The published `VNPAY.NET 8.5.0` package does **not** match the library's GitHub master: it exposes
  `IVnpay` (not `IVnpayClient`), has **no** `AddVnpayClient` DI extension (`Vnpay.Initialize(...)` is
  called from `CollectionPaymentApplication`), returns `PaymentResult.IsSuccess` instead of throwing
  `VnpayException`, and its request type is `PaymentRequest` (not `VnpayPaymentRequest`).
- `vnp_TxnRef` here is **our** value: `DateTime.UtcNow.Ticks` set on `PaymentRequest.PaymentId` and
  stored in `payment_txn_ref`. It is the key an IPN comes back with. The `uk_payment_txn_ref` unique
  index guards uniqueness.
- VNPAY works in **GMT+7** and does not convert. `vnp_CreateDate` is therefore derived from UTC
  (`DateTime.UtcNow.AddHours(7)`) so it is correct whatever the host timezone is, and `vnp_PayDate`
  is converted back to UTC before storing. All module timestamps (`created_at`, `updated_at`,
  `expires_at`, `pay_date`) are **UTC**, matching the UTC database session; mixing them with
  `DateTime.Now` would skew the values by the host offset.
- Amount must be 5.000–1.000.000.000 VND (library-enforced). `vnp_Amount` is sent ×100 and is
  re-verified against `payment_amount` on IPN; mismatch answers `RspCode 04`.
- `GetPaymentResult` reports a bad checksum and a failed payment identically
  (`IsSuccess == false`, codes still echoed). To avoid recording a forged callback as a failed
  payment, the IPN handler verifies the signature itself (`Utils/VnpaySignatureHelper`, algorithm
  verified byte-for-byte against the library) and answers `97` before trusting anything.
- Settlement is **atomic**: the IPN handler issues one conditional
  `UPDATE ... WHERE payment_txn_ref = ? AND payment_status = 'Pending'` (`ExecuteUpdateAsync`), so
  two IPNs arriving together cannot both settle a row and a replay cannot overwrite a result
  decided from a stale read. `0` rows affected means somebody else already settled it. `Expired`
  rows are the one exception: a signed **success** still settles them, because `Expired` is this
  module's own timeout rather than a verdict from VNPAY.
- IPN acknowledgements: `00` processed, `01` order not found, `04` invalid amount, `97` invalid
  signature, `99` unprocessed (VNPAY will retry).
- Outcomes are split finer than success/failure: `Succeeded` (both codes `00`) · `Failed` (any
  other final decline, e.g. `24`/`02`) · **`NeedsReview`** — the customer may already have been
  debited without the transaction succeeding (`vnp_ResponseCode` `07`, or
  `vnp_TransactionStatus` `04`), held apart so it is not buried as an ordinary failure and acked
  `00` so VNPAY stops retrying.
- `vnp_TransactionStatus` `01`/`05` are not outcomes but states on the way to one. The row is left
  `Pending` and the IPN answers `99`, so VNPAY calls back again instead of a half-finished payment
  being frozen as final.
- Only what VNPAY actually sent is stored. The library substitutes the Vietnamese placeholder
  "Không xác định" for absent bank/card values, so the handler reads `vnp_BankTranNo`,
  `vnp_CardType` and `vnp_BankCode` from the query rather than from the library helpers, and never
  writes the placeholder as data. `pay_date` is stored only when `vnp_PayDate` was present (the
  library silently substitutes `DateTime.Now` otherwise).
- Abandoned attempts are swept by `Infrastructure/Background/PaymentExpirySweeper`, a hosted
  service registered in `CollectionPaymentRepository`: every 5 minutes it moves `Pending` rows past
  `payment_expires_at` into `Expired`, in one statement. Without an IPN a row would otherwise stay
  `Pending` forever. The interval is a constant in the class, not configuration.
- `vnp_IpAddr` is mandatory, and the controller resolves it itself rather than through
  `NetworkHelper.GetIpAddress`. That helper reverse-resolves with `Dns.GetHostEntry` whenever the
  address is IPv6, which throws `SocketException: Name or service not known` for the IPv4-mapped
  addresses Kestrel reports inside a container or behind a proxy, and throws `NullReferenceException`
  when the address is null. The controller unwraps `::ffff:1.2.3.4` instead.
- `Program.cs` does **not** call `UseHttpsRedirection()`: VNPAY IPN/return are usually forwarded by
  a TLS-terminating tunnel, and a 307 redirect would break IPN. `[RequireHttps]` is kept on the
  client-facing endpoints only.
- `X-Forwarded-Proto` / `X-Forwarded-For` are honoured (`UseForwardedHeaders` with
  `KnownIPNetworks`/`KnownProxies` cleared), so a proxy that terminates TLS still satisfies
  `[RequireHttps]` and the payer's real IP reaches `vnp_IpAddr`.
- VNPAY needs a public HTTPS callback. For local testing expose the app through a tunnel
  (ngrok/cloudflared) and set `CallbackUrl` plus the IPN URL registered on the VNPAY portal
  (`https://<public-host>/api/payment/ipn`). Sandbox test card details are in
  `paymenttestinfo.txt`.

## Integrating from another service

Payment is a provider; consumers read from it. Join key is the **invoice id**.

```
GET /api/payment/by-invoice/{invoiceId}/status
{
  "invoiceId": "...",
  "isPaid": true,
  "paymentTransactionId": "...",
  "txnRef": "638...",
  "status": "Succeeded",
  "amount": 100000,
  "payDate": "2026-10-17T10:00:00"
}
```

Rules for consumers:

1. `isPaid` is true as soon as one attempt succeeded; do not infer payment state from
   `monthly_invoice.invoice_status` (still `unpaid`) or from `receipt` (still empty).
2. Only `invoice_id` links the two sides. There is no shared foreign key and no shared DbContext.
3. Use `by-invoice/{invoiceId}` when you need attempts/audit, `by-txn-ref/{txnRef}` for support
   lookups, `by-account/{accountId}` for history.
4. Consolidate **twice** on `Succeeded` (or once and store), IPN can be delivered more than once.
5. Do not call `create-url` more than needed: each call creates a new attempt row; only one of them
   will ever succeed.

Known gap: nothing yet updates Contract's `monthly_invoice.invoice_status` or inserts a `receipt`
row after a successful payment. Wiring that means Contract calling this module (pull) or consuming
an event (push); both are deliberate follow-up work.

## Testing

Three levels, cheapest first.

### 1. Smoke test — no VNPAY account needed

Building the payment URL is entirely local (HMAC-SHA512 signing, no network call), so Swagger is
enough. `Payment.http` has ready requests.

1. `POST /api/payment/create-url` with an invoice id, account id and amount
   (min 5.000 VND).
2. Expect `status: Pending`, a `txnRef` and a `paymentUrl` starting
   `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...`.
3. Open the URL in a browser: a correct signature shows the sandbox payment page, a bad one shows
   a checksum error.
4. Confirm what was stored:

```bash
mysql -h 127.0.0.1 -u appuser -p ms_identity_test \
  -e "SELECT payment_status, payment_txn_ref, payment_amount FROM payment_transaction\G"
```

### 2. Drive the IPN yourself — no tunnel needed

This is the useful one, because **only `ipn` settles a transaction**. Sign a callback with the
sandbox `HashSecret`, where `vnp_Amount` is `amount × 100`:

```python
# save as sign_ipn.py ; usage: build-json | python3 sign_ipn.py <HashSecret>
import hashlib, hmac, json, sys, urllib.parse
secret, params = sys.argv[1], json.load(sys.stdin)
items = sorted((k, v) for k, v in params.items()
               if k.startswith("vnp_") and k not in ("vnp_SecureHash", "vnp_SecureHashType") and v)
data = "&".join(f"{urllib.parse.quote_plus(k)}={urllib.parse.quote_plus(str(v))}" for k, v in items)
sig = hmac.new(secret.encode(), data.encode(), hashlib.sha512).hexdigest().upper()
q = dict(params); q["vnp_SecureHash"] = sig
print(urllib.parse.urlencode(q))
```

Then call `GET /api/payment/ipn?...` with `vnp_TxnRef` from step 1 and these expected results:

| Sent | `RspCode` | Effect on the row |
|---|---|---|
| valid hash, `00`/`00`, matching amount | `00` | `succeeded` |
| wrong hash | `97` | nothing written |
| valid hash, `vnp_Amount` ≠ stored × 100 | `04` | nothing written |
| valid hash, `24`/`02` (declined) | `00` | `failed`, `pay_date` NULL |
| valid hash, `01`/`05` (VNPAY still in progress) | `99` | left `pending`, VNPAY retries |
| valid hash, `00`/`04` or `07` (debited, not a clean success) | `00` | `needsreview` |
| unknown `vnp_TxnRef` | `01` | nothing |
| repeated IPN | `00` | ignored, already final |
| malformed / missing fields | `99` | nothing, VNPAY retries |

Ack codes: `00` processed · `01` order not found · `04` invalid amount · `97` invalid signature ·
`99` unprocessed.

### 3. Real sandbox payment — needs a public HTTPS URL

1. Expose the app, pointing the tunnel at the **plain http** port on purpose — `ipn`/`return` skip
   `[RequireHttps]` and the app skips `UseHttpsRedirection()`, which avoids TLS friction:
   `ngrok http http://localhost:5144` (or map the container's `5144`).
2. Set `VNPAY:CallbackUrl` to `https://<id>.ngrok-free.app/api/payment/return`.
3. **Register the IPN URL** `https://<id>.ngrok-free.app/api/payment/ipn` in the VNPAY sandbox
   merchant portal. The library only sends `vnp_ReturnUrl`, so this cannot be set from code.
4. `create-url`, pay with the sandbox card from `paymenttestinfo.txt`, then check that the row is
   `succeeded` and `by-invoice/{id}/status` returns `isPaid: true`.

> Without step 3 nothing settles the transaction: it stays `Pending` until the expiry sweeper moves
> it to `Expired`. Settling happens only on IPN — deliberate, because IPN is the signed,
> server-to-server, authoritative channel.

## Notes / known limitations

- **No module log files yet.** `PaymentController` derives from `ControllerBase`, not
  `CustomControllerBase`, so nothing calls `LogPool.FlushAsync` and no
  `docs/backend/modules/payment/*.jsonl` is produced. `ILogPool` is a singleton, so buffered
  entries accumulate for the process lifetime. Contract has the same gap; only Identity's
  controllers flush. The console (including all EF SQL in Development) is the log to watch.
  Fixing it means giving the controller the same `IAsyncActionFilter` base Identity uses.
- **`Expired` is only ever set by the sweeper**, so between its 5-minute runs an abandoned attempt
  briefly stays `Pending` (harmless, but `by-invoice/{id}/status` reports not-paid either way).
  A signed success arriving after the sweep still settles the row; a *failure* arriving after it is
  discarded and the expiry stands.
- **`NeedsReview` has no consumer.** Rows in that status are only visible through
  `status/NeedsReview`; nothing notifies anyone or drives a refund/reconciliation. The gateway's
  codes are kept in `payment_response_code` / `payment_transaction_status_code` for whoever picks
  that up.
- **Nothing reconciles back.** A succeeded payment does not update Contract's
  `monthly_invoice.invoice_status` nor insert a `receipt`. This module is the source of truth
  until that follow-up exists.
- **Time zones.** VNPAY works in GMT+7 and never converts; everything this module stores is UTC.
  Set up the host clock/database session accordingly if you add new timestamps.
- **Amounts** are limited to 5.000–1.000.000.000 VND by the library, and `vnp_Amount` is always
  `amount × 100`.
