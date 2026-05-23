# Seller Flow

This document covers the seller profile flow and product catalog flow implemented through Part 2. Product image, variant, inventory history, dashboard, order, payment, cart, checkout, review, and delivery flows are future integrations.

## Seller Profile Flow

1. User requests a seller upgrade through the existing profile upgrade flow using `ProfileType.Seller`.
2. Admin or operations assigns the `Seller` role using the existing role/user management tools.
3. Seller creates a profile with `POST /api/Sellers/profile`.
4. Backend verifies the authenticated user has the `Seller` role.
5. Backend rejects duplicate seller profiles for the same user.
6. Backend stores the seller profile and marks the user profile as completed.
7. Seller can read their full profile with `GET /api/Sellers/me`.
8. Seller can update their own profile with `PUT /api/Sellers/profile`.
9. Other authenticated users with the seller-details permission can read the limited public profile with `GET /api/Sellers/{sellerId}`.

## Product Creation Flow

1. Seller creates a seller profile first.
2. Seller sends `POST /api/Products` with product fields.
3. Backend validates the authenticated seller has a seller profile.
4. Backend validates price, stock, status, and category existence when `categoryId` is provided.
5. Backend stores the product under the authenticated seller.
6. Backend returns the created product DTO.

## Product Update/Delete Flow

1. Seller sends `PUT /api/Products/{productId}` or `DELETE /api/Products/{productId}`.
2. Backend loads the product and rejects missing or deleted products.
3. Backend validates that the authenticated seller owns the product.
4. Update changes product fields after validation.
5. Delete marks the product as deleted and inactive.

## Product Image Flow

Not implemented in Part 1. Planned for a later product media part.

## Variant Flow

Not implemented in Part 1. Planned for a later product variant part.

## Inventory Update Flow

Not implemented in Part 1. Planned for a later inventory part.

## Dashboard Flow

Not implemented in Part 2. A lightweight seller summary can be added from seller/product data in a later part.

## Future Integration Notes

Orders, payments, cart, checkout, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, event bus, background workers, and WebSockets are not part of the current seller profile flow.
