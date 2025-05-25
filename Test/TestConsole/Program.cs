using Amazon.EC2;
using Amazon.EC2.Model;
using Microsoft.Extensions.DependencyInjection;
using SmugAWS;

namespace TestConsole
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Crypt.CrpytTest();

            return;

            using var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services.AddAWSService<IAmazonEC2>()
            .AddTransient<EC2Wrapper>()
            )
            .Build();

            // Now the client is available for injection.
            var ec2Client = host.Services.GetRequiredService<IAmazonEC2>();

            try
            {
                // Retrieve information for up to 10 Amazon EC2 security groups.
                var request = new DescribeSecurityGroupsRequest { MaxResults = 10, };
                var securityGroups = new List<SecurityGroup>();

                var paginatorForSecurityGroups =
                    ec2Client.Paginators.DescribeSecurityGroups(request);

                await foreach (var securityGroup in paginatorForSecurityGroups.SecurityGroups)
                {
                    securityGroups.Add(securityGroup);
                }

                // Now print the security groups returned by the call to DescribeSecurityGroupsAsync.
                Console.WriteLine(
                    "Welcome to the EC2 Hello Service example. "
                    + Environment.NewLine
                    + "Let's list your Security Groups:");
                securityGroups.ForEach(group =>
                {
                    Console.WriteLine(
                        $"Security group: {group.GroupName} ID: {group.GroupId}");
                });
            }
            catch (AmazonEC2Exception ex)
            {
                Console.WriteLine($"An Amazon EC2 service error occurred while listing security groups. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while listing security groups. {ex.Message}");
            }
        }
    }
}