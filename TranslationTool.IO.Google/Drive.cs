using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Http;
using Google.Apis.Services;

namespace TranslationTool.IO.Google
{
	public interface IApplication
	{
		string ApplicationName { get; }
	}

	public interface IUserCredentialApplication : IApplication
	{
		System.IO.Stream ClientJson { get; }
	}

	public interface IServiceAccountApplication : IApplication
	{
		byte[] PrivateKey { get; }
		string ServiceAccountMail { get; }
	}
	
	public class DriveCredentialsService
	{			
		public static UserCredential GetUserCredential(IUserCredentialApplication ucApp)
		{
			GoogleWebAuthorizationBroker.Folder = "Drive.Sample";
			UserCredential credential;

			using (var stream = ucApp.ClientJson)			
			{
				var secrets = GoogleClientSecrets.Load(stream).Secrets;
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }, "user", CancellationToken.None).Result;
			}

			return credential;
		}

		public static ServiceAccountCredential GetServiceAccountCredential(IServiceAccountApplication saApp)
		{
			var certificate = new X509Certificate2(saApp.PrivateKey, "notasecret", X509KeyStorageFlags.Exportable);
			ServiceAccountCredential credential = new ServiceAccountCredential(
			   new ServiceAccountCredential.Initializer(saApp.ServiceAccountMail)
			   {
				   Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }
			   }.FromCertificate(certificate));

			return credential;
		}
		
		internal static DriveService GetService(IConfigurableHttpClientInitializer credential, IApplication app)
		{
			var auth = new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = app.ApplicationName,
			};
			// Create the service.
			
			var driveService = new DriveService(auth);
			//driveService.HttpClient.Timeout = new TimeSpan(0, 1, 0);
			driveService.HttpClient.Timeout = new TimeSpan(0, 0, 10);
			return driveService;
		}

		public static DriveService GetService(IUserCredentialApplication ucApp)
		{
			return GetService(GetUserCredential(ucApp), ucApp);
		}

		public static DriveService GetService(IServiceAccountApplication saApp)
		{
			return GetService(GetServiceAccountCredential(saApp), saApp);
		}
	}

	public class Drive
	{		
		DriveService service;		
		IHttpExecuteInterceptor credential;
		IApplication application;

		public Drive(IUserCredentialApplication ucApp)
		{
			credential = DriveCredentialsService.GetUserCredential(ucApp);
			service = DriveCredentialsService.GetService(ucApp);

			application = ucApp;
		}

		public Drive(IServiceAccountApplication saApp)
		{
			credential = DriveCredentialsService.GetServiceAccountCredential(saApp);
			service = DriveCredentialsService.GetService(saApp);

			application = saApp;
		}

		protected HttpClient GetHttpClient()
		{
			var httpClient = new HttpClientFactory().CreateHttpClient(new CreateHttpClientArgs
			{
				ApplicationName = "Downloader",
			});

			httpClient.MessageHandler.ExecuteInterceptors.Add(credential);
			httpClient.Timeout = new TimeSpan(0, 0, 10);

			return httpClient;
		}

		public void UploadXlsx(string name, string fileName, File folder = null)
		{
			using (System.IO.FileStream xlsStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
			{
				var file = new File();

				file.Title = name;
				file.Description = string.Format("Created via {0} at {1}", application.ApplicationName, DateTime.Now.ToString());
				file.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				if (folder != null)
				{
					var parRef = new ParentReference();
					parRef.Id = folder.Id;
					file.Parents = new List<ParentReference>();
					file.Parents.Add(parRef);
				}

				var request = service.Files.Insert(file, xlsStream, file.MimeType);

				request.Convert = true;

				var r = request.UploadAsync().Result;
			}
		}

		public File FindFolder(string folderName)
		{
			var listRequest = service.Files.List();
			listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.folder' and title = '{0}'", folderName);

			var result = listRequest.Execute();

			return result.Items.First();
		}

		public IList<File> FindSpreadsheetFiles(File folder)
		{
			var listRequest = service.Files.List();
			listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.spreadsheet' and '{0}' in parents and trashed = false", folder.Id);

			var result = listRequest.Execute();

			return result.Items;
		}

		public File FindSpreadsheetFile(string name)
		{
			var listRequest = service.Files.List();
			listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.spreadsheet' and title = '{0}' and trashed = false", name);

			var result = listRequest.Execute();

			return result.Items.First();
		}

		public System.IO.Stream DownloadFile(File file, bool asXlsx = true)
		{

			var downloadUrl = asXlsx ? file.ExportLinks["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] : file.DownloadUrl;
		
			if (!String.IsNullOrEmpty(downloadUrl))
			{

				//for each download we need a new httpClient, otherwise Google API will timeout
				using (var httpClient = GetHttpClient())
				{
					//Can't use this, since stream gets unavailable once httpClient was disposed
					//stream = httpClient.GetStreamAsync(downloadUrl).Result;

					//so we get the bytes and construct the stream ourselfs
					var bytes = service.HttpClient.GetByteArrayAsync(downloadUrl).Result;
					return new System.IO.MemoryStream(bytes);
				}
			}
			else
			{
				// The file doesn't have any content stored on Drive.
				return null;
			}

		}
	}
}
