using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TranslationTool.IO;

namespace TranslationTool
{
    public class TranslationProjectCollection
    {
        IEnumerable<string> ProjectNames;
        public Dictionary<string, TranslationProject> Projects;
        string Directory;

        protected TranslationProjectCollection()
        {
            this.Projects = new Dictionary<string, TranslationProject>();
        }
        
        public static TranslationProjectCollection FromResX(IEnumerable<string> projectNames, string directory, string masterLanguage)
        {
            var tpc = new TranslationProjectCollection();

            tpc.ProjectNames = projectNames;
            tpc.Directory = directory;

            foreach (var pName in tpc.ProjectNames)
                tpc.Projects.Add(pName, ResX.FromResX(tpc.Directory, pName, masterLanguage));

            return tpc;
        }

        public static TranslationProjectCollection FromCSV(IEnumerable<string> projectNames, string fileName, string masterLanguage)
        {
            var tpc = new TranslationProjectCollection();

            tpc.ProjectNames = projectNames;            
            

            foreach (var pName in tpc.ProjectNames)
				tpc.Projects.Add(pName, CSV.FromCSV(fileName, pName, masterLanguage));

            return tpc;
        }

        public void ToResX(string targetDir)
        {
            foreach (var tp in Projects)
				ResX.ToResX(tp.Value, targetDir);
        }

        public void ToCSV(string targetDir)
        {
            StringBuilder sb = new StringBuilder();
            string fileName = "";
            foreach (var kvp in Projects)
            {
                sb.Append("ns:").AppendLine(kvp.Key);
                CSV.ToCSV(kvp.Value, sb, false);
				
                fileName += "_" + kvp.Key;
            }
            
            using (StreamWriter outfile = new StreamWriter(targetDir + @"\" + fileName + ".csv", false, Encoding.UTF8))
                outfile.Write(sb.ToString());
        }

        public void ToXLS(string targetDir)
        {
            string fileName = @"\";
            foreach(var kvp in Projects)
                fileName += "_" + kvp.Key;
            fileName += ".xlsx";

            FileInfo newFile = new FileInfo(targetDir + fileName);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(targetDir + fileName);
            }

            using (var package = new OfficeOpenXml.ExcelPackage(newFile))
            {
                var worksheet = package.Workbook.Worksheets.Add("Traductions");

                //write header
                int columnCount = 1;
                worksheet.Cells[1, columnCount++].Value = "";
                worksheet.Cells[1, columnCount++].Value = Projects.First().Value.MasterLanguage;

                foreach (var l in Projects.First().Value.Languages)
                    worksheet.Cells[1, columnCount++].Value = l;

                int rowCount = 2;
                foreach (var kvp in Projects)
                {
                    worksheet.Cells[rowCount, 1].Value = "ns:" + kvp.Key;
                    rowCount++;

                    rowCount = Xls.ToXLS(kvp.Value, worksheet, rowCount);
                }
                
                package.Save();
            }

        }

        public void SyncWith(TranslationProjectCollection tpc)
        {
            foreach (var tp in Projects)
                if(tpc.Projects.ContainsKey(tp.Key))
                    tp.Value.SyncWith(tpc.Projects[tp.Key]);
        }
    }
}
