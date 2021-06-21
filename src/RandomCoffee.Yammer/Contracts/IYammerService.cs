using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RandomCoffee.Yammer.Contracts
{
    public interface IYammerService
    {
        Task<IEnumerable<long>> GetActiveUserIds(long groupId, CancellationToken cancellationToken = default);

        Task PostToGroup(long groupId, string post, CancellationToken cancellationToken = default);
    }
}
