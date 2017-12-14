# MhLabs.AwsLambdaLocal

Helper method to map AWS Lambda environment variables to local variables. Used to run a local instance of a WebAPI API Gateway proxy project that uses resources where the physical ID is unknown, but mapped to environment variables through cloudformation.

## Prerequisites
* Your project uses and follows the pattern described here: https://github.com/mhlabs/MhLabs.DataAccessIoC
* Your CloudFormation stack name is the same as project name (apart from project name using underscore due to VS limitation)
* All references to AWS resources have to be passed to application code via environment variables.
* You have AWS access/secret keys with appropriate permissions set in local environment variables. These should cover the same permissions as your lambdas execution role.

## Setup
1. `Install-Package MhLabs.AwsLambdaLocal`
2. In LocalEntryPoint.cs add this before `var host = new WebHostBuilder()`:
```
LambdaEnvironment.MapToLocal(<<The region endpoint you're targeting, i.e RegionEndpoint.EUWest1>>).Wait();
```
3. Hit F5
