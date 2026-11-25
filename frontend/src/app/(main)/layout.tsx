import Link from 'next/link';

export default function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen">
      <header className="bg-white shadow">
        <nav className="mx-auto flex max-w-5xl items-center gap-6 px-6 py-4">
          <span className="text-lg font-bold text-blue-600">ProductCrud</span>
          <Link href="/products" className="text-sm font-medium hover:text-blue-600">
            Products
          </Link>
        </nav>
      </header>
      <main className="mx-auto max-w-5xl p-6">{children}</main>
    </div>
  );
}
