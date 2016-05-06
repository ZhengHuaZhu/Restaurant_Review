using RestaurantReview.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestaurantReview.BusinessClasses
{
    public class CalculationOfElementsForViews
    {
        public static List<Restaurant> CalReviewsAndAvgrating()
        {
            RestaurantReviewsDB db = new RestaurantReviewsDB();

            double sum = 0;

            foreach (var r in db.Restaurants) 
            { 
                sum = 0;
                foreach (var rw in db.Reviews)
                    if (r.restaurantId == rw.restaurantId)
                    {
                        r.numOfReviews++;
                        sum+=rw.rating;
                        r.avgRating = Math.Round(sum/r.numOfReviews, 1);
                    }
            }

            List<Restaurant> restaurants = db.Restaurants.ToList();
             
            var orderedlist= (from r in restaurants
                       orderby r.avgRating descending
                       select r).ToList();           
            return orderedlist;
        }
    }
}