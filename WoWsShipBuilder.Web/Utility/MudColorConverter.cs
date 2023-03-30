using MudBlazor.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WoWsShipBuilder.Web.Utility;

public class MudColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(MudColor));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON for the Result into a JObject
        JObject jo = JObject.Load(reader);

        // Read the properties which will be used as constructor parameters
        string color = (string)jo["Value"];

        // Construct the Result object using the non-default constructor
        MudColor result = color;

        // (If anything else needs to be populated on the result object, do that here)

        // Return the result
        return result;
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
