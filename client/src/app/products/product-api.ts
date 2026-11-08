import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateProductRequest, Product, StockDirection, UpdateProductRequest } from './product';

/** Thin typed client for /api/products. URLs are relative: dev proxy / nginx route them to the API. */
@Injectable({ providedIn: 'root' })
export class ProductApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/products';

  list(search?: string): Observable<Product[]> {
    const term = search?.trim();
    const params = term ? new HttpParams().set('search', term) : undefined;
    return this.http.get<Product[]>(this.baseUrl, { params });
  }

  get(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, request);
  }

  moveStock(id: string, direction: StockDirection, quantity: number): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/${id}/stock/${direction}`, { quantity });
  }
}
