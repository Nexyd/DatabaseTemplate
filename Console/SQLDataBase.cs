// Daniel Morato Baudi.
using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseTemplate.Console
{
    /// <summary>
    /// Stores and handles all the database related actions.
    /// </summary>
    public class SQLDataBase
    {
        #region Attributes
        private string query;
        private int affectedRegistries;
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataReader reader;
        private SqlConnectionStringBuilder
            connectionData;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the 
        /// DatabaseTemplate.Forms.SQLDataBase class.
        /// </summary>
        public SQLDataBase()
        {
            query = "";
            affectedRegistries = 0;
            connection = new SqlConnection();
            command = new SqlCommand();
            connectionData = new 
                SqlConnectionStringBuilder();

            connectionData.IntegratedSecurity = true;
            connectionData.DataSource = "localhost";
            connectionData.InitialCatalog = "database";
            connection.ConnectionString =
                    connectionData.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseTemplate.Forms.SQLDataBase
        /// class when receives the data source and database.
        /// </summary>
        /// <param name="source">Database host.</param>
        /// <param name="database">Database to connect.</param>
        public SQLDataBase(string source, string database) : this()
        {
            connectionData.DataSource = source;
            connectionData.InitialCatalog = database;
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseTemplate.Forms.SQLDataBase
        /// class when receives the data source, database, username and password.
        /// </summary>
        /// <param name="source">Database host.</param>
        /// <param name="database">Database to connect.</param>
        /// <param name="username">Database username.</param>
        /// <param name="password">Database password.</param>
        public SQLDataBase(string source, string database,
            string username, string password) : this(source, database)
        {
            connectionData.IntegratedSecurity = false;
            connectionData.UserID = username;
            connectionData.Password = password;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Allows DB insertions.
        /// </summary>
        /// <param name="table">Table in which we'll be inserting.</param>
        /// <param name="parameters">SQL parameters keys.</param>
        /// <param name="values">SQL parameters values.</param>
        /// <returns>Insertion status.</returns>
        public bool Insert(string table,
            string[] parameters, object[] values)
        {
            #region Query Prep
            int counter = 0;
            query = "INSERT INTO @table VALUES(";
            for (int i = 0; i < parameters.Length; i++)
                query += parameters[i] + ", ";
            
            int position = query.LastIndexOf(", ");
            query = query.Substring(0, position);
            query += ")";

            command.Parameters.AddRange(parameters);
            command.Parameters.AddWithValue("@table", table);
            foreach (string parameter in parameters)
                command.Parameters[parameter]
                    .Value = values[counter++];
            #endregion

            try {

                StartConnection();
                command.CommandText = query;
                command.Connection = connection;

                affectedRegistries = 
                    command.ExecuteNonQuery();

                if (affectedRegistries < 1)
                    Trace.WriteLine(
                        "Could not be inserted.");

                else {
                    Trace.WriteLine(affectedRegistries
                        + " registry/ies affected");

                    command.Parameters.Clear();
                    return true;
                }

            } catch (SqlException DBError) {
                Trace.WriteLine(
                    "Error: " + DBError.Message);

                return false;

            } finally {
                CloseConnection();
            }

            command.Parameters.Clear();
            return false;
        }

        /// <summary>
        /// Allows database extractions.
        /// </summary>
        /// <param name="table">Table in which we'll be inserting.</param>
        /// <param name="parameters">SQL parameters keys.</param>
        /// <param name="values">SQL parameters values.</param>
        /// <param name="filter">Filter to apply to the query.</param>
        /// <param name="filterValue">Filter value.</param>
        /// <returns>DB data readed.</returns>
        public SqlDataReader Select(string table,
            string[] parameters, object[] values,
            string filter = "", object filterValue = null)
        {
            #region Query Prep
            int counter = 0;
            query = "SELECT ";

            for (int i = 0; i < parameters.Length; i++)
                query += parameters[i] + ", ";

            int position = query.LastIndexOf(", ");
            query = query.Substring(0, position);
            query += "FROM @table";

            if ((filter != "") && (filterValue != null))
                query += "WHERE " + filter
                    + " = '" + filterValue;

            command.Parameters.AddRange(parameters);
            command.Parameters.AddWithValue("@table", table);
            foreach (string parameter in parameters)
                command.Parameters[parameter]
                    .Value = values[counter++];
            #endregion

            try {
                
                StartConnection();
                command = new SqlCommand(
                    query, connection);

                reader = command.ExecuteReader();

            } catch (SqlException DBError) {
                Trace.WriteLine("Error: " 
                    + DBError.Message);
            }

            command.Parameters.Clear();
            return reader;
        }

        /// <summary>
        /// Allows DB updates.
        /// </summary>
        /// <param name="table">Table in which we'll be inserting.</param>
        /// <param name="parameters">SQL parameters keys.</param>
        /// <param name="values">SQL parameters values.</param>
        /// <param name="filter">Filter to apply to the query.</param>
        /// <param name="filterValue">Filter value.</param>
        /// <returns>Update state.</returns>
        public bool Update(string table,
            string[] parameters, object[] values,
            string filter = "", object filterValue = null)
        {
            #region Query Prep
            int counter = 0;
            query = "UPDATE FROM @table SET ";

            for (int i = 0; i < parameters.Length; i++)
                query += parameters[i] + " = " + values[i] + ", ";

            int position = query.LastIndexOf(", ");
            query = query.Substring(0, position);

            if ((filter != "") && (filterValue != null))
                query += "WHERE " + filter
                    + " = '" + filterValue;

            command.Parameters.AddRange(parameters);
            command.Parameters.AddWithValue("@table", table);
            foreach (string parameter in parameters)
                command.Parameters[parameter]
                    .Value = values[counter++];
            #endregion

            try {

                StartConnection();
                command.CommandText = query;
                command.Connection = connection;

                affectedRegistries = 
                    command.ExecuteNonQuery();

                if (affectedRegistries < 1)
                    Trace.WriteLine("Could not be updated.");

                else {

                    Trace.WriteLine(affectedRegistries
                        + " registry/ies affected.");

                    command.Parameters.Clear();
                    return true;
                }

            } catch (SqlException DBError) {
                Trace.WriteLine(
                    "Error: " + DBError.Message);

                return false;

            } finally {
                CloseConnection();
            }

            command.Parameters.Clear();
            return false;
        }

        /// <summary>
        /// Allows DB deletes.
        /// </summary>
        /// <param name="table">Table in which we'll be inserting.</param>
        /// <param name="filter">Filter to apply to the query.</param>
        /// <param name="filterValue">Filter value.</param>
        /// <returns>Delete state.</returns>
        public bool Delete(string table,
            string filter, object filterValue)
        {
            query = "DELETE FROM @table WHERE @filter = @filterValue";
            command.Parameters.AddWithValue("@table", table);
            command.Parameters.AddWithValue("@filter", filter);
            command.Parameters.AddWithValue("@filterValue", filterValue);

            try {

                StartConnection();
                command.CommandText = query;
                command.Connection = connection;

                affectedRegistries = 
                    command.ExecuteNonQuery();

                if (affectedRegistries < 1)
                    Trace.WriteLine("Could not be deleted.");
                    
                else {
                    Trace.WriteLine(affectedRegistries
                        + " registry/ies affected.");

                    command.Parameters.Clear();
                    return true;
                }

            } catch (SqlException DBError) {
                Trace.WriteLine("Error: " + DBError.Message);

            } finally {
                CloseConnection();
            }

            command.Parameters.Clear();
            return false;
        }

        /// <summary>
        /// Starts the connection with the DB.
        /// </summary>
        public void StartConnection()
        {
            try {

                connection = new SqlConnection(
                    connectionData.ToString());

                connection.Open();

            } catch (SqlException ex) {

                Trace.WriteLine("Error: " + ex.Message);
                CloseConnection();
            }
        }

        /// <summary>
        /// Closes the connection with the DB.
        /// </summary>
        public void CloseConnection()
        {
            if (!(connection == null))
                connection.Close();
            if (!(reader == null))
                reader.Close();
        }
        #endregion
    }
}