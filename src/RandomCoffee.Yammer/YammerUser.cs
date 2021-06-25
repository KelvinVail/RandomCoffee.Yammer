using RandomCoffee.Core.Entities;

namespace RandomCoffee.Yammer
{
    public record YammerUser : Person
    {
        public long Id { get; init; }
    }
}
