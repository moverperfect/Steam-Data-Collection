﻿using System;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;

namespace Steam_Data_Collection
{
    /// <summary>
    /// Connects the program to a MySql database
    /// </summary>
    internal class SqlConnecter
    {
        /// <summary>
        /// The main connection object
        /// </summary>
        private MySqlConnection _connection;

        /// <summary>
        /// Create a database connector object with the default options for server name, user id and password
        /// </summary>
        /// <param name="db">The database name</param>
        public SqlConnecter(String db)
        {
            Initialize(db, Program.ServerAddress, Program.Username, Program.Password);
        }

        /// <summary>
        /// Initializes the SQL connection object
        /// </summary>
        private void Initialize(String database, String server, String uid, String password)
        {
            var connectionString = "SERVER=" + server + ";DATABASE=" + database + ";UID=" + uid + ";PASSWORD=" +
                                   password + ";Allow User Variables=true;default command timeout=360";

            _connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Test the connection to the SQL Sever
        /// </summary>
        /// <returns>True if successful connection</returns>
        public bool TestConnection()
        {
            var open = OpenConnection();
            try
            {
                CloseConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        /// <summary>
        /// Closes the connection to the server, true if successful
        /// </summary>
        /// <returns>If successful</returns>
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

        /// <summary>
        /// Execute a NonQuery to the server
        /// </summary>
        /// <param name="query">The query to be sent</param>
        public void NonQuery(String query)
        {
            try
            {
                if (OpenConnection())
                {
                    var cmd = new MySqlCommand(query, _connection);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(query);
                Console.WriteLine(exception);
                File.AppendAllText("Exceptions.txt",query + "\n" + exception + "\n");
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Execute a Select statement to the sql server
        /// </summary>
        /// <param name="query">The select statement</param>
        /// <returns>The datatable containing the data</returns>
        public Object Select(String query)
        {
            try
            {
                var dt = new DataTable();

                if (OpenConnection())
                {
                    var cmd = new MySqlCommand(query, _connection);

                    var dr = cmd.ExecuteReader();

                    var schemaTable = dr.GetSchemaTable();
                    foreach (DataRowView row in schemaTable.DefaultView)
                    {
                        var columnName = (string) row["ColumnName"];
                        var type = (Type) row["DataType"];
                        dt.Columns.Add(columnName, type);
                    }

                    // Hard coded a workaround for the multi column primary key when getting friend links
                    try
                    {
                        if (dt.Columns[1].ColumnName == "UserID2")
                        {
                            dt.PrimaryKey = new[] { dt.Columns[0], dt.Columns[1]};
                        }
                    }
                    catch
                    {
                    }

                    dt.Load(dr);

                    dr.Close();

                    return dt;
                }
                const string error =
                    "ERROR: Connection to database could not be established, please contact an administrator!";
                return error;
            }
            catch (Exception e)
            {
                Console.WriteLine(query);
                Console.WriteLine(e.Message);
            }
            finally
            {
                CloseConnection();
            }
            return null;
        }

        /// <summary>
        /// Executes a count query
        /// </summary>
        /// <param name="query">The SQL query</param>
        /// <returns>The counted amount</returns>
        public int Count(String query)
        {
            var count = 0;

            if (OpenConnection())
            {
                var cmd = new MySqlCommand(query, _connection);

                count = int.Parse(cmd.ExecuteScalar().ToString());

                CloseConnection();

                return count;
            }
            return count;
        }
    }
}