﻿using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using Advertisement.AdPrediction;

namespace Database;



public static class DatabaseLib
{
    // function that takes an array and fills it with each row in monthly stats
    // [0, 1, 2, 3, 4, 5]
    // [month1clicks, month1conversions, month2clicks, month2conversions, month3clicks, month3conversions]

    public static string CategoryNumberToString(int categoryId)
    {
        using var context = DataContext.Instance;

        var categoryName = context.Categories
        .Where(c => c.Id == categoryId)
        .Select(c => c.Name)
        .FirstOrDefault();

        return categoryName;
    }

    public static int[] GetMonthlyStats()
    {
        using var context = DataContext.Instance;

        var statsList = context.MonthlyStats.ToList();

        int[] resultArray = new int[statsList.Count * 2];

        for (int i = 0; i < statsList.Count; i++)
        {
            resultArray[i * 2] = statsList[i].Clicks;
            resultArray[i * 2 + 1] = statsList[i].Conversions;
        }

        return resultArray;
    }

    public static Product GetProductById(string productId)
    {
        using var context = DataContext.Instance;

        var product = context.Products.FirstOrDefault(p => p.Pid == productId);

        if (product != null)
        {
            return product;
        }
        else
        {
            return null;
        }
    }

    public static string GetProductImageById(string productId)
    {
        using var context = DataContext.Instance;

        var product = context.Products.FirstOrDefault(p => p.Pid == productId);
        if (product != null)
        {
            return product.Image;
        }
        else
        {
            return null;
        }
    }

    public static string GetProductImageByName(string productName)
    {
        using var context = DataContext.Instance;

        var product = context.Products.FirstOrDefault(p => string.Equals(p.Name, productName));
        if (product != null)
        {
            return product.Image;
        }
        else
        {
            return null;
        }
    }

    public static Category GetCategoryById(int id)
    {
        using var context = DataContext.Instance;

        var category = context.Categories.FirstOrDefault(c => c.Id == id);

        return category;
    }

    public static Category GetCategoryByName(string categoryName)
    {
        using var context = DataContext.Instance;

        var category = context.Categories.FirstOrDefault(c => c.Name == categoryName);

        return category;
    }

    
    // finds 2 closest priced items within category and return the product struct
    // Needs the products and prodRequest classes to fix errors
    public static List<cart_Product> GetClosestPricedProducts(prodRequest request)
    {
        using var context = DataContext.Instance;

        var closestPricedProducts = new List<cart_Product>();

        var productsInCategory = context.Products
            .Where(p => request.category.Contains(CategoryNumberToString(p.Category)))
            .ToList();

        productsInCategory.Sort((p1, p2) =>
    
            Math.Abs(p1.Price - request.avg_price).CompareTo(Math.Abs(p2.Price - request.avg_price))
        );

        closestPricedProducts.Add(new cart_Product(productsInCategory[0].Price, productsInCategory[0].Name, CategoryNumberToString(productsInCategory[0].Category), productsInCategory[0].Image));
        closestPricedProducts.Add(new cart_Product(productsInCategory[1].Price, productsInCategory[1].Name, CategoryNumberToString(productsInCategory[1].Category), productsInCategory[1].Image));

        return closestPricedProducts;
    }

    public static User GetUserById(int userId)
    {
        using var context = DataContext.Instance;

        var user = context.Users.FirstOrDefault(u => u.Id == userId);

        return user;
    }

    public static retrieved_Data GetUserData(int userId)
    {
        using var context = DataContext.Instance;

        var userData = new retrieved_Data();

        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            userData.subtotal = user.Subtotal;
        }

        var userPurchases = context.UserPurchases.Where(up => up.UserId == userId).ToList();
        foreach (var purchase in userPurchases)
        {
            var category = CategoryNumberToString(purchase.CategoryShopped);
            if (!userData.categ_freq.ContainsKey(category))
            {
                userData.categ_freq[category] = 1;
            }
            else
            {
                userData.categ_freq[category]++;
            }
        }

        var averageProductPrice = context.UserPurchases
            .Where(up => up.UserId == userId)
            .Average(up => up.AvgProductPrice);

        userData.avg_prod_price = averageProductPrice;

        return userData;
    }

    public static void IncrementClicks()
    {
        using var context = DataContext.Instance;

        var monthlyStats = context.MonthlyStats.FirstOrDefault();

        if (monthlyStats != null)
        {
            monthlyStats.Clicks += 1;
            context.SaveChanges();
        }
    }

    public static void IncrementConversions()
    {
        using var context = DataContext.Instance;

        var monthlyStats = context.MonthlyStats.FirstOrDefault();

        if (monthlyStats != null)
        {
            monthlyStats.Conversions += 1;
            context.SaveChanges();
        }
    }

    public static void CreateUser(int userId)
    {
        using var context = DataContext.Instance;

        if (context.Users.Any(u => u.Id == userId))
        {
            return;
        }
        var newUser = new User
        {
            Id = userId,
        };

        context.Users.Add(newUser);
        context.SaveChanges();
    }

    public static string SearchProductByDimensions(int height, int width)
    {
        using var context = DataContext.Instance;

        var product = context.Products.FirstOrDefault(u => u.Height == height && u.Width == width);

        if (product != null)
        {
            return product.Pid;
        }
        else
        {
            return "";
        }
    }

    public static List<cart_Product> TopClicks()
    {
        using var context = DataContext.Instance;

        var topClicked = context.Products
            .OrderByDescending(p => p.Clicked)
            .Take(2)
            .Select(p => new cart_Product
            {
                prod_price = p.Price,
                prod_name = p.Name, 
                prod_categ = CategoryNumberToString(p.Category),
                url = p.Image      
            })
            .ToList();

        return topClicked;
    }

}



