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
                Console.WriteLine(e.Message);
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

        private static string connectionString = "Data Source = localhost; Initial Catalog = TMDBdb; Integrated Security = True";

        public bool InsertMovie(string id, string t, string o, string o_t, string r_d)
        {
            bool success = false;
            var myCommandString = "INSERT INTO Movies (movie_id, title, overview, original_title, release_date) VALUES (@id, @title, @overview, @original_title, @release_date)";

            using (var myConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(myCommandString, myConnection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@title", t);
                command.Parameters.AddWithValue("@overview", o);
                command.Parameters.AddWithValue("@original_title", o_t);
                command.Parameters.AddWithValue("@release_date", r_d);
                try
                {
                    myConnection.Open();
                    command.ExecuteNonQuery();
                    myConnection.Close();
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error inserting to movies table.");
                    Console.WriteLine("Movie title: " + t + ". Movie id: " + id);
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Debug.");
                }
            }
            return success;
        }


        internal bool InsertDirector(string directorId, string name, string imdbId)
        {
            bool success = false;
            var myCommandString = "IF NOT EXISTS (SELECT * FROM Directors WHERE director_id = @id ) INSERT INTO Directors (director_id, name, imdb_id) VALUES (@id, @name, @imdb)";

            using (var myConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(myCommandString, myConnection))
            {
                command.Parameters.AddWithValue("@id", directorId);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@imdb", imdbId);
                try
                {
                    myConnection.Open();
                    command.ExecuteNonQuery();
                    myConnection.Close();
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error inserting to directors table.");
                    Console.WriteLine("Director name: " + name + ". Director id: " + directorId);
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Debug.");
                }
            }
            return success;
        }

        internal bool InsertDirectorToMovie(string movieId, string directorId)
        {
            bool success = false;
            var myCommandString = "INSERT INTO movies_directors (movie_id, director_id) VALUES (@movie_id, @director_id)";

            using (var myConnection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(myCommandString, myConnection))
            {
                command.Parameters.AddWithValue("@movie_id", movieId);
                command.Parameters.AddWithValue("@director_id", directorId);
                try
                {
                    myConnection.Open();
                    command.ExecuteNonQuery();
                    myConnection.Close();
                    success = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error inserting to movies_directors table.");
                    Console.WriteLine("Movie id: " + movieId + ". Director id: " + directorId);
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Debug.");
                }
            }
            return success;
        }
    }
}