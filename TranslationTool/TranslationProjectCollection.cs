using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TranslationTool
{
    public class TranslationProjectCollection
    {
        IEnumerable<string> ProjectNames;
        Dictionary<string, TranslationProject> Projects;
        string Directory;


        public TranslationProjectCollection(IEnumerable<string> projectNames, string directory)
        {
            this.ProjectNames = projectNames;
            this.Directory = directory;

            Projects = new Dictionary<string,TranslationProject>();

            foreach(var pName in this.ProjectNames)
                Projects.Add(pName, TranslationProject.FromResX(this.Directory, pName));

        }

        public void ToCSV(string targetDir)
        {
            StringBuilder sb = new StringBuilder();
            string fileName = "";
            foreach (var kvp in Projects)
            {
                sb.Append("ns:").AppendLine(kvp.Key);
                kvp.Value.ToCSV(sb, false);

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
                foreach (var l in Projects.First().Value.Languages)
                    worksheet.Cells[1, columnCount++].Value = l;

                int rowCount = 2;
                foreach (var kvp in Projects)
                {
                    worksheet.Cells[rowCount, 1].Value = "ns:" + kvp.Key;
                    rowCount++;

                    rowCount = kvp.Value.ToXLS(worksheet, rowCount);
                }
                
                package.Save();
            }

        }
    }
}
