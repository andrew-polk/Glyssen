﻿namespace Glyssen.Character
{
	public interface ICharacterGroupSource
	{
		CharacterGroupTemplate GetTemplate(int numberOfActors);
	}
}
