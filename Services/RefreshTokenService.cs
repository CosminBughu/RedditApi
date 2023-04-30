using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers;
using RestSharp;
using RestSharp.Authenticators;
using System.Text;

namespace RedditApi.Services
{
    public class AuthTokens
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }

    public class RefreshTokenService : IMiddleware
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string AppId = "sNpJAjtXH9Q6oTRHnKp4cg";
        public string AppSecret = "C7guhpLuZvtkYf999UsQL_eGMK8wxQ";

        public RefreshTokenService(string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate? next)
        {
            try
            {
                var code = context.Request.Query["code"].ToString();
                var state = context.Request.Query["state"].ToString();

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("<b>ERROR:  No code and/or state received!</b>");
                    throw new Exception("ERROR:  Request received without code and/or state!");
                }
                RestRequest restRequest = new RestRequest("/api/v1/access_token", Method.POST);

                var client = new RestClient("https://www.reddit.com");
                client.Authenticator = new HttpBasicAuthenticator(AppId, AppSecret);

                restRequest.AddParameter("grant_type", "authorization_code");
                restRequest.AddParameter("code", code);
                restRequest.AddParameter("redirect_uri", "https://localhost:7232/" + "auth/reddit/callback");

                var response = client.Execute(restRequest);
                string content = response.Content;
                var oAuthToken = JsonConvert.DeserializeObject<AuthTokens>(content);

                RedditClient reddit = new RedditClient(appId: AppId, appSecret: AppSecret, refreshToken: oAuthToken.refresh_token, accessToken: oAuthToken.access_token);
                User me = reddit.Account.Me;
                var test = me.GetPostHistory();
                var postX = reddit.User("yumekon").GetPostHistory(sort: "top");
                // send response to client
                //context.Response.StatusCode = 200;
                //await context.Response.WriteAsync($"AccessToken: {oAuthToken.access_token}, RefreshToken: {oAuthToken.refresh_token}");
                //await next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }


    }
}
