# Inches Wig Store — Specification

*Version 0.1 · Draft for review · October 2026*

> This is the source of truth for what Inches Wig Store must do. Where the README and this document differ, this document wins. Decisions that still need an answer are listed in [section 13](#13-open-decisions) and marked **(D-n)** where they matter. Requirements have identifiers (`C-1`, `A-1`, `BR-1`, `TR-1`) so issues, tests and pull requests can point to them.

## 1. Purpose

Inches Wig Store is an online shop for wigs, hair bundles and extensions, closures and frontals, and wig-care products. Customers browse the catalogue, choose the length, texture and colour they want, see how a wig looks on them, and buy it. The owner uses a secure portal to manage products, stock, orders and payments.

**Portfolio goals.** Show real-world full-stack delivery on the Microsoft stack: relational design with variant-level stock, a safe checkout, payment handling that cannot pay an order twice, authentication and authorisation, secure file upload, automated tests, CI, and a browser-based virtual try-on built with free tools.

**Market.** South Africa. Prices are in South African rand (ZAR).

## 2. Scope

### In scope for version 1

- A public storefront: catalogue, search and filters, product pages, the virtual try-on, a cart and checkout.
- Customer accounts, saved delivery addresses and order history.
- Two ways to pay: manual EFT verified by an administrator, and one online payment gateway.
- An administration portal for the catalogue, stock, orders and payments.
- Emails for orders and payments.
- Automated tests, CI and a documented deployment.

### Out of scope for version 1

- Courier integrations and live delivery quotes (a flat or configured delivery fee is used; see **D-3**).
- Product reviews, wishlists, discount codes, loyalty programmes and recommendations (see the README's future enhancements).
- Multiple vendors or a marketplace: there is one business.
- Multiple currencies or languages.
- Mobile apps. The site works on phones in the browser.
- Storing card details. Cards are handled only by the payment gateway.

## 3. Users and roles

| Role | Who | Can do |
| --- | --- | --- |
| **Visitor** | Anyone, signed out | Browse, search, view products, use the try-on, fill a cart |
| **Customer** | A signed-in visitor | Everything a visitor can, plus check out, manage addresses and view their own orders |
| **Administrator** | The owner or staff | Use the administration portal. Cannot be created through public sign-up |

There is one role check per request. A customer can never see or change another customer's data, and a visitor or customer can never reach an administrator page.

## 4. Customer requirements

| ID | Requirement |
| --- | --- |
| C-1 | **Browse by category.** The home page and a category menu lead to lists of products in Wigs, Bundles & Extensions, Closures & Frontals, and Wig Care & Accessories. Only active products are shown. |
| C-2 | **Search and filter.** A customer can search by name and description, and filter by category, length, texture, colour and price. Results can be sorted by price and by newest. Filters work on a phone. |
| C-3 | **Product page.** Shows the name, description, images, price, availability and the choices that apply to the product (length in inches, texture, colour, density, cap size, lace type, and so on). The price and availability follow the chosen variant. |
| C-4 | **Virtual try-on.** A wig page can open a try-on. See [section 8](#8-virtual-try-on). |
| C-5 | **Cart.** A customer can add a chosen variant, change quantities and remove items. The cart shows line totals and a subtotal, and keeps its items between visits. A visitor's cart moves to their account when they sign in. |
| C-6 | **Account.** A customer can register, sign in, sign out and reset their password by email. |
| C-7 | **Delivery addresses.** A signed-in customer can add, edit and delete addresses, and choose one at checkout. |
| C-8 | **Checkout.** The customer reviews the cart and the delivery details, chooses a payment method and places the order. The server re-checks prices, stock and totals first (**BR-3**). |
| C-9 | **Pay by EFT.** The customer gets the banking details and a payment reference for the order, on the screen and by email. The order waits for the administrator to verify the payment (**BR-9**). |
| C-10 | **Pay online.** The customer is sent to the payment gateway, and returns to a page that shows the order's real status (**BR-10**). |
| C-11 | **Orders.** A customer can see their orders with their items, prices, delivery address, payment status and order status. |
| C-12 | **Cancel an order.** A customer can cancel an order that has not been shipped (**BR-12**). |
| C-13 | **Emails.** The customer is emailed when an order is placed, when payment is verified, when the order ships, and when it is cancelled. |
| C-14 | **Contact and policies.** The site shows the business's contact details, delivery information, and returns and privacy policies. |

## 5. Administrator requirements

| ID | Requirement |
| --- | --- |
| A-1 | **Sign in.** Administrators sign in with an email and password. There is no public sign-up. The first administrator is created by a seed script from environment variables. |
| A-2 | **Dashboard.** Shows orders awaiting payment verification, orders to fulfil, recent sales, and variants with low stock. |
| A-3 | **Categories.** Create, edit, order and deactivate categories. |
| A-4 | **Products.** Create, edit and deactivate products, with a name, description, images and a category. A deactivated product disappears from the shop but stays in past orders. |
| A-5 | **Variants.** For each product, manage its variants: the options that apply, a unique SKU, a price and a stock quantity. |
| A-6 | **Images.** Upload product images, choose the main image and order them. Uploads follow **BR-14**. |
| A-7 | **Stock.** Adjust a variant's stock with a reason. Every change is kept in a movement history (**BR-6**). |
| A-8 | **Orders.** Search and filter orders. Open an order to see its items, customer, address, payments and status history. |
| A-9 | **Fulfilment.** Move an order through its statuses within the allowed transitions (**BR-11**). The customer is emailed. |
| A-10 | **EFT verification.** Record that an EFT payment has reached the bank account, with the amount received (**BR-9**). |
| A-11 | **Online payments.** See the status and history of online payments for an order. |
| A-12 | **Try-on pictures.** Upload a try-on picture and set the face opening for each colour of a wig, or use the built-in tool to make one from a wig photo (see [section 8](#8-virtual-try-on)). |
| A-13 | **Settings.** Edit the business name, contact details, banking details, delivery fee, and the text of the policies. |

## 6. Business rules

**BR-1 Money.** All amounts are in ZAR and stored in whole cents, never as floating-point numbers. Whether prices include VAT is a business decision (**D-2**).

**BR-2 Product, variant and SKU.** A product has one or more variants. Stock, price and SKU belong to the variant. Every SKU is unique. The options a variant has depend on its category (for a wig: length, texture, colour, density, cap size, lace type). A product only has the options that apply to it.

**BR-3 The server decides.** The browser's prices, quantities and totals are never trusted. At checkout the server reads the current price and stock for every item, recalculates the totals, and rejects the order, with a clear message, if an item is inactive, unavailable or out of stock, or if the totals sent do not match.

**BR-4 An order keeps its own copy.** When an order is placed, it stores what was bought: the product name, the chosen options, the SKU, the quantity and the price paid, and the delivery address. Changing a product or an address later does not change orders already placed.

**BR-5 The last item goes to one buyer.** Two customers cannot both buy the last item. Checking the stock and reducing it happen together in one database transaction, and the database refuses a stock quantity below zero.

**BR-6 Stock changes are recorded.** Each variant has a stock quantity. Whenever it changes (a sale, a cancellation or an administrator's adjustment), a record is kept with the old and new quantity, a reason, and who or what made the change.

**BR-7 Stock is reserved when an order is placed.** The stock for an order is set aside when the order is placed. If the order is cancelled, or an EFT order is not paid in time, the stock is released. How long an unpaid EFT order is kept is a setting (**D-4**).

**BR-8 Two statuses.** An order has an **order status** and a **payment status**, and they change independently. Each change is saved in a history with the time and who or what made it.

**BR-9 EFT verification.** An EFT order starts with the payment status *Pending*. Only an administrator can mark it *Paid*, and only after the money has reached the business bank account, with the amount received. A proof of payment, if the customer uploads one, helps the administrator but never marks an order as paid. If the amount received differs from the order total, the administrator decides what to do and the difference is recorded.

**BR-10 Online payments.** The customer returning from the gateway does not mark an order as paid. Only a notification from the gateway that the server has authenticated, and whose order reference and amount match the order, does. A repeated or out-of-order notification must not change the order twice (**BR-13**). Card details never reach the server.

**BR-11 Order status transitions.**

| From | Allowed next |
| --- | --- |
| Pending | Confirmed, Cancelled |
| Confirmed | Processing, Cancelled |
| Processing | Shipped, Cancelled |
| Shipped | Delivered |
| Delivered | (end) |
| Cancelled | (end) |

An order becomes *Confirmed* automatically when its payment is *Paid*. Any other move is made by an administrator. Moves not in the table are refused.

**BR-12 Cancellation and returns.** A customer can cancel an order until it is *Shipped*. A cancelled order returns its stock, and a payment already made is refunded manually by the business, recorded on the order. Returns after delivery follow the business's returns policy, which must respect South African consumer law (**D-5**).

**BR-13 Idempotent payments.** Each payment notification has an identifier. The server stores the identifiers it has processed and ignores a repeat. Notifications for the same order are processed one at a time.

**BR-14 Uploads.** Images are checked from their content, not their file name. Accepted types are JPEG, PNG and WebP, up to a size limit, and every image has its metadata removed before it is stored. Files are stored outside the web root and served through the application or a safe path.

**BR-15 Privacy of the try-on.** A customer's camera picture or photo used for the try-on is processed only in their browser. It is never uploaded, sent to another service or stored (**TR-1**).

**BR-16 Delivery.** Delivery is charged as a flat fee, or free over a set amount (**D-3**). The fee is part of the order total and is kept on the order.

## 7. Data model (overview)

The full design, with relationships and constraints, will be written in `docs/database-design.md` (issue #4). The main entities:

| Entity | Purpose |
| --- | --- |
| User | A customer or an administrator (ASP.NET Core Identity) |
| Address | A delivery address that belongs to a customer |
| Category | A product group |
| Product | What is sold: name, description, category, active flag |
| ProductVariant | A sellable combination of options: SKU, price, stock, options |
| ProductImage | A picture of a product, with an order and a main flag |
| TryOnAsset | A try-on picture and face opening for a wig colour |
| Cart, CartItem | A customer's or visitor's cart |
| Order | A placed order: totals, delivery address snapshot, order status |
| OrderItem | One line of an order: the snapshot of what was bought |
| Payment | One payment attempt for an order: method, amount, payment status, reference |
| PaymentNotification | A processed gateway notification, for idempotency |
| StockMovement | A record of one change to a variant's stock: old and new quantity, reason, and who or what made it |
| OrderStatusHistory | Every change of an order's or payment's status |

Design rules that follow from the business rules: money is `decimal(18,2)` or whole cents; SKUs are unique; stock cannot be negative (a check constraint); order items store their own copies of names and prices; indexes support search and the common admin queries.

## 8. Virtual try-on

The "See how it looks on you" feature lets a customer see a chosen wig on their own face before buying.

### Behaviour

| ID | Requirement |
| --- | --- |
| TR-1 | **Runs in the browser.** Face tracking runs on the customer's device with a free, open-source library (MediaPipe). No picture is uploaded, sent to a third party or stored (**BR-15**). The page says so. |
| TR-2 | **Camera or photo.** The customer can use their camera, with a choice of camera when there is more than one, or upload a photo. A customer who refuses or lacks a camera can always upload a photo. |
| TR-3 | **The real wig.** The picture is the product's try-on picture in the chosen colour: a photo of the wig on a transparent background with an opening for the face. |
| TR-4 | **Placement.** The wig is scaled, tilted and positioned from points on the face (forehead, chin, temples) and follows the head live. |
| TR-5 | **Fine-tuning and saving.** Controls adjust size, position and tilt, with a reset. The result can be saved as a picture. |
| TR-6 | **Hide my hair.** On an uploaded photo the customer's own hair can be hidden by painting it over with colours from around it, so only the wig shows. It is optional and is not offered on the live camera. |
| TR-7 | **Clear failures.** If no face is found, there are several faces or the camera cannot open, a plain message explains what to do. |
| TR-8 | **Honest preview.** The page says the result is an approximate preview: a flat overlay without depth, movement or matched lighting. |
| TR-9 | **Accessible.** The controls work with a keyboard and a screen reader, and the page does not depend on colour alone. |
| TR-10 | **Free to run.** It uses no paid service or per-use fee. |

### Wig pictures

For the try-on to show the real wig, each colour needs a try-on picture. A small built-in tool (A-12) turns a photo of a wig on a plain background into one: it removes the background, trims the picture, lets the administrator mark the face opening and saves the PNG. It works best with the wig on a mannequin head, facing the camera, on a plain light background. It runs in the browser.

### Known limits

The overlay is flat, so hair does not fall behind the shoulders or move, the lighting may not match, and a head turned far to the side looks wrong. Hiding the customer's own hair is approximate. These limits are stated to the customer (**TR-8**).

## 9. Payments

| Method | How it works | Who confirms |
| --- | --- | --- |
| **Manual EFT** | The customer pays into the business bank account using the order reference. | An administrator, after seeing the money in the bank (**BR-9**) |
| **Online** | The customer pays at the payment gateway. | The gateway's authenticated notification (**BR-10**) |

The payment gateway is a decision (**D-1**). Whichever it is, it is used through a payment-provider interface so the business rules do not depend on it, and so a second provider can be added later. The application never stores card numbers, and secrets such as API keys stay out of Git.

## 10. Non-functional requirements

### Security

- ASP.NET Core Identity for sign-in, with hashed passwords, a password policy and lock-out after repeated failures.
- Role-based authorisation on every administrator route, tested for refusal.
- Anti-forgery tokens on every form that changes data.
- Server-side validation of every input, and encoded output.
- Rate limits on sign-in, registration, password reset and checkout.
- Security headers, including a content security policy that allows only the sources the site needs (including the try-on's scripts and models).
- Secrets only in configuration outside Git, with a repository secret scan in CI.
- Logs that never hold passwords, tokens or card data.

### Privacy (POPIA)

- Collect only what an order needs. State what is collected, why, and for how long.
- Customer data is protected, and a customer can ask for their data to be corrected or deleted, subject to records that must be kept (such as orders for tax purposes).
- The try-on keeps pictures on the device (**BR-15**).
- Emails about orders are service messages. Marketing email needs separate consent.

### Accessibility

WCAG 2.2 level AA for the storefront and the administration portal: keyboard use, visible focus, labels, contrast, text alternatives for product images, and clear errors.

### Performance and compatibility

- Pages are fast on a mid-range phone on a mobile connection: images are sized and lazy-loaded.
- Current versions of Chrome, Edge, Firefox and Safari on desktop, and Chrome and Safari on phones. The try-on needs a modern browser with WebAssembly, and says so when it cannot run.

### Reliability and operations

- Database migrations run before a new version starts, and a failed migration stops the deploy.
- A health check, logs that show errors, and nightly database backups with a documented restore.
- Orders, payments and stock stay consistent if a request fails halfway (transactions).

### Quality

- Unit tests for the business rules (totals, cart, stock, statuses, payments).
- Integration tests on a real SQL Server database for checkout, stock under concurrency, payments and authorisation.
- GitHub Actions runs the build and tests on every pull request, and the main branch needs it to pass.

## 11. Integrations

| Integration | Status |
| --- | --- |
| Payment gateway | To be chosen (**D-1**) |
| Email provider | To be chosen (**D-6**). Used for order, payment and password emails |
| Face tracking and hair detection | MediaPipe, in the browser, free |
| Courier and delivery | Out of version 1 |

## 12. Acceptance criteria for version 1

1. A visitor can browse, search, filter and open a wig page, and see its price and stock for the chosen length, texture and colour.
2. A customer can try a wig on with their camera or a photo, and save the picture, with nothing uploaded.
3. A customer can register, add an address, fill a cart and place an order paid by EFT. An administrator can verify the payment, and the order moves to *Confirmed*.
4. A customer can place an order paid online, and the order becomes *Paid* only through the gateway's authenticated notification. Repeating the notification changes nothing.
5. Two customers cannot buy the last item at the same time, and stock never goes below zero.
6. An administrator can manage categories, products, variants, images and stock, and every stock change is in the movement history.
7. A customer cannot see another customer's orders, and no one but an administrator can open an administrator page.
8. A past order does not change when its product or price is changed later.
9. The automated tests pass in CI, and the application is deployed and the main flows are checked on the live site.

### Traceability to the milestones

| Milestone | Main requirements |
| --- | --- |
| M1 · Foundation & Database | This document, architecture, style guide, solution, data model, migrations |
| M2 · Product Catalogue | C-1 to C-3, A-3 to A-6, BR-2, BR-14 |
| M2b · Virtual Try-On | C-4, A-12, TR-1 to TR-10, BR-15 |
| M3 · Cart & Checkout | C-5 to C-8, C-11, BR-3 to BR-5, BR-16 |
| M4 · Payments | C-9, C-10, A-10, A-11, BR-8 to BR-10, BR-13 |
| M5 · Admin & Order Management | A-1, A-2, A-7 to A-9, A-13, BR-6, BR-7, BR-11, BR-12 |
| M6 · Testing & Deployment | Section 10 and the acceptance criteria above |

## 13. Open decisions

These need an answer from the owner before the related work starts. Each has a suggestion.

| ID | Decision | Suggestion |
| --- | --- | --- |
| D-1 | Which payment gateway? | Choose a South African gateway with a free test mode and card, instant EFT and webhooks (for example PayFast, Paystack or Yoco). Decide before M4. |
| D-2 | Do displayed prices include VAT? | If the business is VAT-registered, show VAT-inclusive prices and itemise VAT on the order. Otherwise no VAT. |
| D-3 | How is delivery priced? | A flat fee, free above an amount, configurable in the portal. Live courier quotes later. |
| D-4 | How long is stock held for an unpaid EFT order? | 48 hours, then the order is cancelled and the stock returned. |
| D-5 | What is the returns policy? | Write it with South African consumer law in mind (for example the Electronic Communications and Transactions Act and the Consumer Protection Act) and have it checked, because the rules depend on the product and the sale. |
| D-6 | Which email provider? | A service with a free tier that sends over HTTPS, using the business's own domain for the sender. |
| D-7 | Where is it hosted? | A free host for the portfolio demo with made-up data. A real shop needs a server or a managed host, a domain and backups. |
| D-8 | Should guests be able to check out without an account? | No for version 1: an account makes order history and support simpler. |

## 14. Sources and independence

The business rules follow common, publicly documented e-commerce practice. They were written for this project from its README and from the public sources below, independently of any employer's systems, documents or code. The names used (Order, OrderItem, Payment, StockMovement and so on) are ordinary terms.

| Rule | Common practice, and where it is documented publicly |
| --- | --- |
| BR-1 Money | Storing money as whole units of the smallest currency, not floating point: the *Money* pattern in Martin Fowler's *Patterns of Enterprise Application Architecture* |
| BR-3 The server decides | Never trusting prices or totals sent by a browser: OWASP guidance on business-logic and parameter tampering |
| BR-4 An order keeps its own copy | Order lines that store the price paid at the time of purchase: how public shop platforms such as Shopify and WooCommerce record order items |
| BR-5 The last item goes to one buyer | Protecting a shared quantity with a transaction and a constraint: Microsoft Learn, *Handling Concurrency Conflicts* (Entity Framework Core) |
| BR-6 Stock changes are recorded | An audit record of changes: general audit-trail practice, and the stock-change notes shown by public shop platforms |
| BR-7 Stock is reserved when an order is placed | Releasing unpaid orders after a time: WooCommerce's "Hold stock (minutes)" inventory setting |
| BR-8 Two statuses | Keeping payment and fulfilment apart: Shopify's separate payment status and fulfilment status in its public help |
| BR-13 Idempotent payments | Ignoring a repeated request safely: Stripe's public documentation on idempotent requests |

The sources are named so they can be checked. Check each one before relying on it.

## 15. Glossary

| Term | Meaning |
| --- | --- |
| **Variant** | One sellable combination of a product's options, such as a 22-inch wavy wig in black, with its own SKU, price and stock |
| **SKU** | Stock-keeping unit: the unique code of a variant |
| **EFT** | Electronic funds transfer: paying by bank transfer |
| **Gateway** | The service that takes an online payment |
| **Snapshot** | A copy of a product's details kept on an order, so the order never changes |
| **Idempotent** | Doing it twice has the same effect as doing it once |
| **Try-on picture** | A photo of a wig on a transparent background, with an opening for the face |
| **POPIA** | The Protection of Personal Information Act, South Africa's privacy law |
