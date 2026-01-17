using Message_Server.Services.SignalR;

using NSubstitute;
using Message_Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;

namespace Message_ServerTests
{
    internal class MessageHub_Tests
    {
        private class TestHubCallerContext : HubCallerContext
        {
            private readonly string _connectionId;
            private readonly ClaimsPrincipal _user;

            public TestHubCallerContext(string connectionId, ClaimsPrincipal user)
            {
                _connectionId = connectionId;
                _user = user;
            }

            public override string ConnectionId => _connectionId;

            public override ClaimsPrincipal User => _user;

            public override string UserIdentifier => throw new NotImplementedException();

            public override IDictionary<object, object> Items => throw new NotImplementedException();

            public override IFeatureCollection Features => throw new NotImplementedException();

            public override CancellationToken ConnectionAborted => throw new NotImplementedException();

            public override void Abort()
            {
                throw new NotImplementedException();
            }

            // При необходимости переопределите другие свойства или методы
        }

        private HubCallerContext CreateContext(string connID, int userID)
        {
            // Создаем мок для HubCallerContext
            return new TestHubCallerContext(connID,
                            new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "test-user"),
                        new Claim("userID", $"{userID}")
                    })));
        }

        [Test]
        public async Task ConnectClient_Test()
        {
            var notifier = Substitute.For<IClientNotifier>();
            var procer = Substitute.For<IMessageProcessor>();
            var hub = new HubObserver();

            var messHub = new MessageHub(
                null,
                procer,
                notifier,
                hub);
            messHub.Context = CreateContext("abcd",11);

            await messHub.OnConnectedAsync();

            Assert.That(hub.TryGetHub()!=null, Is.EqualTo(true));
        }

        [Test]
        public async Task DisconnectedOne_Test()
        {
            var notifier = Substitute.For<IClientNotifier>();
            var procer = Substitute.For<IMessageProcessor>();
            var hub = new HubObserver();

            var messHub = new MessageHub(
                null,
                procer,
                notifier,
                hub);
            messHub.Context = CreateContext("abcd", 11);
            await messHub.OnConnectedAsync();

            messHub.Context = CreateContext("abcd-12", 22);
            await messHub.OnConnectedAsync();

            await messHub.OnDisconnectedAsync(null);

            Assert.That(hub.TryGetHub() != null, Is.EqualTo(true));
        }

        [Test]
        public async Task DisconnectedAll_Test()
        {
            var notifier = Substitute.For<IClientNotifier>();
            var procer = Substitute.For<IMessageProcessor>();
            var hub = new HubObserver();

            var messHub = new MessageHub(
                null,
                procer,
                notifier,
                hub);
            messHub.Context = CreateContext("abcd", 11);

            await messHub.OnDisconnectedAsync(null);

            Assert.That(hub.TryGetHub() == null, Is.EqualTo(true));
        }
    }
}
