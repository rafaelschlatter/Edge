using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public interface ISerializer<in Type> : ISerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        public string Serialize(Type toSerialize);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISerializer { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class JsonSerializer<Type> : ISerializer<Type>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toSerialize"></param>
        /// <returns></returns>
        public string Serialize(Type toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public interface IDeserializer<out Type> : IDeserializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDeserialize"></param>
        /// <returns></returns>
        public Type Deserialize(string toDeserialize);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDeserializer { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Type"></typeparam>
    public class JsonDeserializer<Type> : IDeserializer<Type>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDeserialize"></param>
        /// <returns></returns>
        public Type Deserialize(string toDeserialize)
        {
            return JsonConvert.DeserializeObject<Type>(toDeserialize);
        }
    }
}
