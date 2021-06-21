using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Entities;
using RandomCoffee.Yammer.Services;
using RandomCoffee.Yammer.Tests.TestDoubles;
using Xunit;

namespace RandomCoffee.Yammer.Tests
{
    public class YammerNotifierTests
    {
        private readonly YammerServiceStub _yammer = new ();
        private readonly List<Match> _matches = new ();
        private YammerNotifier _notifier;

        public YammerNotifierTests() =>
            _notifier = new YammerNotifier(_yammer, 1);

        [Fact]
        public async Task ReturnIfMatchesIsNull() =>
            await _notifier.Notify(null);

        [Fact]
        public async Task ReturnIfMatchesIsEmpty() =>
            await Notify();

        [Fact]
        public async Task ThrowIfYammerIdIsZero()
        {
            AddMatch(0, 1);

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Yammer Id' must not be '0'.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfGroupIdIsZero()
        {
            _notifier = new YammerNotifier(_yammer, 0);

            AddMatch(1, 2);

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Group Id' must not be '0'.", ex.Message);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(3, 4)]
        [InlineData(9876543210, 999999999999)]
        public async Task SingleMatchIsPostedToYammer(long id1, long id2)
        {
            AddMatch(id1, id2);

            await Notify();

            _yammer.AssertPosted(1, $"[[{id1}]] and [[{id2}]].");
        }

        [Theory]
        [InlineData(1, 2, 8, 9)]
        [InlineData(3, 4, 5, 6)]
        [InlineData(9876543210, 999999999999, 111, 222)]
        public async Task MultipleMatchesArePostedToYammer(long id1, long id2, long id3, long id4)
        {
            AddMatch(id1, id2);
            AddMatch(id3, id4);

            await Notify();

            _yammer.AssertPosted(1, $"[[{id1}]] and [[{id2}]].{Environment.NewLine}[[{id3}]] and [[{id4}]].");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(9876543210)]
        public async Task YammerGroupCanBeSet(long groupId)
        {
            _notifier = new YammerNotifier(_yammer, groupId);
            AddMatch(1, 2);

            await Notify();

            _yammer.AssertPosted(groupId, "[[1]] and [[2]].");
        }

        private void AddMatch(long id1, long id2)
        {
            _matches.Add(new Match(
                new List<YammerPerson> {new() {YammerId = id1}, new() {YammerId = id2},}));
        }

        private async Task Notify() =>
            await _notifier.Notify(_matches);
    }
}
