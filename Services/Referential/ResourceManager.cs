using System.Data;
using Dapper;
using JOS.Result;
using models;

namespace Services.Referential
{
    public class ResourceManager(IDbConnection _conn) : IResourceManager
    {
        public async Task<Result<Resource>> Create(string Name)
        {
            var ID = Guid.NewGuid();
            try
            {
                var res = await _conn.ExecuteAsync(
                    @"INSERT INTO resources(id, name)
                VALUES (:id, :name)",
                    new
                    {
                        id = ID,
                        name = Name
                    }
                );

                return Result.Success(new Resource
                {
                    ID = ID,
                    Name = Name
                });
            }
            catch (Exception e)
            {
                return Result.Failure<Resource>(
                    new Error(IReferentialManager.ReferentialError, e.Message));
            }
        }

        public async Task<Result> Delete(Guid resourceID)
        {
            try
            {
                await _conn.ExecuteAsync("DELETE FROM resources WHERE id = :id", new { id = resourceID });
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure<Resource>(
                    new Error(IReferentialManager.ReferentialError, e.Message));
            }
        }

        public async Task<Result<IList<Resource>>> GetAll()
        {
            try
            {
                var res = await _conn.QueryAsync<Resource>(
                    "SELECT id, name FROM resources"
                );
                return Result.Success((IList<Resource>)res.ToList());
            }
            catch (Exception e)
            {
                return Result.Failure<IList<Resource>>(
                                    new Error(IReferentialManager.ReferentialError, e.Message));
            }
        }


        public async Task<Result<Resource>> GetByID(Guid id)
        {
            try
            {
                var res = await _conn.QuerySingleOrDefaultAsync<Resource>(
                    "SELECT id, name FROM resources WHERE id = :id", new { id = id }
                );

                if (res is null)
                    return Result.Failure<Resource>(new Error(IReferentialManager.ReferentialError, $"not found : {id}"));

                return Result.Success(res);
            }
            catch (Exception e)
            {
                return Result.Failure<Resource>(
                    new Error(IReferentialManager.ReferentialError, e.Message));
            }
        }

        public async Task<Result> Update(Resource resource)
        {
            try
            {
                var res = await _conn.ExecuteAsync(
                    @"UPDATE resources SET name = :name
                    WHERE id = :id",
                    new
                    {
                        id = resource.ID,
                        name = resource.Name
                    });
                if (res == 1)
                    return Result.Success();
                return Result.Failure(new Error(IReferentialManager.ReferentialError, $"not found {resource.ID}"));
            }
            catch (Exception e)
            {
                return Result.Failure<Resource>(
                    new Error(IReferentialManager.ReferentialError, e.Message));
            }
        }
    }
}