using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RandomCoffee.Yammer
{
    [DataContract]
    public record YammerUsers
    {
        public IEnumerable<YammerUser> Users { get; init; }

        [DataMember(Name = "more_available")]
        public bool MoreAvailable { get; set; }
    }
}
