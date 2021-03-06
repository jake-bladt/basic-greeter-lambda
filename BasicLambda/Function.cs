using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BasicLambda
{
    public class Person
    {
        public string Name { get; set; }
    }

    public class StepsData
    {
        public StepsData(string dateKey, string countStr)
        {
            Date = DateKeyToDateTime(dateKey);
            StepsCount = Int32.Parse(countStr);
        }

        protected DateTime DateKeyToDateTime(string key)
        {
            var y = Int32.Parse(key.Substring(0, 4));
            var m = Int32.Parse(key.Substring(4, 2));
            var d = Int32.Parse(key.Substring(6, 2));

            return new DateTime(y, m, d);
        }

        public DateTime Date { get; set; }
        public int StepsCount { get; set; }
    }

    public class StepsDataResult
    {
        public List<StepsData> DailyResults { get; protected set; }
        public string UserName { get; protected set; }
        public StepsDataResult(string name, List<StepsData> lst)
        {
            UserName = name;
            DailyResults = lst;
        }
    }


    public class Function
    {
        public StepsDataResult FunctionHandler(Person input, ILambdaContext context)
        {
            var task = FunctionHandlerAsync(input, context);
            return new StepsDataResult(input.Name, task.Result);
        }
        
        public async Task<List<StepsData>> FunctionHandlerAsync(Person input, ILambdaContext context)
        {
            var client = new AmazonDynamoDBClient();
            var table = Table.LoadTable(client, "walker_steps_data");
            
            var userName = input.Name.ToLower();
            var filter = new QueryFilter("user_name", QueryOperator.Equal, userName);

            var config = new QueryOperationConfig
            {
                IndexName = "username-index",
                Filter = filter,
                Select = SelectValues.AllAttributes
            };

            var queryRes = table.Query(config);
            var ret = new List<StepsData>();

            do
            {
                var docs = await queryRes.GetNextSetAsync();
                foreach(var doc in docs)
                {
                    var dt = doc["date_key"].AsPrimitive().Value.ToString();
                    var ct = doc["steps_count"].AsPrimitive().Value.ToString();
                    ret.Add(new StepsData(dt, ct));
                }

            } while (!queryRes.IsDone);

            return ret;

        }
    }
}
