# Navix Freight Audit API Client

An unofficial .NET 10 client for the [Navix Public API](https://docs.navix.io/), generated with [Microsoft Kiota](https://learn.microsoft.com/openapi/kiota/).
The checked-in OpenAPI document describes Navix's v1 integration and v2 endpoints, including endpoints marked provisional.

> This is an independent project maintained by [Taylor White (@taylorchasewhite)](https://github.com/taylorchasewhite).
> It is not authored, supported, or endorsed by Navix.
> The generated client and models are derived from Navix's public OpenAPI specification.

## Packages and requirements

Both projects target `net10.0` and require the .NET 10 SDK to build.

| Package | Contents |
| --- | --- |
| `Navix.FreightAudit.Client` | `ApiClient`, request builders, and the handwritten `NavixResponseCasing` helper |
| `Navix.FreightAudit.Models` | Generated request/response models and Kiota runtime dependencies |

The client references the models package.
This source revision builds version **0.0.7** of both packages; merging a PR does not publish them to a NuGet feed.

Install the client version available on your configured feed:

```powershell
dotnet add package Navix.FreightAudit.Client
```

For local consumption, reference the client project instead:

```powershell
dotnet add reference C:\path\to\navix-api\Navix.FreightAudit\Navix.FreightAudit\Navix.FreightAudit.csproj
```

## Authentication and usage

Navix requires both an API key (`X-Api-Key`) and tenant UUID (`X-Tenant-Uuid`) on every request.
Obtain these from your Navix account team; production and sandbox tenants have different UUIDs.
The generated default base URL is `https://api.navix.io`.
The SDK does not automatically discover credentials.

```csharp
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Navix.FreightAudit;

var apiKey = Environment.GetEnvironmentVariable("NAVIX_API_KEY")
    ?? throw new InvalidOperationException("Set NAVIX_API_KEY.");
var tenantUuid = Environment.GetEnvironmentVariable("NAVIX_TENANT_UUID")
    ?? throw new InvalidOperationException("Set NAVIX_TENANT_UUID.");
var orderExternalId = Environment.GetEnvironmentVariable("NAVIX_ORDER_EXTERNAL_ID")
    ?? throw new InvalidOperationException("Set NAVIX_ORDER_EXTERNAL_ID.");

// Register once at startup, before constructing the client.
NavixResponseCasing.Register(ParseNodeFactoryRegistry.DefaultInstance);

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
httpClient.DefaultRequestHeaders.Add("X-Tenant-Uuid", tenantUuid);

// Headers above provide authentication; the adapter need not add credentials.
using var adapter = new HttpClientRequestAdapter(
    new AnonymousAuthenticationProvider(), httpClient: httpClient);
var client = new ApiClient(adapter);

var invoices = await client.V2.Orders[orderExternalId].Invoices.GetAsync();
```

Reuse the client/adapter for requests to Navix only; do not reuse this authenticated `HttpClient` for unrelated hosts.
Kiota exposes endpoints through request builders such as `client.V2.Orders` and `client.Integration`, not through convenience methods such as `Orders.CreateAsync`.
Some write endpoints return `202 Accepted`: acceptance does not mean processing has completed.
Do not use provisional endpoints without explicit authorization from your Navix account team.

## Latest specification refresh

Refreshed from `https://docs.navix.io/` on **2026-10-20**.
The document still identifies its API version as `v1-v2`; it does not provide a separate dated release number.
It now contains 33 paths and 118 component schemas.

Compared with the previous snapshot, this refresh includes:

- `GET /v2/invoices/{invoiceUuid}/exceptions` and models for open exceptions and work instructions.
- Customer and vendor address/contact request and response models.
- Updated v2 invoice audit-result service details and other upstream schema/documentation changes.

For invoice exceptions, resolved items are omitted from subsequent responses.
Only Flex-rule exceptions and internal Navix errors are returned, as documented upstream.

## Regenerating the client

Use PowerShell 7.3 or later and Kiota **1.32.4**, matching the checked-in generation lock.
If Kiota is not installed:

```powershell
dotnet tool install --global Microsoft.OpenApi.Kiota --version 1.32.4
```

From the repository root:

```powershell
.\Navix.FreightAudit\Update-Client.ps1
dotnet build .\Navix.FreightAudit\Navix.FreightAudit.sln -c Debug
dotnet build .\Navix.FreightAudit\Navix.FreightAudit.sln -c Release
```

The script downloads the OpenAPI data embedded in the public Redoc page, normalizes JSON response media types, and generates the client in a temporary directory.
It copies request builders into the client project and models into the models project, removes obsolete generated C# files, updates the Kiota lock, and preserves handwritten files.
If Navix changes its documentation page format, the script fails explicitly instead of silently using an old snapshot.
A `blob:https://docs.navix.io/...` download URL is browser-session-local and cannot be used as a stable source URL.
Review upstream contract changes before publishing; regeneration does not increment package versions automatically.

### Response content types

The public specification labels six JSON responses as `text/json`.
Without normalization, Kiota generates unstructured `Stream` responses instead of typed models.
The refresh script changes these response media types to `application/json` in the local specification:

- `GET /v2/orders/{orderExternalId}/invoices`
- `GET /v2/documents/metadata/{documentUuid}`
- `GET /v2/documents/orders/{orderExternalId}`
- `GET /v2/documents/metadata/orders/{orderExternalId}`
- `POST /v2/documents/approved`
- `POST /v2/orders/approved`

### Response property casing

Navix responses can use PascalCase property names even though the specification declares camelCase.
Kiota binds property names case-sensitively, so these fields would otherwise deserialize as `null` (see [microsoft/kiota#7060](https://github.com/microsoft/kiota/issues/7060)).

The handwritten `NavixResponseCasing.Register` helper lowercases the first character of JSON property names before deserialization.
For example, it maps `Uuid` to `uuid` and leaves `orderExternalId` unchanged.
Call it once on the parse-node registry used by your adapter, as shown above.
Custom adapters using a different registry must register the helper on that registry.
This workaround does not modify the generated models or outgoing request serialization.

## Building packages

Both projects generate `.nupkg` and `.snupkg` files on build, under their respective `bin\<Configuration>` directories.
The README is included in both packages.
There are currently no automated test projects in the solution; builds do not verify live API behavior.

## License

No license file is currently included in this repository.
Do not assume an MIT or other open-source license grant from the earlier README's claim.
Navix's API documentation and specification remain subject to their applicable terms.
