using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using KtaneIngredients;

public class IngredientsScript : MonoBehaviour
{
	static Dictionary<CookingTechnique, Dictionary<Course, int>> DigitLookupTable = new Dictionary<CookingTechnique, Dictionary<Course, int>>()
	{
		{ CookingTechnique.Egg, new Dictionary<Course, int> {
			{ Course.Starter, 3 },
			{ Course.Soup, 1 },
			{ Course.Fish, 9 },
			{ Course.Meat, 7 },
			{ Course.Dessert, 2 }
		} },
		{ CookingTechnique.Fire, new Dictionary<Course, int> {
			{ Course.Starter, 7 },
			{ Course.Soup, 5 },
			{ Course.Fish, 4 },
			{ Course.Meat, 0 },
			{ Course.Dessert, 3 }
		} },
		{ CookingTechnique.Knife, new Dictionary<Course, int> {
			{ Course.Starter, 1 },
			{ Course.Soup, 6 },
			{ Course.Fish, 5 },
			{ Course.Meat, 9 },
			{ Course.Dessert, 8 }
		} },
		{ CookingTechnique.Pepper, new Dictionary<Course, int> {
			{ Course.Starter, 4 },
			{ Course.Soup, 0 },
			{ Course.Fish, 8 },
			{ Course.Meat, 2 },
			{ Course.Dessert, 6 }
		} }
	};

	// The actual times it takes Twitch Plays to process each step might be
	// longer than the times shown due to "yield return null" lines causing
	// frames to be skipped.
	const float TwitchPlaysFastCycleTime = 0.1f;
	const float TwitchPlaysSlowCycleTime = 1.2f;

	public KMAudio Audio;
	public KMBombInfo BombInfo;
	public KMBombModule BombModule;

	public KMSelectable LeftButton;
	public KMSelectable RightButton;
	public KMSelectable AddButton;
	public KMSelectable ResetButton;

	public KMSelectable[] TechniqueButtons;

	public TextMesh Display;

	static int ModuleIdCounter = 1;
	int ModuleId;

	Recipe SelectedRecipe;
	Ingredient[] InitialIngredientsList;
	List<Ingredient> CurrentIngredientsList;
	int CurrentIndex;

	HashSet<Ingredient> AddedIngredients;

	bool IsSolved;

	void Awake()
	{
		ModuleId = ModuleIdCounter++;

		IsSolved = false;

		for (int i = 0; i < TechniqueButtons.Length; i++)
		{
			KMSelectable button = TechniqueButtons[i];
			CookingTechnique technique = (CookingTechnique)i;
			button.OnInteract += delegate { OnTechniqueButtonPressed(technique, button); return false; };
		}

		LeftButton.OnInteract += delegate { OnArrowButtonPressed(-1, LeftButton); return false; };
		RightButton.OnInteract += delegate { OnArrowButtonPressed(1, RightButton); return false; };

		AddButton.OnInteract += delegate { OnAddButtonPressed(); return false; };
		ResetButton.OnInteract += delegate { OnResetButtonPressed(); return false; };
	}

	// Use this for initialization
	void Start()
	{
		// BombModule.OnActivate += FunctionToCallWhenTheLightsTurnOn;
		int index = UnityEngine.Random.Range(0, RecipeInformation.AllRecipes.Length);
		SelectedRecipe = RecipeInformation.AllRecipes[index];

		ModuleLog("Selected dish: {0}", SelectedRecipe.Name);
		ModuleLog("Course: {0}", SelectedRecipe.Course);
		ModuleLog("Technique: {0} (button: {1})", SelectedRecipe.Technique.ToExtendedString(), SelectedRecipe.Technique);
		ModuleLog("Expected digit: {0}", DigitLookupTable[SelectedRecipe.Technique][SelectedRecipe.Course]);
		ModuleLog("Ingredients: {0}", SelectedRecipe.Ingredients.Select(ingr => ingr.ToFriendlyString()).Join(", "));

		// Build up our ingredients pool

		// The ingredients still needed to complete each dish
		var neededIngredients = new Dictionary<Recipe, HashSet<Ingredient>>();
		foreach (var recipe in RecipeInformation.AllRecipes)
		{
			neededIngredients[recipe] = new HashSet<Ingredient>(recipe.Ingredients.Except(SelectedRecipe.Ingredients));
		}

		// Ingredients we haven't added yet
		var ingredientsEnumerable = Enum.GetValues(typeof(Ingredient)).Cast<Ingredient>().Except(SelectedRecipe.Ingredients);
		var ingredientsPool = new HashSet<Ingredient>(ingredientsEnumerable);

		var recipesContainingEachIngredient = RecipeInformation.RecipesContainingEachIngredient();

		int minFakeIngredients = Math.Max(2, 7 - SelectedRecipe.Ingredients.Count);
		int maxFakeIngredients = Math.Min(5, 10 - SelectedRecipe.Ingredients.Count);
		int fakeIngredientsCount = UnityEngine.Random.Range(minFakeIngredients, maxFakeIngredients + 1);

		var fakeIngredients = new List<Ingredient>();

		for (int i = 0; i < fakeIngredientsCount; i++)
		{
			// Eliminate any ingredients that will complete another dish
			var singletons = neededIngredients.Keys.Where(recipe => neededIngredients[recipe].Count == 1);
			foreach (var ingredient in singletons.Select(recipe => neededIngredients[recipe].First()))
			{
				ingredientsPool.Remove(ingredient);
			}

			// No need to keep track of those singletons anymore
			foreach (var recipe in singletons.ToArray())
			{
				neededIngredients.Remove(recipe);
			}

			// Pick an ingredient...
			if (ingredientsPool.Count == 0)
			{
				// This should never happen, but we include it just in case.
				// We start the loop with at least 51 ingredients and remove
				// at most 6 ingredients per iteration.
				// Nevertheless, the module should still be solvable.
				break;
			}
			var nextIngredient = ingredientsPool.PickRandom();
			fakeIngredients.Add(nextIngredient);

			// ...and mark it as used everywhere else
			ingredientsPool.Remove(nextIngredient);
			foreach (var recipe in recipesContainingEachIngredient[nextIngredient])
			{
				if (neededIngredients.ContainsKey(recipe))
				{
					neededIngredients[recipe].Remove(nextIngredient);
				}
			}
		}

		ModuleLog("Red herrings: {0}", fakeIngredients.Select(ingr => ingr.ToFriendlyString()).Join(", "));

		CurrentIngredientsList = SelectedRecipe.Ingredients.Concat(fakeIngredients).ToList().Shuffle();
		InitialIngredientsList = CurrentIngredientsList.ToArray();

		CurrentIndex = 0;

		UpdateDisplay();

		AddedIngredients = new HashSet<Ingredient>();
	}
	
	void ModuleLog(string format, params object[] args)
	{
		var prefix = string.Format("[Ingredients #{0}] ", ModuleId);
		Debug.LogFormat(prefix + format, args);
	}

	void OnAddButtonPressed()
	{
		AddButton.AddInteractionPunch(0.5f);

		if (IsSolved)
		{
			return;
		}

		if (CurrentIngredientsList.Count == 0)
		{
			Audio.PlaySoundAtTransform("invalid", transform);
			return;
		}

		Audio.PlaySoundAtTransform("select", transform);

		var ingredient = CurrentIngredientsList[CurrentIndex];

		ModuleLog("Adding ingredient: {0}", ingredient.ToFriendlyString());
		AddedIngredients.Add(ingredient);
		CurrentIngredientsList.RemoveAt(CurrentIndex);

		// If we add the last ingredient in the list, wrap back to the start
		if (CurrentIndex == CurrentIngredientsList.Count)
		{
			CurrentIndex = 0;
		}

		UpdateDisplay();
	}

	void OnArrowButtonPressed(int advanceBy, KMSelectable source)
	{
		// The arrow buttons are tiny
		source.AddInteractionPunch(0.25f);

		if (IsSolved)
		{
			return;
		}

		int length = CurrentIngredientsList.Count;
		if (length == 0)
		{
			Audio.PlaySoundAtTransform("invalid", transform);
			return;
		}

		Audio.PlaySoundAtTransform("move", transform);

		CurrentIndex += advanceBy;
		if (CurrentIndex < 0)
		{
			CurrentIndex = (CurrentIndex % length) + length;
		}
		CurrentIndex %= length;

		UpdateDisplay();
	}

	void OnResetButtonPressed()
	{
		ResetButton.AddInteractionPunch(0.5f);

		if (IsSolved)
		{
			return;
		}

		// No need to log extraneous reset button presses
		if (AddedIngredients.Count > 0)
		{
			ModuleLog("Reset button pressed");
		}

		Audio.PlaySoundAtTransform("reset", transform);

		CurrentIngredientsList = InitialIngredientsList.ToList();
		AddedIngredients.Clear();
		CurrentIndex = 0;
		UpdateDisplay();
	}

	void OnTechniqueButtonPressed(CookingTechnique technique, KMSelectable source)
	{
		source.AddInteractionPunch(0.5f);

		if (IsSolved)
		{
			return;
		}

		var time = BombInfo.GetFormattedTime();

		ModuleLog("Pressed {0} at time {1}", technique, time);

		if (!AddedIngredients.SetEquals(new HashSet<Ingredient>(SelectedRecipe.Ingredients)))
		{
			ModuleLog("Strike!  The ingredients are incorrect.");
			BombModule.HandleStrike();
		}
		else if (SelectedRecipe.Technique != technique)
		{
			ModuleLog("Strike!  The selected technique is incorrect.");
			BombModule.HandleStrike();
		}
		else
		{
			char digit = (char)(48 + DigitLookupTable[SelectedRecipe.Technique][SelectedRecipe.Course]);
			if (time.Contains(digit))
			{
				Audio.PlaySoundAtTransform("solved", transform);
				ModuleLog("Delicious!  Module disarmed.");
				IsSolved = true;
				BombModule.HandlePass();
			}
			else
			{
				ModuleLog("Strike!  The timer does not contain the required digit.");
				BombModule.HandleStrike();
			}
		}
	}

	void UpdateDisplay()
	{
		if (CurrentIndex.InRange(0, CurrentIngredientsList.Count - 1))
		{
			Display.text = CurrentIngredientsList[CurrentIndex].ToFriendlyString();
		}
		else
		{
			Display.text = "";
		}
	}

	#pragma warning disable 414
	string TwitchHelpMessage = "Scroll through ingredients with \"!{0} l/r/left/right <#>\" (number is optional), \"!{0} cycle\", or \"!{0} find <ingredient>\".  Add the ingredient shown with \"!{0} add\", or any ingredient(s) with \"!{0} add <ingredient1>, <ingredient2>, ...\".  Reset the module with \"!{0} reset\".  Choose a technique with \"!{0} press egg/fire/knife/pepper on <#>\".  Commands can be chained with semicolons.  Ingredients are case-insensitive, but must otherwise match exactly.";
	#pragma warning restore 414

	// Searches through the current list of ingredients, stopping if we find
	// an ingredient with the given name.
	private IEnumerator TPSearchForIngredient(string ingredient)
	{
		int length = CurrentIngredientsList.Count;
		for (int i = 0; i < length; i++)
		{
			if (Display.text.EqualsIgnoreCase(ingredient))
			{
				break;
			}

			yield return new[] { RightButton };
			if (i + 1 != length)
			{
				yield return new WaitForSeconds(TwitchPlaysFastCycleTime);
			}
		}
	}

	IEnumerator ProcessTwitchCommand(string command)
	{
		// Note: Any sort of error will stop processing commands.
		foreach (var subcommand in command.Split(new[] {';'}))
		{
			var strippedCommand = subcommand.Trim().ToLowerInvariant();

			// Manual scroll
			var scrollMatch = Regex.Match(strippedCommand, "^(l|left|r|right)(\\s+\\d+)?$");
			if (scrollMatch.Success)
			{
				var distanceString = scrollMatch.Groups[2].Value;
				int distance = 1;
				if (!string.IsNullOrEmpty(distanceString) && !int.TryParse(distanceString, out distance))
				{
					yield return string.Format("sendtochat I couldn't parse the distance \"{0}\".  Are you trying to submit a number greater than Int32.MaxValue?  Naughty!  Kappa", distanceString.Trim());
					yield break;
				}

				// Never make two or more full cycles through the list.
				// This prevents trolls from essentially softlocking the game
				// by running commands like "!{0} left 123456789."
				if (distance > 2 * CurrentIngredientsList.Count)
				{
					distance = CurrentIngredientsList.Count + (distance % CurrentIngredientsList.Count);
				}

				var buttonToPress = (scrollMatch.Groups[1].Value.StartsWith("l")) ? LeftButton : RightButton;

				yield return null;
				for (int i = 0; i < distance; i++)
				{
					yield return new[] { buttonToPress };
					yield return new WaitForSeconds(TwitchPlaysFastCycleTime);
				}

				continue;
			}

			// Cycle
			if (strippedCommand.Equals("cycle"))
			{
				yield return null;

				int length = CurrentIngredientsList.Count;
				for (int i = 0; i < length; i++)
				{
					yield return new[] { RightButton };
					yield return new WaitForSeconds(TwitchPlaysSlowCycleTime);
				}

				continue;
			}

			// Find ingredient
			var findMatch = Regex.Match(strippedCommand, "^find (.+)$");
			if (findMatch.Success)
			{
				var ingredient = findMatch.Groups[1].Value.ToLowerInvariant();

				yield return null;

				int length = CurrentIngredientsList.Count;
				bool found = false;

				for (int i = 0; i < length; i++)
				{
					if (Display.text.EqualsIgnoreCase(ingredient))
					{
						found = true;
						break;
					}

					yield return new[] { RightButton };
					yield return new WaitForSeconds(TwitchPlaysFastCycleTime);
				}

				if (found)
				{
					continue;
				}
				else
				{
					yield return string.Format("sendtochat Unable to find ingredient \"{0}\".", ingredient);
					yield break;
				}
			}

			// Add current
			if (strippedCommand.Equals("add"))
			{
				yield return null;
				yield return new[] { AddButton };
				continue;
			}

			// Add list
			var addListMatch = Regex.Match(strippedCommand, "^add (.+)$");
			if (addListMatch.Success)
			{
				var ingredientsToAdd = addListMatch.Groups[1].Value.ToLowerInvariant();

				foreach (var rawIngredient in ingredientsToAdd.Split(new[] { ',' }))
				{
					var ingredient = rawIngredient.Trim();

					yield return null;
					
					int length = CurrentIngredientsList.Count;
					bool found = false;

					for (int i = 0; i < length; i++)
					{
						if (Display.text.EqualsIgnoreCase(ingredient))
						{
							found = true;
							yield return new[] { AddButton };
							yield return new WaitForSeconds(TwitchPlaysFastCycleTime);
							break;
						}

						yield return new[] { RightButton };
						yield return new WaitForSeconds(TwitchPlaysFastCycleTime);
					}

					if (found)
					{
						continue;
					}
					else
					{
						yield return string.Format("unsubmittablepenalty Unable to find ingredient \"{0}\".", ingredient);
						yield break;
					}
				}

				continue;
			}

			// Reset
			if (strippedCommand.Equals("reset"))
			{
				yield return null;
				yield return new[] { ResetButton };
				continue;
			}

			// Submit
			var submitMatch = Regex.Match(strippedCommand, "^press (egg|fire|knife|pepper) on (\\d)$");
			if (submitMatch.Success)
			{
				var technique = submitMatch.Groups[1].Value;
				var digit = submitMatch.Groups[2].Value;

				var buttonNames = new[] { "egg", "fire", "knife", "pepper" };
				var buttonToPress = TechniqueButtons[Array.IndexOf(buttonNames, technique)];

				yield return null;

				while (true)
				{
					if (BombInfo.GetFormattedTime().Contains(digit[0]))
					{
						yield return new[] { buttonToPress };
						break;
					}
					else
					{
						yield return new WaitForSeconds(0.1f);
					}
				}

				continue;
			}

			yield return string.Format("sendtochaterror Unable to recognize command \"{0}\".", strippedCommand);
			yield break;
		}
	}
}
