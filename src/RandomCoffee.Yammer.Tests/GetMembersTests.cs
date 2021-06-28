using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Tests.TestDoubles;
using Utf8Json.Resolvers;
using Xunit;

namespace RandomCoffee.Yammer.Tests
{
    public sealed class GetMembersTests : IDisposable
    {
        private readonly HttpSpy _httpSpy = new ();
        private readonly HttpClient _client;
        private readonly GroupMembers _group = new ();
        private readonly Dictionary<int, GroupMembers> _pages = new ();
        private string _token = "test";

        public GetMembersTests() =>
            _client = new HttpClient(_httpSpy);

        [Fact]
        public async Task ThrowIfGroupIdIsNull()
        {
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => GetMembers(default));
            Assert.Equal("'Group Id' must not be '0'.", ex.Message);
        }

        [Fact]
        public async Task GetHttpMethodIsUsed()
        {
            await GetMembers();

            _httpSpy.AssertHttpMethod(HttpMethod.Get);
        }

        [Fact]
        public async Task HttpsIsUsed()
        {
            await GetMembers();

            _httpSpy.AssertHttps();
        }

        [Fact]
        public async Task ReturnEmptyListAsDefault()
        {
            var members = await GetMembers();

            Assert.IsAssignableFrom<IEnumerable<YammerUser>>(members);
            Assert.Empty(members);
        }

        [Fact]
        public async Task YammerUrlIsCalled()
        {
            await GetMembers();

            _httpSpy.AssertHostCalled("www.yammer.com");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(9876543210)]
        public async Task UserInGroupUrlIsCalledWithGroupId(long groupId)
        {
            await GetMembers(groupId);

            _httpSpy.AssertAbsolutePathCalled($"/api/v1/users/in_group/{groupId}");
        }

        [Theory]
        [InlineData(1, "MemberName")]
        [InlineData(2, "Name")]
        public async Task GroupMembersAreReturned(long id, string name)
        {
            AddMember(id, name);
            AddMember(9876543210, "AnotherUser");

            var members = (await GetMembers()).Cast<YammerUser>().ToList();

            AssertContains(members, id, name);
            AssertContains(members, 9876543210, "AnotherUser");
        }

        [Fact]
        public async Task AllPagesAreReturned()
        {
            AddMemberToPage(1, 1, "PageOneMember");
            AddMemberToPage(2, 2, "PageTwoMember");

            var members = (await GetMembers()).Cast<YammerUser>().ToList();

            AssertContains(members, 1, "PageOneMember");
            AssertContains(members, 2, "PageTwoMember");
        }

        [Theory]
        [InlineData("test")]
        [InlineData("newToken")]
        [InlineData("next")]
        public async Task RequestContainsBearerToken(string token)
        {
            _token = token;

            await GetMembers();

            _httpSpy.AssertBearerToken(token);
        }

        [Fact]
        public async Task ThrowIfBearerTokenIsNull()
        {
            _token = null;

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => GetMembers());
            Assert.Equal("'Bearer Token' must not be empty.", ex.Message);
        }

        [Fact]
        public async Task ThrowIfBearerTokenIsEmpty()
        {
            _token = string.Empty;

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => GetMembers());
            Assert.Equal("'Bearer Token' must not be empty.", ex.Message);
        }

        public void Dispose()
        {
            _httpSpy?.Dispose();
            _client?.Dispose();
        }

        private static void AssertContains(IEnumerable<YammerUser> members, long id, string name) =>
            Assert.Contains(members, p => p.Equals(new YammerUser { Id = id, FullName = name }));

        private Task<IEnumerable<Person>> GetMembers(long groupId = 1)
        {
            if (!_pages.Any())
            {
                _httpSpy.SetResponseBody = Utf8Json.JsonSerializer.ToJsonString(_group, StandardResolver.AllowPrivateCamelCase);
            }
            else
            {
                foreach (var (key, value) in _pages)
                {
                    value.MoreAvailable = key != _pages.Count;
                    _httpSpy.SetResponseBodyPage(
                        key,
                        Utf8Json.JsonSerializer.ToJsonString(value, StandardResolver.AllowPrivateCamelCase));
                }
            }

            var yammer = new YammerGroup(groupId, _client, _token);
            return yammer.GetMembers();
        }

        private void AddMember(long id, string name) =>
            _group.Users.Add(new User { Id = id, Name = name });

        private void AddMemberToPage(int page, long id, string name)
        {
            if (!_pages.ContainsKey(page))
                _pages.Add(page, new GroupMembers());

            _pages[page].Users.Add(new User { Id = id, Name = name });
        }

        [DataContract]
        private class GroupMembers
        {
            [DataMember]
            public IList<User> Users { get; } = new List<User>();

            [DataMember(Name = "more_available")]
            public bool MoreAvailable { get; set; }
        }

        [DataContract]
        private class User
        {
            [DataMember]
            public long Id { get; init; }

            [DataMember]
            public string Name { get; init; }
        }
    }
}
