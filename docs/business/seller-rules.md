# Seller Rules

These rules describe the seller profile functionality implemented in Part 1.

## Seller Permissions

- Creating a seller profile requires authentication, `seller:profile:create`, and the `Seller` role.
- Reading the current seller profile requires `seller:profile:read`.
- Reading a public seller profile requires `seller:profile:details`.
- Updating the current seller profile requires `seller:profile:update`.
- Creating products requires `products:create`.
- Updating products requires `products:update`.
- Deleting products requires `products:delete`.
- Reading current seller products requires `seller:products:read`.

## Seller Profile Rules

- A user can have at most one seller profile.
- Seller profile creation is rejected unless the authenticated user has the `Seller` role.
- Sellers can only update their own profile.
- Public seller profile responses must not expose business phone or business email.
- Seller profile responses must use DTOs only.

## Product Ownership Rules

- Products belong to one seller profile.
- Product create uses the authenticated user's seller profile.
- Product update and delete require the authenticated seller to own the product.
- Public product list and detail endpoints expose DTOs only.
- Deleted products are filtered out of product list/detail responses.

## Product Validation Rules

- Product name is required and max 150 characters.
- Product description max is 2000 characters.
- Product price must be greater than 0.
- Product stock must be greater than or equal to 0.
- Product status must be a valid `ProductStatus`.

## Inventory Rules

- Product stock is stored on the product in Part 2.
- Inventory history/log endpoints are not implemented until the inventory part.

## Category Rules

- Product `categoryId` is optional.
- When provided, `categoryId` must exist in the current `/api/Categories` data.
- No duplicate product category management is implemented.

## Edge Cases

- Creating a profile twice returns a conflict.
- Reading or updating a missing current seller profile returns not found.
- Reading a missing seller id returns not found.
- Invalid request fields are rejected by FluentValidation.
- Updating or deleting a product owned by another seller returns forbidden.
- Listing a missing seller's products returns not found.

## Not Implemented Yet

Product images, product variants, inventory history, orders, cart, checkout, payments, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, dashboards, event bus, background workers, and WebSockets are not implemented for sellers yet.
