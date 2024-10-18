using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using JOS.Result;
using models;

namespace Services.Referential;


public interface IReferentialManager
{
    public const string ReferentialError = "referential";
    IResourceManager Resources { get; }
    List<Builder> Builders { get; set; }
    IRecipeManager Recipes { get; }
}

public interface IResourceManager
{
    public const string ReferentialError = "referential";
    Task<Result<Resource>> Create(string Name);
    Task<Result> Update(Resource resource);
    Task<Result> Delete(Guid resourceID);
    Task<Result<Resource>> GetByID(Guid id);
    Task<Result<IList<Resource>>> GetAll();
}

public interface IRecipeManager
{
    Task<Result<Recipe>> Create(string Name,
        IEnumerable<Ingredient> ingredients,
        Resource output);
    Task<Result> Update(Recipe resource);
    Task<Result> Delete(Guid recipeID);
    Task<Result<Recipe>> GetByID(Guid id, bool hydrate);
    Task<Result<IList<Recipe>>> GetAll();

    Task<Result<IEnumerable<Recipe>>> RecipesWith(Resource resource);
}