using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JOS.Result;
using Npgsql.Replication;

namespace ServicesTests.Utils
{
    public partial class Assert : Xunit.Assert
    {
        public static void Succeeded(Result result)
        {
            True(result.Succeeded);
        }

        public static void Succeeded<T>(Result<T> result)
        {
            True(result.Succeeded);
        }

        public static void Failed(Result result)
        {
            False(result.Succeeded);
        }

        public static void Failed<T>(Result<T> result)
        {
            False(result.Succeeded);
        }
    }
}