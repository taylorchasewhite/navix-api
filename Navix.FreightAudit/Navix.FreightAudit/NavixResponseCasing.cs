using System;
using System.Buffers;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

namespace Navix.FreightAudit;

/// <summary>
/// Normalizes the casing of JSON property names on Navix responses so the
/// Kiota-generated models bind correctly.
/// </summary>
/// <remarks>
/// Navix's OpenAPI schema declares camelCase property names, but the live API
/// returns <b>PascalCase</b> for several read endpoints (for example
/// <c>GET /v2/orders/{id}/invoices</c>, the dispute reads, and the document
/// metadata reads), while others (for example <c>POST /v2/orders/approved</c> and the
/// audit-result) return camelCase. Kiota binds JSON keys case-sensitively, so the
/// PascalCase responses would otherwise deserialize to <c>null</c>
/// (see <see href="https://github.com/microsoft/kiota/issues/7060"/>).
///
/// <see cref="Register"/> installs a parse-node factory for <c>application/json</c>
/// that lower-cases the first character of every JSON property name before Kiota
/// deserializes it. That maps <c>Uuid</c> to <c>uuid</c> while leaving
/// <c>orderExternalId</c> unchanged, so responses bind regardless of which casing an
/// endpoint uses. Normalization is a no-op on already-camelCase payloads.
///
/// Call it once during application start-up, on the
/// <see cref="ParseNodeFactoryRegistry"/> used by the request adapter that backs the
/// <see cref="ApiClient"/> (normally <see cref="ParseNodeFactoryRegistry.DefaultInstance"/>).
///
/// This file is hand-written and is not produced by Kiota generation; regenerating
/// the client does not overwrite it.
/// </remarks>
public static class NavixResponseCasing
{
    /// <summary>The JSON media type Navix responses use.</summary>
    public const string JsonContentType = "application/json";

    /// <summary>
    /// Installs a casing-normalizing JSON parse-node factory for
    /// <c>application/json</c> on <paramref name="registry"/>.
    /// </summary>
    /// <param name="registry">The parse-node factory registry to configure.</param>
    public static void Register(ParseNodeFactoryRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.ContentTypeAssociatedFactories[JsonContentType] = new CaseNormalizingJsonParseNodeFactory();
    }
}

/// <summary>
/// A parse-node factory that normalizes JSON property-name casing (lower-casing the
/// first character of each name) and then parses the payload as JSON via Kiota's
/// <see cref="JsonParseNodeFactory"/>.
/// </summary>
internal sealed class CaseNormalizingJsonParseNodeFactory : IAsyncParseNodeFactory
{
    private readonly JsonParseNodeFactory _json = new();

    /// <inheritdoc />
    public string ValidContentType => NavixResponseCasing.JsonContentType;

    /// <inheritdoc />
    public async Task<IParseNode> GetRootParseNodeAsync(
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default
    )
    {
        using MemoryStream normalized = await NormalizeAsync(content, cancellationToken)
            .ConfigureAwait(false);
        return await _json
            .GetRootParseNodeAsync(NavixResponseCasing.JsonContentType, normalized, cancellationToken)
            .ConfigureAwait(false);
    }

#pragma warning disable CS0618 // Synchronous path retained only to satisfy the interface contract.
    /// <inheritdoc />
    public IParseNode GetRootParseNode(string contentType, Stream content)
    {
        using MemoryStream buffer = new();
        content.CopyTo(buffer);
        using MemoryStream normalized = new(NormalizeBytes(buffer.ToArray()));
        return _json.GetRootParseNode(NavixResponseCasing.JsonContentType, normalized);
    }
#pragma warning restore CS0618

    private static async Task<MemoryStream> NormalizeAsync(
        Stream content,
        CancellationToken cancellationToken
    )
    {
        using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        return new MemoryStream(NormalizeBytes(buffer.ToArray()));
    }

    /// <summary>
    /// Rewrites every JSON property name to its camelCase form (lower first character)
    /// and copies values verbatim. Returns the original bytes when the payload is not
    /// valid JSON.
    /// </summary>
    private static byte[] NormalizeBytes(byte[] json)
    {
        if (json.Length == 0)
        {
            return json;
        }

        try
        {
            Utf8JsonReader reader = new(
                json,
                new JsonReaderOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                }
            );

            ArrayBufferWriter<byte> buffer = new();
            using Utf8JsonWriter writer = new(
                buffer,
                new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            );

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        writer.WriteStartObject();
                        break;
                    case JsonTokenType.EndObject:
                        writer.WriteEndObject();
                        break;
                    case JsonTokenType.StartArray:
                        writer.WriteStartArray();
                        break;
                    case JsonTokenType.EndArray:
                        writer.WriteEndArray();
                        break;
                    case JsonTokenType.PropertyName:
                        writer.WritePropertyName(CamelCaseFirstChar(reader.GetString()!));
                        break;
                    case JsonTokenType.String:
                        writer.WriteStringValue(reader.GetString());
                        break;
                    case JsonTokenType.Number:
                        writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
                        break;
                    case JsonTokenType.True:
                        writer.WriteBooleanValue(true);
                        break;
                    case JsonTokenType.False:
                        writer.WriteBooleanValue(false);
                        break;
                    case JsonTokenType.Null:
                        writer.WriteNullValue();
                        break;
                }
            }

            writer.Flush();
            return buffer.WrittenSpan.ToArray();
        }
        catch (JsonException)
        {
            // Not JSON we can rewrite (for example a plain-text body mislabelled as
            // json). Hand the original bytes back so the parser surfaces the real error.
            return json;
        }
    }

    private static string CamelCaseFirstChar(string name) =>
        name.Length == 0 || char.IsLower(name[0])
            ? name
            : string.Create(
                name.Length,
                name,
                static (span, source) =>
                {
                    source.AsSpan().CopyTo(span);
                    span[0] = char.ToLowerInvariant(span[0]);
                }
            );
}
