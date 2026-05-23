# Seller Rules

These rules describe the seller profile functionality implemented in Part 1.

## Seller Permissions

- Creating a seller profile requires authentication, `seller:profile:create`, and the `Seller` role.
- Reading the current seller profile requires `seller:profile:read`.
- Reading a public seller profile requires `seller:profile:details`.
- Updating the current seller profile requires `seller:profile:update`.

## Seller Profile Rules

- A user can have at most one seller profile.
- Seller profile creation is rejected unless the authenticated user has the `Seller` role.
- Sellers can only update their own profile.
- Public seller profile responses must not expose business phone or business email.
- Seller profile responses must use DTOs only.

## Product Ownership Rules

Product ownership is not implemented in Part 1. Products will belong to seller profiles in a later part.

## Product Validation Rules

Product validation is not implemented in Part 1.

## Inventory Rules

Inventory is not implemented in Part 1.

## Category Rules

Product category integration is not implemented in Part 1. Future product work should reuse existing category APIs if the schema supports it.

## Edge Cases

- Creating a profile twice returns a conflict.
- Reading or updating a missing current seller profile returns not found.
- Reading a missing seller id returns not found.
- Invalid request fields are rejected by FluentValidation.

## Not Implemented Yet

Orders, cart, checkout, payments, wallet, refunds, chat, offers, notifications, reviews, disputes, delivery tracking, dashboards, event bus, background workers, and WebSockets are not implemented for sellers yet.
