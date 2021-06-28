using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RandomCoffee.Yammer.Tests.TestDoubles
{
    public class HttpSpy : HttpMessageHandler
    {
        private readonly Dictionary<int, string> _pages = new ();
        private HttpRequestMessage _request;

        public string SetResponseBody { get; set; } = "test";

        public HttpStatusCode SetResponseCode { get; set; } = HttpStatusCode.OK;

        public void SetResponseBodyPage(int page, string body) =>
            _pages.Add(page, body);

        public void AssertHttps() =>
            Assert.Equal("https", _request?.RequestUri?.Scheme);

        public void AssertHostCalled(string host) =>
            Assert.Equal(host, _request?.RequestUri?.Host);

        public void AssertAbsolutePathCalled(string relative) =>
            Assert.StartsWith(relative, _request?.RequestUri?.AbsolutePath!, StringComparison.OrdinalIgnoreCase);

        public void AssertHttpMethod(HttpMethod method) =>
            Assert.Equal(method, _request.Method);

        public void AssertBearerToken(string value) =>
            Assert.Equal($"Bearer {value}", _request.Headers.Authorization?.ToString());

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            _request = request;

            return new HttpResponseMessage(SetResponseCode) { Content = GetContent(request) };
        }

        private static string GetQueryString(HttpRequestMessage request) =>
            request?.RequestUri?.Query.Replace("?", string.Empty, StringComparison.Ordinal);

        private static string GetPageValue(string query)
        {
            var parameters = query.Split("&");
            return parameters
                .FirstOrDefault(p => p.StartsWith("page", StringComparison.OrdinalIgnoreCase))?
                .Split("=")[1];
        }

        private StringContent GetContent(HttpRequestMessage request)
        {
            var query = GetQueryString(request);
            if (query is null) return new StringContent(SetResponseBody);

            var page = GetPageValue(query);
            if (page is null) return new StringContent(SetResponseBody);

            return PageContentHasBeenSet(page) ?
                new StringContent(GetPageContent(page)) :
                new StringContent(SetResponseBody);
        }

        private bool PageContentHasBeenSet(string page) =>
            _pages.ContainsKey(int.Parse(page, NumberStyles.Integer, new NumberFormatInfo()));

        private string GetPageContent(string page) =>
            _pages[int.Parse(page, NumberStyles.Integer, new NumberFormatInfo())];
    }
}
