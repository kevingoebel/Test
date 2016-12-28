using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace App1.Model
{
    public class OpenWeather
    {
        public async static Task<DailyTemp> GetDailyWeather(double lat, double lon)
        {
            using (HttpClient httpClient = new HttpClient())
            {

                var response = await httpClient.GetAsync(new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&APPID=a42b89cfb671188837b7ebbcb9558568&units=metric&lang=nl"));
                var jsonString = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(DailyTemp));

                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

                var data = (DailyTemp)serializer.ReadObject(ms);
                return data;
            }
        }
        public async static Task<ForeCast> GetForeCastWeather(double lat, double lon, int cnt)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(new Uri($"http://api.openweathermap.org/data/2.5/forecast/daily?lat={lat}&lon={lon}&cnt={cnt}&APPID=a42b89cfb671188837b7ebbcb9558568&units=metric&lang=nl"));
                var jsonString = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(ForeCast));

                var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

                var data = (ForeCast)serializer.ReadObject(ms);
                return data;
            }
        }
    }

    [DataContract]
    public class Coord
    {
        [DataMember]
        public double lon { get; set; }
        [DataMember]
        public double lat { get; set; }
    }
    [DataContract]
    public class Weather
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string main { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string icon { get; set; }
    }
    [DataContract]
    public class Main
    {
        [DataMember]
        public double temp { get; set; }
        [DataMember]
        public int pressure { get; set; }
        [DataMember]
        public int humidity { get; set; }
        [DataMember]
        public double temp_min { get; set; }
        [DataMember]
        public double temp_max { get; set; }
    }
    [DataContract]
    public class Wind
    {
        [DataMember]
        public double speed { get; set; }
        [DataMember]
        public int deg { get; set; }
    }
    [DataContract]
    public class Clouds
    {
        [DataMember]
        public int all { get; set; }
    }
    [DataContract]
    public class Sys
    {
        [DataMember]
        public int type { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public double message { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public int sunrise { get; set; }
        [DataMember]
        public int sunset { get; set; }
    }
    [DataContract]
    public class DailyTemp
    {
        [DataMember]
        public Coord coord { get; set; }
        [DataMember]
        public List<Weather> weather { get; set; }
        [DataMember]
        public string @base { get; set; }
        [DataMember]
        public Main main { get; set; }
        [DataMember]
        public int visibility { get; set; }
        [DataMember]
        public Wind wind { get; set; }
        [DataMember]
        public Clouds clouds { get; set; }
        [DataMember]
        public int dt { get; set; }
        [DataMember]
        public Sys sys { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int cod { get; set; }
    }
    [DataContract]
    public class City
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public Coord coord { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public int population { get; set; }
        [DataMember]
        public int geoname_id { get; set; }
        [DataMember]
        public double lat { get; set; }
        [DataMember]
        public double lon { get; set; }
        [DataMember]
        public string iso2 { get; set; }
        [DataMember]
        public string type { get; set; }
    }
    [DataContract]
    public class ForeCast
    {
        [DataMember]
        public City city { get; set; }
        [DataMember]
        public string cod { get; set; }
        [DataMember]
        public double message { get; set; }
        [DataMember]
        public int cnt { get; set; }
        [DataMember]
        public List<List> list { get; set; }
    }
    [DataContract]
    public class List
    {
        [DataMember]
        public int dt { get; set; }
        [DataMember]
        public Temp temp { get; set; }
        [DataMember]
        public double pressure { get; set; }
        [DataMember]
        public int humidity { get; set; }
        [DataMember]
        public List<Weather> weather { get; set; }
        [DataMember]
        public double speed { get; set; }
        [DataMember]
        public int deg { get; set; }
        [DataMember]
        public int clouds { get; set; }
        [DataMember]
        public double? rain { get; set; }
        [DataMember]
        public double? snow { get; set; }
    }
    [DataContract]
    public class Temp
    {
        [DataMember]
        public double day { get; set; }
        [DataMember]
        public double min { get; set; }
        [DataMember]
        public double max { get; set; }
        [DataMember]
        public double night { get; set; }
        [DataMember]
        public double eve { get; set; }
        [DataMember]
        public double morn { get; set; }
    }
}
