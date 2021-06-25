using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Core.Contracts;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;

namespace RandomCoffee.Yammer
{
    public class YammerGroup : IGroup
    {
        private readonly long _id;
        private readonly HttpClient _client;

        public YammerGroup(long id, HttpClient client)
        {
            _id = id;
            _client = client;
        }

        public async Task<IEnumerable<Person>> GetMembers(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (_id == 0) throw new BadRequestException("'Group Id' must not be '0'.");

            _client.BaseAddress = new Uri("https://www.yammer.com/");
            await _client.GetAsync(new Uri($"api/v1/users/in_group/{_id}", UriKind.Relative), cancellationToken);

            return new List<Person> { new YammerUser { Id = 1, FullName = "MemberName" } };
        }

        public async Task Notify(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
