# Seller API

This project uses the current route style, not `/api/v1`.

Implemented:
- Seller profile endpoints under `/api/Sellers`
- Product catalog endpoints under `/api/Products`
- Seller product list endpoints under `/api/Sellers`

Orders, payments, cart, checkout, reviews, delivery tracking, and earnings dashboards are not implemented yet.
Product image management currently stores image URLs/references only. The existing `ImageController`/`ImageService` do not provide a complete upload flow yet.

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

Use `/api/Sellers/me` for the current seller profile shell. Use `/api/Sellers/{sellerId}` when showing another seller publicly.

## List Products

`GET /api/Products`

Auth: not required.

Public catalog lists active, non-deleted products only.

Query:
- `search`
- `sellerId`
- `categoryId`
- `minPrice`
- `maxPrice`
- `inStock`
- `pageNumber`
- `pageSize`

Response `200 OK`:
```json
{
  "items": [
    {
      "id": "2fd765d8-3f43-4fc5-8af1-384a647383f8",
      "sellerId": "seller-user-id",
      "sellerStoreName": "Fasally Fabrics",
      "categoryId": 1,
      "categoryName": "Cotton",
      "name": "Egyptian Cotton Fabric",
      "description": "Soft cotton fabric",
      "price": 250,
      "stock": 30,
      "status": 1,
      "createdAt": "2026-05-23T00:00:00Z",
      "updatedAt": null,
      "images": [
        {
          "id": "b6bba76a-2368-4947-8352-832930901d43",
          "imageUrl": "https://cdn.example.com/fabric.jpg",
          "altText": "Blue cotton fabric",
          "sortOrder": 0
        }
      ],
      "variants": [
        {
          "id": "6fa00415-b829-4829-b2fb-f5b09cc0ba64",
          "type": "color",
          "value": "blue"
        }
      ]
    }
  ],
  "pageNumber": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## Get Product Details

`GET /api/Products/{productId}`

Auth: not required.

Response: product DTO.

Errors:
- `404 Not Found` when the product does not exist or was deleted.

## Create Product

`POST /api/Products`

Auth: required.

Role/permission: seller must have `products:create`.

Request:
```json
{
  "name": "Egyptian Cotton Fabric",
  "description": "Soft cotton fabric",
  "price": 250,
  "stock": 30,
  "categoryId": 1,
  "status": 1
}
```

Response `201 Created`: product DTO.

Validation:
- `name` is required and max 150 characters.
- `description` max 2000 characters.
- `price` must be greater than 0.
- `stock` must be greater than or equal to 0.
- `categoryId`, when provided, must exist in `/api/Categories`.
- Authenticated user must already have a seller profile.

## Update Product

`PUT /api/Products/{productId}`

Auth: required.

Role/permission: seller must have `products:update`.

Request: same fields as create.

Response:
- `204 No Content`

Validation:
- Same rules as create.
- Only the owning seller can update the product.

## Delete Product

`DELETE /api/Products/{productId}`

Auth: required.

Role/permission: seller must have `products:delete`.

Response:
- `204 No Content`

Notes:
- Delete is implemented as a soft delete.
- Only the owning seller can delete the product.

## List Current Seller Products

`GET /api/Sellers/me/products`

Auth: required.

Role/permission: seller must have `seller:products:read`.

Query: same pagination/filter fields as `GET /api/Products`, plus optional `status`. The backend forces `sellerId` to the authenticated seller and can return inactive products owned by the seller.

## List Seller Products

`GET /api/Sellers/{sellerId}/products`

Auth: not required.

Query: same pagination/filter fields as `GET /api/Products`. The backend forces `sellerId` to the route value and returns active products only.

## Add Product Image

`POST /api/Products/{productId}/images`

Auth: required.

Role/permission: seller must have `products:images:add`.

Request:
```json
{
  "imageUrl": "https://cdn.example.com/fabric.jpg",
  "altText": "Blue cotton fabric",
  "sortOrder": 0
}
```

Response `200 OK`:
```json
{
  "id": "b6bba76a-2368-4947-8352-832930901d43",
  "imageUrl": "https://cdn.example.com/fabric.jpg",
  "altText": "Blue cotton fabric",
  "sortOrder": 0
}
```

Validation:
- `imageUrl` is required and max 500 characters.
- `altText` max 200 characters.
- `sortOrder` must be greater than or equal to 0.
- Only the owning seller can add images.

## Delete Product Image

`DELETE /api/Products/{productId}/images/{imageId}`

Auth: required.

Role/permission: seller must have `products:images:delete`.

Response:
- `204 No Content`

Notes:
- Delete is implemented as a soft delete.
- Only the owning seller can delete product images.

## Add Product Variant

`POST /api/Products/{productId}/variants`

Auth: required.

Role/permission: seller must have `products:variants:add`.

Request:
```json
{
  "type": "color",
  "value": "blue"
}
```

Response `200 OK`:
```json
{
  "id": "6fa00415-b829-4829-b2fb-f5b09cc0ba64",
  "type": "color",
  "value": "blue"
}
```

Validation:
- `type` is required and max 100 characters.
- `value` is required and max 200 characters.
- Only the owning seller can add variants.

## Update Product Variant

`PUT /api/Products/{productId}/variants/{variantId}`

Auth: required.

Role/permission: seller must have `products:variants:update`.

Request:
```json
{
  "type": "size",
  "value": "large"
}
```

Response:
- `204 No Content`

## Delete Product Variant

`DELETE /api/Products/{productId}/variants/{variantId}`

Auth: required.

Role/permission: seller must have `products:variants:delete`.

Response:
- `204 No Content`

Notes:
- Delete is implemented as a soft delete.
- Only the owning seller can update or delete variants.

## Update Product Stock

`PUT /api/Products/{productId}/stock`

Auth: required.

Role/permission: seller must have `products:stock:update`.

Request:
```json
{
  "stock": 25,
  "reason": "Manual stock count"
}
```

Response:
- `204 No Content`

Validation:
- `stock` must be greater than or equal to 0.
- `reason` max is 500 characters.
- Only the owning seller can update stock.

Notes:
- Stock changes create an inventory log with old stock, new stock, and change amount.

## Get Product Inventory

`GET /api/Products/{productId}/inventory`

Auth: required.

Role/permission: seller must have `products:inventory:read`.

Response `200 OK`:
```json
{
  "productId": "2fd765d8-3f43-4fc5-8af1-384a647383f8",
  "currentStock": 25,
  "logs": [
    {
      "id": "4a0d2f80-8efe-4738-93d2-d7adbe496c27",
      "productId": "2fd765d8-3f43-4fc5-8af1-384a647383f8",
      "oldStock": 30,
      "newStock": 25,
      "changeAmount": -5,
      "reason": "Manual stock count",
      "createdAt": "2026-05-23T00:00:00Z",
      "createdById": "seller-user-id"
    }
  ]
}
```

Notes:
- Only the owning seller can view inventory history.

## Seller Dashboard

`GET /api/Sellers/me/dashboard`

Auth: required.

Role/permission: seller must have `seller:dashboard:read`.

Response `200 OK`:
```json
{
  "totalProducts": 12,
  "activeProducts": 10,
  "outOfStockProducts": 2,
  "latestProducts": [
    {
      "id": "2fd765d8-3f43-4fc5-8af1-384a647383f8",
      "name": "Egyptian Cotton Fabric",
      "price": 250,
      "stock": 25,
      "status": 1,
      "createdAt": "2026-05-23T00:00:00Z"
    }
  ]
}
```

Notes:
- Dashboard is based on seller/product data only.
- Orders, earnings, payments, reviews, and delivery metrics are not included because those modules are not implemented.
