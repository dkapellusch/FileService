using MessagePack;
using MessagePack.Resolvers;

namespace FileService.Contracts.Serialization
{
    public class MsgPackSerializer : ISerializer
    {
        static MsgPackSerializer() =>
            MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    PrimitiveObjectResolver.Instance,
                    BuiltinResolver.Instance,
                    DynamicEnumResolver.Instance,
                    StandardResolverAllowPrivate.Instance,
                    ContractlessStandardResolver.Instance,
                    DynamicObjectResolver.Instance,
                    TypelessContractlessStandardResolver.Instance,
                    DynamicContractlessObjectResolverAllowPrivate.Instance
                ))
                .WithCompression(MessagePackCompression.Lz4Block);

        public T Deserialize<T>(byte[] serializedData)
        {
            try
            {
                if (serializedData is null) return default;

                return MessagePackSerializer.Deserialize<T>(serializedData);
            }
            catch
            {
                return default;
            }
        }

        public byte[] Serialize<T>(T dataToSerialize)
        {
            try
            {
                if (dataToSerialize is null) return default;

                return MessagePackSerializer.Serialize(dataToSerialize);
            }
            catch
            {
                return default;
            }
        }
    }
}