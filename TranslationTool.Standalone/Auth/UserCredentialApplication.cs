using TranslationTool.IO.Google;
using System.Reflection;
using System;
using System.IO;

namespace TranslationTool.Standalone.Auth
{
	public class UserCredentialApplicationFromAssembly : IUserCredentialApplication
	{
		public string ApplicationName { get; protected set; }

		public System.IO.Stream ClientJson
		{
			get
			{
				return Assembly.GetCallingAssembly().GetManifestResourceStream("TranslationTool.IO.Google.Secrets.client_secrets.json");				
			}
		}

		public UserCredentialApplicationFromAssembly()
		{
			ApplicationName = "Lucca TMS";
		}
	}

	public class UserCredentialApplicationFromFile : IUserCredentialApplication
	{
		public string ApplicationName { get; protected set; }

		public System.IO.Stream ClientJson
		{
			get
			{
				string path = AssemblyDirectory(Assembly.GetCallingAssembly());
				path = System.IO.Path.Combine(path, "client_secrets.json");
				MemoryStream memStream = new MemoryStream();

				using (FileStream fileStream = File.OpenRead(path))
				{					
					memStream.SetLength(fileStream.Length);
					fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
				}

				return memStream;
			}
		}

		public UserCredentialApplicationFromFile()
		{
			ApplicationName = "Lucca TMS";
		}

		public static string AssemblyDirectory(Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}
	}
}
