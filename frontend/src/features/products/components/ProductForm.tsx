'use client';

import { FormEvent, useState } from 'react';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import { Product, ProductFormValues } from '../types/product.types';

const emptyForm: ProductFormValues = { name: '', description: '', price: '', quantity: '' };

const toFormValues = (product: Product | null): ProductFormValues =>
  product
    ? {
        name: product.name,
        description: product.description ?? '',
        price: String(product.price),
        quantity: String(product.quantity),
      }
    : emptyForm;

interface ProductFormProps {
  product: Product | null;
  saving: boolean;
  onSubmit: (values: ProductFormValues) => void;
  onCancel: () => void;
}

export default function ProductForm({ product, saving, onSubmit, onCancel }: ProductFormProps) {
  const [form, setForm] = useState<ProductFormValues>(() => toFormValues(product));

  // Updates only the field that changed (computed key: [field])
  const handleChange = (field: keyof ProductFormValues, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    onSubmit(form);
  };

  return (
    <form onSubmit={handleSubmit} className="mb-8 rounded-lg bg-white p-5 shadow">
      <h2 className="mb-4 text-lg font-semibold">{product ? `Edit Product #${product.id}` : 'Add Product'}</h2>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Input
          id="name"
          label="Name"
          isRequired
          maxLength={150}
          value={form.name}
          onChange={(e) => handleChange('name', e.target.value)}
        />
        <Input
          id="description"
          label="Description"
          maxLength={500}
          value={form.description}
          onChange={(e) => handleChange('description', e.target.value)}
        />
        <Input
          id="price"
          label="Price"
          isRequired
          type="number"
          step="0.01"
          min="0"
          value={form.price}
          onChange={(e) => handleChange('price', e.target.value)}
        />
        <Input
          id="quantity"
          label="Quantity"
          isRequired
          type="number"
          step="1"
          min="0"
          value={form.quantity}
          onChange={(e) => handleChange('quantity', e.target.value)}
        />
      </div>

      <div className="mt-4 flex gap-2">
        <Button type="submit" disabled={saving}>
          {saving ? 'Saving...' : product ? 'Update' : 'Create'}
        </Button>
        {product && (
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        )}
      </div>
    </form>
  );
}
