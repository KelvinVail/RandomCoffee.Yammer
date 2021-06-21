using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RandomCoffee.Core.Contracts;
using RandomCoffee.Core.Entities;
using RandomCoffee.Core.Exceptions;
using RandomCoffee.Yammer.Contracts;
using RandomCoffee.Yammer.Entities;

namespace RandomCoffee.Yammer.Services
{
    public class YammerNotifier : INotifiable
    {
        private readonly IYammerService _yammer;
        private readonly long _groupId;

        public YammerNotifier(IYammerService yammer, long groupId)
        {
            _yammer = yammer;
            _groupId = groupId;
        }

        public async Task Notify(IEnumerable<Match> matches, CancellationToken cancellationToken = default)
        {
            if (_groupId == 0) throw new BadRequestException("'Group Id' must not be '0'.");
            if (matches is null) return;
            var matchesList = matches.ToList();
            if (!matchesList.Any()) return;

            var lines = matchesList.Select(
                match => YammerMatch(match).Select(Persons)).Select(Format);

            await _yammer.PostToGroup(_groupId, string.Join(Environment.NewLine, lines), cancellationToken);
        }

        private static IEnumerable<YammerPerson> YammerMatch(Match match) =>
            match.Persons.Cast<YammerPerson>();

        private static string Persons(YammerPerson person)
        {
            if (person.YammerId == 0)
                throw new BadRequestException("'Yammer Id' must not be '0'.");

            return $"[[{person.YammerId}]]";
        }

        private static string Format(IEnumerable<string> persons) =>
            string.Join(" and ", persons) + ".";
    }
}
