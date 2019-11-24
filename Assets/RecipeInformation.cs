using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KtaneIngredients
{
	public enum CookingTechnique
	{
		Egg, Fire, Knife, Pepper
	}

	public enum Course
	{
		Starter, Soup, Fish, Meat, Dessert
	}

	// Ingredients are listed in the order they appear in the manual.
	// This is also the order they appear in the KH3 item menu.
	public enum Ingredient
	{
		Veal, Beef, Quail, FiletMignon,
		Crab, Scallop, Lobster, Sole, Eel, SeaBass, Mussel, Cod,
		Pumpkin, Zucchini, Onion, Tomato, Eggplant, Carrot, Garlic, Celery,
		Morel, Porcini, Chanterelle, Portobello, BlackTruffle, KingOysterMushroom, BlackTrumpet, MillerMushroom,
		Cloves, Rosemary, Thyme, BayLeaf, Basil, Dill, Parsley, Saffron,
		Apricot, Gooseberry, Lemon, Orange, Raspberry, Pear, Blackberry, Apple,
		Cheese, Chocolate, Caviar, Butter, OliveOil, Cornichon, Rice, Honey,
		SourCherry, Strawberry, BloodOrange, Banana, Grapes, Melon, Watermelon
	}

	public static class Extensions
	{
		public static string ToFriendlyString(this Ingredient ingr)
		{
			return Regex.Replace(ingr.ToString(), "(.)([A-Z])", "$1 $2");
		}

		public static string ToExtendedString(this CookingTechnique technique)
		{
			switch (technique)
			{
				case CookingTechnique.Egg:
					return "Cracking an egg";
				case CookingTechnique.Fire:
					return "The perfect flambé";
				case CookingTechnique.Knife:
					return "Good knife skills";
				case CookingTechnique.Pepper:
					return "Seasoning with style";
				default:
					return technique.ToString();
			}
		}
	}

	public class Recipe
	{
		public readonly string Name;
		public readonly Course Course;
		public readonly CookingTechnique Technique;
		private readonly Ingredient[] _ingredients;
		public List<Ingredient> Ingredients
		{
			get
			{
				// This ensures that anybody who tries to access this list of ingredients gets a deep copy.
				// The original list will always be left untouched.
				return new List<Ingredient>(_ingredients);
			}
		}

		public Recipe(string name, Course course, CookingTechnique technique, Ingredient[] ingredients)
		{
			Name = name;
			Course = course;
			Technique = technique;
			_ingredients = ingredients;
		}
	}

	public class RecipeInformation
	{
		public static Recipe[] AllRecipes = {
			new Recipe("Mushroom Terrine", Course.Starter, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Morel, Ingredient.Chanterelle, Ingredient.KingOysterMushroom, Ingredient.BlackTrumpet
			}),
			new Recipe("Scallop Poêlé", Course.Starter, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Scallop, Ingredient.OliveOil
			}),
			new Recipe("Ratatouille", Course.Starter, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Zucchini, Ingredient.Eggplant, Ingredient.Tomato, Ingredient.Garlic, Ingredient.BayLeaf
			}),
			new Recipe("Lobster Mousse", Course.Starter, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Lobster, Ingredient.Scallop, Ingredient.Dill
			}),
			new Recipe("Caprese Salad", Course.Starter, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.Strawberry, Ingredient.Tomato, Ingredient.Cheese, Ingredient.Basil
			}),

			new Recipe("Consommé", Course.Soup, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Celery, Ingredient.Onion, Ingredient.Cloves
			}),
			new Recipe("Pumpkin Velouté", Course.Soup, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Pumpkin, Ingredient.BlackTruffle
			}),
			new Recipe("Carrot Potage", Course.Soup, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.Carrot, Ingredient.Onion, Ingredient.Rice, Ingredient.Butter
			}),
			new Recipe("Crab Bisque", Course.Soup, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Crab, Ingredient.Tomato, Ingredient.Carrot, Ingredient.Celery, Ingredient.OliveOil
			}),
			new Recipe("Cold Tomato Soup", Course.Soup, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Watermelon, Ingredient.Tomato, Ingredient.Dill
			}),

			new Recipe("Sole Meunière", Course.Fish, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.Sole, Ingredient.Caviar
			}),
			new Recipe("Eel Matelote", Course.Fish, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Eel, Ingredient.BayLeaf, Ingredient.Parsley
			}),
			new Recipe("Bouillabaisse", Course.Fish, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Mussel, Ingredient.Lobster, Ingredient.Cod, Ingredient.Garlic, Ingredient.Saffron
			}),
			new Recipe("Sea Bass en Papillote", Course.Fish, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.SeaBass, Ingredient.Basil, Ingredient.Thyme, Ingredient.OliveOil
			}),
			new Recipe("Seafood Tartare", Course.Fish, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.BloodOrange, Ingredient.Lobster, Ingredient.SeaBass, Ingredient.OliveOil
			}),
			new Recipe("Sea Bass Poêlé", Course.Fish, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Grapes, Ingredient.SeaBass, Ingredient.Zucchini, Ingredient.Chanterelle, Ingredient.Parsley
			}),

			new Recipe("Sweetbread Poêlé", Course.Meat, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Porcini, Ingredient.Lemon, Ingredient.Veal
			}),
			new Recipe("Beef Sauté", Course.Meat, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.Cornichon, Ingredient.Eggplant, Ingredient.Zucchini, Ingredient.Beef
			}),
			new Recipe("Beef Bourguignon", Course.Meat, CookingTechnique.Pepper, new Ingredient[] {
				Ingredient.Rosemary, Ingredient.BayLeaf, Ingredient.Thyme, Ingredient.Garlic, Ingredient.Beef
			}),
			new Recipe("Stuffed Quail", Course.Meat, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Rice, Ingredient.Portobello, Ingredient.Porcini, Ingredient.MillerMushroom, Ingredient.Parsley, Ingredient.Quail
			}),
			new Recipe("Filet Mignon Poêlé", Course.Meat, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.SourCherry, Ingredient.Butter, Ingredient.BlackTruffle, Ingredient.Rosemary, Ingredient.Cloves, Ingredient.FiletMignon
			}),

			new Recipe("Chocolate Mousse", Course.Dessert, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Chocolate, Ingredient.Lemon, Ingredient.Butter
			}),
			new Recipe("Fresh Fruit Compote", Course.Dessert, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Pear, Ingredient.Apple, Ingredient.Apricot
			}),
			new Recipe("Crêpes Suzette", Course.Dessert, CookingTechnique.Fire, new Ingredient[] {
				Ingredient.Orange, Ingredient.Butter, Ingredient.Honey
			}),
			new Recipe("Berries au Fromage", Course.Dessert, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Cheese, Ingredient.Lemon, Ingredient.Gooseberry, Ingredient.Raspberry, Ingredient.Blackberry
			}),
			new Recipe("Banana Soufflé", Course.Dessert, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.Banana, Ingredient.Butter, Ingredient.Honey
			}),
			new Recipe("Fruit Gelée", Course.Dessert, CookingTechnique.Knife, new Ingredient[] {
				Ingredient.Melon, Ingredient.Pear, Ingredient.Gooseberry
			}),
			new Recipe("Tarte aux Fruits", Course.Dessert, CookingTechnique.Egg, new Ingredient[] {
				Ingredient.SourCherry, Ingredient.Strawberry, Ingredient.BloodOrange, Ingredient.Banana, Ingredient.Grapes, Ingredient.Melon, Ingredient.Watermelon
			})
		};

		public static Dictionary<Ingredient, ICollection<Recipe>> RecipesContainingEachIngredient()
		{
			var result = new Dictionary<Ingredient, ICollection<Recipe>>();
			
			foreach (var ingredient in Enum.GetValues(typeof(Ingredient)).Cast<Ingredient>())
			{
				result[ingredient] = new HashSet<Recipe>();
			}

			foreach (var recipe in AllRecipes)
			{
				foreach (var ingredient in recipe.Ingredients)
				{
					result[ingredient].Add(recipe);
				}
			}

			return result;
		}
	}
}