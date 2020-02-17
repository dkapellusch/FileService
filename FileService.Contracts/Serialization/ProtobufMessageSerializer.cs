using Google.Protobuf;

namespace FileService.Contracts.Serialization
{
    public sealed class ProtobufMessageSerializer<TMessage> : IMessageSerializer<TMessage> where TMessage : IMessage<TMessage>, new()
    {
        private readonly MessageParser<TMessage> _messageParser = new MessageParser<TMessage>(() => new TMessage()).WithDiscardUnknownFields(false);

        public TMessage Deserialize(byte[] bytes)
        {
            if (bytes is null) return default;

            return _messageParser.ParseFrom(bytes);
        }

        public byte[] Serialize(TMessage data)
        {
            if (data is null) return new byte[0];

            return data.ToByteArray();
        }
    }
}