using json2sql.converter;
using json2sql.sqlpersister;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace json2sql
{
    class Program
    {
        static void Main(string[] args)
        {
            var folderPath = @"F:\Github\json2sql\json2sql\SampleJSON\";
            var fileName =  @"sales.json";
            //var fileName = @"ArrayContainsObject.json";
            //var fileName = @"ArrayWithHighlyNestedObjects.json";
            //var fileName = @"manu.json";
            //var fileName = @"NestedObject.json";
            //var fileName = @"NestetStructures.json";
            //var fileName = @"PathConstructor.json";
            //var fileName = @"SimpleArray.json";
            //var fileName = @"SubPathConstructor.json";
            //var fileName = @"widget.json";

            Console.WriteLine("Conersion started");

            Converter("FileUpload", folderPath + fileName, Convertertype.mssql);

            Console.ReadLine();
        }

        public static void Converter(string datasource, string pathortextorurl, Convertertype convertertype)
        {
            StringBuilder script = new StringBuilder();
            IDictionary<string, JToken> root = new Dictionary<string, JToken>();
            try
            {
                switch (datasource)
                {
                    case "FileUpload":
                        root = JsonDeserializer.Deserialize(new FilePath(pathortextorurl));
                        break;
                    case "RawData":
                        root = JsonDeserializer.Deserialize(new JsonText(pathortextorurl));
                        break;
                    case "URL":
                        root = JsonDeserializer.Deserialize(new Url(pathortextorurl));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                var converter = new JsonToTableConverter();
                converter.Parse(root);
                switch (convertertype)
                {
                    case Convertertype.mssql:
                        script = converter.ToSqlScript(new MsSqlPersister());
                        break;
                    case Convertertype.oracle:
                        script = converter.ToSqlScript(new OraclePersister());
                        break;
                    case Convertertype.mysql:
                        script = converter.ToSqlScript(new MySqlPersister());
                        break;
                    case Convertertype.sqlite:
                        script = converter.ToSqlScript(new SqlitePersister());
                        break;
                    default:
                        break;
                }
                Debug.WriteLine(script);
                Console.WriteLine(script);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

         }
    }

    enum Convertertype
    {
        mssql,
        oracle,
        mysql,
        sqlite
    }
}
