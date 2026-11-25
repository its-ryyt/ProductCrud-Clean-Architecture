export interface Product {
  id: number;
  name: string;
  description: string | null;
  price: number;
  quantity: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProductFormValues {
  name: string;
  description: string;
  price: string;
  quantity: string;
}

export interface CreateProductInput {
  name: string;
  description: string | null;
  price: number;
  quantity: number;
}

export interface UpdateProductInput extends CreateProductInput {
  id: number;
}

export interface GetProductsResponse {
  products: Product[];
}
