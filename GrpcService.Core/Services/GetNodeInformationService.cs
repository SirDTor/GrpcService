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
using GrpcService.Core.NodeInfo;
using Microsoft.AspNetCore.Server.IIS.Core;
using Google.Protobuf;

namespace GrpcService.Core.Services
{
    public class GetNodeInformationService : GetNode.GetNodeBase
    {
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
        private OpcUaConnectionFactory _connectionFactory = new OpcUaConnectionFactory();
        private List<OpcData> list = new List<OpcData>();

        public OpcUaConnectionFactory ConnectionFactory { get => _connectionFactory; set => _connectionFactory = value; }

        public override async Task<ConnectReply> ConnectToOPCServer(NodeRequest request, ServerCallContext context)
        {
            try
            {
                ConnectionFactory = await ServerConnection.ConnectToServerAsync();
                return await Task.FromResult(new ConnectReply
                {
                    IsConnect = true
                });
            }
            catch (ArgumentException)
            {
                return await Task.FromResult(new ConnectReply
                {
                    IsConnect = false
                });
            }
        }

        public override async Task<NodeReply> GetNodeInformation(NodeRequest request, ServerCallContext context)
        {
            ConnectionFactory = await ServerConnection.ConnectToServerAsync();
            var opcReader = new OpcUaReader(ConnectionFactory);
            Console.WriteLine($"Read {nodes.Length} tags by OPC UA ");
            list = await opcReader.Read(ServerConnection.UidConection, ServerConnection.AttributeId, new Opc.Ua.InputModels.Node("Modbus.TC.TS1", 1));
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
            ConnectionFactory = await ServerConnection.ConnectToServerAsync();
            var opcReader = new OpcUaReader(ConnectionFactory);

            var readTask = Task.Run(async () =>
            {
                await foreach (NodeRequest node in requestStream.ReadAllAsync())
                {
                    Console.WriteLine("Request from client");
                }
            });
            int i = 0;
            list = await opcReader.Read(ServerConnection.UidConection, ServerConnection.AttributeId, nodes);
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
            ConnectionFactory = await ServerConnection.ConnectToServerAsync();
            var opcReader = new OpcUaReader(ConnectionFactory);
            while (true)
            {
                await Task.Delay(300);
                int i = 0;
                list = await opcReader.Read(ServerConnection.UidConection, ServerConnection.AttributeId, nodes);
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
