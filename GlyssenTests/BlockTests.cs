﻿using System;
using Glyssen;
using Glyssen.Character;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Scripture;

namespace GlyssenTests
{
	[TestFixture]
	class BlockTests
	{
		[Test]
		public void GetAsXml_VerseAndTextElements_XmlHasCorrectAttributesAndAlternatingVerseAndTextElements()
		{
			var block = new Block("p", 4);
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" chapter=\"4\" initialStartVerse=\"1\" characterId=\"narrator-MRK\">" +
				"<verse num=\"1\"/>" +
				"<text>Text of verse one. </text>" +
				"<verse num=\"2\"/>" +
				"<text>Text of verse two.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetAsXml_TextBeginsMidVerse_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" paragraphStart=\"true\" chapter=\"4\" initialStartVerse=\"3\">" +
				"<text>Text of verse three, part two. </text>" +
				"<verse num=\"4\"/>" +
				"<text>Text of verse four. </text>" +
				"<verse num=\"5\"/>" +
				"<text>Text of verse five.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetAsXml_VerseBridge_XmlHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3, 5);
			block.IsParagraphStart = true;
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4-5"));
			block.BlockElements.Add(new ScriptText("Text of verse four and five."));

			AssertThatXmlIn.String("<?xml version=\"1.0\" encoding=\"utf-16\"?><block style=\"p\" paragraphStart=\"true\" chapter=\"4\" initialStartVerse=\"3\" initialEndVerse=\"5\">" +
				"<text>Text of verse three, part two. </text>" +
				"<verse num=\"4-5\"/>" +
				"<text>Text of verse four and five.</text>" +
				"</block>")
				.EqualsIgnoreWhitespace(block.GetAsXml());
		}

		[Test]
		public void GetTextAsHtml_ContainsCharactersWhichNeedToBeEscaped()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText(@"The dog'cat says, <<Woof!>> & ""Meow."""));

			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">The dog&#39;cat says, &lt;&lt;Woof!&gt;&gt; &amp; &quot;Meow.&quot;</div>",
				block.GetTextAsHtml(true, false));
		}

		[Test]
		public void GetTextAsHtml_TextContainsSquareBrackets_OnlyVerseNumbersAreSuperscripted()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two [2]. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers [sic] four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">Text of verse three, part two [2]. " +
							"</div><sup>4&#160;</sup><div id=\"4\" class=\"scripttext\">Text of vers [sic] four. </div><sup>5&#160;</sup>" +
							"<div id=\"5\" class=\"scripttext\">Text of verse five.</div>",
				block.GetTextAsHtml(true, false));
		}

		[Test]
		public void GetTextAsHtml_RightToLeftScript_RtlMarkersAddedCorrectly()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));

			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">Text of verse three, part two. </div>" +
							"<sup>&rlm;4&#160;&rlm;</sup><div id=\"4\" class=\"scripttext\">Text of verse four. </div>",
				block.GetTextAsHtml(true, true));
		}

		[Test]
		public void GetTextAsHtml_OffsetTooHigh_ThrowsArgumentOutOfRangeException()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text"));
			Assert.Throws<ArgumentOutOfRangeException>(() => block.GetTextAsHtml(true, false, "3", 5, "<div></div>"));
		}

		[Test]
		public void GetTextAsHtml_NullExtra_ThrowsArgumentNullException()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text"));
			Assert.Throws<ArgumentNullException>(() => block.GetTextAsHtml(true, false, "3", 4));
		}

		[Test]
		public void GetTextAsHtml_ExtraParameters_InsertsImgElement()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two [2]. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of vers [sic] four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">Text of verse three, part two [2]. " +
							"</div><sup>4&#160;</sup><div id=\"4\" class=\"scripttext\">Text <img src=\"blah\"/>of vers [sic] four. </div><sup>5&#160;</sup>" +
							"<div id=\"5\" class=\"scripttext\">Text of verse five.</div>",
				block.GetTextAsHtml(true, false, "4", 5, "<img src=\"blah\"/>"));
		}

		[Test]
		public void GetTextAsHtml_SpecialCharactersInText_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Нылыс эз кув, сiйö узьö"));
			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">Нылыс эз кув, сiй&#246; <hr/>узь&#246;</div>", block.GetTextAsHtml(false, false, "3", 19, "<hr/>"));
		}

		[Test]
		public void GetTextAsHtml_ExpectedSpecialCharacters_InsertsInCorrectLocation()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("A & <<B>> C"));
			Assert.AreEqual("<div id=\"3\" class=\"scripttext\">A &amp; &lt;&lt;B&gt;&gt; <hr/>C</div>", block.GetTextAsHtml(false, false, "3", 10, "<hr/>"));
		}

		[Test]
		public void SetCharacterAndDelivery_SingleCharacter_SetsCharacterAndDelivery()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { JesusQuestioning });
			Assert.AreEqual("Jesus", block.CharacterId);
			Assert.AreEqual("Questioning", block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_NoCharacters_SetsCharacterToUnknown()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(new CharacterVerse[0]);
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, block.CharacterId);
			Assert.IsNull(block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_MultipleCharacters_SetsCharacterToAmbiguous()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Fred";
			block.Delivery = "Freakin' out";
			block.SetCharacterAndDelivery(new [] { JesusCommanding, JesusQuestioning, Andrew });
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, block.CharacterId);
			Assert.IsNull(block.Delivery);
		}

		[Test]
		public void SetCharacterAndDelivery_SingleMultipleChoiceCharacter_SetsCharacterAndCharacterIdInScript()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Mary", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SettingToSameMultiCharacterWithNonDefaultCharacterIdInScriptSet_CharacterIdInScriptNotChanged()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Mary/Martha";
			block.CharacterIdInScript = "Martha";
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Martha", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SettingToSameMultiCharacterWithNoCharacterIdInScriptSet_CharacterIdInScriptGetsSet()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.CharacterId = "Mary/Martha";
			block.SetCharacterAndDelivery(new[] { new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha", null, null, false) });
			Assert.AreEqual("Mary/Martha", block.CharacterId);
			Assert.AreEqual("Mary", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndDelivery_SameMultipleChoiceCharacterWithDifferentDefaults_CharacterIdInScriptSetBasedOnFirstCvEntry()
		{
			var block = new Block("p", 4, 4, 6);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verses four through seven. "));
			block.SetCharacterAndDelivery(new[]
			{
				new CharacterVerse(new BCVRef(41, 4, 4), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Martha"),
				new CharacterVerse(new BCVRef(41, 4, 5), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Jews"),
				new CharacterVerse(new BCVRef(41, 4, 6), "Mary/Martha/Jews", null, null, false, QuoteType.Dialogue, "Mary")
			});
			Assert.AreEqual("Mary/Martha/Jews", block.CharacterId);
			Assert.AreEqual("Martha", block.CharacterIdInScript);
		}

		[Test]
		public void IsStandardCharacter_BiblicalCharacter_ReturnsFalse()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetCharacterAndDelivery(new[] { JesusQuestioning });
			Assert.IsFalse(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_Narrator_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Narrator);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_ExtraBiblical_ReturnsTrue()
		{
			var block = new Block("p", 4, 4);
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.SetStandardCharacter("GEN", CharacterVerseData.StandardCharacter.ExtraBiblical);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_BookOrChapter_ReturnsTrue()
		{
			var block = new Block("c", 4);
			block.BlockElements.Add(new ScriptText("4"));
			block.SetStandardCharacter("REV", CharacterVerseData.StandardCharacter.BookOrChapter);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void IsStandardCharacter_Intro_ReturnsTrue()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.SetStandardCharacter("ROM", CharacterVerseData.StandardCharacter.Intro);
			Assert.IsTrue(block.CharacterIsStandard);
		}

		[Test]
		public void LastVerse_Intro_ReturnsZero()
		{
			var block = new Block("ip");
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(0, block.LastVerse);
		}

		[Test]
		public void LastVerse_ScriptureBlockWithSingleStartVerseAndNoVerseElements_ReturnsInitialStartVerse()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(15, block.LastVerse);
		}

		[Test]
		public void LastVerse_ScriptureBlockStartingWithVerseBridgeAndNoVerseElements_ReturnsInitialEndVerse()
		{
			var block = new Block("ip", 3, 15, 17);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(17, block.LastVerse);
		}

		[Test]
		public void LastVerse_ScriptureBlockWithVerseElements_ReturnsEndVerseFromBlockElement()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("16"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("17"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(17, block.LastVerse);
		}

		[Test]
		public void LastVerse_ScriptureBlockWithVerseElementContainingBridge_ReturnsEndVerseFromBlockElement()
		{
			var block = new Block("ip", 3, 15);
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("16"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			block.BlockElements.Add(new Verse("17-19"));
			block.BlockElements.Add(new ScriptText("This is a yadda yadda..."));
			Assert.AreEqual(19, block.LastVerse);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NotMultipleChoice_DoNothing()
		{
			var block = new Block("p", 4, 38);
			block.CharacterId = "disciples";
			block.CharacterIdInScript = "Peter (Simon)";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.AreEqual("Peter (Simon)", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_NoExplicitDefault_UseFirst()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_ExplicitDefault_UseDefault()
		{
			var block = new Block("p", 9, 11);
			block.CharacterId = "Peter (Simon)/James/John";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("MRK"));
			Assert.AreEqual("John", block.CharacterIdInScript);
		}

		[Test]
		public void UseDefaultForMultipleChoiceCharacter_AlreadySetToAnotherVlaue_OverwriteWithDefault()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "chief baker";
			block.UseDefaultForMultipleChoiceCharacter(BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoice_CharacterIdInScriptRemainsNull()
		{
			var block = new Block("p", 40, 8);
			block.SetCharacterAndCharacterIdInScript("chief monkey", BCVRef.BookToNumber("EXO"));
			Assert.AreEqual("chief monkey", block.CharacterId);
			Assert.AreEqual("chief monkey", block.CharacterIdInScript);
			Assert.IsNull(block.CharacterIdOverrideForScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NotMultipleChoiceCharacterIdInScriptAlreadySet_CharacterIdInScriUnchanged()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "live frog";
			block.CharacterIdInScript = "dead frog";
			block.SetCharacterAndCharacterIdInScript("subordinate monkey", BCVRef.BookToNumber("REV"));
			Assert.AreEqual("subordinate monkey", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_NoChangeToCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_NoChange()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "dead frog";
			block.SetCharacterAndCharacterIdInScript("chief cupbearer/chief baker", BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("chief cupbearer/chief baker", block.CharacterId);
			Assert.AreEqual("dead frog", block.CharacterIdInScript);
		}

		[Test]
		public void SetCharacterAndCharacterIdInScript_ChangeCharacterIdAndCharacterIdInScriptAlreadySetToAnotherValue_CharacterIdInScriptChanged()
		{
			var block = new Block("p", 40, 8);
			block.CharacterId = "chief cupbearer/chief baker";
			block.CharacterIdInScript = "chief cupbearer";
			block.SetCharacterAndCharacterIdInScript("David/Goliath", BCVRef.BookToNumber("GEN"));
			Assert.AreEqual("David/Goliath", block.CharacterId);
			Assert.AreEqual("David", block.CharacterIdInScript);
		}

		private CharacterVerse JesusQuestioning
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Jesus", "Questioning", null, false);
			}
		}

		private CharacterVerse JesusCommanding
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Jesus", "Commanding", null, false);
			}
		}

		private CharacterVerse Andrew
		{
			get
			{
				return new CharacterVerse(new BCVRef(41, 4, 4), "Andrew", null, null, false);
			}
		}
	}
}