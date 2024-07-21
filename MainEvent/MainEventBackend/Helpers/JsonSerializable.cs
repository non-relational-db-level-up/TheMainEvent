using System.Text.Json;
using Confluent.Kafka;

namespace MainEvent.helpers;

public class JsonSerializable<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return JsonSerializer.SerializeToUtf8Bytes(data);
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<T>(data) ?? throw new InvalidOperationException();
    }
}