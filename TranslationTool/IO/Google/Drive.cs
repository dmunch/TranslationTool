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

		public static void UploadXlsx(string name, string fileName)
		{
			using (System.IO.FileStream xlsStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
			{
				var driveService = Drive.GetService();

				var file = new File();

				file.Title = name;
				file.Description = string.Format("Created via {0} at {1}", Drive.ApplicationName, DateTime.Now.ToString());
				file.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				var request = driveService.Files.Insert(file, xlsStream, file.MimeType);

				request.Convert = true;

				var r = request.UploadAsync().Result;
			}
		}		

    }
}
