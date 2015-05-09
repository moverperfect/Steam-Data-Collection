using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Steam_Data_Collection
{
    class SqlConnecter
    {
        private MySqlConnection _connection;
        private String _server;
        private String _database;
        private String _uid;
        private String _password;

        /// <summary>
        /// Create a database connector object with the default options for server name, user id and password
        /// </summary>
        /// <param name="db">The database name</param>
        public SqlConnecter(String db)
        {
            Initialize(db, "localhost","root","");
        }

        /// <summary>
        /// Creates a database object with defined properties
        /// </summary>
        /// <param name="db">Database name</param>
        /// <param name="server">Sever location</param>
        /// <param name="uid">User ID</param>
        /// <param name="password">Password for user</param>
        public SqlConnecter(String db, String server, String uid, String password)
        {
            Initialize(db, server, uid, password);
        }


        private void Initialize(String _database, String _Server, String _uid, String _password)
        {
            string connectionString = "SERVER=" + _server + ";DATABASE=" + _database + ";UID=" + _uid + ";PASSWORD=" + _password + ";";

            _connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Test the connection to the SQL Sever
        /// </summary>
        /// <returns>True if successful connection</returns>
        public bool TestConnection()
        {
            var open = OpenConnection();
            CloseConnection();
            return open;
        }

        /// <summary>
        /// Opens the connection to the database
        /// </summary>
        /// <returns>True if successful connection</returns>
        private bool OpenConnection()
        {
            try
            {
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server. Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }


        private bool CloseConnection()
        {
            try
            {
                _connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void NonQuery(String query)
        {
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }

        public DataTable Select(String query)
        {
            var dt = new DataTable();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);

                MySqlDataReader dr = cmd.ExecuteReader();

                var schemaTable = dr.GetSchemaTable();
                foreach (DataRowView row in schemaTable.DefaultView)
                {
                    var columnName = (string)row["ColumnName"];
                    var type = (Type)row["DataType"];
                    dt.Columns.Add(columnName, type);
                }


                dt.Load(dr);

                dr.Close();

                CloseConnection();

                return dt;

            }
            else
            {
                return dt;
            }
        }

        public int Count(String query)
        {
            int count = 0;

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, _connection);

                count = int.Parse(cmd.ExecuteScalar().ToString());

                this.CloseConnection();

                return count;
            }
            else
            {
                return count;
            }
        }
    }
}
