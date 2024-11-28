using System.Runtime.Versioning;
using models;

namespace webapp.Models;

public class RecipeEditionModel
{
    public Recipe Recipe { get; init; }
    public IEnumerable<Resource> Resources { get; init; }
}