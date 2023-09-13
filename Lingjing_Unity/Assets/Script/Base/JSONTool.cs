using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class JSONReader {
	JObject json;
	public JSONReader(string _json) {
		json = JObject.Parse(_json);
	}

	#region public methords

	public bool TryPraseString(string key, ref string value) {
		return TryPraseString(json, key, ref value);
	}

	public bool TryPraseBool(string key, ref bool value) {
		return TryPraseBool(json, key, ref value);
	}
	public bool TryPraseInt(string key, ref int value) {
		return TryPraseInt(json, key, ref value);
	}
	public bool TryPraseFloat(string key, ref float value) {
		return TryPraseFloat(json, key, ref value);
	}
	public bool TryPraseDouble(string key, ref double value) {
		return TryPraseDouble(json, key, ref value);
	}
	public bool TryPraseObject<T>(string key, ref T value) {
		return TryPraseObject(json, key, ref value);
	}
	public bool TryPraseArray(string key, out List<string> value) {
		return TryPraseArray(json, key, out value);
	}

	public bool ContainsKey(string key) {
		return ContainsKey(json, key);
	}
	#endregion

	#region static methords

	public static bool TryPraseString(string jsonString, string key, ref string value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseString(json, key, ref value);
		} catch (Exception) {
			return false;
		}

	}

	public static bool TryPraseBool(string jsonString, string key, ref bool value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseBool(json, key, ref value);
		} catch (Exception) {
			return false;
		}
	}
	public static bool TryPraseInt(string jsonString, string key, ref int value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseInt(json, key, ref value);
		} catch (Exception) {
			return false;
		}
	}
	public static bool TryPraseFloat(string jsonString, string key, ref float value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseFloat(json, key, ref value);
		} catch (Exception) {
			return false;
		}
	}
	public static bool TryPraseDouble(string jsonString, string key, ref double value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseDouble(json, key, ref value);
		} catch (Exception) {
			return false;
		}
	}
	public static bool TryPraseObject<T>(string jsonString, string key, ref T value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseObject(json, key, ref value);
		} catch (Exception) {
			return false;
		}
	}
	public static bool TryPraseArray(string jsonString, string key, out List<string> value) {
		try {
			JObject json = JObject.Parse(jsonString);
			return TryPraseArray(json, key, out value);
		} catch (System.Exception) {
			value = null;
			return false;
		}
	}

	public static bool ContainsKey(string jsonString, string key) {
		try {
			JObject json = JObject.Parse(jsonString);
			return ContainsKey(json, key);
		} catch (Exception) {
			return false;
		}
	}

	public static object GetValue(string jsonString, string key) {
		JObject json = JObject.Parse(jsonString);
		return GetValue(json, key);
	}
	#endregion

	#region internal static methords

	protected static bool TryPraseString(JToken json, string key, ref string value) {
		if (ContainsKey(json, key)) {
			value = json[key].ToString();
			return true;
		}
		return false;
	}

	protected static bool TryPraseBool(JToken json, string key, ref bool value) {
		if (ContainsKey(json, key)) {
			return bool.TryParse(json[key].ToString(), out value);
		}
		return false;
	}

	protected static bool TryPraseInt(JToken json, string key, ref int value) {
		if (ContainsKey(json, key)) {
			return int.TryParse(json[key].ToString(), out value);
		}
		return false;
	}
	protected static bool TryPraseFloat(JToken json, string key, ref float value) {
		if (ContainsKey(json, key)) {
			return float.TryParse(json[key].ToString(), out value);
		}
		return false;
	}
	protected static bool TryPraseDouble(JToken json, string key, ref double value) {
		if (ContainsKey(json, key)) {
			return double.TryParse(json[key].ToString(), out value);
		}
		return false;
	}

	protected static bool TryPraseObject<T>(JToken json, string key, ref T value) {
		if (ContainsKey(json, key)) {
			value = json[key].ToObject<T>();
			return true;
		}
		return false;
	}
	protected static bool TryPraseArray(JToken json, string key, out List<string> value) {
		value = new List<string>();
		if (ContainsKey(json, key)) {
			foreach (JToken item in json[key].ToObject<JArray>()) {
				value.Add(item.ToString());
			}
			return true;
		}
		return false;
	}

	protected static bool ContainsKey(JToken json, string key) {
		return json[key] != null;
	}

	protected static object GetValue(JToken json, string key) {
		if (ContainsKey(json, key)) {
			return json[key];
		}
		return "";
	}
	#endregion
}

public class JSONWriter {
	JObject json;
	public JSONWriter() {
		json = new JObject();
	}
	public void Add(string key, string value) {
		json.Add(new JProperty(key, value));
	}
	public void Add(string key, int value) {
		json.Add(new JProperty(key, value));
	}
	public void Add(string key, float value) {
		json.Add(new JProperty(key, value));
	}
	public void Add(string key, bool value) {
		json.Add(new JProperty(key, value));
	}
	public void Add(string key, object value) {
		json.Add(key, JObject.FromObject(value));
	}


	public override string ToString() {
		return json.ToString();
	}
}
