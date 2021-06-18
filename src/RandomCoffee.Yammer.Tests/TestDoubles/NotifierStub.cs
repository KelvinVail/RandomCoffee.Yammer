using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Core.Contracts;
using RandomCoffee.Core.Entities;
using RandomCoffee.Yammer.Entities;
using Xunit;

namespace RandomCoffee.Yammer.Tests.TestDoubles
{
    public class NotifierStub : INotifiable
    {
        private List<Match> _notified = new ();

        public async Task Notify(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            _notified = matches.ToList();
        }

        public void AssertIsNotified(long id) =>
            Assert.Contains(_notified, m => m.Persons.Contains(new YammerPerson { YammerId = id }));
    }
}
