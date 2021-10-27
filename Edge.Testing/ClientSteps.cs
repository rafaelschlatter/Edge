using BoDi;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using FluentAssertions;
using Autofac;
using System.Diagnostics.CodeAnalysis;
using Moq;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A class defining steps for dealing with clients
    /// </summary>
    [Binding]
    public sealed class ClientSteps
    {
        private readonly IObjectContainer _container;
        private readonly TypeMapping _typeMapping;
        private readonly Dictionary<Type, List<object>> _sentDataByType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typeMapping"></param>
        public ClientSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
            _sentDataByType = new();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        [Given("the following client mocks")]
        public void GivenTheFollowingClientMocks(Table table)
        {
            var appContext = _container.Resolve<ApplicationContext>();
            var clientTypes = table.Rows
                .Select(row => (clientType: _typeMapping[row["ClientType"]], connectionType: _typeMapping[row["ConnectionType"]]))
                .ToList();

            foreach (var clientType in clientTypes)
            {
                var type = clientType.clientType.MakeGenericType(clientType.connectionType);
                var setupClientMockMethod = GetType().GetMethod("SetupClientMock", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(type, clientType.connectionType);
                setupClientMockMethod.Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [When(@"clients receive the following data")]
        public void WhenClientsReceiveTheFollowingData(Table table)
        {
            var appContext = _container.Resolve<ApplicationContext>();

            var clientTypes = table.Rows.Select(row => _typeMapping[row["ClientType"]].MakeGenericType(_typeMapping[row["ConnectionType"]])).Distinct().ToList();
            var dataTypeForClientType = clientTypes
                .ToDictionary(clientType => clientType, clientType => GetDataTypeForReceiverClientType(clientType));

            var dataReceiverMethodForClientType = dataTypeForClientType
                .Select(pair => (clientType: pair.Key, client: appContext.Scope.Resolve(pair.Key), dataType: pair.Value))
                .ToDictionary(param => param.clientType, param => (Action<TableRow>)GetType().GetMethod("MakeDataReceiverMethodForClient", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(param.clientType, param.dataType).Invoke(this, new object[] { param.client }));
        
            foreach (var row in table.Rows)
            {
                var clientType = _typeMapping[row["ClientType"]].MakeGenericType(_typeMapping[row["ConnectionType"]]);
                if (!dataReceiverMethodForClientType.TryGetValue(clientType, out Action<TableRow> dataReceived)) continue;
                dataReceived(row);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Then(@"clients send the following data")]
        public void ThenClientsSendTheFollowingData(Table table)
        {
            var rowsWithClientTypes = table.Rows
                .Select(row => (row, clientType: _typeMapping[row["ClientType"]].MakeGenericType(_typeMapping[row["ConnectionType"]])))
                .ToList();

            var rowsForClientTypes = rowsWithClientTypes
                .GroupBy(row => row.clientType)
                .ToDictionary(clientTypeData => clientTypeData.Key, clientTypeData => clientTypeData.Select(data => data.row).ToList());

            foreach (var (clientType, rows) in rowsForClientTypes)
            {
                var dataType = GetDataTypeForSenderClientType(clientType);
                var verifierMethod = GetType().GetMethod("VerifySentDataForClient", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(clientType, dataType);
                verifierMethod.Invoke(this, new object[] { rows });
            }

            foreach (var row in table.Rows)
            {
                var genericClientType = _typeMapping[row["ClientType"]];
                var connectionType = _typeMapping[row["ConnectionType"]];
                var clientType = genericClientType.MakeGenericType(connectionType);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        [Then(@"the following clients will be connected")]
        public void ThenTheFollowingClientsWillBeConnected(Table table)
        {
            var appContext = _container.Resolve<ApplicationContext>();

            foreach (var row in table.Rows)
            {
                var clientType = _typeMapping[row["ClientType"]].MakeGenericType(_typeMapping[row["ConnectionType"]]);
                var client = appContext.Scope.Resolve(clientType);
                var verifyClientHasBeenConnectedMethod = GetType().GetMethod("VerifyClientHasBeenConnected", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(clientType);
                
                verifyClientHasBeenConnectedMethod.Invoke(null, new object[] { client });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        [Then(@"clients will subscribe to the following topics")]
        public void ThenClientsWillSubscribeToTheFollowingTopics(Table table)
        {
            var appContext = _container.Resolve<ApplicationContext>();

            foreach (var row in table.Rows)
            {
                var clientType = _typeMapping[row["ClientType"]].MakeGenericType(_typeMapping[row["ConnectionType"]]);
                var client = appContext.Scope.Resolve(clientType);
                var dataType = GetDataTypeForReceiverClientType(clientType);
                var topicType = GetTopicTypeForSubscribingReceiverClientType(clientType);
                var topic = GetType().GetMethod("GetTopicForTableRow", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(topicType).Invoke(this, new object[] { row });

                var verifyTopicHasBeenSubscribedMethod = GetType().GetMethod("VerifyTopicHasBeenSubscribed", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(clientType, dataType, topicType);

                verifyTopicHasBeenSubscribedMethod.Invoke(null, new object[] { client, topic });
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private static void VerifyClientHasBeenConnected<ClientType>(ClientType client)
            where ClientType : class, IClient
        {
            var mock = Mock.Get(client);
            mock.Verify(c => c.Connect(), Times.Once);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private static void VerifyTopicHasBeenSubscribed<ClientType, DataType, TopicType>(ClientType client, TopicType topic)
            where ClientType : class, ISubscribingReceiverClient<DataType, TopicType>
        {
            var mock = Mock.Get(client);
            mock.Verify(c => c.Subscribe(topic), Times.Once);
        }

        private static Type GetDataTypeForReceiverClientType(Type clientType)
        {
            return clientType.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IReceiverClient<>))
                .Select(iface => iface.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        private static Type GetDataTypeForSenderClientType(Type clientType)
        {
            return clientType.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ISenderClient<>))
                .Select(iface => iface.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        private object GetTopicForTableRow<TopicType>(TableRow row)
        {
            var factory = _container.Resolve<IEventInstanceFactory<TopicType>>();
            return factory.FromTableRow(row);
        }

        private static Type GetTopicTypeForSubscribingReceiverClientType(Type clientType)
        {
            return clientType.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ISubscribingReceiverClient<,>))
                .Select(iface => iface.GetGenericArguments()[1])
                .FirstOrDefault();
        }


        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupClientMock<ClientType, ConnectionType>()
            where ClientType : class, IClient<ConnectionType>
            where ConnectionType : IClientConnection
        {

            var mock = new Mock<ClientType>();

            if (IsSenderClient<ClientType>(out Type dataType))
            {
                var setupSenderMethod = GetType().GetMethod("SetupSenderClientMock", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(ClientType), dataType);
                setupSenderMethod.Invoke(this, new object[] { mock });
            }

            var appContext = _container.Resolve<ApplicationContext>();
            appContext.GetType().GetMethod("WithMock").MakeGenericMethod(typeof(ClientType)).Invoke(appContext, new object[] { mock });
        }

        private static bool IsReceiverClient<ClientType>() where ClientType : IClient
        {
            var senderClientInterfaces = typeof(ClientType).GetInterfaces()
                .Where(iface => iface.IsGenericType)
                .Where(iface => iface.GetGenericTypeDefinition() == typeof(IReceiverClient<>));

            return senderClientInterfaces.Any();
        }

        private static bool IsSenderClient<ClientType>(out Type dataType) where ClientType : IClient
        {
            var senderClientInterfaces = typeof(ClientType).GetInterfaces()
                .Where(iface => iface.IsGenericType)
                .Where(iface => iface.GetGenericTypeDefinition() == typeof(ISenderClient<>));

            dataType = senderClientInterfaces.FirstOrDefault()?.GetGenericArguments()[0];

            return senderClientInterfaces.Any();
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupSenderClientMock<ClientType, DataType>(Mock<ClientType> client)
            where ClientType : class, ISenderClient<DataType>
        {
            var sentData = new List<object>();
            _sentDataByType[typeof(ClientType)] = sentData;
            client.Setup(c => c.SendAsync(It.IsAny<DataType>()))
                .Callback<DataType>(data => sentData.Add(data));
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private Action<TableRow> MakeDataReceiverMethodForClient<ClientType, DataType>(ClientType client)
            where ClientType : class, IReceiverClient<DataType>
        {
            var connectionType = typeof(ClientType).GetInterfaces()
                .Where(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IReceiverClient<,>))
                .Select(ifce => ifce.GetGenericArguments()[0])
                .FirstOrDefault();

            var mockedClient = Mock.Get(client);
            var dataFactory = _container.Resolve<IEventInstanceFactory<DataType>>();

            return (row) =>
            {
                var data = dataFactory.FromTableRow(row);
                mockedClient.Raise(c => c.OnDataReceived += null, connectionType, data);
            };
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void VerifySentDataForClient<ClientType, DataType>(List<TableRow> rows)
            where ClientType : class, ISenderClient<DataType>
        {
            var connectionType = typeof(ClientType).GetInterfaces()
                .Where(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(ISenderClient<,>))
                .Select(ifce => ifce.GetGenericArguments()[0])
                .FirstOrDefault();

            var verifier = _container.Resolve<IProducedEventVerifier<DataType>>();

            var sentData = _sentDataByType[typeof(ClientType)].Select(data => (DataType) data).ToList();
            foreach (var (actual, expected) in sentData.Zip(rows))
            {
                verifier.VerifyFromTableRow(actual, expected);
            }
        }
    }
}
