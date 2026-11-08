import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { problemMessage } from '../../core/problem';
import { LOW_STOCK_THRESHOLD, Product, StockDirection } from '../product';
import { ProductApi } from '../product-api';

@Component({
  selector: 'app-product-list',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css',
})
export class ProductList implements OnInit {
  private readonly api = inject(ProductApi);
  private readonly search$ = new Subject<string>();

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly busyId = signal<string | null>(null);
  protected readonly lowStock = LOW_STOCK_THRESHOLD;

  /** Summary figures for the KPI cards, computed from the currently listed products. */
  protected readonly stats = computed(() => {
    const items = this.products();
    return {
      total: items.length,
      active: items.filter((p) => p.isActive).length,
      units: items.reduce((sum, p) => sum + p.quantityOnHand, 0),
      value: items.reduce((sum, p) => sum + p.quantityOnHand * p.unitPrice, 0),
      low: items.filter((p) => p.isActive && p.quantityOnHand <= LOW_STOCK_THRESHOLD).length,
    };
  });

  constructor() {
    this.search$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((term) => {
          this.loading.set(true);
          return this.api.list(term);
        }),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (items) => this.show(items),
        error: (err: unknown) => this.fail(err),
      });
  }

  ngOnInit(): void {
    this.reload();
  }

  protected onSearch(term: string): void {
    this.search$.next(term);
  }

  protected reload(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (items) => this.show(items),
      error: (err: unknown) => this.fail(err),
    });
  }

  protected move(product: Product, direction: StockDirection, quantityInput: HTMLInputElement): void {
    const quantity = Number(quantityInput.value);
    if (!Number.isInteger(quantity) || quantity <= 0) {
      this.error.set('Enter a whole quantity greater than zero.');
      return;
    }
    this.busyId.set(product.id);
    this.error.set(null);
    this.api.moveStock(product.id, direction, quantity).subscribe({
      next: (updated) => {
        this.products.update((list) => list.map((p) => (p.id === updated.id ? updated : p)));
        quantityInput.value = '';
        this.busyId.set(null);
      },
      error: (err: unknown) => {
        this.error.set(problemMessage(err));
        this.busyId.set(null);
      },
    });
  }

  private show(items: Product[]): void {
    this.products.set(items);
    this.error.set(null);
    this.loading.set(false);
  }

  private fail(err: unknown): void {
    this.error.set(problemMessage(err));
    this.loading.set(false);
  }
}
