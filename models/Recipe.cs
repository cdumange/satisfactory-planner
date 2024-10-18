using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace models
{
    public partial record PartialRecipe
    {
        public required Guid ID { get; init; }
        public required string Name { get; init; }
    }
    public record Recipe : PartialRecipe
    {
        public List<Ingredient> Input { get; init; } = new List<Ingredient>();
        public required Resource Output { get; init; }
    }

    public record Ingredient
    {
        public required Resource Resource { get; init; }
        public required int Quantity { get; init; }
    }

}


