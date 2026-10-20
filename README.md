# Inches Wig Store 💇🏽‍♀️

> **Wigs in every length.**

An e-commerce platform that lets customers discover, browse and buy wigs, hair bundles, closures and frontals, and wig-care products online.

Inches Wig Store is a full-stack web application being developed to give customers a convenient and secure way to shop for hair, and to give the business owner the tools to manage products, stock, orders and payments.

Customers will be able to browse products, choose a length, texture and colour, **see how a wig looks on them with a virtual try-on**, manage a shopping cart, place orders and pay. An administration portal will support product management, inventory control, order fulfilment and payment verification.

The initial implementation targets the South African market, with prices shown in South African rand (ZAR).

📄 The product requirements will be documented in `docs/specification.md`. Where this README and the specification differ, the specification takes precedence.

---

# 🚀 Project Overview

The platform provides two main experiences.

## 🛍️ Customer Storefront

Customers will be able to:

- Browse products by category.
- Search for products and filter the catalogue by length, texture, colour and price.
- View product descriptions, images, prices and available variants.
- Select options such as length (in inches), hair texture, colour, density, cap size or lace type, depending on the product.
- **See how a wig looks on them** with a virtual try-on, using their camera or an uploaded photo.
- Add products to their shopping cart and update quantities.
- Register and sign in securely.
- Manage delivery addresses.
- Complete checkout and place orders.
- Choose from supported online payment and manual EFT options.
- View order history and order statuses.

## 🔐 Administrator Portal

Administrators will be able to:

- View a dashboard of orders, sales and inventory.
- Create and manage product categories.
- Add and update products, descriptions, prices and images.
- Manage product variants and their individual stock levels.
- Monitor inventory and record stock movements.
- View customer orders and update fulfilment statuses.
- Review and verify EFT payments.
- Monitor payment statuses and order history.

---

# 🌍 Real-World Use Case

Inches Wig Store is the customer-facing brand used for this project.

A customer visiting the store will follow a shopping process like this:

```text
Visit Inches Wig Store
        ↓
Browse a Category
        ↓
Select a Product
        ↓
Choose Length, Texture and Colour
        ↓
Try It On (optional)
        ↓
Add Product to Cart
        ↓
Review Cart and Delivery Details
        ↓
Proceed to Checkout
        ↓
Select Payment Method
        ↓
Place Order
        ↓
Payment Verified
        ↓
Order Processed and Fulfilled
```

Wigs are sold in many combinations, and each combination has its own stock. A wig might come in 14, 18, 22 and 26 inches, in several textures and colours. The catalogue is designed so that every combination is a variant with its own price and stock quantity, and so that products that don't need every option, such as a wig-care product, are not forced to have them.

The project demonstrates how a real online retail process can be turned into a maintainable web application.

---

# ✨ Key Features

## 💇🏽‍♀️ Product Catalogue

The initial catalogue will support these product categories.

| Category | Example Products | Possible Variants |
|---|---|---|
| Wigs | Lace front wigs, full lace wigs, closure wigs, headband wigs | Length, texture, colour, density, cap size, lace type |
| Bundles & Extensions | Hair bundles, clip-ins, tape-ins, ponytails | Length, texture, colour, weight |
| Closures & Frontals | Lace closures, lace frontals | Size, length, texture, colour |
| Wig Care & Accessories | Wig caps, wig stands, brushes, shampoo, edge products | Size, colour, style |

Products will have descriptions, prices, images, availability and the variant options that apply to them. Categories and variants are managed in the administration portal.

## 🪞 Virtual Try-On: "See how it looks on you"

A customer can see how a chosen wig looks on them before buying it.

- **Camera or photo.** The wig follows the customer's face live through their camera, or is placed on an uploaded photo.
- **The real wig.** The picture shown is a photo of the actual wig, in the colour and length chosen, with an opening for the face.
- **Fine-tuning.** Simple controls move, resize and tilt the wig, and the result can be saved as a picture.
- **Hide my own hair.** On an uploaded photo, the customer's own hair can be painted over, so only the wig shows.
- **Free to run, and private.** It uses free, open-source tools (Google's MediaPipe face tracking and hair detection) that run **in the customer's browser**. A photo or camera picture never leaves their device, so nothing is uploaded or stored. This also keeps the feature simple under privacy rules such as POPIA.
- **An honest preview.** It is an approximation, not a promise: it is a flat overlay without depth, movement or matched lighting, and the page says so.

**Adding wigs to the try-on.** A small built-in tool turns a photo of a wig on a plain background into a try-on picture: it removes the background, trims the picture, lets the owner mark the face opening, and gives the picture and the line to add to the catalogue. It also runs in the browser.

**How it works, briefly.** Face tracking finds a few points on the face (forehead, chin and both temples). The wig picture is scaled, tilted and placed using those points, so it follows the head.

**Status.** Planned. It will be built with the product catalogue, using real wig pictures on the product pages.

## 🔎 Product Discovery

Customers will be able to discover products through:

- Category navigation.
- Product search.
- Filtering by length, texture, colour and price.
- Product detail pages.
- Product availability and price information.

The store will work on desktop, tablet and mobile.

## 🛒 Shopping Cart

The shopping cart will support:

- Adding products with their chosen variants.
- Updating quantities.
- Removing items.
- Showing item prices and line totals.
- Calculating the cart subtotal.
- Checking stock availability before checkout.

Prices, discounts where supported, and order totals are validated on the server and not trusted from the browser.

## 📦 Order Management

Each order contains the products bought, their variants, quantities, purchase prices, delivery details, payment status and order status.

Order-item snapshots keep the purchase details, so later changes to a product's price or description don't alter past orders. Order status and payment status are managed separately.

Example order statuses:

| Status | Meaning |
|---|---|
| Pending | Order created and awaiting payment or processing |
| Confirmed | Order accepted for processing |
| Processing | Order is being prepared |
| Shipped | Order has been dispatched |
| Delivered | Order has been delivered |
| Cancelled | Order has been cancelled |

The final status transitions and cancellation rules will be defined in the specification.

## 💳 Payments and EFT Verification

The first version is planned to support manual Electronic Funds Transfer (EFT) and an online payment gateway.

### Manual EFT

1. The customer places an order and chooses EFT.
2. The system records the order with a pending payment status.
3. The customer receives the payment instructions.
4. The administrator checks that the funds have reached the business bank account.
5. The administrator records the payment as verified.
6. The order continues through the configured workflow.

A proof of payment, if supported, does not by itself confirm that the money has been received.

### Online Payment

The online payment will use a supported payment provider. The server will validate payment notifications, check the order reference and amount, and handle repeated notifications safely. The provider and integration details will be confirmed before implementation.

## 🧾 Product and Inventory Management

The administration portal will support:

- Product creation and updates.
- Category management.
- Product image management.
- Variant and SKU management.
- Stock quantity tracking.
- Stock adjustments and movement history.
- Product activation and deactivation.
- Stock checks during checkout.

Stock is tracked at variant level: a 22-inch wavy wig in black and a 22-inch straight wig in black have separate quantities. Past orders stay available even if a product is later deactivated.

---

# 🛠️ Technology Stack

The following technologies are planned.

## Backend

- C#
- ASP.NET Core MVC
- .NET 10 LTS
- ASP.NET Core Identity
- Entity Framework Core

## Frontend

- Razor Views
- HTML5
- CSS3
- Bootstrap
- JavaScript

## Database

- Microsoft SQL Server
- Relational database design
- Entity Framework Core migrations

## Libraries and Integrations

- xUnit for automated testing.
- MediaPipe (Google, free and open source) for the try-on: face tracking and hair detection, running in the browser.
- Online payment gateway (provider to be confirmed).
- Email notification service (provider to be selected).

## Development and Deployment

- Git and GitHub.
- GitHub Issues and Projects.
- GitHub Actions.
- Visual Studio.
- Automated testing and continuous integration.

Technology choices and versions will be confirmed as the project is set up.

---

# 🏗️ Architecture

The application will start as a modular ASP.NET Core MVC application. This keeps it simple to build while separating the main business responsibilities.

```text
Customer / Administrator
          |
          v
   ASP.NET Core MVC
          |
    +-----+----------+
    |                |
Controllers       Razor Views
    |
    v
Application Services
    |
    +-- Catalogue
    +-- Cart & Checkout
    +-- Orders
    +-- Payments
    +-- Inventory
    +-- Administration
    |
    v
Entity Framework Core
    |
    v
Microsoft SQL Server

External Integrations
    +-- Payment Gateway
    +-- Email Provider
```

Controllers handle web requests, services hold the business rules, and Entity Framework Core manages database access. Payment and notification integrations are kept apart from the core business logic so providers can be changed without rewriting the application.

---

# 🧩 Core Modules

| Module | Responsibility |
|---|---|
| Identity | Customer sign-in, registration and role-based authorisation |
| Catalogue | Categories, products, images and product details |
| Variants | Length, texture, colour and other options, SKUs, prices and variant stock |
| Search | Product search, category filtering and discovery |
| Try-On | Browser-based virtual try-on: face tracking, wig placement, hair hiding and the wig-picture tool |
| Cart | Cart items, quantities and cart calculations |
| Checkout | Delivery details, stock checks and order creation |
| Orders | Order history, status changes and fulfilment |
| Payments | EFT tracking, payment gateway integration and verification |
| Inventory | Stock quantities, adjustments and stock movement history |
| Administration | Product, inventory, order and payment management |
| Notifications | Customer order and payment emails |

---

# 🔄 Shopping and Checkout Flow

```text
Customer
   |
   v
Browse Catalogue
   |
   v
Select Product and Variant
   |
   v
Add to Cart
   |
   v
Review Cart
   |
   v
Enter Delivery Details
   |
   v
Server-Side Validation
   |
   +-- Validation fails
   |       |
   |       v
   |   Return Errors
   |
   +-- Validation succeeds
           |
           v
      Create Order
           |
           v
    Select Payment Method
           |
      +----+-----------+
      |                |
      v                v
Online Payment      Manual EFT
      |                |
      v                v
Verify Gateway     Await Bank
Notification       Verification
      |                |
      +-------+--------+
              |
              v
      Update Payment Status
              |
              v
        Process Order
```

The server rechecks prices, quantities and stock at checkout. Payment confirmation is handled separately from the customer returning to the website.

---

# 🗄️ Database

The planned database will include these core entities:

- Users
- Addresses
- Categories
- Products
- ProductVariants
- ProductImages
- Orders
- OrderItems
- Payments
- StockMovements
- OrderStatusHistory

The final model may include more entities as the requirements are refined.

Important design considerations:

- Variants must support attributes that differ by category (for example length and texture for wigs, size for accessories).
- SKU values must be unique.
- Money values must use appropriate decimal precision.
- Order items must keep the purchase price and relevant product details.
- Orders must keep the delivery address used at checkout.
- Stock updates must prevent overselling when two customers check out at the same time.
- Historical order and payment records must stay consistent.
- Relationships, constraints and indexes must protect data integrity.

The detailed design will be written up in `docs/database-design.md`.

---

# 🔐 Security

Security is considered throughout the design and implementation.

Planned measures:

- ASP.NET Core Identity for authentication.
- Role-based authorisation for administrator features.
- Server-side input validation.
- Anti-forgery protection for form submissions.
- Server-side validation of prices, quantities, stock and order totals.
- Secure handling and verification of payment gateway notifications.
- Protection against processing the same payment twice.
- Restricted and validated image uploads.
- Secure configuration of connection strings and API credentials.
- Appropriate logging and error handling.
- Protection of customer information in line with privacy requirements, including POPIA.
- Database backup and recovery planning.
- Try-on pictures stay in the customer's browser: they are not uploaded, sent to a service, or stored.

Payment card details are not stored by the application. Secrets and production credentials are never committed to source control.

---

# 🧪 Testing Strategy

Automated tests will cover the critical business rules.

## Unit Tests

```text
✓ Product and variant validation
✓ Price and order-total calculations
✓ Cart quantity validation
✓ Stock availability rules
✓ Order status transitions
✓ Payment status transitions
✓ EFT verification rules
✓ Duplicate payment notification handling
```

## Integration Tests

```text
✓ Database operations
✓ Product and variant retrieval
✓ Cart and checkout operations
✓ Order creation
✓ Inventory updates
✓ Customer authentication and authorisation
✓ Payment notification processing
✓ Administrator access restrictions
```

## Security and Validation Tests

- Invalid or tampered order totals.
- Insufficient stock.
- Invalid product variants.
- Unauthorised administrator requests.
- Invalid payment notifications.
- Duplicate payment notifications.
- Invalid or oversized image uploads.
- Try-on: no face found, several faces, tilted or turned heads, unusual lighting, and wig pictures with a wrong opening.

The tests will grow alongside the implementation, with particular attention to checkout, stock consistency and payment processing.

---

# 📚 Documentation

Project documentation will be kept in a `docs/` folder as the project develops. Planned documents:

| Document | Purpose |
|---|---|
| `specification.md` | Product vision, scope, features and acceptance criteria |
| `requirements.md` | Functional and non-functional requirements |
| `architecture.md` | Application architecture and design decisions |
| `database-design.md` | Database entities, relationships and constraints |
| `testing-strategy.md` | Test coverage and approach |
| `deployment-guide.md` | Environment configuration and deployment |
| `style-guide.md` | Coding conventions and design guidelines |
| `decisions.md` | Important technical decisions and why they were made |

---

# 🗺️ Development Roadmap

Work will be tracked through GitHub milestones, issues and a project board.

| Milestone | Expected Outcome |
|---|---|
| M1 · Foundation & Database | Requirements, architecture, style guide, solution setup, database configuration and first migrations |
| M2 · Product Catalogue | Categories, products, variants, images, search and filtering |
| M2b · Virtual Try-On | The try-on on product pages with real wig pictures, hair hiding, the wig-picture tool and accessibility checks |
| M3 · Cart & Checkout | Customer accounts, shopping cart, delivery details and order creation |
| M4 · Payments | Manual EFT workflow, payment verification and online gateway integration |
| M5 · Admin & Order Management | Product administration, inventory, order management and fulfilment |
| M6 · Testing & Deployment | Automated tests, CI, security checks and deployment documentation |

The roadmap is the planned order of work. Milestones are completed as their acceptance criteria are met.

---

# 🚀 Future Enhancements

Possible later additions:

- ⭐ Product reviews and ratings.
- 🎁 Promotions and discount codes.
- 💝 Customer loyalty programmes.
- 🚚 Courier and delivery integrations.
- 📧 Order and delivery notifications.
- 📊 Sales and inventory reporting.
- ❤️ Wishlists.
- 🤖 Personalised product recommendations.
- 📱 Progressive Web App features.
- 📏 A length guide that shows how each length falls on the body.
- 🪞 Hair hiding on the live camera, and a more realistic try-on with depth and matched lighting.

These will be prioritised after the core shopping flow is working.

---

# 🎯 Project Goals

This project demonstrates practical full-stack development through a realistic online retail scenario.

It shows experience with:

- C# and ASP.NET Core MVC.
- Object-oriented application design.
- Relational database modelling.
- Entity Framework Core and migrations.
- Authentication and role-based authorisation.
- Product catalogue and variant modelling.
- Shopping cart and checkout workflows.
- Inventory management and data consistency.
- Payment integration and verification.
- Secure handling of customer data.
- Computer vision in the browser (face tracking, hair segmentation) with free tools, and privacy by design.
- Automated testing.
- Git-based version control and CI.
- Technical documentation and project planning.

---

# 📁 Repository Structure

The planned structure is:

```text
InchesWigStore/
│
├── src/
│   └── InchesWigStore.Web/
│       ├── Controllers/
│       ├── Data/
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Models/
│       ├── ViewModels/
│       ├── Services/
│       ├── Views/
│       │   ├── Home/
│       │   ├── Shop/
│       │   ├── Cart/
│       │   ├── Checkout/
│       │   ├── Orders/
│       │   └── Admin/
│       ├── wwwroot/
│       │   ├── css/
│       │   ├── js/
│       │   └── images/
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   └── InchesWigStore.Tests/
│
├── docs/
│
├── .github/
│   └── workflows/
│
├── .gitignore
├── InchesWigStore.sln
└── README.md
```

This is the planned structure and may change as the project is built. The `tests/` folder will hold the automated tests, and the application code stays under `src/`.

---

# 🚧 Project Status

**Status: Planning**

The repository has been created and the project is in the initial planning and design phase. The specification, requirements, architecture, style guide, database design, milestones and tasks will be written before the main features are built.

The features described here are the intended scope and are not implemented yet.

---

## 💇🏽‍♀️ Inches Wig Store

**Wigs in every length.**
