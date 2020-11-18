using Nancy.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ConsoleApp2
{
    public class Movie
    {
        class Program
        {
            static HttpClient client = new HttpClient();
            static string date_start = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
            static string include_video = "false";
            static string include_adult = "false";
            static string api_version = "3";
            static string api_base_url = $"https://api.themoviedb.org/{api_version}";
            static string api_key = "26b0924beb1a602a494bf23da021b807";
            static string language_code = "en-US";
            static void Main()
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Manager.Instance.DeleteExistingData();
                RunAsync().GetAwaiter().GetResult();
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                Console.ReadLine();
            }

            static async Task RunAsync()
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //API call to get all movies released over the past week
                string discover_API_call = $"{api_base_url}/discover/movie?api_key={api_key}&language={language_code}&include_adult={include_adult}&include_video={include_video}&release_date.gte={date_start}";
                int pageNo = 1;
                int totalPages = 0;
                dynamic data = null;
                do
                {
                    data = await GetRequestAsync($"{discover_API_call}&page={pageNo}"); //discover api call, returns one page
                    await IterateDiscoverDataAsync(data);
                    totalPages = Convert.ToInt32(data["total_pages"]);
                    pageNo++;

                } while (pageNo < totalPages); //go through all pages returned from discover call
            }

            private static async Task IterateDiscoverDataAsync(dynamic data)
            {
                foreach (var member in data["results"])
                {
                    string movieId = "";
                    try
                    {
                        movieId = member["id"].ToString();
                        Manager.Instance.InsertMovie(movieId, member["title"], member["overview"], member["original_title"], member["release_date"]);
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine("Unable to insert movie with id: " + movieId);
                        Console.WriteLine("Formatting of member is not compatible with database.");
                        continue;
                    }
                    try
                    {
                        await GetCreditsAsync(movieId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Getting the credits.\nMovie id: " + movieId + " does not exist on the TMDB leading to a null ref.");
                        Console.WriteLine(e.Message);
                    }
                }
            }

            private static async Task GetCreditsAsync(string movieId)
            {
                //Make another API call here for crew members, from movie id
                string getCredits_API_call = $"{ api_base_url }/movie/{ movieId }/credits?api_key={api_key}&language={language_code}";
                dynamic creditsDataset = await GetRequestAsync(getCredits_API_call);
                foreach (var creditMember in creditsDataset["crew"])
                {
                    dynamic externalIdDataset = null;
                    if (creditMember["job"] == "Director")
                    {
                        string directorName = creditMember["name"];
                        string directorId = creditMember["id"].ToString();
                        string directorImdb = "";

                        //Call get external id api to get imdb_id of director
                        string getExternalIdAPICall = $"{ api_base_url }/person/{ directorId }/external_ids?api_key={api_key}&language={language_code}";
                        externalIdDataset = await GetRequestAsync(getExternalIdAPICall);
                        try
                        {
                            string imdbId = externalIdDataset["imdb_id"];
                            if (imdbId != null) //check if the director has an imdb page
                            {
                                directorImdb = $"https://www.imdb.com/name/{ imdbId }";
                            }
                            else
                            {
                                directorImdb = "Does not exist";
                            }

                            //Insert director id, name, imdb_id in directors
                            Manager.Instance.InsertDirector(directorId, directorName, directorImdb);
                            //Insert director_id - movie_id in movies_directors
                            Manager.Instance.InsertDirectorToMovie(movieId, directorId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            static async Task<dynamic> GetRequestAsync(string path)
            {
                dynamic d = null;
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    d = js.Deserialize<dynamic>(responseBody);
                    return d;
                }
                else
                {
                    return d;
                }
            }
        }
    }
}