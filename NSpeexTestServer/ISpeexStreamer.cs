using System.ServiceModel;

namespace NSpeexTestServer
{
    [ServiceContract(CallbackContract = typeof(ISpeexStreamerCallback))]
    public interface ISpeexStreamer
    {
        [OperationContract(IsOneWay = true)]
        void Publish(byte[] data);

        [OperationContract]
        void Subscribe();
    }

    public interface ISpeexStreamerCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPublish(byte[] data);
    }

    public interface ISpeexStreamerChannel : ISpeexStreamer, IClientChannel
    {}
}
