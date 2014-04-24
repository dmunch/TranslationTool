using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Drive.v2.Data;
using TranslationTool.Helpers;

namespace TranslationTool.IO.Provider.Google
{
	public class GoogleTranslationProject : ITranslationProject
	{
		Dictionary<string, TranslationModule> Cache;
		Dictionary<string, File> ModuleFileCache;

		string CachingDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\GoogleTranslationProjectCache";
		string GDriveFolder;

		public GoogleTranslationProject(string gDriveFolder = "Visual Studio Translations")
		{
			this.GDriveFolder = gDriveFolder;
			
			CheckCacheDirectories();

			this.Cache = new Dictionary<string, TranslationModule>();
		}


		public ILogging Logging { get; set; }
		public void Update()
		{
			this.ModuleFileCache = UpdateFiles().ToDictionary(file => file.Title);		
		}

		protected void CheckCacheDirectories()
		{
			if (!System.IO.Directory.Exists(CachingDir))
			{
				System.IO.Directory.CreateDirectory(CachingDir);
			}

			CachingDir += @"\" + this.GDriveFolder;

			if (!System.IO.Directory.Exists(CachingDir))
			{
				System.IO.Directory.CreateDirectory(CachingDir);
			}
		}

		protected IEnumerable<File> UpdateFiles()
		{
			ILogging logging = this.Logging ?? new ConsoleLogging();

			var folder = IO.Google.Drive.FindFolder(this.GDriveFolder);
			var files = IO.Google.Drive.FindSpreadsheetFiles(folder).Where(file => !(file.ExplicitlyTrashed ?? false));

			var filesGroup = files.GroupBy(file => file.Title);

			var cachedFiles = System.IO.Directory.GetFiles(CachingDir, "*.xlsx");
			var cachedFilesAccessDate = cachedFiles.ToDictionary(file => System.IO.Path.GetFileNameWithoutExtension(file),
																 file => System.IO.Directory.GetLastWriteTimeUtc(file));

			foreach (var file in files)
			{
				//var modifiedDate = FromRFC3339(file.ModifiedDate ?? file.CreatedDate);
				var modifiedDate = file.ModifiedDate ?? file.CreatedDate;
				
				if (!cachedFilesAccessDate.ContainsKey(file.Title))
				{
					//file does'nt exist yet, download it
					logging.Write("File {0} doesn't exist yet, downloading it...", file.Title);
					DownloadFile(file);
					logging.WriteLine(".Done.");
				}
				else if (cachedFilesAccessDate[file.Title] < modifiedDate)
				{
					//file's been updated, download it
					logging.Write("File {0} with local date {1} has been updated on {2}, downloading it...", file.Title, cachedFilesAccessDate[file.Title], modifiedDate);
					DownloadFile(file);
					logging.WriteLine(".Done.");
				}
				else
				{
					logging.WriteLine("File {0} with local date {1} is up to date.", file.Title, cachedFilesAccessDate[file.Title]);
				}
			}

			return files;
		}

		protected static DateTime FromRFC3339(string dateString)
		{
			return System.Xml.XmlConvert.ToDateTime(dateString, System.Xml.XmlDateTimeSerializationMode.Utc);
		}

		protected string GetLocalFileName(File file)
		{
			return CachingDir + @"\" + file.Title + ".xlsx";
		}

		protected void DownloadFile(File file)
		{
			var memStream = IO.Google.Drive.DownloadFile(file, true);
			var fileName = GetLocalFileName(file);

			if(System.IO.File.Exists(fileName))
				System.IO.File.Delete(fileName);

			using (var fileStream = System.IO.File.Create(fileName))
			{
				memStream.CopyTo(fileStream);
			}
			System.IO.File.SetLastWriteTimeUtc(fileName, file.ModifiedDate ?? file.CreatedDate.Value);
		}

		public IEnumerable<string> ModuleNames
		{
			get { return this.ModuleFileCache.Keys; }
		}

		public IEnumerable<ITranslationModule> Modules
		{
			get 
			{
				//eager load all modules in cache
				foreach (var moduleFileCacheKvp in ModuleFileCache)
				{
					if (Cache.ContainsKey(moduleFileCacheKvp.Key)) continue;

					var tpGoogle = XlsX.FromXLSX(moduleFileCacheKvp.Key, "en", GetLocalFileName(moduleFileCacheKvp.Value));

					if (tpGoogle != null)
						Cache.Add(moduleFileCacheKvp.Key, tpGoogle);
				}

				return Cache.Values;
			}
		}

		public ITranslationModule this[string moduleName]
		{
			get 
			{
				if (Cache.ContainsKey(moduleName))
					return Cache[moduleName];

				if (!ModuleFileCache.ContainsKey(moduleName))
					return null;

				//var tpGoogle = GDataSpreadSheet.FromGDoc(moduleName);
				var tpGoogle = XlsX.FromXLSX(moduleName, "en", GetLocalFileName(ModuleFileCache[moduleName]));

				if(tpGoogle != null)
					Cache.Add(moduleName, tpGoogle);

				return tpGoogle;
			}
		}

		public void Add(ITranslationModule module)
		{
			throw new NotImplementedException();
		}

		public void SyncWith(ITranslationProject other)
		{
			throw new NotImplementedException();
		}
	}
}
