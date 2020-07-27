using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("m_AssetGUID", "m_Operation")]
	public class ES3UserType_AssetReference : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_AssetReference() : base(typeof(UnityEngine.AddressableAssets.AssetReference)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (UnityEngine.AddressableAssets.AssetReference)obj;
			
			writer.WritePrivateField("m_AssetGUID", instance);
			writer.WritePrivateField("m_Operation", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (UnityEngine.AddressableAssets.AssetReference)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "m_AssetGUID":
					reader.SetPrivateField("m_AssetGUID", reader.Read<System.String>(), instance);
					break;
					case "m_Operation":
					reader.SetPrivateField("m_Operation", reader.Read<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new UnityEngine.AddressableAssets.AssetReference();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_AssetReferenceArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_AssetReferenceArray() : base(typeof(UnityEngine.AddressableAssets.AssetReference[]), ES3UserType_AssetReference.Instance)
		{
			Instance = this;
		}
	}
}