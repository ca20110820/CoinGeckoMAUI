using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinGeckoApp.Services
{
    public class SQLiteService
    {
        private readonly string _connectionString;

        public SQLiteService(string databaseFilePath)
        {
            _connectionString = $"Data Source={databaseFilePath}";
        }

        private async Task<SqliteConnection> GetConnectionAsync()
        {
            SqliteConnection connection;
            using (connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();
                return connection;
            }
        }

        public async Task ExecuteNonQueryAsync(string query)
        {
            /* Use-cases:
             * - Create or Delete Table
             * - Insert Data
             * - Update Data
             * - Remove Data
             * - Remove All Data from Table
             */

            SqliteConnection connection = null;
            try
            {
                using (connection = await GetConnectionAsync())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;

                        // Execute the non-query command asynchronously
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync(); // Make sure to close the connection in case of any error
                    await connection.DisposeAsync(); // Dispose of the connection
                }
            }
        }

        public async Task ExecuteSelectDataReaderAsync(string query, Action<SqliteDataReader> readerAction)
        {
            /* Example: (readerAction):
             * ------------------------
             * What to do with the reader and row data.

               Action<SqliteDataReader> readerAction = (reader) =>
                {
                    Console.Write($"ID: {reader.GetInt32(0)}\t");
                    Console.Write($"Name: {reader.GetString(1)}\t");
                    Console.Write($"Age: {reader.GetInt32(2)}");
                    Console.WriteLine("");
                };
             */

            // Reads the whole table and iterate over the rows.

            SqliteConnection connection = null;
            SqliteDataReader reader = null;
            try
            {
                using (connection = await GetConnectionAsync())
                {
                    await connection.OpenAsync();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = query;  // By Default, we select all columns per row
                    using (reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            await Task.Run(() => readerAction(reader));
                        }
                    }
                }
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync(); // Make sure to close the connection in case of any error
                    await connection.DisposeAsync(); // Dispose of the connection
                }
                if (reader != null)
                {
                    await reader.CloseAsync();
                    await reader.DisposeAsync();
                }
            }
        }

        public async Task ExecuteTableDataReaderAsync(string tableName, Action<SqliteDataReader> readerAction)
        {
            await ExecuteSelectDataReaderAsync($"SELECT * FROM {tableName};", readerAction);
        }

        public async Task DeleteTableAsync(string tableName)
        {
            string query = $"DROP TABLE IF EXISTS {tableName}";
            await ExecuteNonQueryAsync(query);
        }

        public async Task CreateTableAsync(string tableName, params string[] columns)
        {
            /* Examples:
             * columns = ["id INTEGER PRIMARY KEY AUTOINCREMENT", "username TEXT NOT NULL", "email TEXT NOT NULL"]
             * Concatenated into "id INTEGER PRIMARY KEY AUTOINCREMENT,username TEXT NOT NULL,email TEXT NOT NULL"
             */

            string cols = string.Join(",", columns);
            string query = "";
            query += $"CREATE TABLE IF NOT EXISTS {tableName} ({cols});";
            await ExecuteNonQueryAsync(query);
        }

        public async Task RemoveAllDataFromTableAsync(string tableName)
        {
            await ExecuteNonQueryAsync($"DELETE FROM {tableName}");
        }
    }
}
