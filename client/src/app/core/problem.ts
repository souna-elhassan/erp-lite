import { HttpErrorResponse } from '@angular/common/http';

/**
 * Turns an API error into a message for the user.
 * The API returns RFC 9457 ProblemDetails; its `title` is written to be shown as-is.
 */
export function problemMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) {
      return 'The server cannot be reached. Check that the API is running.';
    }
    const title = (error.error as { title?: unknown } | null)?.title;
    if (typeof title === 'string' && title.length > 0) {
      return title;
    }
    return `Request failed (HTTP ${error.status}).`;
  }
  return 'Unexpected error.';
}
