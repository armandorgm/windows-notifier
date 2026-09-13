using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Npgsql;
using Dapper;
using WindowsNotifier.Models;

namespace WindowsNotifier.Services
{
    public class DatabaseService
    {
        private readonly AppSettings _settings;

        public DatabaseService(AppSettings settings)
        {
            _settings = settings;
        }

        private IDbConnection CreateConnection()
        {
            if (_settings.DbType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                return new NpgsqlConnection(_settings.ConnectionString);
            }
            return new SqliteConnection(_settings.ConnectionString);
        }


        public double? GetEntryPriceForTakeProfit(string tpOrderId, string entryOrderId)
        {
            using var connection = CreateConnection();
            connection.Open();

            // Buscamos el precio de entrada del pipeline asignado a esta orden de salida (TP)
            const string query = @"
                SELECT entry_price 
                FROM bot_pipeline_processes 
                WHERE exit_order_id = @TpOrderId 
                   OR entry_order_id = @EntryOrderId 
                LIMIT 1";

            try
            {
                return connection.QueryFirstOrDefault<double?>(query, new { TpOrderId = tpOrderId, EntryOrderId = entryOrderId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error consultando DB: {ex.Message}");
                return null;
            }
        }
    }
}
