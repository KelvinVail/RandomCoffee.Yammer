using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        internal static IList<string> Format(IEnumerable<Match> matches)
        {
            var posts = new List<string>();
            var sb = new StringBuilder();
            foreach (var match in matches)
            {
                foreach (var person in match.Persons)
                {
                    if (!IsFirstPerson(match, person))
                        sb.Append(" and ");

                    var yammerUser = person as YammerUser;
                    sb.Append($"[[user:{yammerUser!.Id}]]");
                }

                sb.Append('.');
                sb.Append(Environment.NewLine);

                if (sb.Length <= 9500)
                    continue;

                posts.Add(sb.ToString());
                sb = new StringBuilder();
            }

            posts.Add(sb.ToString());
            return posts;
        }

        private static bool IsFirstPerson(Match match, Person person) =>
            match.Persons.First().FullName == person.FullName;
    }
}
