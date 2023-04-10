using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace HttpTrigger
{
    public static class GetEvents
    {
        [FunctionName("ReadEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            StreamReader reader = new StreamReader(req.Body);

            string requestBody = reader.ReadToEnd();

            log.LogInformation($"Received request body {requestBody}");

            EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();

            EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestBody);

            foreach (var item in eventGridEvents)
            {
                if(item.Data is SubscriptionValidationEventData)
                {
                    SubscriptionValidationEventData subscriptionValidationEventData = (SubscriptionValidationEventData)item.Data;

                    log.LogInformation($"Validation code {subscriptionValidationEventData.ValidationCode}");
                    log.LogInformation($"Validation url {subscriptionValidationEventData.ValidationUrl}");

                    SubscriptionValidationResponse subscriptionValidationResponse = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = subscriptionValidationEventData.ValidationCode
                    };

                    return new OkObjectResult(subscriptionValidationResponse);
                }
            }

            return new OkObjectResult(string.Empty);
        }
    }
}
