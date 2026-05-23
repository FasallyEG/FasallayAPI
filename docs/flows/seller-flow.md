# Seller Flow

This document covers the seller profile flow implemented in Part 1. Product, image, variant, inventory, dashboard, order, payment, cart, checkout, review, and delivery flows are future integrations.

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

Not implemented in Part 1. Planned for Part 2.

## Product Update/Delete Flow

Not implemented in Part 1. Planned for Part 2.

## Product Image Flow

Not implemented in Part 1. Planned for a later product media part.

## Variant Flow

Not implemented in Part 1. Planned for a later product variant part.

## Inventory Update Flow

Not implemented in Part 1. Planned for a later inventory part.

## Dashboard Flow

Not implemented in Part 1. A lightweight seller summary can be added after product data exists.

## Future Integration Notes

Orders, payments, cart, checkout, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, event bus, background workers, and WebSockets are not part of the current seller profile flow.
