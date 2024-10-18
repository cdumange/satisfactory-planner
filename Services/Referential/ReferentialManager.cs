using System.Data;
using models;

namespace Services.Referential;

public class ReferentialManager : IReferentialManager
{
    public ReferentialManager(IDbConnection conn)
    {
        Resources = new ResourceManager(conn);
        Recipes = new RecipeManager(conn);
    }
    public IResourceManager Resources { get; }

    public List<Builder> Builders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IRecipeManager Recipes { get; }
}