using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Core.Contracts;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;
using Utf8Json;
using Utf8Json.Resolvers;

namespace RandomCoffee.Yammer
{
    public class YammerGroup : IGroup
    {
        private const string GroupUsersUri = "api/v1/users/in_group/";
        private readonly long _id;
        private readonly HttpClient _client;

        public YammerGroup(long id, HttpClient client, string bearerToken)
        {
            _id = id;
            _client = client;
            _client.BaseAddress = new Uri("https://www.yammer.com/");
            SetBearerToken(bearerToken);
        }

        public async Task<IEnumerable<Person>> GetMembers(CancellationToken cancellationToken = default)
        {
            ValidateAndThrow();

            return await GetAllPages(cancellationToken);
        }

        public async Task Notify(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        private static async Task<YammerUsers> DeserializeResponse(
            HttpResponseMessage response,
            CancellationToken cancellationToken) =>
            await JsonSerializer.DeserializeAsync<YammerUsers>(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                StandardResolver.AllowPrivateExcludeNullCamelCase);

        private static IEnumerable<YammerUser> ActiveMembers(YammerUsers page) =>
            page.Users.Where(x => x.State == "active");

        private void SetBearerToken(string token) =>
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private void ValidateAndThrow()
        {
            if (_id == 0) throw new BadRequestException("'Group Id' must not be '0'.");
            if (string.IsNullOrEmpty(_client.DefaultRequestHeaders.Authorization!.Parameter))
                throw new BadRequestException("'Bearer Token' must not be empty.");
        }

        private async Task<List<YammerUser>> GetAllPages(CancellationToken cancellationToken)
        {
            bool more;
            var members = new List<YammerUser>();
            var pageNumber = 0;
            do
            {
                pageNumber++;
                more = await AddPage(members, pageNumber, cancellationToken);
            }
            while (more);

            return members;
        }

        private async Task<bool> AddPage(List<YammerUser> members, int pageNumber, CancellationToken cancellationToken)
        {
            var page = await GetPage(pageNumber, cancellationToken);
            members.AddRange(ActiveMembers(page));
            return page.MoreAvailable;
        }

        private async Task<YammerUsers> GetPage(int page, CancellationToken cancellationToken) =>
            await DeserializeResponse(
                await _client.GetAsync(
                    new Uri($"{GroupUsersUri}{_id}?page={page}", UriKind.Relative),
                    cancellationToken),
                cancellationToken);
    }
}
