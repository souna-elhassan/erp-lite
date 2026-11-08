import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { problemMessage } from '../../core/problem';
import { ProductApi } from '../product-api';

/** Limits mirror the domain rules (Product.SkuMaxLength / NameMaxLength). The API re-checks them. */
export const SKU_MAX_LENGTH = 32;
export const NAME_MAX_LENGTH = 200;

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.html',
})
export class ProductForm {
  private readonly api = inject(ProductApi);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    sku: ['', [Validators.required, Validators.maxLength(SKU_MAX_LENGTH)]],
    name: ['', [Validators.required, Validators.maxLength(NAME_MAX_LENGTH)]],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    this.api.create(this.form.getRawValue()).subscribe({
      next: () => void this.router.navigate(['/products']),
      error: (err: unknown) => {
        this.error.set(problemMessage(err));
        this.saving.set(false);
      },
    });
  }

  protected invalid(control: 'sku' | 'name' | 'unitPrice'): boolean {
    const c = this.form.controls[control];
    return c.invalid && c.touched;
  }
}
