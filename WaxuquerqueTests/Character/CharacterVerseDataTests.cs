﻿using System.IO;
using System.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.Scripture;
using Waxuquerque.Character;
using WaxuquerqueTests.Properties;

namespace WaxuquerqueTests.Character
{
	/// <summary>
	/// Not that these tests purport to test the GetCharacters method, but in fact that is just a simple LINQ statement;
	/// they're really testing the Load method.
	/// </summary>
	[TestFixture]
	class CharacterVerseDataTests
	{
		private static readonly int kGENbookNum = BCVRef.BookToNumber("GEN");
		private static readonly int k1SAbookNum = BCVRef.BookToNumber("1SA");
		private static readonly int kMRKbookNum = BCVRef.BookToNumber("MRK");
		private static readonly int kLUKbookNum = BCVRef.BookToNumber("LUK");
		private static readonly int kACTbookNum = BCVRef.BookToNumber("ACT");

		private ScrVers m_testVersification;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerse;

			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, Resources.TestVersification);
				m_testVersification = Versification.Table.Implementation.Load(tempFile.Path);
			}
		}

		[Test]
		public void GetCharacters_NoMatch_EmptyResults()
		{
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 1, 1).Any());
		}

		[Test]
		public void GetCharacters_One()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(kGENbookNum, 15, 20).ToList();
			Assert.AreEqual(1, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
		}

		[Test]
		public void GetCharacter_VerseBridge_StartVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 43).Single();
			Assert.AreEqual("Elizabeth", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_MiddleVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kGENbookNum, 15, 20).Single();
			Assert.AreEqual("God", character.Character);
		}

		[Test]
		public void GetCharacter_VerseBridge_EndVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kLUKbookNum, 1, 55).Single();
			Assert.AreEqual("Mary (Jesus' mother)", character.Character);
		}

		[Test]
		public void GetCharacters_ControlHasNoDataForInitialStartVerseButDoesForSecondVerse_FindsCharacterForSecondVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kACTbookNum, 11, 2, 0, 3).Single();
			Assert.AreEqual("believers, circumcised", character.Character);
		}

		[Test]
		public void GetCharacters_ControlHasNoDataForInitialStartVerseButDoesForThirdVerse_FindsCharacterForThirdVerse()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(kACTbookNum, 11, 1, 0, 3).Single();
			Assert.AreEqual("believers, circumcised", character.Character);
		}

		[Test] public void GetCharacters_MoreThanOneWithNoDuplicates_ReturnsAll()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(kMRKbookNum, 6, 24).ToList();
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Herodias' daughter"));
		}

		[Test]
		public void GetCharacters_MultipleCharactersInOneButNotAllVerses_ReturnsSingleCharacter()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 6, 4, 0, 6).Single();
			Assert.AreEqual("Philistine priests and diviners", character.Character);
		}

		[Test]
		public void GetCharacters_MultipleCharactersInMultipleVerses_ReturnsAmbiguous()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 8, 21, 0, 22);
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Samuel"));
		}

		[Test]
		public void GetCharacters_MultipleCharactersInMultipleVerses_NoCharacterInInitialStartVerse_ReturnsAmbiguous()
		{
			var characters = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 8, 20, 0, 22);
			Assert.AreEqual(2, characters.Count());
			Assert.AreEqual(1, characters.Count(c => c.Character == "God"));
			Assert.AreEqual(1, characters.Count(c => c.Character == "Samuel"));
		}

		[Test]
		public void GetCharacters_SingleCharactersInMultipleVerses_NoCharacterInInitialStartVerse_ReturnsFirstUniqueCharacter()
		{
			var character = ControlCharacterVerseData.Singleton.GetCharacters(k1SAbookNum, 9, 4, 0, 6).Single();
			Assert.AreEqual("Saul", character.Character);
		}

		[Test]
		public void GetCharacters_NonEnglishVersification()
		{
			// Prove the test is valid
			var character = ControlCharacterVerseData.Singleton.GetCharacters(1, 32, 6).Single();
			Assert.AreEqual("messengers of Jacob", character.Character);
			var verseRef = new VerseRef(1, 32, 6, ScrVers.English);
			verseRef.ChangeVersification(m_testVersification);
			Assert.AreEqual(1, verseRef.BookNum);
			Assert.AreEqual(32, verseRef.ChapterNum);
			Assert.AreEqual(7, verseRef.VerseNum);

			// Run the test
			character = ControlCharacterVerseData.Singleton.GetCharacters(verseRef.BookNum, verseRef.ChapterNum, verseRef.VerseNum, versification: m_testVersification).Single();
			Assert.AreEqual("messengers of Jacob", character.Character);
		}
	}
}
