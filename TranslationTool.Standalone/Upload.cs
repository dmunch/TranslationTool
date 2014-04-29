
using System;
namespace TranslationTool.Standalone
{
	class Uploader
	{
		IO.Google.Drive drive;
		public Uploader(IO.Google.Drive drive)
		{
			this.drive = drive;
		}
	
		public void Upload(UploadOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.ModuleName) && options.Batch)
			{
				Console.WriteLine("Batch upload all modules found in {0}", options.ResXDir);
				UploadBatch(options);

				return;
			}

			Console.Write("Uploading {0} ...", options.ModuleName);

			var folder = drive.FindFolder(options.GDriveFolder);

			var t = IO.ResX.FromResX(options.ResXDir, options.ModuleName, "en");
			
			string tempPath = System.IO.Path.GetTempPath();
			string xlsFilename = System.IO.Path.Combine(tempPath, options.ModuleName + ".xls");
			IO.Xls.ToXLS(t, xlsFilename);
								
			//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. 
			//Hence we generate Xls files and convert them to XlsX files (using LibreOffice) as a workaround... 			
			string xlsx = IO.Xls.ToXlsX(xlsFilename, options.SOfficePath);
			drive.UploadXlsx(options.ModuleName, xlsx, folder);
			
			Console.WriteLine("Done");			
		}

		protected void UploadBatch(UploadOptions options)
		{
			var folder = drive.FindFolder(options.GDriveFolder);

			var t = IO.Collection.ResX.FromResX(options.ResXDir, "en");			
			string tempPath = System.IO.Path.GetTempPath();

			foreach (var file in IO.Collection.XlsCollection.ToDir(t, tempPath))
			{
				Console.Write("Uploading {0} ...", file.Value.Name);
		
				//I know this looks stupid, but Google Docs would'nt accept our generated XlsX files. 
				//Hence we generate Xls files and convert them to XlsX files (using LibreOffice) as a workaround... 			
				string file2 = IO.Xls.ToXlsX(file.Key, options.SOfficePath);
				drive.UploadXlsx(file.Value.Name, file2, folder);

				Console.WriteLine("Done");
			}
			return;
		}
	}
}
