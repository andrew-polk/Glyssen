﻿using System;
using System.IO;
using System.Xml.Serialization;
using Glyssen.Properties;
using SIL.Xml;

namespace Glyssen
{
	[XmlRoot("ProtoscriptGeneratorMetadata")]
	public class ApplicationMetadata
	{
		private const string kFilename = "ApplicationMetadata.xml";

		[XmlAttribute("dataVersion")]
		public int DataVersion { get; set; }

		public static ApplicationMetadata Load(out Exception exception)
		{
			exception = null;

			ApplicationMetadata metadata;
			if (!File.Exists(FilePath))
			{
				metadata = new ApplicationMetadata();
				if (!Directory.Exists(Program.BaseDataFolder))
				{
					// In production, Installer is responsible for creating the base data folder.
					// The version number will be initially set to 0, but since their won't be any
					// projects to migrate, the migrator won't do anything but set the version number.
					// However, on a developer machine (or in the event that a user has blown away or
					// renamed the folder), we need to force its creation now.
					Directory.CreateDirectory(Program.BaseDataFolder);
					metadata.DataVersion = Settings.Default.DataFormatVersion;
					metadata.Save();
				}
			}
			else
				metadata = XmlSerializationHelper.DeserializeFromFile<ApplicationMetadata>(FilePath, out exception);
			return metadata;
		}

		public void Save()
		{
			XmlSerializationHelper.SerializeToFile(FilePath, this);
		}

		private static string FilePath
		{
			get { return Path.Combine(Program.BaseDataFolder, kFilename); }
		}
	}
}
