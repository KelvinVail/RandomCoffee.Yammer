using System;
using System.Collections.Generic;
using System.Linq;
using RandomCoffee.Core.Entities;

namespace RandomCoffee.Yammer
{
    public class PostFormatter
    {
        public PostFormatter(string title) =>
            Title = title;

        public string Title { get; }

        public static string Format(IEnumerable<Match> matches) =>
            matches.Aggregate(
                $"Matches for {DateTime.UtcNow:dddd, dd MMMM yyyy}.{Environment.NewLine}{Environment.NewLine}",
                (current, match) => current + (
                    string.Join(" and ", match.Persons.Cast<YammerUser>().Select(x => $"[[{x.Id}]]"))
                    + "." + Environment.NewLine));
    }
}
