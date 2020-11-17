using Nancy.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HttpClientSample
{
    public class Movie
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string Original_title { get; set; }
        public string Release_date { get; set; }
    }

    public class Data
    {
        public string Results { get; set; }
    }


    class Program
    {
        static HttpClient client = new HttpClient();

        static void ShowProduct(Movie movie)
        {
            Console.WriteLine($"Title: {movie.Title}\tRelease Date: " +
                $"{movie.Release_date}\tOverview: {movie.Overview}");
        }

        static async Task<Data> GetMovieAsync(string path)
        {
            Data data = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsAsync<Data>();
            }
            return data;
        }

        static async Task<Data> GetRecentMoviesAsync(string path)
        {
            Data data = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic d = js.Deserialize<dynamic>(responseBody);

                long page = d["page"];
                long total_results = d["total_results"];
                long total_pages = d["total_pages"];
                foreach (var member in d["results"])
                {
                    Console.WriteLine(member["title"]);
                }
                for (int i = 2; i <= total_pages; i++)
                {
                    response = await client.GetAsync($"{path}&page={i}");
                    responseBody = await response.Content.ReadAsStringAsync();
                    d = js.Deserialize<dynamic>(responseBody);
                    foreach (var member in d["results"])
                    {
                        Console.WriteLine(member["title"]);
                    }
                }
;

                //Object jObject = JObject.Parse(responseBody);

                //JToken memberName = jObject["Results"][0];



            }
            Console.ReadLine();
            return data;
        }

        static void Main()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            string two_weeks_ago = DateTime.Today.AddDays(-14).ToString("yyyy-MM-dd");
            string include_video = "false";
            string include_adult = "false";
            string action = "/discover/movie";
            string api_version = "3";
            string api_base_url = $"https://api.themoviedb.org/{api_version}";
            string api_key = "26b0924beb1a602a494bf23da021b807";
            string language_code = "en-US";
            string endpoint_path = $"{action}?api_key={api_key}&language={language_code}&include_adult={include_adult}&include_video={include_video}&release_date.gte={two_weeks_ago}";

            //API call to get all movies released over the past 2 weeks
            string discover_API_call = $"{api_base_url}{endpoint_path}";
            string endpoint = discover_API_call;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                //string url = "https://api.themoviedb.org/3/movie/738646?api_key=26b0924beb1a602a494bf23da021b807&language=en-US";
                // Get the movie
                //Movie movie = await GetMovieAsync(url);
                Data data = await GetRecentMoviesAsync(endpoint);
                //ShowProduct(movie);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
