using Mirror.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Mirror.Models
{
    public partial class Sys
    {
        [JsonProperty("population")]
        public int Population { get; set; }
    }

    public class City
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("coord")]
        public Coord Coord { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("population")]
        public int Population { get; set; }

        [JsonProperty("sys")]
        public Sys Sys { get; set; }
    }

    public class List
    {
        [JsonProperty("dt")]
        public double Dt { get; set; }

        public DateTime DateTime => Dt.FromUnixTimeStamp();

        [JsonProperty("main")]
        public Main Main { get; set; }

        [JsonProperty("weather")]
        public IList<Weather> Weather { get; set; }

        [JsonProperty("wind")]
        public Wind Wind { get; set; }

        [JsonProperty("temp")]
        public Temp Temp { get; set; }

        [JsonProperty("sys")]
        public Sys Sys { get; set; }
    }

    public class Temp
    {
        [JsonProperty("day")]
        public double Day { get; set; }

        [JsonProperty("min")]
        public double Min { get; set; }

        [JsonProperty("max")]
        public double Max { get; set; }

        [JsonProperty("night")]
        public double Night { get; set; }

        [JsonProperty("eve")]
        public double Eve { get; set; }

        [JsonProperty("morn")]
        public double Morn { get; set; }
    }

    public class Forecast
    {
        [JsonProperty("city")]
        public City City { get; set; }

        [JsonProperty("cod")]
        public string Cod { get; set; }

        [JsonProperty("message")]
        public double Message { get; set; }

        [JsonProperty("cnt")]
        public int Cnt { get; set; }

        [JsonProperty("list")]
        public IList<List> List { get; set; }
    }
}