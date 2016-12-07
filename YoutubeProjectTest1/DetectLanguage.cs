using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;


namespace YoutubeProjectTest1
{
    class DetectLanguage
    {
        public class Detection
        {
            public string language { get; set; }
            public bool isReliable { get; set; }
            public float confidence { get; set; }
        }

        public class ResultData
        {
            public List<Detection> detections { get; set; }
        }

        public class Result
        {
            public ResultData data { get; set; }
        }

        public static List<string> detectString(string s)
        {
            var client = new RestClient("http://ws.detectlanguage.com");
            var request = new RestRequest("/0.2/detect", Method.POST);

            request.AddParameter("key", "cfd22c77f0d1bb26a396f5b226bfd27a"); // replace "demo" with your API key
            request.AddParameter("q", s);

            IRestResponse response = client.Execute(request);

            RestSharp.Deserializers.JsonDeserializer deserializer = new RestSharp.Deserializers.JsonDeserializer();

            var result = deserializer.Deserialize<Result>(response);

           // Detection detection = result.data.detections[0];

            List<string> languages = new List<string>();
            foreach (Detection detection in result.data.detections)
                languages.Add(detection.language);


            return languages;

        }
    }
}
