using RandomCoffee.Core.Entities;

namespace RandomCoffee.Yammer
{
    public record YammerUser : Person
    {
        public long Id { get; init; }

        public string State { get; init; }

        public string Name
        {
            get
            {
                return FullName;
            }

            init
            {
                FullName = value;
            }
        }
    }
}
