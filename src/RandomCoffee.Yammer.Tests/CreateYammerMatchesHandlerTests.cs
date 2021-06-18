using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Entities;
using RandomCoffee.Yammer.Tests.TestDoubles;
using Xunit;

namespace RandomCoffee.Yammer.Tests
{
    public class CreateYammerMatchesHandlerTests
    {
        private readonly CreateYammerMatchesHandler _handler;
        private readonly CreateYammerMatchesCommand _request = new () { GroupId = 1 };
        private readonly NotifierStub _notifier = new ();
        private readonly AlphabeticalSorterStub _sorter = new ();
        private readonly YammerServiceStub _yammer = new ();

        public CreateYammerMatchesHandlerTests() =>
            _handler = new CreateYammerMatchesHandler(_notifier, _sorter, _yammer);

        [Fact]
        public async Task ThrowIfRequestIsNull()
        {
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(null));
            Assert.Equal("'Request' must not be empty.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfGroupIdIsEmpty()
        {
            _request.GroupId = 0;

            var ex = await Assert.ThrowsAsync<BadRequestException>(CreateMatches);
            Assert.Equal("'Group Id' must not be '0'.", ex.Message);
        }

        [Fact]
        public async Task PeopleInYammerGroupAreSentToNotifier()
        {
            _yammer.AddUserId(1);
            _yammer.AddUserId(2);

            await CreateMatches();

            _notifier.AssertIsNotified(1);
            _notifier.AssertIsNotified(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(9918)]
        [InlineData(123456789)]
        public async Task GroupIdFromRequestIsSentToYammer(long groupId)
        {
            _request.GroupId = groupId;

            await CreateMatches();

            _yammer.AssertGroupIdReceived(groupId);
        }

        private Task CreateMatches() =>
            _handler.Handle(_request);
    }
}
