using System;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BasicLambda
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(string input, ILambdaContext context)
        {
            string greeting = $"Welcome, {input}";
            string specs = $"Request Id: " + context.AwsRequestId + ", ";
            specs += "Function Name: " + context.FunctionName + ", ";
            specs += "Function Version: " + context.FunctionVersion + ", ";
            specs += "Time Remaining: " + context.RemainingTime + ", ";
            specs += "Memory Limit (in MB): " + context.MemoryLimitInMB.ToString();
            string message = $"{greeting}.{Environment.NewLine}{specs}";

            return message;

        }
    }
}
