using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Typesense;
using Typesense.Setup;
using Newtonsoft.Json.Linq;
using System;


namespace Example
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var provider = new ServiceCollection()
                .AddTypesenseClient(config =>
                {
                    config.ApiKey = "Hu52dwsas2AdxdE";
                    config.Nodes = new List<Node> { new Node { Host = "localhost", Port = "8108", Protocol = "http" } };
                }).BuildServiceProvider();

            var typesenseClient = provider.GetService<ITypesenseClient>();
            var schema = new Schema();
            schema.Name = "Adresses";
            schema.Fields = new List<Field>();
            schema.Fields.Add(new Field("id", "string", false));
            schema.Fields.Add(new Field("houseNumber", "int32", false));
            schema.Fields.Add(new Field("accesAdress", "string", false));
            schema.DefaultSortingField = "houseNumber";
            var doc = @"{
                id : '1231231fd',
                houseNumber : 2,
                accesAdress : 'Smedgade 25B'
            }";
            var doc1 = @"{
                id : '123122fd',
                houseNumber : 66,
                accesAdress : 'Smedgade 67B'
            }";
            var doc2 = @"{
                id : '1s3122fd',
                houseNumber : 33,
                accesAdress : 'Medad 55A'
            }";
            var doc3 = @"{
                id : '1s3sss22fd',
                houseNumber : 3,
                accesAdress : 'Daramed'
            }";



            dynamic jo = JObject.Parse(doc);
            dynamic jo1 = JObject.Parse(doc1);
            dynamic jo2 = JObject.Parse(doc2);
            dynamic jo3 = JObject.Parse(doc3);



            var query = new SearchParameters();
            query.Text = "da";
            query.QueryBy = "accesAdress";





            await typesenseClient.CreateCollection(schema);
            await typesenseClient.RetrieveCollections();
            await typesenseClient.CreateDocument("Adresses",jo);
            await typesenseClient.CreateDocument("Adresses",jo1);
            await typesenseClient.CreateDocument("Adresses",jo2);
            await typesenseClient.CreateDocument("Adresses",jo3);
            await typesenseClient.RetrieveCollection("Adresses");
            await typesenseClient.Search("Adresses",query);

        }
    }
}
