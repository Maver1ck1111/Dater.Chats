using Chats.Application;
using Chats.Domain;
using Chats.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chats.Tests
{
    public class RepositoryTests
    {
        private readonly Mock<ILogger<ChatRepository>> _logger = new Mock<ILogger<ChatRepository>>();
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Chat> _chats;

        private Chat CreateFactory()
        {
            Guid chatID = Guid.NewGuid();

            List<Guid> usersID = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };

            return new Chat()
            {
                ID = chatID,
                UsersId = usersID,
                Messages = new List<Message>()
                {
                    new Message()
                    {
                        SenderId = usersID[0],
                        ID = Guid.NewGuid(),
                        Text = "First message",
                        Timestamp = DateTime.UtcNow.AddMinutes(-2),
                    },
                    new Message()
                    {
                        SenderId = usersID[1],
                        ID = Guid.NewGuid(),
                        Text = "Second message",
                        Timestamp = DateTime.Now,
                    },
                }
            };
        }

        public RepositoryTests()
        {
            _database = DbConnection.GetMongoDataBase();

            _chats = _database.GetCollection<Chat>("Chats");
        }

        [Fact]
        public async Task FindChatByUsersAsync_ShouldReturnCorrectValue()
        {
            Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result<IEnumerable<Guid>> result = await repository.FindChatsByUserAsync(chat.UsersId[0]);

            result.IsSuccess.Should().BeTrue();

            result.Value?.First().Should().Be(chat.ID);
        }

        [Fact]
        public async Task FindChatByUsersAsync_ShouldReturnNotFound()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            Result<IEnumerable<Guid>> result = await repository.FindChatsByUserAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.ErrorMessage.Should().Be("Chats not found");
        }

        [Fact]
        public async Task GetMessagesFromChatAsync_ShouldReturnCorrectMessages()
        {
            Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result<IEnumerable<Message>> result = await repository.GetMessagesFromChatAsync(chat.ID, 0, 10);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.First().Text.Should().Be("First message");
            result.Value.Last().Text.Should().Be("Second message");

            result.Value.First().Timestamp.ToLocalTime().Should().BeCloseTo(chat.Messages.First().Timestamp.ToLocalTime(), TimeSpan.FromSeconds(3));
            result.Value.Last().Timestamp.ToLocalTime().Should().BeCloseTo(chat.Messages.Last().Timestamp.ToLocalTime(), TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task GetMessagesFromChatAsync_ShouldReturnNotFound()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            Result<IEnumerable<Message>> result = await repository.GetMessagesFromChatAsync(Guid.NewGuid(), 0, 10);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetMessagesFromChatAsync_ShouldReturnEmptyArray()
        {
           Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result<IEnumerable<Message>> result = await repository.GetMessagesFromChatAsync(chat.ID, 2, 10);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateChatAsync_SholdReturnCorrectValue()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            List<Guid> usersID = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };

            Result<Guid> result = await repository.CreateChatAsync(usersID);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateChatAsync_SholdReturnBadRequest()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            List<Guid> usersID = new List<Guid>();

            Result<Guid> result = await repository.CreateChatAsync(usersID);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task AddMessagesToChatAsync_ShouldReturnCorrectResponse()
        {
            Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            List<Message> messages = new List<Message>()
            {
                new Message()
                {
                    SenderId = chat.UsersId[0],
                    ID = Guid.NewGuid(),
                    Text = "Third message",
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                },
                new Message()
                {
                    SenderId = chat.UsersId[1],
                    ID = Guid.NewGuid(),
                    Text = "Fourth message",
                    Timestamp = DateTime.UtcNow,
                }
            };

            Result result = await repository.AddMessagesToChatAsync(chat.ID, messages);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task AddMessagesToChatAsync_ShouldReturnNotFound()
        {
            Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            List<Message> messages = new List<Message>()
            {
                new Message()
                {
                    SenderId = chat.UsersId[0],
                    ID = Guid.NewGuid(),
                    Text = "Third message",
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                },
                new Message()
                {
                    SenderId = chat.UsersId[1],
                    ID = Guid.NewGuid(),
                    Text = "Fourth message",
                    Timestamp = DateTime.UtcNow,
                }
            };

            Result result = await repository.AddMessagesToChatAsync(Guid.NewGuid(), messages);

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }


        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnCorrectResponse()
        {
            var chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            chat.Messages.First().Text = "Updated message text";

            Result result = await repository.UpdateMessageAsync(chat.ID, chat.Messages.First());

            result.IsSuccess.Should().BeTrue();

            Chat getResult = await (await _chats.FindAsync(x => x.ID == chat.ID)).FirstOrDefaultAsync();

            getResult.Should().NotBeNull();
            getResult.Messages.Should().NotBeNullOrEmpty();
            getResult.Messages.First().Text.Should().Be("Updated message text");
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_InccorectChatID()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            Result result = await repository.UpdateMessageAsync(Guid.NewGuid(), new Message() { ID = Guid.NewGuid() });

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.ErrorMessage.Should().Be("Chat not found");
        }

        [Fact]
        public async Task UpdateMessageAsync_ShouldReturnNotFound_InccorectMessageID()
        {
            var chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result result = await repository.UpdateMessageAsync(chat.ID, new Message()
            {
                ID = Guid.NewGuid()
            });

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.ErrorMessage.Should().Be("Message not found");
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnCorrectResponse()
        {
            var chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result result = await repository.DeleteMessageAsync(chat.ID, chat.Messages.First().ID);

            result.IsSuccess.Should().BeTrue();

            Chat? getResult = await (await _chats.FindAsync(x => x.ID == chat.ID)).FirstOrDefaultAsync();

            getResult.Messages.Should().HaveCount(1);
            getResult.Messages.First().ID.Should().Be(chat.Messages.Last().ID);
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_InccorectChatID()
        {
            var repository = new ChatRepository(_database, _logger.Object);

            Result result = await repository.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.ErrorMessage.Should().Be("Chat not found");
        }

        [Fact]
        public async Task DeleteMessageAsync_ShouldReturnNotFound_InccorectMessageID()
        {
            Chat chat = CreateFactory();

            await _chats.InsertOneAsync(chat);

            var repository = new ChatRepository(_database, _logger.Object);

            Result result = await repository.DeleteMessageAsync(chat.ID, Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.ErrorMessage.Should().Be("Message not found");
        }
    }
}
