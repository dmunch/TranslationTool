﻿using System;
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
	public class Drive
	{
		public static string ApplicationName = "Lucca TMS";

		public static System.IO.Stream ClientJson
		{
			get
			{
				return Assembly.GetCallingAssembly().GetManifestResourceStream("TranslationTool.IO.Google.client_secrets.json");
			}
		}

		public static byte[] PrivateKey
		{
			get
			{
				var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("TranslationTool.IO.Google.ServiceAccountPrivateKey.p12");
				using (var br = new System.IO.BinaryReader(stream))
				{
					return br.ReadBytes((int)stream.Length);
				}
			}
		}
		public static DriveService GetService()
		{			
			return GetService(GetUserCredential());
		}

		public static UserCredential GetUserCredential()
		{
			GoogleWebAuthorizationBroker.Folder = "Drive.Sample";
			UserCredential credential;

			using (var stream = ClientJson)
			//using (var stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var secrets = GoogleClientSecrets.Load(stream).Secrets;
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }, "user", CancellationToken.None).Result;
			}

			return credential;
		}

		public static ServiceAccountCredential GetServiceAccountCredential()
		{
			String serviceAccountEmail = "249650506594-3dcgpm1qve3pmbblo1js6o7t8ebhckol@developer.gserviceaccount.com";

			var certificate = new X509Certificate2(PrivateKey, "notasecret", X509KeyStorageFlags.Exportable);
			ServiceAccountCredential credential = new ServiceAccountCredential(
			   new ServiceAccountCredential.Initializer(serviceAccountEmail)
			   {
				   Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }
			   }.FromCertificate(certificate));

			return credential;
		}
		
		public static DriveService GetServiceAccountService()
		{			
			return GetService(GetServiceAccountCredential());
		}

		internal static DriveService GetService(IConfigurableHttpClientInitializer credential)
		{
			var auth = new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Drive API Sample",
			};
			// Create the service.
			
			var driveService = new DriveService(auth);
			//driveService.HttpClient.Timeout = new TimeSpan(0, 1, 0);
			driveService.HttpClient.Timeout = new TimeSpan(0, 0, 10);
			return driveService;
		}			

		public static void UploadXlsx(string name, string fileName, File folder = null)
		{
			using (System.IO.FileStream xlsStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
			{
				var driveService = Drive.GetService();

				var file = new File();

				file.Title = name;
				file.Description = string.Format("Created via {0} at {1}", Drive.ApplicationName, DateTime.Now.ToString());
				file.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				if (folder != null)
				{
					var parRef = new ParentReference();
					parRef.Id = folder.Id;
					file.Parents = new List<ParentReference>();
					file.Parents.Add(parRef);
				}

				var request = driveService.Files.Insert(file, xlsStream, file.MimeType);

				request.Convert = true;
				
				var r = request.UploadAsync().Result;
			}
		}		

		public static File FindFolder(string folderName)
		{
			using (var service = Drive.GetService())
			{
				var listRequest = service.Files.List();
				listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.folder' and title = '{0}'", folderName);

				var result = listRequest.Execute();
				
				return result.Items.First();
			}
		}

		public static IList<File> FindSpreadsheetFiles(File folder)
		{
			using (var service = Drive.GetService())
			{
				var listRequest = service.Files.List();
				listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.spreadsheet' and '{0}' in parents and trashed = false", folder.Id);

				var result = listRequest.Execute();

				return result.Items;
			}
		}

		public static File FindSpreadsheetFile(string name)
		{
			using (var service = Drive.GetService())
			{
				var listRequest = service.Files.List();
				listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.spreadsheet' and title = '{0}' and trashed = false", name);

				var result = listRequest.Execute();

				return result.Items.First();
			}
		}
			
		public static System.IO.Stream DownloadFile(File file, bool asXlsx = true)
		{
			
			var downloadUrl = asXlsx ? file.ExportLinks["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] : file.DownloadUrl;

			if (!String.IsNullOrEmpty(downloadUrl))
			{
				try
				{
					//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(downloadUrl));
					//authenticator.ApplyAuthenticationToRequest(request);
					/*
					var request = Spreadsheets.GetRequestFactory().CreateRequest(GDataRequestType.Query, new Uri(downloadUrl));
					request.Execute();
					
					var responseStream = request.GetResponseStream();
					var memStream = new System.IO.MemoryStream();
					responseStream.CopyTo(memStream);
		
					memStream.Seek(0, System.IO.SeekOrigin.Begin);
					return memStream;
					*/

					using (var service = Drive.GetService())
					{
						//var bytes = service.HttpClient.GetByteArrayAsync(downloadUrl).Result;
						return service.HttpClient.GetStreamAsync(downloadUrl).Result;
						//return new System.IO.MemoryStream(bytes);
					}					
				}
				catch (Exception e)
				{
					Console.WriteLine("An error occurred: " + e.Message);
					return null;
				}
			}
			else
			{
				// The file doesn't have any content stored on Drive.
				return null;
			}

		}
    }

	public class Drive2
	{		
		DriveService service;		
		IHttpExecuteInterceptor credential;

		public Drive2(UserCredential credential)
			: this(credential, credential)
		{			
		}
		public Drive2(ServiceAccountCredential credential)
			: this(credential, credential)
		{			
		}

		protected Drive2(IConfigurableHttpClientInitializer credential, IHttpExecuteInterceptor credential2)
		{
			this.service = Drive.GetService(credential);
			this.credential = credential2;			
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
				file.Description = string.Format("Created via {0} at {1}", Drive.ApplicationName, DateTime.Now.ToString());
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