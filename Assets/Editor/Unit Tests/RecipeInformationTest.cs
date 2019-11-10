using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;

using KtaneIngredients;

public class RecipeInformationTest
{
	[Test]
	/// This shows that when we select "red herrings" that don't belong in the
	/// target dish, we only have to remove at most one ingredient from those
	/// that remain to avoid accidentally making another recipe possible.
	public void TestIngredientInvalidation()
	{
		foreach (var recipe in RecipeInformation.AllRecipes)
		{
			Debug.LogFormat("Dish: {0}", recipe.Name);
			Debug.LogFormat("  Ingredients: {0}", recipe.Ingredients.Join(", "));

			var otherRecipes = RecipeInformation.AllRecipes.Except(new Recipe[] { recipe });
			var unusedIngredientLists = otherRecipes.Select(r => r.Ingredients.Except(recipe.Ingredients));
			var singletons = unusedIngredientLists.Where(ingrs => ingrs.Count() == 1);

			foreach (var singleton in singletons)
			{
				Debug.LogFormat("    Invalidates {0}", singleton.First());
			}

			Assert.IsTrue(otherRecipes.Count() + 1 == RecipeInformation.AllRecipes.Length);
			Assert.IsTrue(singletons.Count() <= 1);
		}
	}

	[Test]
	/// This shows that no one recipe is fully contained within another.
	public void TestRecipeOverlap()
	{
		foreach (var recipe in RecipeInformation.AllRecipes)
		{
			foreach (var otherRecipe in RecipeInformation.AllRecipes)
			{
				if (recipe.Name.Equals(otherRecipe.Name))
				{
					continue;
				}

				// If `otherRecipe` contains all ingredients in `recipe`,
				// then the expression below will be empty, and we fail.
				Assert.IsNotEmpty(recipe.Ingredients.Except(otherRecipe.Ingredients));
			}
		}
	}
}
