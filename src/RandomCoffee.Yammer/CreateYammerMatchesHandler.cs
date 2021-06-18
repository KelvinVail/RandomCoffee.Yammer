using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Core;
using RandomCoffee.Core.Contracts;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Contracts;
using RandomCoffee.Yammer.Entities;

namespace RandomCoffee.Yammer
{
    public class CreateYammerMatchesHandler
    {
        private readonly IYammerService _yammer;
        private readonly CreateMatchesHandler _matchMaker;

        public CreateYammerMatchesHandler(INotifiable notifier, ISorter sorter, IYammerService yammer)
        {
            _yammer = yammer;
            _matchMaker = new CreateMatchesHandler(notifier, sorter);
        }

        public async Task Handle(CreateYammerMatchesCommand request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new BadRequestException("'Request' must not be empty.");

            if (request.GroupId == 0)
                throw new BadRequestException("'Group Id' must not be '0'.");

            var ids = await _yammer.GetActiveUserIds(request.GroupId, cancellationToken);
            var users = ids.Select(id => new YammerPerson {YammerId = id});

            await _matchMaker.Handle(new CreateMatchesCommand { Persons = users }, cancellationToken);
        }
    }
}
