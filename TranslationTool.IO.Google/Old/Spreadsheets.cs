using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Microsoft.Win32;
using System;

#if false 

namespace TranslationTool.IO.Google
{
	class Spreadsheets1
	{
		public static SpreadsheetsService GetService()
		{
			var spreadsheetsService = new SpreadsheetsService(DriveCredentialsService.ApplicationName);
			spreadsheetsService.RequestFactory = GetRequestFactory();

			return spreadsheetsService;
		}

		public static void SaveSate(RegistryKey registryKey, OAuth2Parameters parameters)
		{
			registryKey.SetValue("AccessToken", parameters.AccessToken);
			registryKey.SetValue("AccessCode", parameters.AccessCode);
			registryKey.SetValue("RefreshToken", parameters.RefreshToken);
			registryKey.SetValue("TokenExpiry", parameters.TokenExpiry.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}

		public static OAuth2Parameters LoadSate(RegistryKey registryKey)
		{
			OAuth2Parameters parameters = new OAuth2Parameters();

			using (var stream = DriveCredentialsService.ClientJson)
			{
				//https://developers.google.com/google-apps/spreadsheets/#authorizing_requests_with_oauth_20
				//https://developers.google.com/accounts/docs/OAuth2InstalledApp

				var secrets = GoogleClientSecrets.Load(stream).Secrets;
				parameters.ClientId = secrets.ClientId;
				parameters.ClientSecret = secrets.ClientSecret;			
			}

			parameters.Scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";			
			parameters.AccessToken = (string) registryKey.GetValue("AccessToken", null);
			parameters.AccessCode = (string)registryKey.GetValue("AccessCode", null);
			parameters.RefreshToken = (string)registryKey.GetValue("RefreshToken", null);
			string expiryString = (string)registryKey.GetValue("TokenExpiry", null);

			parameters.TokenExpiry = expiryString != null ? DateTime.Parse(expiryString, System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-1);
			return parameters;
		}


		public static void Authorize(OAuth2Parameters parameters)
		{
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
		}

		public static GOAuth2RequestFactory GetRequestFactory()
		{
			using(var regKey = Registry.CurrentUser.CreateSubKey("Tyr-TMS"))
			{
			 return GetRequestFactory(regKey);
			}			
		}

		public static GOAuth2RequestFactory GetRequestFactory(RegistryKey registryKey)
		{
			var parameters = LoadSate(registryKey);

			if (parameters.TokenExpiry >= DateTime.Now && parameters.RefreshToken != null)
			{
				OAuthUtil.RefreshAccessToken(parameters);
			}
			else
			{
				Authorize(parameters);
			}
			SaveSate(registryKey, parameters);
			return new GOAuth2RequestFactory(null, DriveCredentialsService.ApplicationName, parameters);
		}
	}
}
#endif