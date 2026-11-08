import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductApi } from './product-api';
import { Product } from './product';

const sample: Product = {
  id: '1',
  sku: 'GLV-NIT-M',
  name: 'Nitrile gloves',
  unitPrice: 89,
  quantityOnHand: 240,
  isActive: true,
};

describe('ProductApi', () => {
  let api: ProductApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(ProductApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists products without a search parameter when the term is blank', () => {
    api.list('  ').subscribe((items) => expect(items).toEqual([sample]));
    const req = http.expectOne('/api/products');
    expect(req.request.params.has('search')).toBe(false);
    req.flush([sample]);
  });

  it('passes a trimmed search term', () => {
    api.list(' mask ').subscribe();
    const req = http.expectOne((r) => r.url === '/api/products');
    expect(req.request.params.get('search')).toBe('mask');
    req.flush([]);
  });

  it('posts stock movements to the right endpoint', () => {
    api.moveStock('1', 'issue', 4).subscribe();
    const req = http.expectOne('/api/products/1/stock/issue');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ quantity: 4 });
    req.flush({ ...sample, quantityOnHand: 236 });
  });

  it('creates a product', () => {
    api.create({ sku: 'NEW', name: 'New', unitPrice: 1 }).subscribe((p) => expect(p.sku).toBe('NEW'));
    const req = http.expectOne('/api/products');
    expect(req.request.method).toBe('POST');
    req.flush({ ...sample, sku: 'NEW' });
  });
});
