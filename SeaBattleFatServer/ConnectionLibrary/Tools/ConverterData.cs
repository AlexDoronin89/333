using ConnectionLibrary.Entity;
using System;

namespace ConnectionLibrary.Tools
{
    public class ConverterData
    {
        public static string SerializeRequest(Request request)
        {
            string data = request.Command + '|';

            if (request.Parameters != null && request.Parameters.Length > 0)
                data += string.Join(",", request.Parameters);

            return data;
        }

        public static Request DeserializeRequest(string data)
        {
            string[] values = data.Split('|');
            return new Request() { Command = values[0], Parameters = values[1].Split(',') };
        }

        public static string SerializeResponce(Responce responce)
        {
            return $"{responce.Value}|{responce.Result}";
        }

        public static Responce DeserializeResponce(string data)
        {
            string[] values = data.Split('|');
            return new Responce() { Value = values[0], Result = values[1] };
        }
    }
}
