using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;

namespace playfabdemofunction
{
	public static class PlayFabDemo
    {
        [FunctionName("PlayFabDemo")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable("DeveloperSecretKey");
            PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable("TitleId");
            FunctionExecutionContext<dynamic> context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());

            var giveItemReq = new GrantItemsToUserRequest()
            {
                 ItemIds = new System.Collections.Generic.List<string> { "One", "Two" },
                  PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId
            };

			PlayFabResult<GrantItemsToUserResult> result = await PlayFabServerAPI.GrantItemsToUserAsync(giveItemReq);

            return new { result };
        }
    }
}
