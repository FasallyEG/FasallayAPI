# Seller API

This project uses the current route style, not `/api/v1`.

Implemented in Part 1:
- Seller profile endpoints under `/api/Sellers`

Products, images, variants, inventory, dashboard, orders, payments, cart, checkout, reviews, and delivery tracking are not implemented in Part 1.

## Create Seller Profile

`POST /api/Sellers/profile`

Auth: required.

Role/permission: user must have `seller:profile:create` and the `Seller` role.

Request:
```json
{
  "storeName": "Fasally Fabrics",
  "description": "Premium fabric seller",
  "businessPhone": "+201000000000",
  "businessEmail": "seller@example.com",
  "shopImageUrl": "https://cdn.example.com/shop.jpg"
}
```

Response `201 Created`:
```json
{
  "id": "user-id",
  "storeName": "Fasally Fabrics",
  "description": "Premium fabric seller",
  "businessPhone": "+201000000000",
  "businessEmail": "seller@example.com",
  "shopImageUrl": "https://cdn.example.com/shop.jpg",
  "status": 1,
  "isVerified": true,
  "averageRating": 0,
  "totalReviews": 0
}
```

Validation:
- `storeName` is required and max 100 characters.
- `description` max 1000 characters.
- `businessPhone` max 50 characters.
- `businessEmail` must be a valid email and max 256 characters.
- `shopImageUrl` max 500 characters.
- The authenticated user cannot already have a seller profile.

Errors:
- `401 Unauthorized` when the token is missing or invalid.
- `403 Forbidden` when the user does not have the seller role or permission.
- `409 Conflict` for duplicate seller profiles.

## Get Current Seller Profile

`GET /api/Sellers/me`

Auth: required.

Role/permission: `seller:profile:read`.

Response `200 OK`:
```json
{
  "id": "user-id",
  "storeName": "Fasally Fabrics",
  "description": "Premium fabric seller",
  "businessPhone": "+201000000000",
  "businessEmail": "seller@example.com",
  "shopImageUrl": "https://cdn.example.com/shop.jpg",
  "status": 1,
  "isVerified": true,
  "averageRating": 0,
  "totalReviews": 0
}
```

Errors:
- `404 Not Found` when the authenticated user does not have a seller profile.

## Get Seller Profile By Id

`GET /api/Sellers/{sellerId}`

Auth: required.

Role/permission: `seller:profile:details`.

Public response fields are limited and do not expose business email or phone.

Response `200 OK`:
```json
{
  "id": "seller-user-id",
  "storeName": "Fasally Fabrics",
  "description": "Premium fabric seller",
  "shopImageUrl": "https://cdn.example.com/shop.jpg",
  "isVerified": true,
  "averageRating": 0,
  "totalReviews": 0
}
```

Errors:
- `404 Not Found` when the seller profile does not exist.

## Update Current Seller Profile

`PUT /api/Sellers/profile`

Auth: required.

Role/permission: `seller:profile:update`.

Request:
```json
{
  "storeName": "Fasally Fabrics",
  "description": "Updated seller description",
  "businessPhone": "+201000000000",
  "businessEmail": "seller@example.com",
  "shopImageUrl": "https://cdn.example.com/shop.jpg"
}
```

Response:
- `204 No Content`

Validation:
- Same field rules as create.
- Only the authenticated user's seller profile can be updated.

Errors:
- `404 Not Found` when the authenticated user does not have a seller profile.

## Frontend Notes

Send authenticated requests with:
```http
Authorization: Bearer <access_token>
```

Use `/api/Sellers/me` for the current seller dashboard/profile shell. Use `/api/Sellers/{sellerId}` when showing another seller publicly.

Product endpoints are planned for later parts and should not be called yet.
