using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace json2sql.converter
{
    class JsonDeserializer
    {
        public static IDictionary<string, JToken> Deserialize(FilePath filePath)
        {
            var json = File.ReadAllText(filePath.Path);
            return Deserialize(new JsonText(json));
        }

        public static IDictionary<string, JToken> Deserialize(JsonText jsonText)
        {
            StringReader stringReader = null;
            if (jsonText.Text.StartsWith("["))        //It means it is starting with array and not an object
                stringReader = new StringReader("{\"root\":" + jsonText.Text + "}");
            else
                stringReader = new StringReader(jsonText.Text);

            JsonTextReader rdr = new JsonTextReader(stringReader);
            JsonSerializer d = new JsonSerializer();

            var dic = (IEnumerable<KeyValuePair<string, JToken>>)d.Deserialize(rdr);
            return new Dictionary<string, JToken>(dic);

        }

        public static IDictionary<string, JToken> Deserialize(Url url)
        {
            HttpClient httpClient = new HttpClient();
            string data = httpClient.GetStringAsync(url.URL).GetAwaiter().GetResult();
            return Deserialize(new JsonText(data));
        }
    }

    class FilePath
    {
        public FilePath(string filePath)
        {
            Path = filePath;
        }

        public string Path { get; set; }
    }

    class JsonText
    {
        public JsonText(string jsonText)
        {
            Text = jsonText;
        }

        public string Text { get; set; }
    }

    class Url
    {
        public Url(string url)
        {
            URL = url;
        }
        public string URL { get; set; }
    }
}
