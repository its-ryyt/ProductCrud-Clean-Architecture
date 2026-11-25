'use client';

import Button from '@/components/ui/Button';
import { formatCurrency } from '@/lib/utils/format';
import { Product } from '../types/product.types';

interface ProductTableProps {
  products: Product[];
  deleting: boolean;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
}

export default function ProductTable({ products, deleting, onEdit, onDelete }: ProductTableProps) {
  if (products.length === 0) {
    return <p className="p-5 text-gray-500">No products found.</p>;
  }

  return (
    <table className="w-full text-left text-sm">
      <thead className="bg-gray-50">
        <tr>
          <th className="px-4 py-3">ID</th>
          <th className="px-4 py-3">Name</th>
          <th className="px-4 py-3">Description</th>
          <th className="px-4 py-3 text-right">Price</th>
          <th className="px-4 py-3 text-right">Qty</th>
          <th className="px-4 py-3 text-center">Actions</th>
        </tr>
      </thead>
      <tbody>
        {products.map((p) => (
          <tr key={p.id} className="border-t">
            <td className="px-4 py-3">{p.id}</td>
            <td className="px-4 py-3 font-medium">{p.name}</td>
            <td className="px-4 py-3 text-gray-600">{p.description ?? '-'}</td>
            <td className="px-4 py-3 text-right">{formatCurrency(p.price)}</td>
            <td className="px-4 py-3 text-right">{p.quantity}</td>
            <td className="px-4 py-3">
              <div className="flex justify-center gap-2">
                <Button variant="warning" className="px-3 py-1" onClick={() => onEdit(p)}>
                  Edit
                </Button>
                <Button variant="danger" className="px-3 py-1" disabled={deleting} onClick={() => onDelete(p)}>
                  Delete
                </Button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
