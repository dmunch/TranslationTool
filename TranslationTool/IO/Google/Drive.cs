using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.GData.Spreadsheets;
using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Google.GData.Client;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using System.Net;
using Google.Apis.Authentication;

namespace TranslationTool.IO.Google
{
	public class Drive
	{
		public static string ApplicationName = "Lucca TMS";

		public static DriveService GetService()
		{
			GoogleWebAuthorizationBroker.Folder = "Drive.Sample";
			UserCredential credential;

			using (var stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
			{
				var secrets = GoogleClientSecrets.Load(stream).Secrets;
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive }, "user", CancellationToken.None).Result;
			}

			var auth = new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Drive API Sample",
			};
			// Create the service.
			var driveService = new DriveService(auth);

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
			using (var service = GetService())
			{
				var listRequest = service.Files.List();
				listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.folder' and title = '{0}'", folderName);

				var result = listRequest.Execute();
				
				return result.Items.First();
			}
		}

		public static IList<File> FindSpreadsheetFiles(File folder)
		{
			using (var service = GetService())
			{
				var listRequest = service.Files.List();
				listRequest.Q = String.Format("mimeType = 'application/vnd.google-apps.spreadsheet' and '{0}' in parents", folder.Id);

				var result = listRequest.Execute();

				return result.Items;
			}
		}
		
		public static System.IO.Stream DownloadFile(File file, bool asXlsx = true)
		{
			using (var service = GetService())
			{
				return DownloadFile(service.Authenticator, file, asXlsx);
			}
		}

		public static System.IO.Stream DownloadFile(IAuthenticator authenticator, File file, bool asXlsx = true)
		{
			
			var downloadUrl = asXlsx ? file.ExportLinks["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] : file.DownloadUrl;

			if (!String.IsNullOrEmpty(downloadUrl))
			{
				try
				{
					//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(downloadUrl));
					//authenticator.ApplyAuthenticationToRequest(request);
					var request = Spreadsheets.GetRequestFactory().CreateRequest(GDataRequestType.Query, new Uri(downloadUrl));
					request.Execute();
					var response = request;
					//HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					if (true)//response.StatusCode == HttpStatusCode.OK)
					{
						var responseStream = response.GetResponseStream();
						//var memStream = new System.IO.MemoryStream(new byte[responseStream.Length], true);
						var memStream = new System.IO.MemoryStream();
						//	xlsStream.Seek(0, System.IO.SeekOrigin.Begin);
						responseStream.CopyTo(memStream);

						using (var fileStream = System.IO.File.Create(@"D:\Users\login\Documents\i18n\TTTT.xlsx"))
						{
							memStream.Seek(0, System.IO.SeekOrigin.Begin);
							memStream.CopyTo(fileStream);
						}

						memStream.Seek(0, System.IO.SeekOrigin.Begin);
						return memStream;
					}
					else
					{
						Console.WriteLine(
							"An error occurred: ");//+ response.StatusDescription);
						return null;
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
}
