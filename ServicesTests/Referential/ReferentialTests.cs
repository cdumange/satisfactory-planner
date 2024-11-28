
using System.Data;
using models;
using Services.Referential;
using ServicesTests.Integrations;
using ServicesTests.Utils;

namespace ServicesTests;

public class ReferentialTests : IAsyncLifetime
{
    private DBFixture db;

    public async Task DisposeAsync()
    {
        if (db is not null)
        {
            await db.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        db = new DBFixture("testpostgres");
        await db.InitContainer();
    }

    [Fact]
    public async Task TestResourceCreation()
    {
        var s = new ResourceManager(await db.GetConnection());
        var r = await s.Create("test");
        Assert.Succeeded(r);

        var e = await s.GetByID(r.Data.ID);
        Assert.Succeeded(e);
        Assert.Equal(r.Data, e.Data);
    }

    [Fact]
    public async Task TestRecipeCreation()
    {
        var s = new ResourceManager(await db.GetConnection());
        var i = new RecipeManager(await db.GetConnection());

        // Given
        var input = await s.Create("iron");
        Assert.Succeeded(input);
        var output = await s.Create("screws");
        Assert.Succeeded(output);

        // When
        var recipe = await i.Create(
            "screws",
            new Ingredient[] {
                new Ingredient {
                    Quantity = 20,
                    Resource = input.Data,
                }
            }.AsEnumerable(),
            output.Data,
            1);
        Assert.Succeeded(recipe);

        // Then

        var ex = await i.GetByID(recipe.Data.ID, true);
        Assert.Succeeded(ex);

        Assert.Equal(20, ex.Data.Input[0].Quantity);
    }

    [Fact]
    public async Task TestRecipeGetAll()
    {
        var s = new ResourceManager(await db.GetConnection());
        var i = new RecipeManager(await db.GetConnection());

        // Given
        var iron = await s.Create("iron");
        Assert.Succeeded(iron);
        var ironTube = await s.Create("ironTube");
        Assert.Succeeded(ironTube);
        var steel = await s.Create("steel");
        Assert.Succeeded(steel);
        var screw = await s.Create("screw");
        Assert.Succeeded(screw);

        // When

        var ironScrew = await i.Create(
            "IronScrew",
            [
                new Ingredient { Quantity = 20, Resource = iron.Data },
                new Ingredient { Quantity = 5, Resource = ironTube.Data },
            ],
            screw.Data,
            1
        );
        Assert.Succeeded(ironScrew);

        var steelScrew = await i.Create(
            "SteelScrew",
            [new Ingredient { Quantity = 10, Resource = steel.Data }],
            screw.Data,
            1
        );
        Assert.Succeeded(steelScrew);

        var recipes = await i.GetAll();
        Assert.Succeeded(recipes);
        Assert.Equal(2, recipes.Data.Count);

        // Then
        Assert.Contains(ironScrew.Data.Name, recipes.Data.Select(x => x.Name));
    }
}