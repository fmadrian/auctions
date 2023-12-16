namespace AuctionService; // IMPORTANT: Same namespace as the proto file namespace option (csharp_namespace).

// Conversion between BCL decimal and custom protobuf decimal implementation as defined in:
// https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/protobuf-data-types#decimals
public partial class DecimalValue
{
    private const decimal NanoFactor = 1_000_000_000;
    public DecimalValue(long units, int nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    public static implicit operator decimal(DecimalValue grpcDecimal)
    {
        return grpcDecimal.Units + grpcDecimal.Nanos / NanoFactor;
    }

    public static implicit operator DecimalValue(decimal value)
    {
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * NanoFactor);
        return new DecimalValue(units, nanos);
    }
}