# Inches Wig Store — Requirements

*Version 0.1 · Draft for review · October 2026*

> The [specification](specification.md) says **what the store is and why**. This document turns it into **testable requirements**: statements that are either true or false of the finished system. Each requirement points back to the specification (the **Spec** column) and says how it will be checked (the **Check** column). Where this document and the specification differ, the specification wins. Numbers marked *(initial)* are starting values, kept in configuration so they can be tuned without changing code.

## 1. How to read this document

**Identifiers.** `FR-` is a functional requirement, `NFR-` a non-functional one. The letters after it name the area (for example `FR-CAT-2` is the second requirement of the catalogue). Issues, tests and pull requests refer to these identifiers.

**Wording.** *Shall* means a requirement that must be met for version 1. *Should* means desirable, and may be left out if time is short.

**Check** says how a requirement is verified:

| Code | Meaning |
| --- | --- |
| **U** | Automated unit test |
| **I** | Automated integration test against a real database |
| **E** | Automated end-to-end or browser test |
| **M** | Manual check, written as a checklist (for example a device check) |
| **R** | Review of a document, configuration or the code |

## 2. Functional requirements

### 2.1 Catalogue (FR-CAT)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-CAT-1 | The system shall list products by category, and shall show only active products to customers and visitors. | C-1, A-4 | I, E |
| FR-CAT-2 | The system shall support the categories Wigs, Bundles & Extensions, Closures & Frontals, and Wig Care & Accessories, and shall let an administrator add, rename, reorder and deactivate categories. | C-1, A-3 | I |
| FR-CAT-3 | A product page shall show the name, description, images, price and availability, and the choices that apply to that product. | C-3 | E |
| FR-CAT-4 | The price and the availability shown shall be those of the chosen variant, and shall update when the choice changes. | C-3 | E |
| FR-CAT-5 | A deactivated product shall not be offered for sale, shall not appear in search or listings, and shall still appear, unchanged, in past orders. | A-4, BR-4 | I |
| FR-CAT-6 | A variant that is out of stock shall be shown as unavailable and shall not be addable to a cart. | C-3, BR-5 | I, E |

### 2.2 Variants and stock (FR-VAR)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-VAR-1 | A product shall have one or more variants, and a variant shall have its own SKU, price and stock quantity. | BR-2 | I |
| FR-VAR-2 | Every SKU shall be unique. Saving a duplicate SKU shall be refused with a clear message. | BR-2 | I |
| FR-VAR-3 | A wig variant shall support these options: length (in inches), texture, colour, density, cap size and lace type. Other categories shall support the options that suit them, such as size or style. | BR-2, C-3 | U, I |
| FR-VAR-4 | A product shall not be required to have options that do not apply to it. | BR-2 | U |
| FR-VAR-5 | Stock quantity shall never be negative. The database shall refuse a change that would make it negative. | BR-5 | I |
| FR-VAR-6 | Whenever a variant's stock changes, a record shall be saved with the old and new quantity, a reason, the time, and who or what made the change. | BR-6, A-7 | I |

### 2.3 Search and filtering (FR-SRC)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-SRC-1 | A customer shall be able to search products by name and description. | C-2 | I, E |
| FR-SRC-2 | A customer shall be able to filter by category, length, texture, colour and price range, and to combine filters. | C-2 | I, E |
| FR-SRC-3 | Results shall be sortable by price (low to high and high to low) and by newest. | C-2 | I |
| FR-SRC-4 | A search with no matches shall say so and suggest clearing filters. | C-2 | E |
| FR-SRC-5 | Search and filters shall be usable on a phone-sized screen. | C-2 | M |

### 2.4 Cart (FR-CRT)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-CRT-1 | A visitor or customer shall be able to add a chosen variant to a cart, change its quantity and remove it. | C-5 | I, E |
| FR-CRT-2 | The cart shall show each line total and the subtotal, calculated in whole cents and rounded correctly. | C-5, BR-1 | U |
| FR-CRT-3 | A cart shall keep its items between visits *(initial: 30 days)*. | C-5 | I |
| FR-CRT-4 | When a visitor signs in, the items in their cart shall be added to their account's cart without losing either set. | C-5 | I |
| FR-CRT-5 | A quantity shall not exceed the stock available, and a request that does shall be refused with a clear message. | BR-3, BR-5 | U, I |

### 2.5 Accounts and addresses (FR-ACC)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-ACC-1 | A visitor shall be able to register with an email address and a password, and to sign in and out. | C-6 | I, E |
| FR-ACC-2 | Passwords shall be at least 12 characters *(initial)* and shall be stored only as salted hashes. | C-6, NFR-SEC-1 | U, R |
| FR-ACC-3 | An account shall be locked for a short time *(initial: 15 minutes)* after repeated failed sign-ins *(initial: 5)*. | C-6, NFR-SEC-1 | I |
| FR-ACC-4 | A customer shall be able to reset a forgotten password by email, with a link that works once and expires *(initial: 1 hour)*. | C-6 | I |
| FR-ACC-5 | A sign-in or password-reset response shall not reveal whether an email address has an account. | C-6, NFR-SEC-1 | I |
| FR-ACC-6 | A signed-in customer shall be able to add, edit and delete delivery addresses. | C-7 | I, E |
| FR-ACC-7 | A customer shall only ever be able to see and change their own addresses, orders and cart. | C-7, C-11, §3 | I |
| FR-ACC-8 | Administrator accounts shall not be creatable through public registration. The first administrator shall be created by a seed script from configuration. | A-1 | I, R |

### 2.6 Checkout and orders (FR-ORD)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-ORD-1 | At checkout the system shall recalculate every price, line total, delivery fee and the order total on the server from current data, and shall ignore any amounts sent by the browser. | C-8, BR-3 | U, I |
| FR-ORD-2 | The system shall refuse an order, with a message that names the problem, if an item is inactive, unavailable or out of stock, or if the total sent does not match the server's total. | BR-3 | I |
| FR-ORD-3 | Creating an order and reducing the stock of its variants shall happen in one database transaction. If any part fails, nothing shall be saved. | BR-5 | I |
| FR-ORD-4 | Two customers buying the last item at the same time shall not both succeed. Exactly one order shall be created. | BR-5 | I |
| FR-ORD-5 | An order shall store, for each item, the product name, the chosen options, the SKU, the quantity and the price paid, and shall store the delivery address used. | BR-4 | I |
| FR-ORD-6 | Changing a product, a price or an address after an order is placed shall not change that order. | BR-4 | I |
| FR-ORD-7 | An order shall have an order status and a payment status that change independently, and every change shall be saved in a history with the time and who or what made it. | BR-8 | I |
| FR-ORD-8 | The system shall only allow the order status changes in the specification's transition table, and shall refuse any other. | BR-11 | U |
| FR-ORD-9 | A customer shall be able to see their own orders with items, prices, delivery address, payment status and order status. | C-11 | I, E |
| FR-ORD-10 | A customer shall be able to cancel an order that has not been shipped. Cancelling shall release the order's stock. | C-12, BR-12 | I |
| FR-ORD-11 | The delivery fee shall be calculated from settings, shown before the customer pays, and kept on the order. | BR-16 | U |
| FR-ORD-12 | Checkout shall require a signed-in customer. | D-8 | I, E |

### 2.7 Payments (FR-PAY)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-PAY-1 | After a customer chooses EFT, the system shall show, and email, the banking details and a payment reference for the order. The order's payment status shall be *Pending*. | C-9, BR-9 | I |
| FR-PAY-2 | Only an administrator shall be able to mark an EFT payment as *Paid*, and shall record the amount received. The time and the administrator shall be saved. | A-10, BR-9 | I |
| FR-PAY-3 | A proof of payment uploaded by a customer shall never mark a payment as *Paid*. | BR-9 | I |
| FR-PAY-4 | If the amount received differs from the order total, the system shall record the difference and shall not mark the order *Paid* until an administrator decides. | BR-9 | I |
| FR-PAY-5 | An EFT order that is not paid within the hold time shall be cancelled automatically and its stock released. | BR-7, D-4 | I |
| FR-PAY-6 | An order shall become *Confirmed* automatically when its payment is *Paid*. | BR-11 | I |
| FR-PAY-7 | A customer returning from the payment gateway shall see the order's real status, and shall not change it by returning. | C-10, BR-10 | I, E |
| FR-PAY-8 | The system shall accept an online payment notification only if it is authenticated by the gateway's mechanism and its order reference and amount match the order. | BR-10 | I |
| FR-PAY-9 | An invalid notification shall be rejected and logged, without storing card details. | BR-10, NFR-SEC-7 | I |
| FR-PAY-10 | Processing the same payment notification twice shall change the order only once. | BR-13 | U, I |
| FR-PAY-11 | Notifications for the same order that arrive at the same time shall be processed one after the other, and an out-of-order notification shall not undo a later status. | BR-13 | I |
| FR-PAY-12 | The system shall not store card numbers or security codes anywhere. | §9 | R |
| FR-PAY-13 | Payments shall go through a payment-provider interface, so the gateway can be replaced without changing the business rules. | §9 | R |

### 2.8 Administration (FR-ADM)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-ADM-1 | Every administrator page and action shall require the Administrator role. A visitor or customer shall be refused. | A-1, §3 | I |
| FR-ADM-2 | An administrator shall be able to create, edit and deactivate categories, products and variants. | A-3 to A-5 | I, E |
| FR-ADM-3 | An administrator shall be able to upload product images, choose the main image and set their order. | A-6 | I, E |
| FR-ADM-4 | An uploaded image shall be accepted only if its content is JPEG, PNG or WebP and it is within the size limit *(initial: 5 MB)*. Its metadata shall be removed before it is stored. | BR-14 | U, I |
| FR-ADM-5 | An administrator shall be able to adjust a variant's stock with a reason, and the adjustment shall create a stock record. | A-7, BR-6 | I |
| FR-ADM-6 | An administrator shall be able to search and filter orders, open an order, and see its items, customer, address, payments and status history. | A-8 | I, E |
| FR-ADM-7 | An administrator shall be able to move an order through its allowed statuses, and the customer shall be emailed. | A-9, C-13 | I |
| FR-ADM-8 | The dashboard shall show orders awaiting payment verification, orders to fulfil, recent sales and variants with low stock *(initial: 3 or fewer)*. | A-2 | I |
| FR-ADM-9 | An administrator shall be able to edit the business name, contact details, banking details, delivery settings and the text of the policies. | A-13 | I |

### 2.9 Virtual try-on (FR-TRY)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-TRY-1 | The try-on shall run entirely in the customer's browser. No camera picture or photo shall be uploaded, sent to another service or stored. | TR-1, BR-15 | I, R |
| FR-TRY-2 | The try-on shall work from the camera or from an uploaded photo, and shall offer a choice of camera when there is more than one. | TR-2 | M |
| FR-TRY-3 | A customer who refuses or lacks a camera shall be able to upload a photo instead. | TR-2 | E |
| FR-TRY-4 | The try-on shall show the try-on picture of the chosen product colour, with an opening for the face. | TR-3 | E |
| FR-TRY-5 | The wig shall be scaled, tilted and positioned from the face, and shall follow the head live on the camera. | TR-4 | M |
| FR-TRY-6 | The customer shall be able to adjust size, position and tilt, reset them, and save the result as a picture. | TR-5 | E |
| FR-TRY-7 | On an uploaded photo, the customer shall be able to switch hiding their own hair on or off. If hiding fails, the wig shall still be shown. | TR-6 | E, M |
| FR-TRY-8 | If no face is found, if there is more than one face, or if the camera cannot open, the try-on shall give a plain message that says what to do. | TR-7 | E |
| FR-TRY-9 | The try-on shall tell the customer that the result is an approximate preview. | TR-8 | E |
| FR-TRY-10 | The try-on shall work with a keyboard and a screen reader. | TR-9, NFR-ACC-1 | M |
| FR-TRY-11 | An administrator shall be able to make a try-on picture from a wig photo: remove the background, trim it, mark the face opening and save a PNG. | A-12 | M |
| FR-TRY-12 | A try-on picture and its face opening shall be stored per colour of a wig, and the try-on shall show the colour chosen on the product page. | A-12, TR-3 | I, E |
| FR-TRY-13 | The try-on shall not use any paid service or per-use fee. | TR-10 | R |

### 2.10 Emails (FR-EML)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| FR-EML-1 | The system shall email the customer when an order is placed, when its payment is verified, when it ships and when it is cancelled. | C-13 | I |
| FR-EML-2 | The payment-instructions email shall contain the banking details and the payment reference. | C-9 | I |
| FR-EML-3 | An email shall be available in plain text and in HTML, with the same words. | C-13 | U |
| FR-EML-4 | If an email cannot be sent, the order shall still be saved, the failure shall be recorded, and the customer shall be told on screen. | C-13 | I |
| FR-EML-5 | Emails shall not contain a password, a full card number or any secret. | NFR-SEC-7 | U |

## 3. Non-functional requirements

### 3.1 Security (NFR-SEC)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-SEC-1 | Sign-in shall use ASP.NET Core Identity, with hashed passwords, a password policy and lock-out. | §10 | I, R |
| NFR-SEC-2 | Every form or request that changes data shall be protected by an anti-forgery token. | §10 | I |
| NFR-SEC-3 | Every input shall be validated on the server, and every output shall be encoded. | §10 | U, I |
| NFR-SEC-4 | Sign-in, registration, password reset and checkout shall be rate limited *(initial: 10 requests per 15 minutes per address for sign-in; other limits in configuration)*. | §10 | I |
| NFR-SEC-5 | Responses shall carry security headers, including a content security policy that allows only the sources the site needs, among them the try-on's scripts and models. | §10 | I, M |
| NFR-SEC-6 | Secrets (connection strings, gateway keys) shall be kept outside Git, and CI shall fail if a secret is committed. | §10 | R, I |
| NFR-SEC-7 | Logs shall never contain passwords, tokens, card data or payment secrets. | §10 | I |
| NFR-SEC-8 | Pages with personal data (orders, addresses, account) shall not be cached by shared caches. | §10 | I |
| NFR-SEC-9 | Uploaded files shall be stored outside the web root, and never executed. | BR-14 | R |

### 3.2 Privacy (NFR-PRV)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-PRV-1 | The system shall collect only the personal data an order needs, and the privacy policy shall say what is collected, why, and for how long. | §10 | R |
| NFR-PRV-2 | A customer shall be able to ask for their data to be corrected or deleted, and an administrator shall be able to act on it, keeping only what must be kept (such as order records for tax). | §10 | I, R |
| NFR-PRV-3 | Marketing email shall be sent only with separate consent. Order emails are service messages. | §10 | R |
| NFR-PRV-4 | The try-on shall keep pictures on the customer's device (see FR-TRY-1). | BR-15 | I |

### 3.3 Accessibility (NFR-ACC)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-ACC-1 | The storefront and the administration portal shall meet WCAG 2.2 level AA. | §10 | E, M |
| NFR-ACC-2 | Every interactive element shall work with a keyboard and show a visible focus. | §10 | E |
| NFR-ACC-3 | Every product image shall have a text alternative, and every form field a label. | §10 | E |
| NFR-ACC-4 | Text and controls shall meet the minimum contrast, and no information shall depend on colour alone. | §10 | E |
| NFR-ACC-5 | Errors shall say what went wrong and how to fix it, next to the field. | §10 | E |

An automated accessibility check shall run on every main page in CI, and a manual keyboard and screen-reader pass shall be done before release.

### 3.4 Performance and compatibility (NFR-PRF)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-PRF-1 | On a mid-range phone over a typical mobile connection, the home page and a product page shall show their main content within 3 seconds *(initial)*. | §10 | M |
| NFR-PRF-2 | Product images shall be sized for the screen and loaded lazily below the first screen. | §10 | R, M |
| NFR-PRF-3 | A search or filter on a catalogue of 5,000 products *(initial)* shall return within 1 second on the server. | FR-SRC | I |
| NFR-PRF-4 | The site shall work on current versions of Chrome, Edge, Firefox and Safari on desktop, and Chrome and Safari on phones. | §10 | M |
| NFR-PRF-5 | When a browser cannot run the try-on (for example, no WebAssembly), the rest of the product page shall still work and the try-on shall say it is not available. | TR-7 | E, M |

### 3.5 Reliability and operations (NFR-OPS)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-OPS-1 | Database migrations shall run before a new version starts. A failed migration shall stop the deploy and leave the running version untouched. | §10 | R, M |
| NFR-OPS-2 | The application shall expose a health check that reports whether it can reach its database. | §10 | I |
| NFR-OPS-3 | Errors shall be written to the host's logs with enough detail to find the cause, and without personal data. | §10 | R |
| NFR-OPS-4 | The database shall be backed up nightly, and the restore steps shall be documented and tried at least once before release. | §10 | M, R |
| NFR-OPS-5 | If a request fails part-way, orders, payments and stock shall stay consistent (see FR-ORD-3). | §10 | I |

### 3.6 Quality and maintainability (NFR-QUA)

| ID | Requirement | Spec | Check |
| --- | --- | --- | --- |
| NFR-QUA-1 | Unit tests shall cover the business rules: totals and rounding, cart rules, stock rules, status transitions and payment handling. | §10 | U |
| NFR-QUA-2 | Integration tests shall run on a real SQL Server database and cover checkout, stock under concurrency, payments and authorisation. | §10 | I |
| NFR-QUA-3 | GitHub Actions shall build the solution and run all tests on every pull request, and the main branch shall require them to pass. | §10 | R |
| NFR-QUA-4 | The code shall follow the style guide, and formatting and analyser checks shall run in CI. | §10 | R |
| NFR-QUA-5 | Every change shall go through an issue and a pull request. | §10 | R |
| NFR-QUA-6 | The documents in `docs/` shall be kept up to date with the system. | §10 | R |

## 4. Constraints and assumptions

- **Technology:** C#, ASP.NET Core MVC on .NET 10 LTS, Entity Framework Core and SQL Server, as in the README.
- **Cost:** the portfolio version shall use free tools and free tiers where it can, and shall say where it cannot (for example a real payment gateway account).
- **Single business:** there is one shop, one currency (ZAR) and one language (English).
- **Data:** the portfolio demo holds only made-up data.
- **Unanswered decisions** (D-1 to D-8 in the specification) affect FR-PAY-5 (hold time), FR-ORD-11 (delivery pricing), FR-ORD-12 (guest checkout) and FR-PAY-8 (the gateway). Until they are answered, the *(initial)* values and suggestions in the specification apply.

## 5. Traceability to the milestones

| Milestone | Requirements |
| --- | --- |
| M1 · Foundation & Database | FR-VAR-1, FR-VAR-2, FR-VAR-5, NFR-QUA-4 to NFR-QUA-6, and the data model |
| M2 · Product Catalogue | FR-CAT, FR-VAR, FR-SRC, FR-ADM-2 to FR-ADM-4 |
| M2b · Virtual Try-On | FR-TRY, NFR-PRV-4, NFR-PRF-5 |
| M3 · Cart & Checkout | FR-CRT, FR-ACC, FR-ORD |
| M4 · Payments | FR-PAY, FR-EML-1 and FR-EML-2 |
| M5 · Admin & Order Management | FR-ADM, FR-VAR-6, FR-EML-1 |
| M6 · Testing & Deployment | NFR-SEC, NFR-ACC, NFR-PRF, NFR-OPS, NFR-QUA |
