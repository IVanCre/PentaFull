using Penta_Server.Interfaces;
using Penta_Server.Services.MessagesProcessors;
using MessageLib;

namespace Penta_ServerTests
{
    internal class MessageSaver_Tests
    {
        private class Repo : IMessageRepository
        {
            public int counter = 0;
            public void Add(Message msg)
            {
                counter++;
            }

            public Task<List<Message>> GetNonSendedForUserAsync(int userID)
            {
                throw new NotImplementedException();
            }

            public bool HasNonSended(int userID)
            {
                throw new NotImplementedException();
            }

            public void MarkForDelete(int messageID)
            {
                throw new NotImplementedException();
            }

            public void MarkForDelete(long messageID)
            {
                throw new NotImplementedException();
            }

            Task<bool> IMessageRepository.HasNonSended(int userID)
            {
                throw new NotImplementedException();
            }
        }


        [Test]
        public void SaveMessage()
        {
            var repo = new Repo();

            var saver = new MessageSaver(repo);
            saver.Save(new Message(0, 1, 1, 1, MessageType.Unknown, null, DateTime.Now));

            Thread.Sleep(1000);
            Assert.That(repo.counter, Is.EqualTo(1));
        }
    }
}
