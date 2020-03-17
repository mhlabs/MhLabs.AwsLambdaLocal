using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using MhLabs.AwsCliSso;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace MhLabs.AwsLambdaLocal
{
    public static class LambdaEnvironment
    {
        public static async Task MapToLocal(RegionEndpoint region, string stackName = null, string apiProxyLogicalId = "ApiProxy", string accountId = null, string startUrl = null, string roleName = null)
        {
            AmazonCloudFormationClient cloudFormation;
            AmazonLambdaClient lambdaClient;
            AWSCredentials credentials;
            startUrl = startUrl ?? System.Environment.GetEnvironmentVariable("AWS_SSO_START_URL");
            if (!string.IsNullOrEmpty(startUrl))
            {
                var service = new AwsCliSsoService(region);
                credentials = await service.GetCredentials(startUrl, accountId ?? System.Environment.GetEnvironmentVariable("AWS_SSO_ACCOUNT_ID"), roleName ?? System.Environment.GetEnvironmentVariable("AWS_SSO_LAMBDA_LOCAL_ROLE_NAME"));
                var immutableCredentials = credentials.GetCredentials();
                System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", immutableCredentials.AccessKey);
                System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", immutableCredentials.SecretKey);
                System.Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", immutableCredentials.Token);
                cloudFormation = new AmazonCloudFormationClient(credentials, region);
                lambdaClient = new AmazonLambdaClient(credentials, region);
            }
            else
            {
                cloudFormation = new AmazonCloudFormationClient(region);
                lambdaClient = new AmazonLambdaClient(region);
            }
            stackName = stackName ?? Assembly.GetEntryAssembly().GetName().Name.Replace('_', '-');
            var resources =
                await cloudFormation.DescribeStackResourcesAsync(
                    new DescribeStackResourcesRequest { StackName = stackName });
            var apiProxyLambda = resources.StackResources.FirstOrDefault(p => p.LogicalResourceId == apiProxyLogicalId)?.PhysicalResourceId;
            var lambdaFunction = await lambdaClient.GetFunctionAsync(new GetFunctionRequest { FunctionName = apiProxyLambda });
            foreach (var entry in lambdaFunction.Configuration.Environment.Variables)
            {
                System.Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }
            System.Environment.SetEnvironmentVariable("AWS_DEFAULT_REGION", region.SystemName);
        }
    }
}