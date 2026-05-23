# Seller Flow

This document covers the seller profile flow, product catalog flow, product image URL/reference flow, and product variant flow implemented through Part 3. Inventory history, dashboard, order, payment, cart, checkout, review, and delivery flows are future integrations.

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

1. Seller uploads or prepares an image outside the current product API.
2. Seller sends `POST /api/Products/{productId}/images` with the image URL/reference.
3. Backend validates that the authenticated seller owns the product.
4. Backend stores the image URL/reference, alt text, and sort order.
5. Seller can delete the image with `DELETE /api/Products/{productId}/images/{imageId}`.
6. Delete is soft delete; deleted images are hidden from product responses.

## Variant Flow

1. Seller sends `POST /api/Products/{productId}/variants` with `type` and `value`.
2. Backend validates that the authenticated seller owns the product.
3. Backend stores the variant.
4. Seller can update a variant with `PUT /api/Products/{productId}/variants/{variantId}`.
5. Seller can delete a variant with `DELETE /api/Products/{productId}/variants/{variantId}`.
6. Delete is soft delete; deleted variants are hidden from product responses.

## Inventory Update Flow

Not implemented in Part 1. Planned for a later inventory part.

## Dashboard Flow

Not implemented in Part 2. A lightweight seller summary can be added from seller/product data in a later part.

## Future Integration Notes

Orders, payments, cart, checkout, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, event bus, background workers, and WebSockets are not part of the current seller profile flow.
