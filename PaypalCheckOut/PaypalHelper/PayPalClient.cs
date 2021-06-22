using System;
using PayPalCheckoutSdk.Core;
using PayPalHttp;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace PaypalCheckOut.PaypalHelper
{
    //Configuracion del HttpClient para consumir el API.

    public class PayPalClient
    {
        public static HttpClient client(SandboxEnvironment enviroment)
        {
            return new PayPalHttpClient(enviroment);
        }

        public static HttpClient client(SandboxEnvironment enviroment, string refreshToken)
        {
            return new PayPalHttpClient(enviroment, refreshToken);
        }

        /**
            Use this method to serialize Object to a JSON string.
        */
        public static String ObjectToJSONString(Object serializableObject)
        {
            MemoryStream memoryStream = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        memoryStream, Encoding.UTF8, true, true, "  ");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(serializableObject.GetType(), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true });
            ser.WriteObject(writer, serializableObject);
            memoryStream.Position = 0;
            StreamReader sr = new StreamReader(memoryStream);
            return sr.ReadToEnd();
        }
    }

}
