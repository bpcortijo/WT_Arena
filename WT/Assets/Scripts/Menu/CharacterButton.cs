using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterButton : MonoBehaviour
{
	public MenuScript menu;
	public DetailScript details;
	public CharacterStats character;

	public void CharacterSelect()
	{
		details.currentCharacter = character.gameObject;
		details.UpdatePage(character.agility, character.energy, character.experience);
		menu.Next();
	}
}
