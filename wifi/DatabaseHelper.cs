using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace wifi.Helpers
{
    /// <summary>
    /// Класс-помощник для взаимодействия с базой данных MySQL.
    /// Содержит методы для выполнения запросов (SELECT, INSERT, UPDATE, DELETE)
    /// и удобного добавления параметров в команды.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Строка подключения к базе данных,
        /// загружается из файла конфигурации (App.config).
        /// </summary>
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;

        /// <summary>
        /// Создаёт и возвращает новое соединение с базой данных.
        /// </summary>
        /// <returns>Объект <see cref="MySqlConnection"/> для выполнения SQL-запросов.</returns>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Выполняет SQL-запрос SELECT и возвращает результаты в виде <see cref="DataTable"/>.
        /// </summary>
        /// <param name="query">SQL-запрос SELECT.</param>
        /// <param name="parameters">Словарь параметров запроса (опционально).</param>
        /// <returns>Таблица с результатами выполнения запроса.</returns>
        public static DataTable ExecuteSelect(string query, Dictionary<string, object>? parameters = null)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    AddParameters(cmd, parameters);
                    using (var adapter = new MySqlDataAdapter(cmd))
                        adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Выполняет SQL-запрос, который возвращает одно значение
        /// (например, результат COUNT или MAX).
        /// </summary>
        /// <param name="query">SQL-запрос.</param>
        /// <param name="parameters">Параметры запроса (опционально).</param>
        /// <returns>Первое значение из результата запроса.</returns>
        public static object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    AddParameters(cmd, parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Выполняет SQL-команду, не возвращающую результаты —
        /// например INSERT, UPDATE или DELETE.
        /// </summary>
        /// <param name="query">SQL-запрос.</param>
        /// <param name="parameters">Параметры запроса (опционально).</param>
        /// <returns>Количество затронутых строк.</returns>
        public static int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    AddParameters(cmd, parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Добавляет параметры в SQL-команду с автоматическим определением типов данных.
        /// </summary>
        /// <param name="cmd">SQL-команда <see cref="MySqlCommand"/>.</param>
        /// <param name="parameters">Словарь параметров и их значений.</param>
        private static void AddParameters(MySqlCommand cmd, Dictionary<string, object>? parameters)
        {
            if (parameters == null) return;

            foreach (var kv in parameters)
            {
                string name = kv.Key.StartsWith("@") ? kv.Key : "@" + kv.Key;
                object val = kv.Value ?? DBNull.Value;

                // Определение типа параметра
                if (val is int i)
                {
                    cmd.Parameters.Add(name, MySqlDbType.Int32).Value = i;
                }
                else if (val is decimal dec)
                {
                    var p = new MySqlParameter(name, MySqlDbType.Decimal)
                    {
                        Precision = 10,
                        Scale = 2,
                        Value = dec
                    };
                    cmd.Parameters.Add(p);
                }
                else if (val is double db)
                {
                    var p = new MySqlParameter(name, MySqlDbType.Decimal)
                    {
                        Precision = 10,
                        Scale = 2,
                        Value = Convert.ToDecimal(db)
                    };
                    cmd.Parameters.Add(p);
                }
                else
                {
                    cmd.Parameters.Add(name, MySqlDbType.VarChar).Value = val;
                }
            }
        }
    }
}
