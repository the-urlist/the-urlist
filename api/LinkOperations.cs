using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheUrlist.Infrastructure;
using TheUrlist.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TheUrlist
{
    public partial class LinkOperations
    {
        protected IHttpContextAccessor _contextAccessor;
        protected IBlackListChecker _blackListChecker;
        protected Hasher _hasher;
        protected TelemetryClient _telemetryClient;
        protected const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected const string VANITY_REGEX = @"^([\w\d-])+(/([\w\d-])+)*$";
        protected const string QRCODECONTAINER = "qrcodes";

        public LinkOperations(IHttpContextAccessor contextAccessor, IBlackListChecker blackListChecker, Hasher hasher)
        {
            _contextAccessor = contextAccessor;
            _blackListChecker = blackListChecker;
            _hasher = hasher;
            TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.TelemetryInitializers.Add(new HeaderTelemetryInitializer(contextAccessor));
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        // protected UserInfo GetAccountInfo(HttpRequest req)
        // {
        //     var claimsPrincipal = StaticWebAppsAuth.Parse(req);
        //     var userInfo = new UserInfo(claimsPrincipal);

        //     var socialIdentities = _contextAccessor.HttpContext.User
        //           .Identities.Where(id => !id.AuthenticationType.Equals("WebJobsAuthLevel", StringComparison.InvariantCultureIgnoreCase));

        //     if (socialIdentities.Any())
        //     {
        //         var provider = _contextAccessor.HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();

        //         var primaryIdentity = socialIdentities.First();
        //         var email = primaryIdentity.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email).Value;
        //         var userInfo = new UserInfo(provider, _hasher.HashString(email));

        //         var evt = new EventTelemetry("UserInfo Retrieved");
        //         evt.Properties.Add("Provider", provider);
        //         evt.Properties.Add("EmailAquired", (string.IsNullOrEmpty(email).ToString()));
        //         _telemetryClient.TrackEvent(evt);

        //         return userInfo;
        //     }

        //     return UserInfo.Empty; ;
        // }
    }
}