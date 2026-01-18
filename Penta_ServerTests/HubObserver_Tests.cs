using Penta_Server.Services.SignalR;
using NSubstitute;

namespace Penta_ServerTests
{
    internal class HubObserver_Tests
    {
        [Test]
        public void AddClient()
        {
            var observer = new HubObserver();
            var hub = Substitute.For<IMessageHub>();
            observer.ClientConnected(hub);

            Assert.That(observer.TryGetHub() != null, Is.EqualTo(true));
        }

        [Test]
        public void RemoveClient()
        {
            var observer = new HubObserver();
            var hub = Substitute.For<IMessageHub>();
            observer.ClientConnected(hub);
            observer.ClientDisconnected();

            Assert.That(observer.TryGetHub() == null, Is.EqualTo(true));
        }

        [Test]
        public void RemoveOne()
        {
            var observer = new HubObserver();
            var hub = Substitute.For<IMessageHub>();
            observer.ClientConnected(hub);
            observer.ClientConnected(hub);
            observer.ClientDisconnected();

            Assert.That(observer.TryGetHub() != null, Is.EqualTo(true));
        }
    }
}
