# Developer Evaluation Project

`READ CAREFULLY`

## Instructions
**The test below will have up to 7 calendar days to be delivered from the date of receipt of this manual.**

- The code must be versioned in a public Github repository and a link must be sent for evaluation once completed
- Upload this template to your repository and start working from it
- Read the instructions carefully and make sure all requirements are being addressed
- The repository must provide instructions on how to configure, execute and test the project
- Documentation and overall organization will also be taken into consideration

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

### Running the project

To run this project, just run the following command in the root folder, where the docker-compose file is located.
```
docker-compose up --build
```
This will build the project, start the containers and run the migrations

From there, you can use the following endpoints:

If, for some reason, the migrations didn`t run and the sales table isnt found, you can try the following:
-> Navigate to Ambev.DeveloperEvaluation.WebApi
-> Make sure that you have EF Core tools installed (if not, run dotnet tool install --global dotnet-ef)
-> run dotnet ef database update

## Endpoints

```
POST /api/sales -- Create a sale
```
```json
{
  "saleDate": "2025-01-27T09:00:00Z",
  "customerId": "12345678-1234-1234-1234-123456789abc",
  "customerName": "John Doe 2",
  "branchId": "98765432-9876-9876-9876-987654321def",
  "branchName": "Main Branch",
  "items": [
    {
      "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
      "productName": "Product 1",
      "unitPrice": 100.00,
      "quantity": 5
    },
    {
      "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
      "productName": "Product 2",
      "unitPrice": 150.00,
      "quantity": 3
    }
  ]
}
```
returns
```json
{
    "data": {
        "saleNumber": "SALE-20250127-4C370364",
        "sale": {
            "id": "50276b72-eac7-4f6b-8785-c20d662d98d3",
            "saleNumber": "SALE-20250127-4C370364",
            "saleDate": "2025-01-27T09:00:00Z",
            "customerId": "12345678-1234-1234-1234-123456789abc",
            "customerName": "John Doe 2",
            "branchId": "98765432-9876-9876-9876-987654321def",
            "branchName": "Main Branch",
            "totalAmount": 900.0000,
            "isCancelled": false,
            "items": [
                {
                    "id": "6815fb1e-7ed0-495f-92bd-8bb6cb272bd7",
                    "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
                    "productName": "Product 1",
                    "unitPrice": 100.00,
                    "quantity": 5,
                    "discount": 0.10,
                    "totalAmount": 450.0000,
                    "isCancelled": false
                },
                {
                    "id": "7c1f8def-84e2-40bd-8b5d-226e12ab4e2b",
                    "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
                    "productName": "Product 2",
                    "unitPrice": 150.00,
                    "quantity": 3,
                    "discount": 0,
                    "totalAmount": 450.00,
                    "isCancelled": false
                }
            ]
        }
    },
    "success": true,
    "message": "Sale created successfully",
    "errors": []
}
```
```
PUT /api/sales/{SALE_ID} -- Updates a sale
```
```json
{
  "customerId": "12345678-1234-1234-1234-123456789abc",
  "customerName": "John Doe Test",
  "branchId": "98765432-9876-9876-9876-987654321def",
  "branchName": "Main Branch Test",
  "items": [
    {
      "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
      "productName": "Product 1 Test",
      "unitPrice": 100.00,
      "quantity": 11
    },
    {
      "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
      "productName": "Product 2 Test",
      "unitPrice": 150.00,
      "quantity": 18
    }
  ]
  
}
```
returns
```json
{
    "data": {
        "sale": {
            "id": "50276b72-eac7-4f6b-8785-c20d662d98d3",
            "saleNumber": "SALE-20250127-4C370364",
            "saleDate": "2025-01-27T09:00:00Z",
            "customerId": "12345678-1234-1234-1234-123456789abc",
            "customerName": "John Doe Test",
            "branchId": "98765432-9876-9876-9876-987654321def",
            "branchName": "Main Branch Test",
            "totalAmount": 3040.0000,
            "isCancelled": false,
            "items": [
                {
                    "id": "6815fb1e-7ed0-495f-92bd-8bb6cb272bd7",
                    "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
                    "productName": "Product 1 Test",
                    "unitPrice": 100.00,
                    "quantity": 11,
                    "discount": 0.20,
                    "totalAmount": 880.0000,
                    "isCancelled": false
                },
                {
                    "id": "7c1f8def-84e2-40bd-8b5d-226e12ab4e2b",
                    "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
                    "productName": "Product 2 Test",
                    "unitPrice": 150.00,
                    "quantity": 18,
                    "discount": 0.20,
                    "totalAmount": 2160.0000,
                    "isCancelled": false
                }
            ]
        }
    },
    "success": true,
    "message": "",
    "errors": []
}
```
```
POST /api/sales/{SALE_ID}/cancel -- Cancels a sale
```
returns
```json
{
    "data": {
        "sale": {
            "id": "50276b72-eac7-4f6b-8785-c20d662d98d3",
            "saleNumber": "SALE-20250127-4C370364",
            "saleDate": "2025-01-27T09:00:00Z",
            "customerId": "12345678-1234-1234-1234-123456789abc",
            "customerName": "John Doe Test",
            "branchId": "98765432-9876-9876-9876-987654321def",
            "branchName": "Main Branch Test",
            "totalAmount": 0,
            "isCancelled": true,
            "items": [
                {
                    "id": "6815fb1e-7ed0-495f-92bd-8bb6cb272bd7",
                    "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
                    "productName": "Product 1 Test",
                    "unitPrice": 100.00,
                    "quantity": 11,
                    "discount": 0.20,
                    "totalAmount": 880.00,
                    "isCancelled": true
                },
                {
                    "id": "7c1f8def-84e2-40bd-8b5d-226e12ab4e2b",
                    "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
                    "productName": "Product 2 Test",
                    "unitPrice": 150.00,
                    "quantity": 18,
                    "discount": 0.20,
                    "totalAmount": 2160.00,
                    "isCancelled": true
                }
            ]
        }
    },
    "success": true,
    "message": "",
    "errors": []
}
```
```
GET /api/sales/{SALE_ID} -- get a sale by its id
```
returns
```json
{
    "data": {
        "sale": {
            "id": "50276b72-eac7-4f6b-8785-c20d662d98d3",
            "saleNumber": "SALE-20250127-4C370364",
            "saleDate": "2025-01-27T09:00:00Z",
            "customerId": "12345678-1234-1234-1234-123456789abc",
            "customerName": "John Doe Test",
            "branchId": "98765432-9876-9876-9876-987654321def",
            "branchName": "Main Branch Test",
            "totalAmount": 3040.00,
            "isCancelled": false,
            "items": [
                {
                    "id": "6815fb1e-7ed0-495f-92bd-8bb6cb272bd7",
                    "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
                    "productName": "Product 1 Test",
                    "unitPrice": 100.00,
                    "quantity": 11,
                    "discount": 0.20,
                    "totalAmount": 880.00,
                    "isCancelled": false
                },
                {
                    "id": "7c1f8def-84e2-40bd-8b5d-226e12ab4e2b",
                    "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
                    "productName": "Product 2 Test",
                    "unitPrice": 150.00,
                    "quantity": 18,
                    "discount": 0.20,
                    "totalAmount": 2160.00,
                    "isCancelled": false
                }
            ]
        }
    },
    "success": true,
    "message": "",
    "errors": []
}
```
```
GET /api/sales -- list all sale records
```
returns
```json
{
    "data": {
        "currentPage": 1,
        "totalPages": 1,
        "totalCount": 1,
        "data": [
            {
                "id": "50276b72-eac7-4f6b-8785-c20d662d98d3",
                "saleNumber": "SALE-20250127-4C370364",
                "saleDate": "2025-01-27T09:00:00Z",
                "customerId": "12345678-1234-1234-1234-123456789abc",
                "customerName": "John Doe 2",
                "branchId": "98765432-9876-9876-9876-987654321def",
                "branchName": "Main Branch",
                "totalAmount": 900.00,
                "isCancelled": false,
                "items": [
                    {
                        "id": "6815fb1e-7ed0-495f-92bd-8bb6cb272bd7",
                        "productId": "abcdef12-abcd-abcd-abcd-abcdef123456",
                        "productName": "Product 1",
                        "unitPrice": 100.00,
                        "quantity": 5,
                        "discount": 0.10,
                        "totalAmount": 450.00,
                        "isCancelled": false
                    },
                    {
                        "id": "7c1f8def-84e2-40bd-8b5d-226e12ab4e2b",
                        "productId": "fedcba98-fedc-fedc-fedc-fedcba987654",
                        "productName": "Product 2",
                        "unitPrice": 150.00,
                        "quantity": 3,
                        "discount": 0.00,
                        "totalAmount": 450.00,
                        "isCancelled": false
                    }
                ]
            }
        ],
        "success": true,
        "message": "",
        "errors": []
    },
    "success": true,
    "message": "",
    "errors": []
}
```