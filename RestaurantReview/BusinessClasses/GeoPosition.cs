using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace RestaurantReview.BusinessClasses
{
    public class GeoPosition
    {
        public static XElement GetGeocodingSearchResults(string address) //XElement holds the XML result, within the System.Xml.Linq namespace
        {
            var url = String.Format("http://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false", HttpUtility.UrlEncode(address));  //Url encode since it was provided by user

            // Load the XML into an XElement object
            var results = XElement.Load(url);

            return results;
        }

    }
}