using Azure;
using Azure.AI.Language.QuestionAnswering;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Labb_1_Natural_Language_Processing
{
    internal class Program
    {
        //AI SERVICES
        private static string tranlatorEndpoint = "https://api.cognitive.microsofttranslator.com";
        private static string translatorServiceKey;
        private static string serviceRegion;
        //LANGUAGE SERVICE
        private static Uri endpoint;
        private static AzureKeyCredential credential;
        private static string projectName;
        private static string deploymentName;

        static async Task Main(string[] args)
        {
            //get settings from appsettings
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            //Ai service settings
            translatorServiceKey = configuration["ServiceKey"];
            serviceRegion = configuration["ServiceRegion"];

            //Language service settings
            endpoint = new Uri(configuration["LANGUAGE_ENDPOINT"]);
            credential = new AzureKeyCredential(configuration["LANGUAGE_KEY"]);
            projectName = configuration["PROJECT_NAME"];
            deploymentName = configuration["DEPLOYMENT_NAME"];

            // Set console encoding to unicode
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            await AnswerQuestion();


        }



        private static async Task AnswerQuestion()
        {
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            while (true)
            {
                Console.WriteLine("write your question about AI in any language. type exit to leave.");
                string input = Console.ReadLine();
                if (input.ToLower() == "exit")
                {
                    break;
                }

                try
                {

                    Response<AnswersResult> response = client.GetAnswers(input, project);

                    foreach(KnowledgeBaseAnswer answer in response.Value.Answers)
                    {
                        Console.WriteLine($"Q:{input}");
                        string language = await GetLanguage(input);

                        Console.WriteLine($"A:{await TranslateFromEnglish(answer.Answer, language)}");
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Request error:{ex}");
                }

            }

        }

   
        static async Task<string> GetLanguage(string text)
        {
            string language = "en";

            object[] body = new object[] { new { Text = text } };
            var requestcontent = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {

                using (var request = new HttpRequestMessage())
                {
                    string path = "/detect?api-version=3.0";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(tranlatorEndpoint + path);
                    request.Content = new StringContent(requestcontent, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", translatorServiceKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", serviceRegion);

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    string responseContent = await response.Content.ReadAsStringAsync();

                    JArray jsonResponse = JArray.Parse(responseContent);
                    language = (string)jsonResponse[0]["language"];
                }
            }

            return language;
        }

        //NOT USED. Language services translates input
        //static async Task<string> TranslateToEnglish(string text, string sourceLanguage)
        //{
        //    string translation = "";


        //    object[] body = new object[] { new { Text = text } };
        //    var requestcontent = JsonConvert.SerializeObject(body);
        //    using (var client = new HttpClient())
        //    {

        //        using (var request = new HttpRequestMessage())
        //        {
        //            string path = "/translate?api-version=3.0&from=" + sourceLanguage + "&to=en";
        //            request.Method = HttpMethod.Post;
        //            request.RequestUri = new Uri(tranlatorEndpoint + path);
        //            request.Content = new StringContent(requestcontent, Encoding.UTF8, "application/json");
        //            request.Headers.Add("Ocp-Apim-Subscription-Key", translatorServiceKey);
        //            request.Headers.Add("Ocp-Apim-Subscription-Region", serviceRegion);


        //            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

        //            string responseContent = await response.Content.ReadAsStringAsync();

        //            JArray jsonResponse = JArray.Parse(responseContent);
        //            translation = (string)jsonResponse[0]["translations"][0]["text"];
        //        }
        //    }

        //    return translation;
        //}



        static async Task<string> TranslateFromEnglish(string text, string targetLanguage)
        {
            string translation = "";


            object[] body = new object[] { new { Text = text } };
            var requestcontent = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {

                using (var request = new HttpRequestMessage())
                {
                    string path = "/translate?api-version=3.0&from=en&to=" + targetLanguage;
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(tranlatorEndpoint + path);
                    request.Content = new StringContent(requestcontent, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", translatorServiceKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", serviceRegion);


                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    string responseContent = await response.Content.ReadAsStringAsync();

                    JArray jsonResponse = JArray.Parse(responseContent);
                    translation = (string)jsonResponse[0]["translations"][0]["text"];
                }
            }

            return translation;
        }


    }
}
