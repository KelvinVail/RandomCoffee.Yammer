using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RandomCoffee.Yammer.Tests.TestDoubles
{
    public class HttpSpy : HttpMessageHandler
    {
        private HttpRequestMessage _request;

        public string SetResponseBody { get; set; } = "test";

        public HttpStatusCode SetResponseCode { get; set; } = HttpStatusCode.OK;

        public void AssertHttps() =>
            Assert.Equal("https", _request?.RequestUri?.Scheme);

        public void AssertHostCalled(string host) =>
            Assert.Equal(host, _request?.RequestUri?.Host);

        public void AssertAbsolutePathCalled(string relative) =>
            Assert.StartsWith(relative, _request?.RequestUri?.AbsolutePath!, StringComparison.OrdinalIgnoreCase);

        public void AssertHttpMethod(HttpMethod method) =>
            Assert.Equal(method, _request.Method);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _request = request;

            var responseMessage = new HttpResponseMessage(SetResponseCode)
            {
                Content = new StringContent(SetResponseBody),
            };

            return await Task.FromResult(responseMessage);
        }
    }
}
