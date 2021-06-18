using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RandomCoffee.Yammer.Contracts
{
    public interface IYammerService
    {
        Task<IEnumerable<long>> GetActiveUserIds(long groupId, CancellationToken cancellationToken = default);

        Task PostMessageToGroup(long groupId, string message, CancellationToken cancellationToken = default);
    }
}
