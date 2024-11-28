using JOS.Result;
using models;
namespace Services.Referential;


public class InMemoryManager : IReferentialManager
{
    public IResourceManager Resources { get; } = new InMemoryResource();
    public List<Builder> Builders { get; set; } = new List<Builder>();
    public IRecipeManager Recipes { get; } = new InMemoryRecipe();

}

public class InMemoryResource : IResourceManager
{
    private readonly IList<Resource> resources = new List<Resource>();
    public Task<Result<Resource>> Create(string Name)
    {
        if (resources.Any(x => x.Name == Name))
        {
            return Task.FromResult(Result.Failure<Resource>(new Error(IReferentialManager.ReferentialError, "resource already exists")));
        }
        var r = new Resource
        {
            ID = Guid.NewGuid(),
            Name = Name
        };

        resources.Add(r);

        return Task.FromResult(Result.Success(r));
    }

    public Task<Result> Delete(Guid resourceID)
    {
        for (var n = 0; n < resources.Count; n++)
        {
            if (resources[n].ID == resourceID)
            {
                resources.Remove(resources[n]);
                return Task.FromResult(Result.Success());
            }
        }
        return Task.FromResult(Result.Success());
    }

    public Task<Result<IList<Resource>>> GetAll()
    {
        return Task.FromResult(Result.Success(resources));
    }

    public Task<Result<Resource>> GetByID(Guid id)
    {
        var res = resources.First(x => x.ID == id);
        if (res is null)
            return Task.FromResult(
                Result.Failure<Resource>(
                    new Error(
                        IReferentialManager.ReferentialError,
                        $"resource not found: {id}"
                    )
                )
            );

        return Task.FromResult(Result.Success(res));
    }

    public Task<Result> Update(Resource resource)
    {
        for (var n = 0; n < resources.Count; n++)
        {
            if (resources[n].ID == resource.ID)
            {
                resources[n] = resource;
                return Task.FromResult(Result.Success());
            }
        }

        return Task.FromResult(Result.Failure(
            new Error(IReferentialManager.ReferentialError, $"resource not found: {resource.ID}")));
    }
}

public class InMemoryRecipe : IRecipeManager
{
    private readonly IList<Recipe> recipes = new List<Recipe>();
    public Task<Result<Recipe>> Create(string Name,
        IEnumerable<Ingredient> ingredients,
        Resource output, int quantity)
    {
        if (recipes.Any(x => x.Name == Name))
        {
            return Task.FromResult(Result.Failure<Recipe>(new Error(IReferentialManager.ReferentialError, "resource already exists")));
        }
        var r = new Recipe
        {
            ID = Guid.NewGuid(),
            Name = Name,
            Output = output,
            Quantity = quantity,
            Input = ingredients.ToList(),
        };

        recipes.Add(r);

        return Task.FromResult(Result.Success(r));
    }

    public Task<Result> Delete(Guid resourceID)
    {
        for (var n = 0; n < recipes.Count; n++)
        {
            if (recipes[n].ID == resourceID)
            {
                recipes.Remove(recipes[n]);
                return Task.FromResult(Result.Success());
            }
        }
        return Task.FromResult(Result.Success());
    }

    public Task<Result<IList<Recipe>>> GetAll()
    {
        return Task.FromResult(Result.Success(recipes));
    }

    public Task<Result<Recipe>> GetByID(Guid id, bool hydrate)
    {
        var res = recipes.First(x => x.ID == id);
        if (res is null)
            return Task.FromResult(
                Result.Failure<Recipe>(
                    new Error(
                        IReferentialManager.ReferentialError,
                        $"resource not found: {id}"
                    )
                )
            );

        return Task.FromResult(Result.Success(res));
    }

    public Task<Result<IEnumerable<Recipe>>> RecipesWith(Resource resource)
    {
        return Task.FromResult(Result.Success(recipes.Where(x => x.Input.Any(y => y.Resource.ID == resource.ID))));
    }

    public Task<Result> Update(Recipe recipe)
    {
        for (var n = 0; n < this.recipes.Count; n++)
        {
            if (this.recipes[n].ID == recipe.ID)
            {
                this.recipes[n] = recipe;
                return Task.FromResult(Result.Success());
            }
        }

        return Task.FromResult(Result.Failure(
            new Error(IReferentialManager.ReferentialError, $"resource not found: {recipe.ID}")));
    }
}
