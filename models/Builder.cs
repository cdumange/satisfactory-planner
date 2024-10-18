using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace models
{
    public record Builder
    {
        public Guid ID { get; init; }
        public string Name { get; init; }
        protected int nbIngredient { get; init; }

        public List<Ingredient> CurrentQuantity { get; init; } = new List<Ingredient>();
        public Recipe? CurrentRecipe { get; set; }
    }
}