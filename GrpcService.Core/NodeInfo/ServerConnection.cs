using Opc.Ua.OpcConnectionFactory.Models;
using Opc.Ua.OpcConnectionFactory;
using Opc.Ua.OpcUaReader.InputModels;
using Opc.Ua.OpcUaReader;
using Workstation.ServiceModel.Ua;

namespace GrpcService.Core.NodeInfo
{
    public static class ServerConnection
    {
        private static string serverUrl = "opc.tcp://10.13.0.192:62450";
        private static IUserIdentity userAuthentication = new IUserIdentity();
        private static SecurityPolicy securityPolicy = new SecurityPolicy();
        private static string? certificateStorePath = @"E:\projects\POWERSIBERIA\Logs";
        private static string applicationName = "DispCalc";
        private static Guid _uidConection;
        private static AttributeId _attributeId;

        public static Guid UidConection { get => _uidConection; set => _uidConection = value; }
        public static AttributeId AttributeId { get => _attributeId; set => _attributeId = value; }

        public static async Task<OpcUaConnectionFactory> ConnectToServerAsync()
        {
            var opcConnection = new OpcUaConnectionFactory();
            var model = new ConnectionInputModel(serverUrl, userAuthentication,
                securityPolicy, certificateStorePath, applicationName);
            UidConection = await opcConnection.CreateUaChannel(model);
            //var opcReader = new OpcUaReader(opcConnection);
            AttributeId = AttributeId.Value;

            return opcConnection;
            
        }
    }
}
