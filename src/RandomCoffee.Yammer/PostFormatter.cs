using System;
using System.Collections.Generic;
using System.Linq;
using RandomCoffee.Core.Entities;

namespace RandomCoffee.Yammer
{
    public class PostFormatter
    {
        private readonly string _groupName;

        public PostFormatter(string groupName) =>
            _groupName = groupName;

        public string Title =>
            $"{_groupName} matches, {DateTime.UtcNow:MMMM yyyy}.";

        public static string Format(IEnumerable<Match> matches) =>
            matches.Aggregate(
                string.Empty,
                (current, match) => current + (
                    string.Join(" and ", match.Persons.Cast<YammerUser>().Select(x => $"[[user:{x.Id}]]"))
                    + "." + Environment.NewLine));
    }
}
