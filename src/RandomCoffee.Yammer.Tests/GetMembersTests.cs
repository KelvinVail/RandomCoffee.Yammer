using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

            var members = (await GetMembers()).Cast<YammerUser>();

            Assert.Contains(members, p => p.Equals(new YammerUser { Id = id, FullName = name }));
        }

        public void Dispose()
        {
            _httpSpy?.Dispose();
            _client?.Dispose();
        }

        private Task<IEnumerable<Person>> GetMembers(long groupId = 1)
        {
            var yammer = new YammerGroup(groupId, _client);
            return yammer.GetMembers();
        }

        private void AddMember(long id, string name)
        {
            var group = new GroupMembers();
            group.Users.Add(new User { Id = id, Name = name });
            _httpSpy.SetResponseBody = Utf8Json.JsonSerializer.ToJsonString(group, StandardResolver.AllowPrivateExcludeNull);
        }

        private class GroupMembers
        {
            public IList<User> Users { get; } = new List<User>();
        }

        private class User
        {
            public long Id { get; init; }

            public string Name { get; init; }
        }
    }
}
