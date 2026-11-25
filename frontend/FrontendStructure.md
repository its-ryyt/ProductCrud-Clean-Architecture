# ProductCrud – Frontend Structure Guide (Next.js + TypeScript + GraphQL)

## Folder Structure

```
frontend/
├── .env.local
└── src/
    ├── app/                              # Routing only (thin pages)
    │   ├── (main)/
    │   │   ├── layout.tsx                # App shell (header/nav)
    │   │   └── products/page.tsx
    │   ├── globals.css
    │   ├── layout.tsx                    # Root layout + providers
    │   └── page.tsx                      # Redirect to /products
    ├── components/
    │   └── ui/                           # Reusable, business-agnostic UI
    │       ├── Alert.tsx
    │       ├── Button.tsx
    │       └── Input.tsx
    ├── config/
    │   └── env.ts                        # Environment variables in one place
    ├── features/
    │   └── products/                     # Product module
    │       ├── api/
    │       │   ├── product.queries.ts
    │       │   └── product.mutations.ts
    │       ├── components/
    │       │   ├── ProductForm.tsx
    │       │   ├── ProductTable.tsx
    │       │   └── ProductsView.tsx      # Page container (state + logic)
    │       ├── hooks/
    │       │   └── useProducts.ts
    │       ├── types/
    │       │   └── product.types.ts
    │       ├── utils/
    │       │   └── product.validation.ts
    │       └── index.ts                  # Public exports of the module
    ├── lib/
    │   ├── apollo/
    │   │   └── apollo-client.ts
    │   └── utils/
    │       ├── error.ts
    │       └── format.ts
    └── providers/
        └── AppProviders.tsx
```

## Folder Responsibilities

| Folder | Purpose |
|---|---|
| `app/` | Routes and layouts only. Pages just render a feature component |
| `components/ui/` | Generic, reusable UI with no business logic |
| `config/` | Environment variables and app settings |
| `features/<module>/` | Everything for one business module (api, components, hooks, types, utils) |
| `lib/` | Third-party setup (Apollo) and shared helpers |
| `providers/` | Global React context providers |

## Creation Checklist (in order)

### 1. Setup
- [ ] `.env.local`
- [ ] `src/config/env.ts`

### 2. lib and providers
- [ ] `src/lib/apollo/apollo-client.ts`
- [ ] `src/lib/utils/format.ts`
- [ ] `src/lib/utils/error.ts`
- [ ] `src/providers/AppProviders.tsx`

### 3. Shared UI
- [ ] `src/components/ui/Button.tsx`
- [ ] `src/components/ui/Input.tsx`
- [ ] `src/components/ui/Alert.tsx`

### 4. Products feature
- [ ] `types/product.types.ts`
- [ ] `api/product.queries.ts`
- [ ] `api/product.mutations.ts`
- [ ] `utils/product.validation.ts`
- [ ] `hooks/useProducts.ts`
- [ ] `components/ProductForm.tsx`
- [ ] `components/ProductTable.tsx`
- [ ] `components/ProductsView.tsx`
- [ ] `index.ts`

### 5. App routes
- [ ] `src/app/layout.tsx`
- [ ] `src/app/page.tsx`
- [ ] `src/app/(main)/layout.tsx`
- [ ] `src/app/(main)/products/page.tsx`

### 6. Run
- [ ] Backend: `dotnet run --project ProductCrud.API --launch-profile http`
- [ ] Frontend: `npm run dev`, then open `http://localhost:3000`