
using models;
using Services.Referential;

namespace webapp.Views.Referential
{
    public class ReferentialModel
    {
        public IEnumerable<Resource> Resources { get; init; } = new List<Resource>();
        public IEnumerable<Recipe> Recipes { get; init; } = new List<Recipe>();

        public static async Task<ReferentialModel> Initialize(IReferentialManager referential)
        {
            var recipes = await referential.Recipes.GetAll();
            var resources = await referential.Resources.GetAll();

            if (recipes.Failed)
            {
                throw new Exception("failed to load recipes : " + recipes.Error);
            }

            if (resources.Failed)
            {
                throw new Exception("failed to load resources : " + resources.Error);
            }

            return new ReferentialModel
            {
                Recipes = recipes.Data,
                Resources = resources.Data,
            };
        }
    }
}