# Payment module — API, internals & notes

Working reference for the Payment module. Complements [`README.md`](README.md), which covers
running the app, Docker, configuration and testing; this file is about **the API surface, how the
code is put together, and the traps**.

- Base route: `api/payment` · Ports: https `7093`, http `5144` · Swagger in Development at `/swagger`
- JSON is camelCase (ASP.NET Core web defaults) and **enums travel as strings**
  (`JsonStringEnumConverter`): `"Pending" | "Succeeded" | "Failed" | "Expired" | "NeedsReview"`
- Everything this module stores is **UTC**; VNPAY speaks **GMT+7** (see §3)

---

## 1. Available API

| Method | Route | HTTPS | Purpose |
|---|---|---|---|
| POST | `create-url` | required | Create an attempt, get the VNPAY redirect URL |
| GET | `ipn` | — | VNPAY server-to-server notification. **The only endpoint that settles a transaction** |
| GET | `return` | — | Browser redirect target. **Read-only** |
| GET | `{paymentTransactionId:guid}` | required | One attempt by id |
| GET | `by-txn-ref/{txnRef}` | required | One attempt by VNPAY transaction reference |
| GET | `by-invoice/{invoiceId:guid}` | required | Every attempt of an invoice (newest first) |
| GET | `by-invoice/{invoiceId:guid}/status` | required | Minimal paid / not-paid answer for consumers |
| GET | `by-account/{accountId:guid}` | required | Paged attempts of an account |
| GET | `status/{status}` | required | Paged attempts by status |

`ipn` and `return` deliberately have **no `[RequireHttps]`**: they are reached through a
TLS-terminating tunnel that forwards plain HTTP. `Program.cs` also skips `UseHttpsRedirection()`
(a 307 would break IPN) and honours `X-Forwarded-Proto`, so `[RequireHttps]` still works behind a
proxy. See the note on `HTTPS` in §3.

**No authentication.** The API has `[RequireHttps]` only — no API key, no JWT, no token. Any caller
that can reach the port can read every transaction and create payment attempts. For now callers
(Frontend, other modules) must broker access themselves; putting this behind a gateway/network
policy is the intended pattern. `create-url` is the only write a caller can perform, and it can
only *add a pending attempt*, never move money.

### `POST create-url`

```jsonc
// request — RecordCreatePaymentRequest
{
  "invoiceId": "guid",          // required
  "accountId": "guid",          // required
  "amount": 10000000,           // required, 5000 … 1000000000 VND
  "description": "Thanh toan hoa don 10/2026",  // optional, sanitized to ASCII, max 255
  "bankCode": "NCB",            // optional (VNPAY BankCode enum name); unknown → ANY
  "language": "Vietnamese"      // optional (DisplayLanguage enum name); default Vietnamese
}
```

```jsonc
// 200 — RecordPaymentResponse (paymentUrl is only non-null on this endpoint)
{
  "paymentTransactionId": "f2144bc3-…",
  "invoiceId": "…", "accountId": "…",
  "amount": 10000000, "currency": "VND",
  "status": "Pending",
  "txnRef": "639279530295644378",
  "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=1000000000&…&vnp_SecureHash=…",
  "gatewayTransactionNumber": null, "bankCode": null, "bankTranNo": null,
  "responseCode": null, "transactionStatusCode": null, "cardType": null,
  "orderInfo": "Thanh toan hoa don 10/2026",
  "payDate": null,
  "expiresAt": "2026-10-18T20:58:49Z",
  "createdAt": "2026-10-18T20:43:49Z",
  "updatedAt": "2026-10-18T20:43:49Z"
}
```

The row is persisted **before** the URL is returned, so an IPN always finds its transaction.
Errors: `400` with a plain message (missing ids, amount out of range, no client IP, VNPAY error).

### `GET ipn`

The signed notification VNPAY sends server-to-server. It always answers `200` with a small body
(camelCase because of the web defaults):

```json
{ "rspCode": "00", "message": "Confirm Success" }
```

| `rspCode` | Message | Meaning | Row effect |
|---|---|---|---|
| `00` | `Confirm Success` | Accepted (also for already-final replays) | settled, or untouched if already final |
| `01` | `Order not found` | Unknown/missing `vnp_TxnRef` | none |
| `04` | `Invalid amount` | `vnp_Amount / 100 ≠ payment_amount` | none |
| `97` | `Invalid signature` | Checksum failed | none |
| `99` | `Unprocessed` | VNPAY still in flight, or an unexpected error | none; **VNPAY retries** |

The handler never throws out (a catch-all answers `99`), because a non-200 would make VNPAY give up.

### `GET return`

Browser landing after payment. Read-only: it looks the row up by `vnp_TxnRef` and returns the same
`RecordPaymentResponse` shape (`404` if unknown). **It never settles anything** — settlement is IPN's
job.

> The response usually looks empty (`status: "Pending"`, all gateway fields `null`). That is not a
> bug: the browser redirect typically races the IPN, so at that instant the row has not been settled
> yet. See §3 for why the frontend should poll instead of trusting this payload.

### Read endpoints

- `GET {paymentTransactionId:guid}`, `GET by-txn-ref/{txnRef}` → `RecordPaymentResponse` or `404`
- `GET by-invoice/{invoiceId:guid}` → array of `RecordPaymentResponse`, or `404` when there are none
- `GET by-invoice/{invoiceId:guid}/status` → always `200`:

```jsonc
// RecordInvoicePaymentStatusResponse
{
  "invoiceId": "…",
  "isPaid": true,                    // true as soon as one attempt succeeded
  "paymentTransactionId": "f2144bc3-…",
  "txnRef": "639279530295644378",
  "status": "Succeeded",
  "amount": 10000000,
  "payDate": "2026-10-18T20:44:33Z"
}
```

- `GET by-account/{accountId:guid}?cursor={guid}&pageSize=20`
- `GET status/{status}?cursor={guid}&pageSize=20` (`status` is the enum name, e.g. `NeedsReview`)

Both paged endpoints return `RecordBaseCursorPage` and answer `404` when a page is empty:

```jsonc
{ "items": [ /* RecordPaymentResponse */ ], "nextCursor": "guid|null", "isHasMore": true }
```

Paging walks `payment_transaction_id` **descending**. Ids are UUIDv7 (`DEFAULT (UUID_v7())`), so they
are time-ordered and the cursor is effectively "older than this id". `pageSize ≤ 0` → `400`.

> There is no route for plain "all transactions" paging even though `IPaymentApplication` has
> `GetApplyPagingAsync`; use `status/{status}` or `by-account/{accountId}`.

### `RecordPaymentResponse` fields

| Field | Notes |
|---|---|
| `paymentTransactionId` | server-generated UUIDv7 |
| `invoiceId` / `accountId` | logical link only — **no foreign keys** to Contract/Identity |
| `amount` / `currency` | `currency` is always `VND` |
| `status` | enum string, see §2 |
| `txnRef` | our reference: `DateTime.UtcNow.Ticks`; unique |
| `paymentUrl` | **only** set by `create-url`, always `null` elsewhere |
| `gatewayTransactionNumber` | VNPAY's `vnp_TransactionNo` |
| `bankCode` / `bankTranNo` / `cardType` | raw `vnp_*` values, `null` until an IPN settles |
| `responseCode` / `transactionStatusCode` | the raw `vnp_ResponseCode` / `vnp_TransactionStatus` |
| `orderInfo` | sanitized description |
| `payDate` | UTC (`vnp_PayDate` − 7 h); only set on success |
| `expiresAt` | created + 15 minutes |
| `createdAt` / `updatedAt` | UTC |

---

## 2. How the code works

### File map

```
Program.cs                     pipeline; ForwardedHeaders; no UseHttpsRedirection
Application/
  PaymentApplication.cs        all business logic (create / ipn / reads / classify)
Interfaces/
  IApplication/IPaymentApplication.cs        application contract
  IRepository/IPaymentTransactionRepository.cs  includes SettleByTxnRefAsync + ExpireStalePendingAsync
Models/Payment/
  PaymentTransactionModel.cs   entity + domain helpers (IsSucceeded, IsFinalized, …)
  PaymentSettlementModel.cs    the terminal outcome written by one conditional UPDATE
  PaymentUrlResult.cs          (transaction, paymentUrl)
  IpnResult.cs                 (RspCode, Message)
  VnpaySettingsModel.cs        bound from the `VNPAY` section
Infrastructure/
  DIContainer/CollectionPaymentRepository.cs   DbContext, repos, unit of work, sweeper
  DIContainer/CollectionPaymentApplication.cs  VNPAY settings + IVnpay, app service
  Persistence/DbContext/PaymentDbContext.cs
  Persistence/Configurations/ContextPaymentTransactionConfiguration.cs
  Repository/EfPaymentUnitOfWork.cs
  Repository/PaymentTransactionRepository/PaymentTransactionRepository.cs
  Background/PaymentExpirySweeper.cs            hosted service, 5 min
Presentation/
  Controller/PaymentController.cs                endpoints + MapResponse + IP resolution
  Record/Payment/*.cs                            request/response DTOs
Utils/
  VnpaySignatureHelper.cs      verifies vnp_SecureHash (algorithm mirrors VNPAY.NET)
  PaymentDescriptionHelper.cs  strips accents/specials from vnp_OrderInfo
  Enum/EPaymentStatus.cs
```

Only this module references `Shared` for `LogPool`, unit of work, paging and cursor records. It owns
one table (`payment_transaction`) in the **same** database as the other modules
(`ConnectionStrings:IdentityTest`), and never writes `receipt` / `monthly_invoice` / `account`.

### `create-url` sequence

```
controller ─► PaymentApplication.CreatePaymentUrlAsync
  1. validate invoiceId/accountId non-empty, 5000 ≤ amount ≤ 1_000_000_000, ipAddress present
  2. orderInfo = PaymentDescriptionHelper.Sanitize(description) ?? "Thanh toan hoa don {invoiceId}"
  3. txnRef = DateTime.UtcNow.Ticks
  4. PaymentRequest { PaymentId = txnRef, CreatedDate = UtcNow.AddHours(7), Currency = VND, … }
  5. paymentUrl = IVnpay.GetPaymentUrl(request)      // signing only, no network call
  6. INSERT payment_transaction (status Pending, expires_at = now + 15 min)
  7. return (row, paymentUrl)
```

The URL is built entirely locally (HMAC-SHA512 over the params), so `create-url` works with no
network access — which is why the smoke test needs no VNPAY account.

### `ipn` sequence — the gate order matters

```
1. vnp_TxnRef missing?                      → rspCode 01
2. row not found?                           → rspCode 01
3. row already final and not Expired?       → rspCode 00 (idempotent no-op)
4. VnpaySignatureHelper.IsValid(query)?     no → rspCode 97
5. vnp_Amount / 100 == payment_amount?      no → rspCode 04
6. ClassifySettlement:
     vnp_TransactionStatus 01/05            → in flight → rspCode 99, nothing written
     IsSuccess                              → Succeeded
     ResponseCode 07 or TransactionStatus 04→ NeedsReview   (debited, not a clean success)
     otherwise                              → Failed
7. SettleByTxnRefAsync(txnRef, settlement)  → one conditional UPDATE
8. rspCode 00
```

Step 3 short-circuits replays *before* the signature check, so a repeated IPN is cheap. Step 4 exists
because the library reports "bad checksum" and "failed payment" with the same shape
(`PaymentResult.IsSuccess == false`) — without it a forged callback would be stored as a plain
failure. Step 5 is ours too: `vnp_Amount` is not among the library's mandatory fields.

### Settlement is one atomic statement

`PaymentTransactionRepository.SettleByTxnRefAsync` runs a single `ExecuteUpdateAsync` and puts the
settleable-state test **inside** it:

```sql
UPDATE payment_transaction
   SET payment_status = ?, …, payment_updated_at = ?
 WHERE payment_txn_ref = ?
   AND (payment_status = 'Pending'
        OR (payment_status = 'Expired' AND ? = 'Succeeded'))
```

Consequences:

- Two IPNs arriving together cannot both settle the row, and a replay cannot overwrite a result
  decided from a stale read — `0` rows affected means "somebody else already settled it".
- `Expired` is revived **only** by a success. `Expired` came from this module's own timeout, not from
  VNPAY, so a late signed success is a fact that beats our guess; a late *failure* leaves the expiry
  standing.
- Any future pull-based settlement (e.g. `querydr`, §6) can reuse this same method and compose safely
  with the IPN.

### Status model

| Status | Set by | Means |
|---|---|---|
| `Pending` | `create-url` | attempt created, no gateway verdict yet |
| `Succeeded` | IPN (both codes `00`) | paid |
| `Failed` | IPN | declined; `pay_date` stays NULL |
| `Expired` | **only** `PaymentExpirySweeper` | past `expires_at` with no verdict |
| `NeedsReview` | IPN (`07` / status `04`) | customer may have been debited without a clean success |

`IsFinalized` = `Succeeded | Failed | Expired | NeedsReview`. `Pending` is the only non-final state
writable by an IPN.

### Expiry sweeper

`PaymentExpirySweeper` is a `BackgroundService` registered in `CollectionPaymentRepository`. Every
5 minutes it runs one statement moving `Pending` rows past `payment_expires_at` to `Expired`. It is
**the only writer of `Expired`**. The interval is a constant in the class, not configuration. Because
a transaction lives 15 minutes and the sweep runs every 5, an abandoned attempt is `Pending` for at
most 5 minutes past its expiry.

### Table

`payment_transaction` (DDL: `docs/sql/payment/payment_schema.sql`)

- PK `payment_transaction_id` UUIDv7 · `UNIQUE uk_payment_txn_ref (payment_txn_ref)` ·
  indexes on invoice/account/status/created_at
- `payment_status` is a MariaDB `ENUM('pending','succeeded','failed','expired','needsreview')`
  (lowercase, case-insensitive collation). EF maps the enum to a string, so the API emits
  `"Succeeded"` while the table stores `succeeded`.
- **Adding a status needs an `ALTER TABLE`** — EF migrations are not used anywhere in this solution.
  The upgrade statement is at the bottom of `payment_schema.sql`.
- No foreign keys: `payment_invoice_id` / `payment_account_id` are logical links so modules stay
  decoupled.

---

## 3. Notable info (traps)

**1. There are two different VNPAY signature schemes.**

| | Payment / IPN | Merchant API (`querydr`, refund) |
|---|---|---|
| Separator | `&` | `\|` |
| Encoding | URL-encoded | **not** encoded |
| Order | alphabetical | **fixed field order** |
| Fields | every `vnp_*` except the hash ones | an explicit list |

Reusing `VnpaySignatureHelper` for a `querydr` call will fail every time. The `|` form is
`vnp_RequestId|vnp_Version|vnp_Command|vnp_TmnCode|vnp_TxnRef|vnp_TransactionDate|vnp_CreateDate|vnp_IpAddr|vnp_OrderInfo`
— HMAC-SHA512, hex.

**2. `vnp_ResponseCode` means two different things.**

- In an **IPN**, `vnp_ResponseCode` is **the payment's result**.
- In a **`querydr` response**, `vnp_ResponseCode` describes **the query** (`00` = "QueryDR success"),
  and the payment's state is in `vnp_TransactionStatus`.

Conflating them marks unpaid transactions as paid.

**3. The published `VNPAY.NET 8.5.0` differs from the library's GitHub master.**

- Interface is `IVnpay` (not `IVnpayClient`); there is **no** `AddVnpayClient` DI extension — the
  client is configured by hand: `new Vnpay(); vnpay.Initialize(tmn, secret, baseUrl, callbackUrl, version, orderType)`
  in `CollectionPaymentApplication`.
- Results come back as `PaymentResult.IsSuccess` instead of thrown `VnpayException`.
- Request type is `PaymentRequest` (not `VnpayPaymentRequest`).
- `GetPaymentResult` **throws** when VNPAY omits mandatory fields (`vnp_BankCode` among them).
- Helpers substitute the Vietnamese placeholder `"Không xác định"` for absent values — the module
  therefore reads `vnp_TransactionNo` / `vnp_BankCode` / `vnp_BankTranNo` / `vnp_CardType` straight
  from the query rather than through them.
- The library silently substitutes `DateTime.Now` when `vnp_PayDate` is absent, so a pay date is
  stored only when `vnp_PayDate` was actually present.

**4. Amounts are always ×100.** `vnp_Amount` is `amount × 100` (10.000.000 VND → `1000000000`), and
the IPN re-checks it (`/100 == payment_amount`) before accepting. Library range: 5.000–1.000.000.000 VND.

**5. Time zones.** VNPAY works in GMT+7 and never converts. `vnp_CreateDate` is derived from
`DateTime.UtcNow.AddHours(7)` (not `DateTime.Now`, which would follow the host timezone), and
`vnp_PayDate` is converted back (`− 7 h`) before storing. All module columns are UTC; the DB session
runs UTC.

**6. `/return` is read-only and races the IPN.** It reports the stored row, which is usually still
`Pending` at redirect time. Never settle there and never treat its payload as the final answer.

**7. The return URL and the IPN carry the identical signed parameter set.** Deliberately convenient:
a real return query string can be replayed into `GET /api/payment/ipn` to settle a row locally with
no tunnel (§4). It also means the signed payload reaches the browser (address bar, history,
`Referer`) — harmless here, because settling can only ever apply to a transaction that genuinely
happened, but it is why the IPN endpoint must never be able to *create* a row.

**8. The IPN URL is portal configuration, not code.** `VNPAY:CallbackUrl` only sets `vnp_ReturnUrl`,
which is signed into the payment URL at creation (so changing it affects only **new** attempts). The
IPN URL is registered in the merchant portal and applied at delivery time (so changing it affects
existing rows too). `VNPAY.NET` has no way to set it.

**9. There is no `querydr` in the library.** Nothing in the module can ask VNPAY for a transaction's
status; it can only wait to be told. See §4 (verifying by hand) and §6 (the gap).

**10. `txnRef` is `DateTime.UtcNow.Ticks`.** Unique index `uk_payment_txn_ref` guards it, but a
collision is not retried — it would surface as an insert failure. (Ticks resolution makes this
effectively impossible in practice.)

**11. No log files.** `PaymentController` derives from `ControllerBase`, not `CustomControllerBase`,
so nothing calls `LogPool.FlushAsync` and no `docs/backend/modules/payment/*.jsonl` is produced.
`ILogPool` is a singleton, so buffered entries just accumulate. Watch the console instead (all EF SQL
too, in Development).

**12. `[RequireHttps]` reads endpoints need real HTTPS or a forwarded header.** Calling a read
endpoint over plain HTTP without `X-Forwarded-Proto: https` answers `403`. Use the `https` profile /
`https://localhost:7093`, or run a proxy that sets the header.

**13. `vnp_IpAddr` is resolved by the controller, on purpose.** `NetworkHelper.GetIpAddress` reverse
resolves IPv6 with `Dns.GetHostEntry` — which throws on the IPv4-mapped addresses Kestrel reports in
a container/proxy — and NREs on null. The controller unwraps `::ffff:1.2.3.4` instead.

---

## 4. Verifying whether a transaction actually succeeded

Three independent sources. Use them in this order.

### 4.1 Our own record (fastest)

```bash
mysql -h 127.0.0.1 -u appuser -p ms_identity_test -e \
"SELECT payment_status, payment_response_code, payment_transaction_status_code,
        payment_gateway_transaction_number, payment_bank_code, payment_amount, payment_pay_date
   FROM payment_transaction WHERE payment_txn_ref = '<txnRef>'\G"

curl -k https://localhost:7093/api/payment/by-txn-ref/<txnRef>
curl -k https://localhost:7093/api/payment/by-invoice/<invoiceId>/status   # {"isPaid":true,…}
```

This is only as trustworthy as the signed message that wrote it: `succeeded` means a valid
`vnp_SecureHash` and a matching amount produced codes `00`/`00`.

### 4.2 VNPAY's own gateway — `querydr` (authoritative)

The published library does not expose this, but the API works and was validated against the live
sandbox. POST JSON to `https://sandbox.vnpayment.vn/merchant_webapi/api/transaction`:

```jsonc
{
  "vnp_RequestId": "<uuid>",
  "vnp_Version": "2.1.0",
  "vnp_Command": "querydr",
  "vnp_TmnCode": "ZNKS8YP6",
  "vnp_TxnRef": "<txnRef>",
  "vnp_OrderInfo": "<anything>",
  "vnp_TransactionDate": "<see below>",
  "vnp_CreateDate": "<now, GMT+7, yyyyMMddHHmmss>",
  "vnp_IpAddr": "127.0.0.1",
  "vnp_SecureHash": "<HMAC-SHA512 hex, over the '|' sign data>"
}
```

`vnp_TransactionDate` — **validated rule**: the *create* date works, so a **pending** row can be
queried. Use `payment_created_at + 7 h` (that is exactly the `vnp_CreateDate` we sent); the pay date
also works once it exists.

| Sent as `vnp_TransactionDate` | Result |
|---|---|
| the create date (`created_at + 7 h`) | `00` QueryDR success |
| the pay date (`vnp_PayDate`) | `00` QueryDR success |
| a wrong date | `91` `Transaction not found` |
| an unknown `vnp_TxnRef` | `91` `Transaction not found` |

Successful response (real, from this sandbox):

```jsonc
{ "vnp_ResponseCode":"00", "vnp_Message":"QueryDR success",
  "vnp_TmnCode":"ZNKS8YP6", "vnp_TxnRef":"639279530295644378",
  "vnp_Amount":"1000000000", "vnp_BankCode":"NCB",
  "vnp_PayDate":"20261019034433", "vnp_TransactionNo":"15707936",
  "vnp_TransactionType":"01",     // 01 = payment, 02/03 = refunds
  "vnp_TransactionStatus":"00",   // ← the payment's state
  "vnp_SecureHash":"e6a16bff…" }  // verify this too
```

Remember trap #2: here `vnp_ResponseCode` is the *query's* result, and `vnp_TransactionStatus` is the
payment's. `91` is a clean "nothing there yet", not an error — leave the row pending.

### 4.3 The merchant portal (a UI over the same data)

`https://sandbox.vnpayment.vn/merchantv2/` — for the sandbox credentials in `paymenttestinfo.txt`.

If a transaction that `querydr` confirms is **not** in the portal list, it is a viewer problem, not a
payment problem. Check, in this order:

1. **Date filter.** The portal runs on GMT+7. The gateway-side date of the example above is
   `20261019034433` = **2026-10-19 03:44:33 +07**, while a UTC host clock reads `2026-10-18`.
   A "today" default evaluated in the wrong zone hides it — set the range explicitly.
2. **Menu.** Transactions live under *Quản lý giao dịch* / *Tra cứu giao dịch*, not the landing page.
3. **Search keys.** merchant order id `vnp_TxnRef`, or VNPAY's `vnp_TransactionNo`.
4. **Sandbox limitation.** Sandbox portals under-report SIT transactions. A `querydr` hit proves the
   record exists.

Also useful there: the transaction's **IPN history**, which shows VNPAY's delivery attempts. If the
registered IPN URL points at `localhost`, every attempt fails — that is the usual reason a row never
leaves `Pending`.

---

## 5. Worked example (real, sandbox)

Traced end to end to show how the pieces fit:

```
POST create-url {amount: 10000000, description: "nuke iran", …}
  → row created   status=Pending   txnRef=639279530295644378   created=2026-10-18T20:43:49Z
      (vnp_CreateDate sent = 20261019034349)

customer pays with the sandbox NCB card, VNPAY redirects to /return with:
  vnp_Amount=1000000000  vnp_ResponseCode=00  vnp_TransactionStatus=00
  vnp_TransactionNo=15707936  vnp_BankTranNo=VNP15707936  vnp_BankCode=NCB
  vnp_PayDate=20261019034433  vnp_TxnRef=639279530295644378  vnp_SecureHash=C067AB5C…
  → signature verified against the sandbox HashSecret: valid

/return at that moment: status=Pending, gateway fields null   (IPN had not arrived — no IPN URL
                                                               was registered in the portal)

replaying the same signed query string into GET /api/payment/ipn →
  {"rspCode":"00","message":"Confirm Success"}
  → row settled: status=Succeeded, responseCode=00, transactionStatusCode=00,
                 bankCode=NCB, bankTranNo=VNP15707936, gatewayTransactionNumber=15707936,
                 payDate=2026-10-18T20:44:33Z

GET by-invoice/{invoiceId}/status → { "isPaid": true, "status": "Succeeded", … }

querydr confirm → vnp_ResponseCode=00, vnp_TransactionStatus=00, vnp_TransactionNo=15707936
```

Three sources agree; the payment genuinely succeeded. What was missing was only the **delivery** of
the IPN, not the payment.

---

## 6. Known gaps / next steps

1. **No gateway self-check.** The module cannot query VNPAY (no `querydr` in the library), so it
   depends entirely on inbound IPN delivery. Adding a `querydr` helper + a reconcile endpoint
   (`POST by-txn-ref/{txnRef}/reconcile`) and a background sweep over stale `Pending` rows would make
   settlement work without a tunnel, without portal registration, and without a UI — reusing
   `SettleByTxnRefAsync`, so it composes with the IPN. The request is already validated (§4.2).
2. **Frontend has no payment code.** Nothing in `Frontend/` calls this API yet. The intended shape:
   point `VNPAY:CallbackUrl` at the UI, have the UI ignore all `vnp_*` params except `vnp_TxnRef`,
   then poll `by-invoice/{id}/status` (or `by-txn-ref`) until a terminal status.
3. **No authentication** on any endpoint (trap above) — put it behind a gateway/network policy.
4. **`NeedsReview` has no consumer.** Those rows are only visible via `status/NeedsReview`; nothing
   notifies or drives reconciliation. The raw codes are kept in the row for whoever picks it up.
5. **Nothing reconciles back.** A succeeded payment does not update Contract's
   `monthly_invoice.invoice_status` nor insert a `receipt`.
6. **`txnRef` collision is not retried** (see trap #10).
7. **No log files** (trap #11) — would mean deriving the controller from `CustomControllerBase`.
8. **Ordering rule for any future reconciler:** reconcile *before* the sweeper expires a row, or the
   chance to settle it is thrown away.
