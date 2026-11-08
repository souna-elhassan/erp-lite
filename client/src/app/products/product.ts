/** Mirrors ErpLite.Application.Products.ProductDto (API contract). */
export interface Product {
  id: string;
  sku: string;
  name: string;
  unitPrice: number;
  quantityOnHand: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  unitPrice: number;
}

export interface UpdateProductRequest {
  name: string;
  unitPrice: number;
  isActive: boolean;
}

export type StockDirection = 'receive' | 'issue';

/** Stock at or below this level is flagged as low in the UI. */
export const LOW_STOCK_THRESHOLD = 10;
