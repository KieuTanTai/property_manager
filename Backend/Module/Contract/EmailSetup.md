# Consultation email setup

The public consultation form sends a message through the Contract API. The API reads the current recipient from the
newest `contact_info` record by `contact_info_updated_at` and sends using SMTP.

Set these environment variables on the Contract API process. Keep credentials out of source control:

- `Email__SmtpHost`: SMTP server hostname.
- `Email__SmtpPort`: SMTP port; defaults to `587`.
- `Email__SecureSocketOptions`: MailKit `SecureSocketOptions` value; defaults to `StartTls` (use `SslOnConnect` for
  implicit TLS servers).
- `Email__Username` and `Email__Password`: SMTP authentication credentials.
- `Email__FromAddress`: sender mailbox authorized by the SMTP server.
- `Email__FromName`: optional sender display name; defaults to `ManagerSpace`.

The public endpoint permits five requests per minute per client IP. A successful response means the SMTP server accepted
the message for delivery.
