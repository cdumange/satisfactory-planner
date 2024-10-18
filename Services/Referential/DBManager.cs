
using System.Data;
using Dapper;
using models;

namespace Services.Referential
{
    public class DBManager(IDbConnection _conn) : IReferentialManager
    {
        public IResourceManager Resources => throw new NotImplementedException();

        public List<Builder> Builders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IRecipeManager Recipes => throw new NotImplementedException();
    }
}