using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace TranslationTool.IO.Google
{
	class Spreadsheets
	{
		public static SpreadsheetsService GetService()
		{
			GOAuth2RequestFactory requestFactory;

			using (var stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var secrets = GoogleClientSecrets.Load(stream).Secrets;

				//https://developers.google.com/google-apps/spreadsheets/#authorizing_requests_with_oauth_20
				//https://developers.google.com/accounts/docs/OAuth2InstalledApp

				OAuth2Parameters parameters = new OAuth2Parameters();
				parameters.ClientId = secrets.ClientId;
				parameters.ClientSecret = secrets.ClientSecret;
				parameters.Scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";
				//parameters.RedirectUri = "urn:ietf:wg:oauth:2.0:oob";
				parameters.RedirectUri = "http://localhost:8080";

				string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);

				var listener = new HttpListener();
				listener.Prefixes.Add("http://localhost:8080/");
				listener.Start();
				var contextAsync = listener.GetContextAsync();
				System.Diagnostics.Process.Start(authorizationUrl);

				contextAsync.Wait();
				parameters.AccessCode = contextAsync.Result.Request.QueryString["code"];

				HttpListenerResponse response = contextAsync.Result.Response;
				// Construct a response.
				string responseString = "<HTML><head><script>window.open('', '_self', ''); /* bug fix chrome*/ window.close();</script></head></HTML>";
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
				// Get a response stream and write the response to it.
				response.ContentLength64 = buffer.Length;
				System.IO.Stream output = response.OutputStream;
				output.Write(buffer, 0, buffer.Length);
				// You must close the output stream.
				output.Close();
				listener.Stop();


				//http://stackoverflow.com/questions/12077455/gdata-oauthutil-getaccesstoken-does-not-return-a-refresh-token-value
				OAuthUtil.GetAccessToken(parameters);

				requestFactory = new GOAuth2RequestFactory(null, "MySpreadsheetIntegration-v1", parameters);
			}

			var spreadsheetsService = new SpreadsheetsService(Drive.ApplicationName);
			spreadsheetsService.RequestFactory = requestFactory;

			return spreadsheetsService;
		}
	}
}
