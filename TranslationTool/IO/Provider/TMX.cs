using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TranslationTool.IO
{
	public class TMX
	{
		public static void ToTMX(TranslationModule tp, string targetDir)
		{
			var tmx = new tmx();
			var tus = new List<tu>();

			foreach (var key in tp.Keys)
			{
				var tu = new tu();
				tu.tuid = key;

				var tuvs = new List<tuv>();

				//tuvs.Add(new tuv() { lang = tp.masterLanguage.ToUpper(), lang1 = tp.masterLanguage.ToUpper(), seg = new seg() { Text = new string[] { kvp.Value } } });
				foreach (var l in tp.Languages)
				{
					if (tp.Dicts.ContainsKey(l) && tp.Dicts[l].ContainsKey(key))
						tuvs.Add(new tuv() { lang = l.ToUpper(), lang1 = l.ToUpper(), seg = new seg() { Text = new string[] { tp.Dicts[l][key] } } });
				}

				tu.tuv = tuvs.ToArray();
				tus.Add(tu);
			}

			tmx.body = tus.ToArray();
			tmx.header = new header() { srclang = "*all*", creationtool = "iLuccaTranslationTool", segtype = headerSegtype.phrase };

			var extraTypes = new Type[] { typeof(tu), typeof(tuv), typeof(seg) };
			XmlSerializer xs = new XmlSerializer(typeof(tmx), null, extraTypes, new XmlRootAttribute("tmx"), null);

			//specify encoding to add BOM
			using (var sw = new StreamWriter(targetDir + @"\" + tp.Project + ".tmx", false, new UTF8Encoding(false)))
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = false;
				settings.NewLineHandling = NewLineHandling.None;
				//xs.Serialize(sw, tmx);

				using (XmlWriter writer = XmlWriter.Create(sw, settings))
				{
					xs.Serialize(writer, tmx);
					//_serializer.Serialize(o, writer);
				}
			}
		}
        
	}
}
