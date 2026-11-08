import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ProductForm } from './product-form';

describe('ProductForm', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductForm],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('does not call the API when the form is invalid and shows field errors', async () => {
    const fixture = TestBed.createComponent(ProductForm);
    await fixture.whenStable();
    const http = TestBed.inject(HttpTestingController);
    const el = fixture.nativeElement as HTMLElement;

    el.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();

    http.expectNone('/api/products');
    expect(el.querySelectorAll('.field-error').length).toBe(2);
  });

  it('shows the API error message when creation fails', async () => {
    const fixture = TestBed.createComponent(ProductForm);
    await fixture.whenStable();
    const http = TestBed.inject(HttpTestingController);
    const el = fixture.nativeElement as HTMLElement;

    for (const [id, value] of [['sku', 'DUP'], ['name', 'Duplicate']] as const) {
      const input = el.querySelector<HTMLInputElement>(`#${id}`)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    }
    el.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    http.expectOne('/api/products').flush({ title: "A product with SKU 'DUP' already exists." }, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();

    expect(el.querySelector('.alert')?.textContent).toContain('already exists');
  });
});
