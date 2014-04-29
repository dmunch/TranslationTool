using TranslationTool.IO.Google;
using System.Reflection;

namespace TranslationTool.Standalone.Auth
{
	public class ServiceAccountApplication : IServiceAccountApplication
	{
		public string ApplicationName { get; protected set; }

		public byte[] PrivateKey
		{
			get
			{
				//load private key for service account from assemblie resources.
				//Note: you have to add your own private key, this is only an example

				var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("TranslationTool.Standalone.ServiceAccountPrivateKey.p12");
				using (var br = new System.IO.BinaryReader(stream))
				{
					return br.ReadBytes((int)stream.Length);
				}
			}
		}

		public string ServiceAccountMail { get; set; }

		public ServiceAccountApplication()
		{
			ApplicationName = "Lucca TMS";
			ServiceAccountMail = "enter-your-own-service-account-here@developer.gserviceaccount.com";
		}
	}
}
