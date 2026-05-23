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
- Adding product images requires `products:images:add`.
- Deleting product images requires `products:images:delete`.
- Adding product variants requires `products:variants:add`.
- Updating product variants requires `products:variants:update`.
- Deleting product variants requires `products:variants:delete`.
- Updating product stock requires `products:stock:update`.
- Viewing product inventory requires `products:inventory:read`.
- Viewing the seller dashboard requires `seller:dashboard:read`.

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

- Product stock is stored on the product.
- Stock cannot be negative.
- Stock updates must validate product ownership.
- Stock updates through `PUT /api/Products/{productId}/stock` create inventory logs.
- Inventory logs store old stock, new stock, change amount, reason, and audit metadata.
- Product update still accepts stock as part of the full product payload; frontend should use the stock endpoint when it needs an auditable inventory adjustment.

## Product Image Rules

- Product image management stores URL/reference data only.
- File upload integration is not implemented in the product API yet.
- Product image URL is required and max 500 characters.
- Product image alt text max is 200 characters.
- Product image sort order must be greater than or equal to 0.
- Only the owning seller can add or delete product images.
- Deleted product images are hidden from product responses.

## Product Variant Rules

- Product variants use `type` and `value`.
- Variant `type` is required and max 100 characters.
- Variant `value` is required and max 200 characters.
- Only the owning seller can add, update, or delete variants.
- Deleted variants are hidden from product responses.

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
- Updating or deleting an image or variant on another seller's product returns forbidden.
- Updating stock on another seller's product returns forbidden.
- Reading inventory for another seller's product returns forbidden.
- Dashboard fields do not include orders, earnings, payments, reviews, or delivery metrics.

## Not Implemented Yet

File upload integration for product images, orders, cart, checkout, payments, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, earnings dashboards, event bus, background workers, and WebSockets are not implemented for sellers yet.
