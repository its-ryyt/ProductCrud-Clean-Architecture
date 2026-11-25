import { CreateProductInput, ProductFormValues } from '../types/product.types';

export type ProductValidationResult = { ok: true; input: CreateProductInput } | { ok: false; error: string };

export function validateProductForm(values: ProductFormValues): ProductValidationResult {
  const name = values.name.trim();
  const price = Number(values.price);
  const quantity = Number(values.quantity);

  if (!name) return { ok: false, error: 'Product name is required.' };
  if (name.length > 150) return { ok: false, error: 'Product name must not exceed 150 characters.' };
  if (values.price === '' || Number.isNaN(price) || price < 0) return { ok: false, error: 'Enter a valid price.' };
  if (values.quantity === '' || !Number.isInteger(quantity) || quantity < 0)
    return { ok: false, error: 'Enter a valid whole-number quantity.' };

  return {
    ok: true,
    input: {
      name,
      description: values.description.trim() || null,
      price,
      quantity,
    },
  };
}
