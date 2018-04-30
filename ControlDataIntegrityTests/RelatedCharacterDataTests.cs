﻿using System.Linq;
using NUnit.Framework;
using Waxuquerque.Character;

namespace ControlDataIntegrityTests
{
	class RelatedCharacterDataTests
	{
		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var characterId in RelatedCharactersData.Singleton.GetCharacterIdToRelatedCharactersDictionary().Keys)
			{
				Assert.True(CharacterDetailData.Singleton.GetDictionary().Keys.Contains(characterId), "The following character ID was not found in CharacterDetail: {0}", characterId);
			}
		}
	}
}