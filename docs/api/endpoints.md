# API Endpoints

This file documents the implemented controller routes. The codebase is the source of truth.

## Auth `/Auth`
- `POST /Auth`
- `POST /Auth/refresh`
- `POST /Auth/revoke-refresh-token`
- `POST /Auth/register`
- `POST /Auth/google`
- `POST /Auth/confirm-email`
- `POST /Auth/resend-confirmation-email`
- `POST /Auth/forget-password`
- `POST /Auth/reset-password`

## Current Account `/me`
- `GET /me`
- `PUT /me/info`
- `PUT /me/change-password`
- `POST /me/request-upgrade`

## Users `/api/Users`
- `GET /api/Users`
- `GET /api/Users/{id}`
- `POST /api/Users`
- `PUT /api/Users/{id}`
- `PUT /api/Users/toggle-status/{id}`
- `PUT /api/Users/unlock-user/{id}`

## Roles `/api/Roles`
- `GET /api/Roles`
- `GET /api/Roles/{id}`
- `POST /api/Roles`
- `PUT /api/Roles/{id}`
- `PUT /api/Roles/{id}/toggle-status`

## Categories `/api/Categories`
- `GET /api/Categories`
- `GET /api/Categories/{id}`
- `POST /api/Categories`
- `PUT /api/Categories/{id}`
- `DELETE /api/Categories/{id}`

## Tailors `/api/Tailors`
- `POST /api/Tailors/profile`
- `PUT /api/Tailors/profile`
- `GET /api/Tailors/my-portfolio`
- `POST /api/Tailors/my-portfolio`
- `PUT /api/Tailors/{tailorId}/approve`
- `PUT /api/Tailors/{tailorId}/reject`

## Tailor Browsing `/api/TailorBrowsing`
- `GET /api/TailorBrowsing`
- `GET /api/TailorBrowsing/{tailorId}`

## Sellers `/api/Sellers`
- `POST /api/Sellers/profile`
- `GET /api/Sellers/me`
- `GET /api/Sellers/me/products`
- `GET /api/Sellers/{sellerId}`
- `GET /api/Sellers/{sellerId}/products`
- `PUT /api/Sellers/profile`

## Products `/api/Products`
- `GET /api/Products`
- `GET /api/Products/{productId}`
- `POST /api/Products`
- `PUT /api/Products/{productId}`
- `DELETE /api/Products/{productId}`
- `POST /api/Products/{productId}/images`
- `DELETE /api/Products/{productId}/images/{imageId}`
- `POST /api/Products/{productId}/variants`
- `PUT /api/Products/{productId}/variants/{variantId}`
- `DELETE /api/Products/{productId}/variants/{variantId}`

## Planned But Not Implemented
File upload integration for product images, inventory history, seller dashboard, search, cart, checkout, orders, payments, bookings, offers, notifications, chat, wallet, disputes, reviews, delivery/tracking, and dashboards are not implemented in the current backend.
