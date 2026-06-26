# Navix Freight Audit API Client

A .NET client library for interacting with the Navix Freight Audit API.  
This library was **generated using Microsoft Kiota** based solely on the public OpenAPI specification provided by Navix.

> ⚠️ **Disclaimer**  
> This client is **not an official Navix product**.  
> It is offered independently by **Taylor White (@taylorchasewhite)** and is not authored, supported, or endorsed by Navix.  
> All generated code is derived exclusively from the publicly available Navix API specification.

---

## 🌐 About the Navix API

Navix provides a hands-free AI freight audit solution that integrates with TMS platforms to automate:

- Document ingestion  
- Invoice auditing  
- Billing & payment support  
- Dispute tracking and resolution  
- Exception workflows  
- External webhook operations  

Their API supports multiple versions (v1, v2, provisional), providing both synchronous and asynchronous endpoints, and supports additive non-breaking changes.

Official documentation: **https://docs.navix.io**

---

## 🛠 Technology & Generation Details

This client was generated using:

- **Microsoft Kiota**  
- Input: Navix OpenAPI spec (`openapi.json`)  
- Output: C# client using `HttpClient`  
- Partial classes enabled  
- Namespace: `Navix.FreightAudit`

If the Navix OpenAPI spec evolves, the client can be regenerated as needed from the same source to stay current with the API.

---

## 📦 Installation (after publishing)

```bash
dotnet add package Navix.FreightAudit.Client

Or use it via a project reference if you're consuming it locally.

## ✅ Example Usage

```C#
using Navix.FreightAudit;
using System.Net.Http;

// Create the HttpClient
var httpClient = new HttpClient();

// Instantiate the client (method/class names depend on generated code)
var client = new ApiClient(httpClient, new Uri("https://api.navix.io"));

// Example: create or submit an order
var newOrder = new OrderRequest { /* set required fields */ };
var response = await client.Orders.CreateAsync(newOrder);

// Example: retrieve audit results
var result = await client.Invoices.GetAuditResultAsync("invoice-uuid-here");

```
Note: Some endpoints may return 202 Accepted and complete asynchronously.

## Required setup for consumers

Call `NavixResponseCasing.Register` once at start-up. It is the only setup a
consumer needs to add, and it fixes the property-name casing mismatch described
under "Response property casing" below.

```csharp
using Navix.FreightAudit;
using Microsoft.Kiota.Abstractions.Serialization;

// Once at start-up, on the registry your adapter uses
// (HttpClientRequestAdapter uses ParseNodeFactoryRegistry.DefaultInstance by default).
NavixResponseCasing.Register(ParseNodeFactoryRegistry.DefaultInstance);
```

## Response property casing

**This is the real runtime issue, confirmed against the live Navix API.** The
OpenAPI schema declares camelCase property names, but the live API is inconsistent:
some endpoints return camelCase (for example `orderExternalId` from
`POST /v2/orders/approved`) while others return **PascalCase** (for example `Uuid`
from `GET /v2/orders/{id}/invoices`, and `Uuid` / `ETag` / `CreatedAt` from the
dispute reads). Kiota binds JSON keys case-sensitively, so PascalCase fields
deserialize to `null` (see
[microsoft/kiota#7060](https://github.com/microsoft/kiota/issues/7060)).

The generated models are left exactly as Kiota emits them. Instead, the
`NavixResponseCasing.Register` call shown above installs a parse-node factory that
lower-cases the first character of every JSON property name before deserializing.
That maps `Uuid` to `uuid` while leaving `orderExternalId` unchanged, so responses
bind regardless of which casing an endpoint uses. Normalization is a no-op on
already-camelCase payloads. **This requires the `Register` call** — without it,
PascalCase fields are `null`.

## �📜 License

This client library is released under the MIT License, which permits commercial use, modification, distribution, and private use.

See the LICENSE file in this repository for full terms.

## ✍️ Author & Acknowledgments
Author: @taylorchasewhite
Generated using Microsoft Kiota
Thanks to @navix for making their OpenAPI spec available