using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static JsonToolkit.Mappers.DynamicJsonConverter;

namespace JsonToolkit.Mappers
{
	/// <summary>
	/// Generates a key value array list for the given json 
	/// </summary>
	public class PropertyMap
	{
		#region Constants

		private	const string		CONTENT_OBJECT_OUTPUT_FORMAT								= "{{0}}";
		private const string        CONTENT_OBJECT_SPLITER 										= ".";
		private const string        CONTENT_OBJECT_FORMAT 										= "{0}" + CONTENT_OBJECT_SPLITER + "{1}";

		#endregion

		#region Methods

		/// <summary>
		/// Gets a property list with given string format
		/// </summary>
		public static List<KeyValuePair<string, string>> GetPropertyList(string json)
		{
			List<KeyValuePair<string, string>>					result				= new List<KeyValuePair<string, string>>();
			IDictionary<string, object>							dictionary			= GetDictonary(json);
			return GetObjectKeyValueList(String.Empty, dictionary);
		}

		private static IDictionary<string, object> GetDictonary(string json)
		{
			IDictionary<string, object>					result					= null;

			JavaScriptSerializer						serializer				= new JavaScriptSerializer();

			if(serializer != null)
			{
				// Register the converter
				serializer.RegisterConverters(new[] { new DynamicJsonConverter() });

				object									tempObject				= serializer.Deserialize(json, typeof(object));

				if(tempObject != null)
				{
					DynamicJsonObject					holder					= (DynamicJsonObject) tempObject;

					if(holder != null && holder.Dictionary != null)
					{
						result													= holder.Dictionary;
					}
				}
			}

			return result;
		}


		private static List<KeyValuePair<string, string>> GetObjectKeyValueList(string key, IDictionary<string, object> dictionary)
		{
			List<KeyValuePair<string, string>>						result					= new List<KeyValuePair<string, string>>();
			List<string>											keyList					= new List<string>();

			if(dictionary != null && dictionary.Keys != null)
			{
				keyList																		= dictionary.Keys.ToList();

				for (int i = 0; i < keyList.Count; i++)
				{
					object											tempValue				= null;

					if(dictionary.TryGetValue(keyList[i],out tempValue))
					{
						string										keyName					= GetCleanObject(key, keyList[i]);
						if(IsValue(tempValue))
						{
							result.Add(new KeyValuePair<string, string>(string.Format(CONTENT_OBJECT_OUTPUT_FORMAT, keyName), tempValue.ToString()));
						}
						else
						{
							if(IsDictionary(tempValue))
							{
								result.AddRange(GetObjectKeyValueList(keyName, (IDictionary<string, object>) tempValue));
							}
							else
							{
								result.AddRange(GetObjectKeyValueList(keyName, (ArrayList) tempValue));
							}
						}
					}
				}
			}

			return  result;
		}

		private static string GetCleanObject(string prefix, string postFix)
		{
			string					result				= postFix;

			if(!String.IsNullOrEmpty(prefix))
			{
				result									= String.Format(CONTENT_OBJECT_FORMAT, prefix, postFix); 
			}

            return result;
		}

		/// <summary>
		/// Gets the list of 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="arrayList"></param>
		/// <returns></returns>
		private static List<KeyValuePair<string, string>> GetObjectKeyValueList(string key, ArrayList arrayList)
		{
			List<KeyValuePair<string, string>>						result					= new List<KeyValuePair<string, string>>();

			for (int i = 0; i < arrayList.Count; i++)
			{
				if(arrayList[i] != null)
				{
					result.AddRange(GetObjectKeyValueList(GetCleanObject(key, "[" + i.ToString() + "]"), (IDictionary<string, object>) arrayList[i]));
				}
			}

			return  result;
		}

		/// <summary>
		/// Gets if the given object is value object. Ex String , int etc
		/// </summary>
		/// <param name="value">The object to evaluate</param>
		/// <returns>Is an atom level value</returns>
		private static bool IsValue(object value)
		{
			List<string>		valueTypeList		= new List<string>();

			valueTypeList.Add("system.string");
			valueTypeList.Add("system.decimal");
			valueTypeList.Add("system.double");
			valueTypeList.Add("system.float");
			valueTypeList.Add("system.int32");
			valueTypeList.Add("system.boolean");
			valueTypeList.Add("system.datetime");

			return valueTypeList.Contains(GetTypeName(value));
		}

		/// <summary>
		/// Check if the given object is a dictionary
		/// </summary>
		/// <param name="value">The object to evaluate</param>
		/// <returns>Is a list</returns>
		private static bool IsDictionary(object value)
		{
			return GetTypeName(value) == "system.collections.generic.dictionary`2[system.string,system.object]";
		}

		/// <summary>
		/// Gets the C# type text for the given object
		/// </summary>
		/// <param name="value">Object to get the type</param>
		/// <returns>C# type name</returns>
		private static string GetTypeName(object value)
		{
			Type				type				= value.GetType();
			return type.ToString().ToLower();
		}


		#endregion
	}
}
