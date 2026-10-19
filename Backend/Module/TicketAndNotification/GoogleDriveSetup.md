# Google Drive ticket images

The TicketAndNotification API uploads ticket images to a Google Drive folder. Configure these environment variables on the API process; do not put service account keys in the frontend or commit them:

- `GoogleDrive__ServiceAccountJson`: the complete service account JSON document.
- `GoogleDrive__FolderId`: the ID of a folder in a Shared Drive where the service account can add files.

Enable the Google Drive API for the service account's Google Cloud project, add the service account to the Shared Drive with permission to add files, and use a folder ID from that drive. The API requests `drive.file` scope and makes each uploaded image readable to anyone with its link so that the browser can display it. The upload endpoint accepts JPEG, PNG, WebP, and GIF files up to 10 MB each.

Uploaded image links use `https://drive.google.com/uc?export=view&id=...`. Manager ticket attachments keep these links in `ticketMedias` for later viewing.
