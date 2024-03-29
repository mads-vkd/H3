﻿using DataLayer;
using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServiceLayer
{
    public class RestaurantService : IRestaurantService
    {
        private readonly AppDbContext _ctx;

        public RestaurantService(AppDbContext ctx)
        {
            ctx.Database.EnsureCreated();   // lav til kommentar for at Add-demo
            _ctx = ctx;
        }

       

        public IQueryable<Restaurant> GetRestaurants() 
        {
            return _ctx.Restaurants;
        }

        public IQueryable<Restaurant> GetRestaurantsByName(string name = null)
        {
            return _ctx.Restaurants
                .Where(r => string.IsNullOrEmpty(name) || r.Name.StartsWith(name))
                .OrderBy(r => r.Name);
        }

        public Restaurant GetRestaurantById(int restaurantId)
        {
            return _ctx.Restaurants.Find(restaurantId);
        }

        public Restaurant Update(Restaurant updatedRedstaurant)
        {
            _ctx.Restaurants.Update(updatedRedstaurant);
            //_ctx.Entry<Restaurant>(updatedRedstaurant).Property(x => x.ImageUrl).IsModified = false;

            //var entity = _ctx.Restaurants.Attach(updatedRedstaurant);
            //entity.State = EntityState.Modified;
            
            return updatedRedstaurant;
        }  

        public Restaurant Add(Restaurant newRestaurant)
        {
            _ctx.Restaurants.Add(newRestaurant);
            return newRestaurant;
        }   

        public Restaurant Delete(int id)
        {
            var restaurant = GetRestaurantById(id);
            if (restaurant != null)
            {
                _ctx.Restaurants.Remove(restaurant);
            }
            return restaurant;
        }

        public int GetCountOfRestaurants()
        {
            return _ctx.Restaurants.Count();
        }

        public int Commit()
        {
            _ctx.SaveChanges();
            return 0;
        }
    }
}
