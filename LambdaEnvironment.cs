using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace MhLabs.AwsLambdaLocal
{
    public static class LambdaEnvironment
    {
        public static async Task MapToLocal(RegionEndpoint region, string stackName = null, string apiProxyLogicalId = "ApiProxy")
        {
            stackName = stackName ?? Assembly.GetEntryAssembly().GetName().Name.Replace('_', '-');
            var cloudFormation = new AmazonCloudFormationClient(region);
            var lambdaClient = new AmazonLambdaClient(region);
            var resources =
                await cloudFormation.DescribeStackResourcesAsync(
                    new DescribeStackResourcesRequest { StackName = stackName });
            var apiProxyLambda = resources.StackResources.FirstOrDefault(p => p.LogicalResourceId == "ApiProxy")?.PhysicalResourceId;
            var lambdaFunction = await lambdaClient.GetFunctionAsync(new GetFunctionRequest { FunctionName = apiProxyLambda });
            foreach (var entry in lambdaFunction.Configuration.Environment.Variables)
            {
                System.Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }
            System.Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", region.SystemName);
        }
    }
}
