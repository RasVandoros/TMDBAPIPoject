using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace ConsoleApp2
{
    public sealed class Manager
    {

        #region Singleton

        static Manager instance = null;
        static readonly object myLock = new object();
        private int id;

        Manager()
        {
            try
            {
                string curFile = "";
                id = 0;
                do
                {
                    id++;
                    curFile = string.Format(String.IsNullOrEmpty(id.ToString()) ? "{0}.log" : "{0}_{1}.log", DateTime.Now.ToString("yyyy_MM_dd"), id);
                }
                while (File.Exists(curFile));

            }
            catch (Exception e)
            {
            }
        }

        public static Manager Instance
        {
            get
            {
                lock (myLock)
                {
                    if (instance == null)
                    {
                        instance = new Manager();

                    }
                }
                return instance;
            }


        }

        #endregion


        public void InsertMovie(string t, string o, string o_t, string r_d)
        {
            string connectionString = "Data Source = localhost; Initial Catalog = TMDBdb; Integrated Security = True";
            var myCommandString = "INSERT INTO Movies (title, overview, original_title, release_date) VALUES (@title, @overview, @original_title, @release_date)";

            using (var myConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(myCommandString, myConnection))
            {
                SqlParameter title = new SqlParameter();
                SqlParameter overview = new SqlParameter();
                SqlParameter original_title = new SqlParameter();
                SqlParameter release_date = new SqlParameter();
                command.Parameters.AddWithValue("@title", t);
                command.Parameters.AddWithValue("@overview", o);
                command.Parameters.AddWithValue("@original_title", o_t);
                command.Parameters.AddWithValue("@release_date", r_d);

                try
                {
                    myConnection.Open();
                    command.ExecuteNonQuery();
                    myConnection.Close();
                }
                catch (Exception e)
                {
                    
                }
            }
        }
    }
}