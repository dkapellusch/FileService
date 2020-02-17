using Google.Protobuf;

namespace FileService.Contracts.Serialization
{
    public class ProtoSerializer<T> : ISerializer<T> where T : IMessage<T>, new()
    {
        private readonly IMessageSerializer<T> _messageSerializer;

        public ProtoSerializer() => _messageSerializer = new ProtobufMessageSerializer<T>();

        public T Deserialize(byte[] serializedData)
        {
            var result = _messageSerializer.Deserialize(serializedData);
            return result;
        }

        public byte[] Serialize(T dataToSerialize)
        {
            var result = _messageSerializer.Serialize(dataToSerialize);
            return result;
        }
    }
}