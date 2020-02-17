using Google.Protobuf;

namespace FileService.Contracts.Serialization
{
    public interface IMessageSerializer<T> : ISerializer<T> where T : IMessage<T>, new()
    {
    }
}