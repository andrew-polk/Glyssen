﻿using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Rules;
using Glyssen.VoiceActor;
using GlyssenTests.Properties;
using NUnit.Framework;
using SIL.Scripture;

namespace GlyssenTests.Rules
{
	[TestFixture]
	class CharacterGroupsAdjusterTests
	{
		private Project m_testProject;

		[TestFixtureSetUpAttribute]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Resources.TestCharacterVerseOct2015;
			CharacterDetailData.TabDelimitedCharacterDetailData = Resources.TestCharacterDetailOct2015;
			RelatedCharactersData.Source = null;
			CreateTestProject();
		}

		[SetUpAttribute]
		public void SetUp()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = true;
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = true;
			m_testProject.AvailableBooks.Single(b => b.Code == "ACT").IncludeInScript = false;
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
		}

		private void CreateTestProject()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK, TestProject.TestBook.LUK, TestProject.TestBook.ACT, TestProject.TestBook.JUD);
			// Since we're testing something that should only happen once phase 1 is complete, we simulate that by assigning all ambiguous/unknown blocks
			foreach (var block in m_testProject.IncludedBooks.SelectMany(b => b.Blocks).Where(b => b.CharacterIsUnclear()))
				block.CharacterId = "Adam";
		}
	
		private void GenerateGroups()
		{
			new CharacterGroupGenerator(m_testProject, m_testProject.GetKeyStrokesByCharacterId()).UpdateProjectCharacterGroups();
		}

		private void RenameCharacter(int bookNum, int chapter, int verse, string existingCharacterId, string newCharacterId)
		{
			var block = m_testProject.Books.Single(book => book.BookId == BCVRef.NumberToBookCode(bookNum)).GetBlocksForVerse(chapter, verse)
				.Single(b => b.CharacterId == existingCharacterId);
			block.CharacterId = newCharacterId;

			m_testProject.ProjectCharacterVerseData.Add(new CharacterVerse(new BCVRef(bookNum, chapter, verse),
				newCharacterId, string.Empty, string.Empty, true));

			if (!m_testProject.AllCharacterDetailDictionary.ContainsKey(newCharacterId))
				m_testProject.AddProjectCharacterDetail(new CharacterDetail { CharacterId = newCharacterId });
		}

		private void SetBlockCharacterToNarrator(string bookCode, int chapter, int verse, string existingCharacterId)
		{
			m_testProject.Books.Single(book => book.BookId == bookCode).GetBlocksForVerse(chapter, verse)
				.Single(b => b.CharacterId == existingCharacterId).CharacterId =
				CharacterVerseData.GetStandardCharacterId(bookCode, CharacterVerseData.StandardCharacter.Narrator);
		}

		[Test]
		public void Constructor_PerfectCoverage_NoAdditionsOrDeletions()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			GenerateGroups();
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
		}

		[Test]
		public void CharacterGroupsToRemove_EmptyGroup_ReturnsFalse()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.CharacterGroupList.CharacterGroups[0].AssignVoiceActor(m_testProject.VoiceActorList.Actors[2].Id);
			foreach (var character in m_testProject.CharacterGroupList.CharacterGroups[0].CharacterIds)
				m_testProject.CharacterGroupList.CharacterGroups[1].CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups[0].CharacterIds.Clear();
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
		}

		[Test]
		public void Constructor_CharactersAddedToProject_AdditionsButNoDeletions()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "ACT").IncludeInScript = true;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("ACT", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("Gamaliel"));
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Contains("Jesus"));
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Contains("Peter (Simon)"));
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			Assert.AreEqual(0, adjuster.CharacterGroupsToRemove.Count());
			Assert.IsTrue(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsTrue(adjuster.FullRegenerateRecommended);
			Assert.IsTrue(adjuster.GroupsAreNotInSynchWithData);
		}

		[Test]
		public void Constructor_CharactersRemovedFromProjectButNotEnoughToResultInAnEmptyGroup_AdditionsButNoDeletions()
		{
			// By keeping the number of actors really low, we guarantee that groups will have lots of characters,
			// thus more-or-less ensuring that no groups will consist only of characters no longer in use after excluding Mark from the
			// project.
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(7, 1, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.AreEqual(0, adjuster.CharacterGroupsToRemove.Count());
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsTrue(adjuster.BooksHaveBeenExcluded);
			Assert.IsTrue(adjuster.FullRegenerateRecommended);
			Assert.IsTrue(adjuster.GroupsAreNotInSynchWithData);
		}

		[Test]
		public void Constructor_TwoCharactersRenamed_FullRegenerateNotRecommended()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = false;
			GenerateGroups();

			try
			{
				RenameCharacter(41, 1, 27, "men in Capernaum synagogue", "dudes in the Capernaum teaching center");
				RenameCharacter(41, 2, 12, "everyone who saw healing of paralytic", "witnesses of paralytic's healing");

				var adjuster = new CharacterGroupsAdjuster(m_testProject);
				Assert.AreEqual(2, adjuster.CharactersNotCoveredByAnyGroup.Count());
				Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("dudes in the Capernaum teaching center"));
				Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("witnesses of paralytic's healing"));
				Assert.AreEqual(2, adjuster.CharactersNoLongerInUse.Count());
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("men in Capernaum synagogue"));
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("everyone who saw healing of paralytic"));
				Assert.AreEqual(0, adjuster.CharacterGroupsToRemove.Count());
				Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
				Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
				Assert.IsFalse(adjuster.FullRegenerateRecommended);
				Assert.IsTrue(adjuster.GroupsAreNotInSynchWithData);
			}
			finally
			{
				CreateTestProject();
			}
		}

		[Test]
		public void Constructor_FiveCharactersAddedRemovedOrRenamed_FullRegenerateRecommended()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			m_testProject.AvailableBooks.Single(b => b.Code == "LUK").IncludeInScript = false;
			GenerateGroups();

			try
			{
				RenameCharacter(41, 1, 27, "men in Capernaum synagogue", "dudes in the Capernaum teaching center");
				RenameCharacter(41, 2, 12, "everyone who saw healing of paralytic", "witnesses of paralytic's healing");
				SetBlockCharacterToNarrator("MRK", 9, 26, "many in crowd");
				SetBlockCharacterToNarrator("MRK", 5, 35, "men from Jairus' house");
				RenameCharacter(41, 7, 1,
					CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator),
					"Mephibosheth");

				var adjuster = new CharacterGroupsAdjuster(m_testProject);
				Assert.AreEqual(3, adjuster.CharactersNotCoveredByAnyGroup.Count());
				Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("dudes in the Capernaum teaching center"));
				Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("witnesses of paralytic's healing"));
				Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains("Mephibosheth"));
				Assert.AreEqual(4, adjuster.CharactersNoLongerInUse.Count());
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("men in Capernaum synagogue"));
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("everyone who saw healing of paralytic"));
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("many in crowd"));
				Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains("men from Jairus' house"));
				Assert.AreEqual(0, adjuster.CharacterGroupsToRemove.Count());
				Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
				Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
				Assert.IsTrue(adjuster.FullRegenerateRecommended);
				Assert.IsTrue(adjuster.GroupsAreNotInSynchWithData);
			}
			finally
			{
				CreateTestProject();
			}
		}

		[Test]
		public void Constructor_CharactersRemovedFromProjectLeavingGroupsThatWouldHaveNoCharacters_CharacterGroupsToRemoveNotEmpty()
		{
			// By jacking up the number of actors really high, we guarantee that most characters will end up in a group by themselves,
			// thus more-or-less ensuring that some groups will no longer contain any characters in use after excluding Mark from the
			// project.
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(99, 22, 7);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "MRK").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(adjuster.CharactersNoLongerInUse.Contains(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.IsTrue(adjuster.CharacterGroupsToRemove.Any());
			Assert.IsTrue(adjuster.GroupsAreNotInSynchWithData);
		}

		[Test]
		public void MakeMinimalAdjustments_FewAdditions_NewGroupAddedWithNewCharacters()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(adjuster.CharactersNotCoveredByAnyGroup.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var countOfCharactersNotCovered = adjuster.CharactersNotCoveredByAnyGroup.Count();
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			
			adjuster.MakeMinimalAdjustments();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			Assert.AreEqual(originalCountOfGroups + 1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator));
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.GroupNumber == newGroup.GroupNumber && g != newGroup));
			Assert.IsTrue(newGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(newGroup.CharacterIds.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.AreEqual(countOfCharactersNotCovered, newGroup.CharacterIds.Count);
		}

		[Test]
		public void MakeMinimalAdjustments_FewDeletionsAndFewGroups_CharactersRemovedFromExistingCharacterGroups()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(20, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Count > 0);
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;

			adjuster.MakeMinimalAdjustments();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.AreEqual(originalCountOfGroups, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => g.CharacterIds.Any(c => charactersNotInUse.Contains(c))));
		}

		[Test]
		public void MakeMinimalAdjustments_FewDeletionsAndManyGroups_CharactersRemovedFromExistingCharacterGroupsAndEmptyGroupsRemoved()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(90, 7, 2);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Count > 0);
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.IsTrue(groupsToRemove.Any());

			adjuster.MakeMinimalAdjustments();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.AreEqual(originalCountOfGroups - groupsToRemove.Count, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c))));
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => groupsToRemove.Contains(g)));
		}

		[Test]
		public void MakeMinimalAdjustments_CameoGroupsWithCharactersNoLongerInUse_EmptyCameoGroupsNotRemoved()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(290, 17, 8);
			GenerateGroups();
			var frankie = m_testProject.VoiceActorList.Actors.First(a => a.Gender == ActorGender.Male);
			frankie.Name = "Frankie";
			frankie.IsCameo = true;
			var michaelTheArchAngelGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Michael, archangel");
			michaelTheArchAngelGroup.AssignVoiceActor(frankie.Id);
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Contains("Michael, archangel"));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.AreEqual(1, groupsToRemove.Count);
			Assert.IsFalse(groupsToRemove.Contains(michaelTheArchAngelGroup));

			adjuster.MakeMinimalAdjustments();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.AreEqual(originalCountOfGroups - groupsToRemove.Count, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(michaelTheArchAngelGroup,
				m_testProject.CharacterGroupList.CharacterGroups.Single(g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c))));
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => groupsToRemove.Contains(g)));
			Assert.IsFalse(michaelTheArchAngelGroup.CharacterIds.Any());
			Assert.AreEqual(frankie.Id, michaelTheArchAngelGroup.VoiceActorId);
		}

		[Test]
		public void FullyRegenerateGroups_FewAdditions_GroupsRegenerated()
		{
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(9, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var addedCharacters = adjuster.CharactersNotCoveredByAnyGroup.ToList();
			Assert.IsTrue(addedCharacters.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(addedCharacters.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(addedCharacters.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;

			adjuster.FullyRegenerateGroups();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			Assert.AreEqual(originalCountOfGroups, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => !g.CharacterIds.Any()));
			Assert.IsTrue(addedCharacters.All(c => m_testProject.CharacterGroupList.GroupContainingCharacterId(c) != null));
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
		}

		[Test]
		public void FullyRegenerateGroups_FewDeletionsAndFewGroups_GroupsRegenerated()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(20, 2, 1);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Count > 0);
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;

			adjuster.FullyRegenerateGroups();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.AreEqual(0, adjuster.CharactersNoLongerInUse.Count());
			Assert.AreEqual(0, adjuster.CharactersNotCoveredByAnyGroup.Count());
			Assert.AreEqual(originalCountOfGroups, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c)))); Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
		}

		[Test]
		public void FullyRegenerateGroups_FewDeletionsAndManyGroups_GroupsRegenerated()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(190, 4, 2);
			GenerateGroups();
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Count > 0);
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.Narrator)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.ExtraBiblical)));
			Assert.IsTrue(charactersNotInUse.Contains(CharacterVerseData.GetStandardCharacterId("JUD", CharacterVerseData.StandardCharacter.BookOrChapter)));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.IsTrue(groupsToRemove.Any());

			adjuster.FullyRegenerateGroups();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.AreEqual(originalCountOfGroups - 2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c))));
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => groupsToRemove.Contains(g)));
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
		}

		[Test]
		public void FullyRegenerateGroups_CameoGroupsWithCharactersNoLongerInUse_GroupsRegenerated()
		{
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = true;
			m_testProject.VoiceActorList.Actors = CharacterGroupGeneratorTests.GetVoiceActors(290, 17, 8);
			GenerateGroups();
			var frankie = m_testProject.VoiceActorList.Actors.First(a => a.Gender == ActorGender.Male);
			frankie.Name = "Frankie";
			frankie.IsCameo = true;
			var michaelTheArchAngelGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Michael, archangel");
			michaelTheArchAngelGroup.AssignVoiceActor(frankie.Id);
			m_testProject.AvailableBooks.Single(b => b.Code == "JUD").IncludeInScript = false;
			var adjuster = new CharacterGroupsAdjuster(m_testProject);
			var charactersNotInUse = adjuster.CharactersNoLongerInUse.ToList();
			Assert.IsTrue(charactersNotInUse.Contains("Michael, archangel"));
			var originalCountOfGroups = m_testProject.CharacterGroupList.CharacterGroups.Count;
			var groupsToRemove = adjuster.CharacterGroupsToRemove.ToList();
			Assert.AreEqual(1, groupsToRemove.Count);
			Assert.IsFalse(groupsToRemove.Contains(michaelTheArchAngelGroup));

			adjuster.FullyRegenerateGroups();
			Assert.IsFalse(adjuster.GroupsAreNotInSynchWithData);
			Assert.IsFalse(adjuster.CharactersNoLongerInUse.Any());
			Assert.IsFalse(adjuster.CharactersNotCoveredByAnyGroup.Any());
			Assert.IsFalse(adjuster.CharacterGroupsToRemove.Any());
			Assert.AreEqual(originalCountOfGroups - groupsToRemove.Count, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(michaelTheArchAngelGroup,
				m_testProject.CharacterGroupList.CharacterGroups.Single(g => !g.CharacterIds.Any() || g.CharacterIds.Any(c => charactersNotInUse.Contains(c))));
			Assert.IsFalse(m_testProject.CharacterGroupList.CharacterGroups.Any(g => groupsToRemove.Contains(g)));
			Assert.IsFalse(michaelTheArchAngelGroup.CharacterIds.Any());
			Assert.AreEqual(frankie.Id, michaelTheArchAngelGroup.VoiceActorId);
			Assert.IsFalse(adjuster.NewBooksHaveBeenIncluded);
			Assert.IsFalse(adjuster.BooksHaveBeenExcluded);
			Assert.IsFalse(adjuster.FullRegenerateRecommended);
		}
	}
}
