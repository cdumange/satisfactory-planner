using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using models;
using Services.Referential;
using webapp.Views.Referential;

namespace webapp.Controllers
{
    public record CreationRequest(string name);

    public record RecipeRequest(
        string name,
        List<string> resource,
        List<int> quantity,
        Guid output
    );

    public class ReferentialController(
        IReferentialManager referential,
        ILogger<ReferentialController> logger
    ) : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View("Referential", await ReferentialModel.Initialize(referential));
        }

        public async Task<IActionResult> AddResource([FromForm] CreationRequest resource)
        {
            var res = await referential.Resources.Create(resource.name);
            if (res.Failed)
            {
                return BadRequest(res.Error);
            }

            return PartialView("Resource", res.Data);
        }

        [HttpPut("/referential/resource/{id}")]
        public async Task<IActionResult> UpdateResource([FromForm] string name, Guid id)
        {
            var resource = new Resource { ID = id, Name = name };
            var res = await referential.Resources.Update(resource);
            if (res.Failed)
            {
                return BadRequest(res.Error);
            }

            return PartialView("Resource", resource);
        }

        [HttpDelete("/referential/resource/{id}")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var res = await referential.Resources.Delete(id);
            if (res.Failed)
            {
                return BadRequest(res.Error);
            }

            return Ok();
        }

        [HttpGet("/referential/recipes")]
        public async Task<IActionResult> GetRecipes()
        {
            return PartialView("Recipes", await ReferentialModel.Initialize(referential));
        }

        public async Task<IActionResult> AddRecipe([FromForm] RecipeRequest request)
        {
            if (request.name == null
            || request.quantity.Count == 0
            || request.quantity.Count != request.resource.Count
            || request.name.Length == 0
            )
            {
                return BadRequest();
            }

            var ingredients = new List<Ingredient>();
            for (var n = 0; n < request.quantity.Count; n++)
            {
                ingredients.Add(new Ingredient
                {
                    Quantity = request.quantity[n],
                    Resource = new Resource
                    {
                        ID = Guid.Parse(request.resource[n]),
                        Name = string.Empty // no need for the name to create te recipe
                    }
                });
            }


            var res = await referential.Recipes.Create(request.name, ingredients, new Resource { ID = request.output, Name = string.Empty } /* TODO */);
            if (res.Failed)
            {
                return BadRequest(res.Error);
            }

            return PartialView("Recipe", res.Data);
        }

        [HttpDelete("/referential/recipe/{id}")]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            var res = await referential.Recipes.Delete(id);
            if (res.Failed)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}