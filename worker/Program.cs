using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;

namespace Worker
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
                var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
                var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
                var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");

                var connectionString = $"Host={dbHost};Username={dbUser};Password={dbPassword};Database={dbName};Include Error Detail=true;";
                var pgsql = OpenDbConnection(connectionString);
                var redisConn = OpenRedisConnection("redis");
                var redis = redisConn.GetDatabase();

                var keepAliveCommand = pgsql.CreateCommand();
                keepAliveCommand.CommandText = "SELECT 1";

                var definition = new { vote = "", voter_id = "" };
                while (true)
                {
                    Thread.Sleep(100);

                    // Se reconnecter à Redis si la connexion est perdue
                    if (redisConn == null || !redisConn.IsConnected)
                    {
                        Console.WriteLine("Reconnecting Redis");
                        redisConn = OpenRedisConnection("redis");
                        redis = redisConn.GetDatabase();
                    }
                    string json = redis.ListLeftPopAsync("votes").Result;
                    if (json != null)
                    {
                        var vote = JsonConvert.DeserializeAnonymousType(json, definition);
                        Console.WriteLine($"Processing vote for '{vote.vote}' by '{vote.voter_id}'");

                        // Se reconnecter à PostgreSQL si la connexion est perdue
                        if (!pgsql.State.Equals(System.Data.ConnectionState.Open))
                        {
                            Console.WriteLine("Reconnecting DB");
                            pgsql = OpenDbConnection("Host=postgres;Username=postgres;Password=postgres;Database=postgres;");
                        }
                        else
                        {
                            UpdateVote(pgsql, vote.voter_id, vote.vote);
                        }
                    }
                    else
                    {
                        keepAliveCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static NpgsqlConnection OpenDbConnection(string connectionString)
        {
            NpgsqlConnection connection;

            while (true)
            {
                // try
                // {
                //     connection = new NpgsqlConnection(connectionString);
                //     connection.Open();
                //     break;
                // }
                // catch (SocketException)
                // {
                //     Console.Error.WriteLine("Waiting for db");
                //     Thread.Sleep(1000);
                // }
                // catch (DbException)
                // {
                //     Console.Error.WriteLine("Waiting for db dbexception");
                //     Thread.Sleep(1000);
                // }
                try
                {
                    Console.WriteLine("Attempting to connect to database...");
                    connection = new NpgsqlConnection(connectionString);
                    connection.Open();
                    Console.WriteLine("Connected to database");
                    break;
                }
                catch (SocketException)
                {
                    Console.Error.WriteLine("Waiting for DB connection...");
                    Thread.Sleep(1000);
                }
                catch (DbException ex)
                {
                    Console.Error.WriteLine($"DB exception: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }

            Console.Error.WriteLine("Connected to db");

            var command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS votes (
                                        id VARCHAR(255) NOT NULL UNIQUE,
                                        vote VARCHAR(255) NOT NULL
                                    )";
            command.ExecuteNonQuery();

            return connection;
        }

        private static ConnectionMultiplexer OpenRedisConnection(string hostname)
        {
            // Use IP address to workaround https://github.com/StackExchange/StackExchange.Redis/issues/410
            var ipAddress = GetIp();
            Console.WriteLine($"Found redis at {ipAddress}");

            while (true)
            {
                try
                {
                    Console.Error.WriteLine("Connecting to redis");
                    return ConnectionMultiplexer.Connect(ipAddress);
                }
                catch (RedisConnectionException)
                {
                    Console.Error.WriteLine("Waiting for redis");
                    Thread.Sleep(1000);
                }
            }
        }

        // private static string GetIp(string hostname)
        //     => Dns.GetHostEntryAsync(hostname)
        //         .Result
        //         .AddressList
        //         .First(a => a.AddressFamily == AddressFamily.InterNetwork)
        //         .ToString();
        private static string GetIp()
        {
            return "redis";  // Retourner directement le nom du service
        }

        private static void UpdateVote(NpgsqlConnection connection, string voterId, string vote)
        {
            var command = connection.CreateCommand();
            try
            {
                command.CommandText = "INSERT INTO votes (id, vote) VALUES (@id, @vote)";
                command.Parameters.AddWithValue("@id", voterId);
                command.Parameters.AddWithValue("@vote", vote);
                command.ExecuteNonQuery();
            }
            catch (DbException)
            {
                command.CommandText = "UPDATE votes SET vote = @vote WHERE id = @id";
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Dispose();
            }
        }
    }
}