using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Tests.TestDoubles;
using Xunit;

namespace RandomCoffee.Yammer.Tests
{
    public class NotifyTests
    {
        private readonly HttpSpy _httpSpy = new ();
        private List<Match> _matches = new ();
        private YammerGroup _group;
        private long _id = 1;
        private string _token = "token";
        private PostFormatter _formatter = new ("Group Name");

        public NotifyTests() =>
            AddMatch(999, 888);

        [Fact]
        public async Task ThrowIfGroupIdIsZero()
        {
            _id = 0;

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Group Id' must not be '0'.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfBearerTokenIsNull()
        {
            _token = null;

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Bearer Token' must not be empty.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfBearerTokenIsEmpty()
        {
            _token = string.Empty;

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Bearer Token' must not be empty.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfMatchesIsNull()
        {
            _matches = null;

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("'Matches' must not be empty.", ex.Message);
        }

        [Fact]
        public async Task PostHttpMethodIsUsed()
        {
            await Notify();

            _httpSpy.AssertHttpMethod(HttpMethod.Post);
        }

        [Fact]
        public async Task HttpsIsUsed()
        {
            await Notify();

            _httpSpy.AssertHttps();
        }

        [Fact]
        public async Task YammerUrlIsCalled()
        {
            await Notify();

            _httpSpy.AssertHostCalled("www.yammer.com");
        }

        [Fact]
        public async Task MessageUrlIsCalled()
        {
            await Notify();

            _httpSpy.AssertAbsolutePathCalled("/api/v1/messages.json");
        }

        [Fact]
        public async Task MultiPartFormDataIsPosted()
        {
            await Notify();

            _httpSpy.AssertMultiPartFormData();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(9876543210)]
        public async Task FormDataContainsGroupId(long groupId)
        {
            _id = groupId;

            await Notify();

            _httpSpy.AssertFormParameter("group_id", groupId.ToString(new NumberFormatInfo()));
        }

        [Fact]
        public async Task IsRichTextIsFalse()
        {
            await Notify();

            _httpSpy.AssertFormParameter("is_rich_text", "false");
        }

        [Fact]
        public async Task MessageTypeIsAnnouncement()
        {
            await Notify();

            _httpSpy.AssertFormParameter("message_type", "announcement");
        }

        [Theory]
        [InlineData("Group Name")]
        [InlineData("This is the group name")]
        [InlineData("This is a different group name")]
        public async Task MessageTitleIsSet(string groupName)
        {
            _formatter = new PostFormatter(groupName);
            var title = $"{groupName} matches, {DateTime.UtcNow:MMMM yyyy}.";

            await Notify();

            _httpSpy.AssertFormParameter("title", title);
        }

        [Theory]
        [InlineData(1, 2, 9, 8)]
        [InlineData(3, 4, 5, 6)]
        [InlineData(5, 9876543210, 100, 101)]
        public async Task MessageBodyIsSet(long id1, long id2, long id3, long id4)
        {
            AddMatch(id1, id2);
            AddMatch(id3, id4);
            var message = string.Empty;
            message += "[[user:999]] and [[user:888]]." + Environment.NewLine;
            message += $"[[user:{id1}]] and [[user:{id2}]]." + Environment.NewLine;
            message += $"[[user:{id3}]] and [[user:{id4}]]." + Environment.NewLine;

            await Notify();

            _httpSpy.AssertFormParameter("body", message);
        }

        [Fact]
        public async Task MakeTwoPostsIfBodyIsMoreThan10000Characters()
        {
            for (int i = 0; i < 500; i++)
                AddMatch(i, i + 501);

            await Notify();

            Assert.EndsWith("1 of 2", _httpSpy.GetFormValue("title", 0), StringComparison.InvariantCulture);
            Assert.EndsWith("2 of 2", _httpSpy.GetFormValue("title", 1), StringComparison.InvariantCulture);
        }

        [Fact]
        public async Task DoNotPostIfMatchesIsEmpty()
        {
            _matches = new List<Match>();

            await Notify();

            _httpSpy.AssertNotCalled();
        }

        [Fact]
        public async Task ThrowIfCallIsNotSuccessful()
        {
            _httpSpy.SetResponseCode = HttpStatusCode.BadRequest;
            _httpSpy.SetResponseBody = "This is an error.";

            var ex = await Assert.ThrowsAsync<BadRequestException>(Notify);
            Assert.Equal("This is an error.", ex.Message);
        }

        private async Task Notify()
        {
            using var client = new HttpClient(_httpSpy);
            _group = new YammerGroup(_id, client, _token, _formatter);

            await _group.Notify(_matches);
        }

        private void AddMatch(long id1, long id2)
        {
            var match1 = new List<YammerUser>
            {
                new () { Id = id1, FullName = $"{id1}" },
                new () { Id = id2, FullName = $"{id2}" },
            };
            _matches.Add(new Match(match1));
        }
    }
}
