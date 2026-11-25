import type { Metadata } from 'next';
import './globals.css';
import AppProviders from '@/providers/AppProviders';

export const metadata: Metadata = {
  title: 'Product CRUD',
  description: 'Next.js + GraphQL + ASP.NET Core',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="bg-gray-100 text-gray-900">
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
