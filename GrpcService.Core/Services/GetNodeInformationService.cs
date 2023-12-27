using Opc.Ua;
using Opc.Ua.OpcConnectionFactory;
using Opc.Ua.OpcConnectionFactory.Models;
using Opc.Ua.OpcUaReader.InputModels;
using Opc.Ua.OpcUaReader;
using Workstation.ServiceModel.Ua;
using GrpcService.Protocol;
using Grpc.Core;
using System.Reflection.PortableExecutable;
using Opc.Ua.ViewModels;
using System;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using Microsoft.AspNetCore.Server.IIS.Core;
using Google.Protobuf;

namespace GrpcService.Core.Services
{
    public class GetNodeInformationService : GetNode.GetNodeBase
    {
        public string serverUrl = "opc.tcp://10.13.33.1:62450";
        public IUserIdentity userAuthentication = new IUserIdentity();
        public SecurityPolicy securityPolicy = new SecurityPolicy();
        public string? certificateStorePath = @"E:\projects\POWERSIBERIA\Logs";
        public string applicationName = "TestServer";
        public Guid _uidConection;
        public AttributeId _attributeId;
        public Guid UidConection { get => _uidConection; set => _uidConection = value; }
        public AttributeId AttributeId { get => _attributeId; set => _attributeId = value; }        
        private Opc.Ua.InputModels.Node[] nodes =
            {
                new Opc.Ua.InputModels.Node("Modbus.TC.TS1", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS2", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS3", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS4", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS5", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS6", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS7", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS8", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS9", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS10", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS11", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS12", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS13", 1),
                new Opc.Ua.InputModels.Node("Modbus.TC.TS14", 1)
            };
        private List<OpcData> list = new List<OpcData>();
        private OpcUaConnectionFactory opcConnection;

        public ConnectionInputModel ConnectToServerAsync()
        {
            opcConnection = new OpcUaConnectionFactory();
            var model = new ConnectionInputModel(serverUrl, userAuthentication,
                securityPolicy, certificateStorePath, applicationName);
            return model;
        }

        public override async Task<NodeReply> GetNodeInformation(NodeRequest request, ServerCallContext context)
        {
            var model = ConnectToServerAsync();
            UidConection = await opcConnection.CreateUaChannel(model);
            AttributeId = AttributeId.Value;
            var integrityServerDataReader = new OpcUaReader(opcConnection);
            //ConnectionFactory = await ServerConnection.ConnectToServerAsync();
            Console.WriteLine($"Read {nodes.Length} tags by OPC UA ");
            list = await integrityServerDataReader.Read(UidConection, AttributeId, new Opc.Ua.InputModels.Node("Modbus.TC.TS1", 1));
            return await Task.FromResult(new NodeReply
            {
                NodeName = nodes[0].Identifier,
                NodeValue = (bool)list[0].Variant.GetValue()
            });
        }

        public async IAsyncEnumerable<OpcData> GetNumbersAsync()
        {
            foreach (var node in list)
            {
                await Task.Delay(100);
                yield return node;
            }
        }

        public override async Task GetNodeInformationClientStream(IAsyncStreamReader<NodeRequest> requestStream,
            IServerStreamWriter<NodeReply> responseStream, ServerCallContext context)
        {
            var model = ConnectToServerAsync();
            UidConection = await opcConnection.CreateUaChannel(model);
            //ConnectToServerAsync();
            var integrityServerDataReader = new OpcUaReader(opcConnection);
            var readTask = Task.Run(async () =>
            {
                await foreach (NodeRequest node in requestStream.ReadAllAsync())
                {
                    Console.WriteLine("Request from client");
                }
            });
            int i = 0;
            list = await integrityServerDataReader.Read(UidConection, AttributeId, nodes);
            await foreach (var node in GetNumbersAsync())
            {
                // Посылаем ответ, пока клиент не закроет поток
                await responseStream.WriteAsync(new NodeReply
                {
                    NodeName = nodes[i].Identifier,
                    NodeValue = (bool)node.Variant.GetValue()
                });
                Console.WriteLine(node);
                i++;
            }
        }

        public override async Task GetNodeInformationServerStream(NodeRequest request, IServerStreamWriter<NodeReply> responseStream, ServerCallContext context)
        {
            var model = ConnectToServerAsync();
            UidConection = await opcConnection.CreateUaChannel(model);
            var integrityServerDataReader = new OpcUaReader(opcConnection);
            while (true)
            {
                int i = 0;
                //await Task.Delay(1000);
                list = await integrityServerDataReader.Read(UidConection, AttributeId, nodes);
                await foreach (var node in GetNumbersAsync())
                {
                    // Посылаем ответ, пока клиент не закроет поток
                    await responseStream.WriteAsync(new NodeReply
                    {
                        NodeName = nodes[i].Identifier,
                        NodeValue = (bool)node.Variant.GetValue()
                    });
                    Console.WriteLine($"Node Time : {node.SourceTimestamp}");
                    i++;
                }
            }
        }
    }
}
