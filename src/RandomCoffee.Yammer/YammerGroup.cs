using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string UsersInGroupUri = "api/v1/users/in_group/";
        private const string MessageUri = "api/v1/messages.json";
        private readonly long _id;
        private readonly HttpClient _client;
        private readonly PostFormatter _formatter;

        public YammerGroup(long id, HttpClient client, string bearerToken, PostFormatter formatter)
        {
            _id = id;
            _client = client;
            _formatter = formatter;
            _client.BaseAddress = new Uri("https://www.yammer.com/");
            SetBearerToken(bearerToken);
        }

        public async Task<IEnumerable<Person>> GetMembers(CancellationToken cancellationToken = default)
        {
            ValidateAndThrow();

            return await GetAllMembers(cancellationToken);
        }

        public async Task Notify(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
        {
            ValidateAndThrow();
            if (matches is null) throw new BadRequestException("'Matches' must not be empty.");
            var matchList = matches.ToList();
            if (!matchList.Any()) return;

            using var groupId = new StringContent(_id.ToString(new NumberFormatInfo()));
            using var richText = new StringContent("false");
            using var messageType = new StringContent("announcement");
            using var title = new StringContent(_formatter.Title);
            using var body = new StringContent(PostFormatter.Format(matchList));
            using var content = new MultipartFormDataContent
            {
                { groupId, "group_id" },
                { richText, "is_rich_text" },
                { messageType, "message_type" },
                { title, "title" },
                { body, "body" },
            };

            await _client.PostAsync(new Uri(MessageUri, UriKind.Relative), content, cancellationToken);
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

        private async Task<List<YammerUser>> GetAllMembers(CancellationToken cancellationToken)
        {
            var members = new List<YammerUser>();
            var pageNumber = 0;
            do
            {
                pageNumber++;
            }
            while (await AddPage(members, pageNumber, cancellationToken));

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
                    new Uri($"{UsersInGroupUri}{_id}?page={page}", UriKind.Relative),
                    cancellationToken),
                cancellationToken);
    }
}
