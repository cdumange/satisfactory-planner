using System.Data;
using System.Text;
using Dapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using JOS.Result;
using Npgsql;

namespace ServicesTests.Integrations
{
    public class DBFixture(string containerName) : IAsyncDisposable
    {
        private DockerClient? _client;
        private string? containerID;
        private IDbConnection? connection = null;

        private DockerClient GetDockerClient()
        {
            if (_client == null)
            {
                _client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
            }

            return _client;
        }

        public async Task<IDbConnection> GetConnection()
        {
            if (connection == null)
            {
                connection = await NpgsqlDataSource
                    .Create($"host=localhost;port=5433;UserName=postgres;password=postgres;Database=satisfactory").OpenConnectionAsync();
            }

            return connection;
        }

        public async Task InitContainer()
        {
            var client = GetDockerClient();

            var container = await client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "postgres:16-alpine",
                Name = containerName,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "5432", new List<PortBinding> { new PortBinding { HostPort = "5433" } } }
                    },
                },
                Healthcheck = new HealthConfig
                {
                    Test = new List<string> { "pg_isready -h localhost -p 5433" },
                    Retries = 5,
                    Timeout = TimeSpan.FromSeconds(1 * 1000000000),
                    Interval = TimeSpan.FromSeconds(2)
                },
                Env = new List<string> { "POSTGRES_PASSWORD=postgres", "POSTGRES_USER=postgres", "POSTGRES_DB=satisfactory" },
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    { "5432", default } // Set any port you want, it just needs to match the one from your configuration
                },
            });
            containerID = container.ID;

            Assert.True(await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters { }));

            Thread.Sleep(8000);

            await MigrationManager.Apply(await GetConnection());
        }


        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                if (containerID != null)
                {
                    await _client.Containers.StopContainerAsync(containerID, new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 1
                    });
                    await _client.Containers.RemoveContainerAsync(containerID, new ContainerRemoveParameters
                    {
                        Force = true
                    });
                }

                _client.Dispose();
            }

            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }

    public abstract class MigrationManager()
    {
        public static async Task<Result> Apply(IDbConnection _conn)
        {
            foreach (var file in Directory.GetFiles("../../../../Migrations/SQL"))
            {
                var data = await File.ReadAllBytesAsync(file);
                var sql = Encoding.UTF8.GetString(data);
                await _conn.ExecuteAsync(sql);
            }
            return Result.Success();
        }
    }
}