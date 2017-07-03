using System.Collections.Generic;
using System.Diagnostics;
using Glyssen.Shared.Script;
using SIL.Xml;

namespace Glyssen
{
	public static class ScriptExporter
	{
		public static void MakeGlyssenScriptFile(Project project, IEnumerable<ProjectExporter.ExportRow> data, string outputPath)
		{
			var gs = new GlyssenScript(project.Metadata) {Script = new Script {Books = new List<ScriptBook>()}};

			int iClipFileLink = ProjectExporter.GetColumnIndex(ExportColumn.ClipFileLink, project);

			string bookCode = null;
			int blockId = 1;
			List<ScriptChapter> chapters = new List<ScriptChapter>();
			int chapter = 0;
			List<ScriptBlock> blocks = new List<ScriptBlock>();
			foreach (var row in data)
			{
				string blockBookCode = row.BookId;
				int blockChapterNumber = row.ChapterNumber;
				string blockCharacterId = row.CharacterId;

				if (blockChapterNumber != chapter)
				{
					chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
					blocks = new List<ScriptBlock>();
					blockId = 1;
					chapter = blockChapterNumber;
				}
				if (bookCode != null && blockBookCode != bookCode)
				{
					gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});
					chapters = new List<ScriptChapter>();
					bookCode = blockBookCode;
					chapter = 0;
				}
				if (!project.IncludeCharacter(blockCharacterId))
					continue;

				// I don't see any point in exporting a block with no vernacular text
				string vernacularText = row.Text;
				if (!string.IsNullOrWhiteSpace(vernacularText))
				{
					var gsBlock = new ScriptBlock
					{
						Character = row.CharacterId,
						File = row.ClipFilePath,
						Id = blockId++,
						Primary =
							new Reference
							{
								LanguageCode = project.ReferenceText.LanguageLdml,
								Text = row.PrimaryReferenceText
							},
						Secondary =
							new Reference
							{
								LanguageCode = project.ReferenceText.SecondaryReferenceText.LanguageLdml,
								Text = row.SecondaryReferenceText
							},
						Tag = row.StyleTag,
						Vernacular = new Vernacular {Text = vernacularText},
						Verse = row.InitialStartVerseNumber.ToString()
					};
					var actor = row.VoiceActor;
					gsBlock.Actor = !string.IsNullOrEmpty(actor) ? actor : "unassigned";
					Debug.Assert(gsBlock.Actor != "unassigned");

					blocks.Add(gsBlock);
				}
				bookCode = blockBookCode;
				chapter = blockChapterNumber;
			}

			chapters.Add(new ScriptChapter {Id = chapter, Blocks = blocks});
			gs.Script.Books.Add(new ScriptBook {Id = bookCode, Chapters = chapters});

			XmlSerializationHelper.SerializeToFile(outputPath, gs);
		}
	}
}