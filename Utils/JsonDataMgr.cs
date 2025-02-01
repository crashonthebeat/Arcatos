using System.Dynamic;
using System.Text.Json;

namespace Arcatos.Utils
{
    public static class JsonDataMgr
    {
        // JsonDataMgr is a custom built module for serializing and deserializing Json data to a known or unknown class.
        
        private readonly static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        // Simple method that loads JSON to a known type.
        public static T LoadToType<T>(string jsonPath)
        {
            using StreamReader streamReader = new StreamReader(jsonPath);
            return JsonSerializer.Deserialize<T>(streamReader.ReadToEnd(), JsonOptions) ?? throw new InvalidOperationException();
        }
        
        // Dynamic Json Loading
        // Reference: enum JsonValueKind{ Undefined, Object, Array, String, Number, True, False, Null }

        // LoadGeneric is the Entry Point for loading dynamic objects from a Json
        public static dynamic LoadGeneric(string jsonPath)
        {
            // Load file to string
            using StreamReader streamReader = new StreamReader(jsonPath);
            string jsonString = streamReader.ReadToEnd();

            return jsonString[0] switch
            {
                '{' => LoadObject(jsonString),
                '[' => LoadArray(jsonString),
                _ => throw new InvalidOperationException()
            };
        }

        // LoadArray gets the value type of the array elements and returns an array of that type. 
        private static dynamic LoadArray(dynamic data)
        {
            dynamic jsonArray = JsonSerializer.Deserialize<List<dynamic>>(data, JsonOptions) ?? throw new InvalidOperationException();
            
            // Switch on ValueKind Property of the first element. I *could* do this as a foreach but with so many dynamic types this is gonna be slow af.
            switch (jsonArray[0].ValueKind)
            {
                // Simpler deserializations of Strings and Booleans
                case JsonValueKind.String:
                    return JsonSerializer.Deserialize<string[]>(data, JsonOptions);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return JsonSerializer.Deserialize<bool[]>(data, JsonOptions);
                // Numbers are a little tricky as we have to figure out what type they are, and I don't know how to do that dynamically.
                case JsonValueKind.Number:
                    List<dynamic> jsonNumList = [];
                    return LoadNumberArray(jsonArray);
                // For an array of objects or arrays, we need to make an empty list to throw all the items objects into, then return it as an array.
                case JsonValueKind.Object:
                    List<dynamic> jsonObjectList = [];
                    foreach (dynamic obj in jsonArray) { jsonObjectList.Add(LoadObject(obj)); }
                    return jsonObjectList.ToArray();
                case JsonValueKind.Array:
                    List<dynamic> jsonArrayList = [];
                    foreach (dynamic arr in jsonArray) { jsonArrayList.Add(LoadArray(arr)); }
                    return jsonArrayList.ToArray();
                // Idk, arrays of null or empty arrays?
                default:
                    throw new InvalidOperationException();
            }
        }

        
        private static IDictionary<string,object> LoadObject(dynamic data)
        {
            dynamic jsonObject = JsonSerializer.Deserialize<ExpandoObject>(data, JsonOptions) ?? throw new InvalidOperationException();
            
            // Time for my new friend: the expandoobject. Marrying it to IDictionary allows us to add properties dynamically.
            // This is the empty object that we will fill with properties and then return.
            IDictionary<string, object> dynObject = new ExpandoObject() as IDictionary<string, object>;

            // Now we iterate over the jsonObject as if it were a dictionary, and check the values of each "property". 
            foreach (var kvp in jsonObject)
            {
                dynObject[kvp.Key] = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value,
                    JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => kvp.Value,
                    JsonValueKind.Object => LoadObject(kvp.Value),
                    JsonValueKind.Array => LoadArray(kvp.Value),
                    _ => throw new InvalidOperationException()
                };
            }

            return dynObject;
        }

        private static dynamic LoadNumberArray(dynamic jsonArray)
        {
            throw new NotImplementedException();
        }
    }
}