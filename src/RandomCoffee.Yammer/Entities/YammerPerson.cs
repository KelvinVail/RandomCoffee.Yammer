using RandomCoffee.Core.Entities;

namespace RandomCoffee.Yammer.Entities
{
    public record YammerPerson : Person
    {
        public long YammerId { get; init; }
    }
}
