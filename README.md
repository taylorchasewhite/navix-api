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

## 📜 License

This client library is released under the MIT License, which permits commercial use, modification, distribution, and private use.

See the LICENSE file in this repository for full terms.

## ✍️ Author & Acknowledgments
Author: @taylorchasewhite
Generated using Microsoft Kiota
Thanks to @navix for making their OpenAPI spec available