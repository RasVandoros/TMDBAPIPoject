using Nancy.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ConsoleApp2
{
    public class Movie
    {
        class Program
        {
            static HttpClient client = new HttpClient();

            static void Main()
            {
                RunAsync().GetAwaiter().GetResult();
            }

            static async Task RunAsync()
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string date_start = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
                string include_video = "false";
                string include_adult = "false";
                string action = "/discover/movie";
                string api_version = "3";
                string api_base_url = $"https://api.themoviedb.org/{api_version}";
                string api_key = "26b0924beb1a602a494bf23da021b807";
                string language_code = "en-US";

                //API call to get all movies released over the past 2 weeks
                string discover_API_call = $"{api_base_url}{action}?api_key={api_key}&language={language_code}&include_adult={include_adult}&include_video={include_video}&release_date.gte={date_start}";
                int pageNo = 1;
                int totalPages = 0;
                dynamic data = null;
                string getCredits_API_call = "";
                bool success = false;
                do
                {
                    data = await GetRequestAsync($"{discover_API_call}&page={pageNo}");
                    foreach (var member in data["results"])
                    {
                        string movieId = member["id"].ToString();
                        success = Manager.Instance.InsertMovie(movieId, member["title"], member["overview"], member["original_title"], member["release_date"]);
                        if (!success) continue;
                        //Make another API call here for crew members, from movie id
                        action = "/movie/";

                        getCredits_API_call = $"{ api_base_url }{ action }{ movieId }/credits?api_key={api_key}&language={language_code}";
                        dynamic creditsDataset = await GetRequestAsync(getCredits_API_call);

                        foreach (var creditMember in creditsDataset["crew"])
                        {
                            if (creditMember["job"] == "Director")
                            {
                                string directorName = creditMember["name"];
                                string directorId = creditMember["id"].ToString();
                                string directorImdb = "";

                                //Call get external id api to get imdb_id of director
                                string getExternalIdAPICall = $"{ api_base_url }/person/{ directorId }/external_ids?api_key={api_key}&language={language_code}";
                                dynamic externalIdDataset = await GetRequestAsync(getExternalIdAPICall);
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
                                success = Manager.Instance.InsertDirector(directorId, directorName, directorImdb);
                                //Insert director_id - movie_id in movies_directors
                                success = Manager.Instance.InsertDirectorToMovie(movieId, directorId);
                                if (!success) continue;
                            }
                        }
                    }
                    totalPages = Convert.ToInt32(data["total_pages"]);
                    pageNo++;
                } while (pageNo < totalPages);
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