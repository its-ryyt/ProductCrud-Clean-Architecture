'use client';

import { useState } from 'react';
import Alert, { AlertMessage } from '@/components/ui/Alert';
import { getErrorMessage } from '@/lib/utils/error';
import { useProducts } from '../hooks/useProducts';
import { Product, ProductFormValues } from '../types/product.types';
import { validateProductForm } from '../utils/product.validation';
import ProductForm from './ProductForm';
import ProductTable from './ProductTable';

export default function ProductsView() {
  const { products, loading, error, saving, deleting, createProduct, updateProduct, deleteProduct } = useProducts();

  const [editing, setEditing] = useState<Product | null>(null);
  const [formKey, setFormKey] = useState(0); // changing the key resets the form
  const [message, setMessage] = useState<AlertMessage | null>(null);

  const resetForm = () => {
    setEditing(null);
    setFormKey((k) => k + 1);
  };

  const handleSubmit = async (values: ProductFormValues) => {
    setMessage(null);

    const result = validateProductForm(values);
    if (!result.ok) {
      setMessage({ type: 'error', text: result.error });
      return;
    }

    try {
      if (editing) {
        await updateProduct({ id: editing.id, ...result.input });
        setMessage({ type: 'success', text: 'Product updated.' });
      } else {
        await createProduct(result.input);
        setMessage({ type: 'success', text: 'Product created.' });
      }
      resetForm();
    } catch (err) {
      setMessage({ type: 'error', text: getErrorMessage(err) });
    }
  };

  const handleEdit = (product: Product) => {
    setEditing(product);
    setFormKey((k) => k + 1);
    setMessage(null);
  };

  const handleDelete = async (product: Product) => {
    if (!confirm(`Delete "${product.name}"?`)) return;
    setMessage(null);

    try {
      await deleteProduct(product.id);
      if (editing?.id === product.id) resetForm();
      setMessage({ type: 'success', text: 'Product deleted.' });
    } catch (err) {
      setMessage({ type: 'error', text: getErrorMessage(err) });
    }
  };

  return (
    <section>
      <h1 className="mb-6 text-2xl font-bold">Products</h1>

      {message && <Alert message={message} onClose={() => setMessage(null)} />}

      <ProductForm key={formKey} product={editing} saving={saving} onSubmit={handleSubmit} onCancel={resetForm} />

      <div className="overflow-x-auto rounded-lg bg-white shadow">
        {loading ? (
          <p className="p-5">Loading products...</p>
        ) : error ? (
          <p className="p-5 text-red-600">Error: {error.message}</p>
        ) : (
          <ProductTable products={products} deleting={deleting} onEdit={handleEdit} onDelete={handleDelete} />
        )}
      </div>
    </section>
  );
}
