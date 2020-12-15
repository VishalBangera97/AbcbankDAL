using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace BankDAL
{
    public static class StoredProcedure
    {

        public static IEnumerable<T> ExecuteStoredProcedureWithResult<T>(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            var commandAndConnection = GetSqlCommandAndConnection(sqlParameters, storedProcedureName);
            using (SqlConnection connection = commandAndConnection.connection)
            using (SqlCommand command = commandAndConnection.command)
            using (SqlDataReader dataReader = command.ExecuteReader())
                while (dataReader.Read())
                {
                    var item = Activator.CreateInstance<T>();
                    foreach (var property in typeof(T).GetProperties())
                        if (!dataReader.IsDBNull(dataReader.GetOrdinal(property.Name)))
                        {
                            Type convertTo = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            property.SetValue(item, Convert.ChangeType(dataReader[property.Name], convertTo), null);
                        }
                    yield return item;
                }
        }

        public static void ExecuteStoredProcedure(string storedProcedureName, List<SqlParameter> sqlParameters)
        {
            var commandAndConnection = GetSqlCommandAndConnection(sqlParameters, storedProcedureName);
            using (SqlConnection connection = commandAndConnection.connection)
            using (SqlCommand command = commandAndConnection.command)
                command.ExecuteNonQuery();
        }

        //tuples
        private static (SqlCommand command, SqlConnection connection) GetSqlCommandAndConnection(List<SqlParameter> sqlParameters, string procedureName)
        {

            SqlConnection connection = GetSqlConnection($"ConnectionStrings:DefaultConnection");
            SqlCommand command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            if (sqlParameters != null)
                foreach (SqlParameter paramater in sqlParameters)
                    command.Parameters.AddWithValue(paramater.ParameterName, paramater.Value);
            return (command, connection);
        }

        public static SqlConnection GetSqlConnection(string sectionName)
        {
            string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection(sectionName).Value;
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}




