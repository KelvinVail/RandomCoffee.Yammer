using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Yammer.Contracts;
using Xunit;

namespace RandomCoffee.Yammer.Tests.TestDoubles
{
    public class YammerServiceStub : IYammerService
    {
        private readonly List<long> _userIds = new () { 999, 998 };
        private readonly Dictionary<long, List<string>> _posts = new ();
        private long _groupId;

        public async Task<IEnumerable<long>> GetActiveUserIds(long groupId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);

            _groupId = groupId;
            return _userIds;
        }

        public async Task PostToGroup(long groupId, string post, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);

            if (!_posts.ContainsKey(groupId))
                _posts.Add(groupId, new List<string>());

            _posts[groupId].Add(post);
        }

        public void AddUserId(long id) =>
            _userIds.Add(id);

        public void AssertGroupIdReceived(long id) =>
            Assert.Equal(id, _groupId);

        public void AssertPosted(long groupId, string post) =>
            Assert.Contains(post, _posts[groupId]);
    }
}
