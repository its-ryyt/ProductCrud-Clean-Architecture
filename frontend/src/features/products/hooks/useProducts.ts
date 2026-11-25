'use client';

import { useMutation, useQuery } from '@apollo/client';
import { GET_PRODUCTS } from '../api/product.queries';
import { CREATE_PRODUCT, DELETE_PRODUCT, UPDATE_PRODUCT } from '../api/product.mutations';
import { CreateProductInput, GetProductsResponse, UpdateProductInput } from '../types/product.types';

const refetchOptions = {
  refetchQueries: [{ query: GET_PRODUCTS }],
  awaitRefetchQueries: true,
};

export function useProducts() {
  const { data, loading, error } = useQuery<GetProductsResponse>(GET_PRODUCTS);

  const [createMutation, { loading: creating }] = useMutation(CREATE_PRODUCT, refetchOptions);
  const [updateMutation, { loading: updating }] = useMutation(UPDATE_PRODUCT, refetchOptions);
  const [deleteMutation, { loading: deleting }] = useMutation(DELETE_PRODUCT, refetchOptions);

  return {
    products: data?.products ?? [],
    loading,
    error,
    saving: creating || updating,
    deleting,
    createProduct: (input: CreateProductInput) => createMutation({ variables: { input } }),
    updateProduct: (input: UpdateProductInput) => updateMutation({ variables: { input } }),
    deleteProduct: (id: number) => deleteMutation({ variables: { id } }),
  };
}
