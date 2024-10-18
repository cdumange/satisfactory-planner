using System.Data;
using System.Data.Common;
using Dapper;
using JOS.Result;
using Microsoft.VisualBasic;
using models;

namespace Services.Referential;

public class RecipeManager : IRecipeManager
{
    private readonly IDbConnection _conn;

    public RecipeManager(IDbConnection conn)
    {
        _conn = conn;
    }
    public async Task<Result<Recipe>> Create(
        string Name,
        IEnumerable<Ingredient> ingredients,
        Resource output)
    {
        var recipe = new Recipe
        {
            ID = Guid.NewGuid(),
            Name = Name,
            Output = output
        };

        try
        {
            _conn.Open();
            using var tr = _conn.BeginTransaction();
            var res = await _conn.ExecuteAsync(@"
                INSERT INTO recipes(id, name, output_id)
                VALUES(:id, :name, :output)", new
            {
                id = recipe.ID,
                name = Name,
                output = output.ID
            });

            var ig = new List<Ingredient>();
            var args = ingredients.Select(x =>
            {
                ig.Add(new Ingredient
                {
                    Quantity = x.Quantity,
                    Resource = x.Resource,
                });

                return new
                {
                    resource = x.Resource.ID,
                    recipe = recipe.ID,
                    qt = x.Quantity
                };
            });

            await _conn.ExecuteAsync(@"
                    INSERT INTO ingredients (resource_id, recipe_id, quantity)
                    VALUES (:resource, :recipe, :qt);
                ", args);

            tr.Commit();
            return Result.Success(recipe with { Input = ig });
        }
        catch (Exception e)
        {
            return Result.Failure<Recipe>(new Error(IReferentialManager.ReferentialError, e.Message));
        }

    }

    public async Task<Result> Delete(Guid recipeID)
    {
        try
        {
            await _conn.ExecuteAsync(@"
                DELETE FROM recipes WHERE id = :id", new { id = recipeID });
        }
        catch (Exception e)
        {
            return Result.Failure<Recipe>(new Error(IReferentialManager.ReferentialError, e.Message));
        }
        return Result.Success();
    }

    public async Task<Result<IList<Recipe>>> GetAll()
    {
        var res = await _conn.QueryAsync<(Guid id, string name,
        Guid output_id, string output_name,
        Guid resource_id, string resource_name, int quantity)>(@"
            SELECT r.id, r.name, r.output_id, r1.name as output_name , i.resource_id, r2.name as resource_name, i.quantity
            FROM recipes r
            INNER JOIN ingredients i
                ON i.recipe_id = r.id
            INNER JOIN resources r1
                on r.output_id = r1.id
            INNER JOIN resources r2
                on i.resource_id = r2.id
            ORDER BY r.id
        ");


        var recipes = new List<Recipe>();
        Recipe? currentRecipe = null;

        foreach (var (id, name, output_id, output_name, resource_id, resource_name, quantity) in res)
        {
            if (currentRecipe is null || currentRecipe.ID != id)
            {
                currentRecipe = new Recipe
                {
                    ID = id,
                    Name = name,
                    Output = new Resource
                    {
                        ID = output_id,
                        Name = output_name
                    },
                    Input = new List<Ingredient>()
                };
                recipes.Add(currentRecipe);
            }

            currentRecipe.Input.Add(new Ingredient
            {
                Quantity = quantity,
                Resource = new Resource
                {
                    ID = resource_id,
                    Name = resource_name,
                }
            });

        }

        return Result.Success((IList<Recipe>)recipes);
    }

    public async Task<Result<Recipe>> GetByID(Guid id, bool hydrate = false)
    {
        var res = await _conn.QueryFirstAsync<PartialRecipe>("SELECT id, name FROM recipes WHERE id = :id", new { id });
        if (!hydrate)
            return Result.Success(new Recipe
            {
                ID = res.ID,
                Name = res.Name,
                Output = null,
                Input = null
            });

        return await FillRecipe(res);
    }

    private async Task<Result<Recipe>> FillRecipe(PartialRecipe recipe)
    {
        var ingredients = await _conn.QueryAsync<(Guid id, string name, int quantity)>(@"
        SELECT r.id, r.name, i.quantity
        FROM ingredients i
        INNER JOIN resources r
            on r.id = i.resource_id
        WHERE i.recipe_id = :id", new { id = recipe.ID });

        var output = await _conn.QuerySingleAsync<Resource>(@"SELECT r.id, r.name 
            FROM resources r
            INNER JOIN recipes r1
                ON r1.output_id = r.id
            WHERE r1.id = :id", new { id = recipe.ID });

        return Result<Recipe>.Success(new Recipe
        {
            ID = recipe.ID,
            Name = recipe.Name,
            Output = output,
            Input = ingredients.Select(x => new Ingredient
            {
                Resource = new Resource
                {
                    ID = x.id,
                    Name = x.name,
                },
                Quantity = x.quantity
            }).ToList()
        });
    }

    public async Task<Result<IEnumerable<Recipe>>> RecipesWith(Resource resource)
    {
        var recipes = await _conn.QueryAsync<PartialRecipe>(@"
            SELECT id, name 
            FROM recipes r
            INNER JOIN ingredients i
                on i.recipe_id = r.id
            WHERE i.resource_id = :id", new { id = resource.ID });

        if (!recipes.Any())
        {
            return Result.Success(Array.Empty<Recipe>().AsEnumerable());
        }

        var tasks = recipes.Select(x => FillRecipe(x)).ToArray();
        Task.WaitAll(tasks);

        return Result.Success(tasks.Select(x => x.Result.Data));
    }

    public async Task<Result> Update(Recipe resource)
    {
        await _conn.ExecuteAsync(@"
            UPDATE recipes
                set name = :name, ouput_id = :output
            WHERE id = :id", new { id = resource.ID, name = resource.Name, output = resource.Output.ID });
        return Result.Success();
    }
}