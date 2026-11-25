export const formatCurrency = (value: number): string =>
  Number(value).toLocaleString('en-PH', { style: 'currency', currency: 'PHP' });

export const formatDate = (value: string | null): string => (value ? new Date(value).toLocaleString('en-PH') : '-');
