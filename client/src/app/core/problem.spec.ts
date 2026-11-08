import { HttpErrorResponse } from '@angular/common/http';
import { problemMessage } from './problem';

describe('problemMessage', () => {
  it('uses the ProblemDetails title from the API', () => {
    const err = new HttpErrorResponse({ status: 409, error: { title: "A product with SKU 'X' already exists." } });
    expect(problemMessage(err)).toBe("A product with SKU 'X' already exists.");
  });

  it('explains when the server cannot be reached', () => {
    expect(problemMessage(new HttpErrorResponse({ status: 0 }))).toContain('cannot be reached');
  });

  it('falls back to the HTTP status', () => {
    expect(problemMessage(new HttpErrorResponse({ status: 502, error: 'Bad gateway' }))).toBe('Request failed (HTTP 502).');
  });

  it('handles non-HTTP errors', () => {
    expect(problemMessage(new Error('boom'))).toBe('Unexpected error.');
  });
});
