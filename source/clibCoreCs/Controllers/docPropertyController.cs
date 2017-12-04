using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Controllers;
using Contensive.Core.Controllers.genericController;

namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// doc properties are properties limited in scope to this single hit, or viewing
	/// </summary>
	public class docPropertyController
	{
		//
		private coreClass cpCore;
		//
		private Dictionary<string, docPropertiesClass> docPropertiesDict = new Dictionary<string, docPropertiesClass>();
		//
		public docPropertyController(coreClass cpCore) : base()
		{
			this.cpCore = cpCore;
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, int value)
		{
			setProperty(key, value.ToString());
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, DateTime value)
		{
			setProperty(key, value.ToString());
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, bool value)
		{
			setProperty(key, value.ToString());
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, string value)
		{
			setProperty(key, value, false);
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, string value, bool isForm)
		{
			try
			{
				docPropertiesClass prop = new docPropertiesClass();
				prop.NameValue = key;
				prop.FileSize = 0;
				prop.fileType = "";
				prop.IsFile = false;
				prop.IsForm = isForm;
				prop.Name = key;
				prop.NameValue = key + "=" + value;
				prop.Value = value;
				setProperty(key, prop);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public void setProperty(string key, docPropertiesClass value)
		{
			string propKey = encodeDocPropertyKey(key);
			if (!string.IsNullOrEmpty(propKey))
			{
				if (docPropertiesDict.ContainsKey(propKey))
				{
					docPropertiesDict.Remove(propKey);
				}
				docPropertiesDict.Add(propKey, value);
			}
		}
		//
		//====================================================================================================
		//
		public bool containsKey(string RequestName)
		{
			return docPropertiesDict.ContainsKey(encodeDocPropertyKey(RequestName));
		}
		//
		//====================================================================================================
		//
		public List<string> getKeyList()
		{
			List<string> keyList = new List<string>();
			foreach (KeyValuePair<string, docPropertiesClass> kvp in docPropertiesDict)
			{
				keyList.Add(kvp.Key);
			}
			return keyList;
		}
		//
		//=============================================================================================
		//
		public double getNumber(string RequestName)
		{
			try
			{
				return genericController.EncodeNumber(getProperty(RequestName).Value);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return 0;
		}
		//
		//=============================================================================================
		//
		public int getInteger(string RequestName)
		{
			try
			{
				return genericController.EncodeInteger(getProperty(RequestName).Value);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return 0;
		}
		//
		//=============================================================================================
		//
		public string getText(string RequestName)
		{
			try
			{
				return genericController.encodeText(getProperty(RequestName).Value);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//=============================================================================================
		//
		public string getRenderedActiveContent(string RequestName)
		{
			try
			{
				return cpCore.html.convertEditorResponseToActiveContent(genericController.encodeText(getProperty(RequestName).Value));
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//=============================================================================================
		//
		public bool getBoolean(string RequestName)
		{
			try
			{
				return genericController.EncodeBoolean(getProperty(RequestName).Value);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return false;
		}
		//
		//=============================================================================================
		//
		public DateTime getDate(string RequestName)
		{
			try
			{
				return genericController.EncodeDate(getProperty(RequestName).Value);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return DateTime.MinValue;
		}
		//
		//====================================================================================================
		//
		public docPropertiesClass getProperty(string RequestName)
		{
			try
			{
				string Key;
				//
				Key = encodeDocPropertyKey(RequestName);
				if (!string.IsNullOrEmpty(Key))
				{
					if (docPropertiesDict.ContainsKey(Key))
					{
						return docPropertiesDict(Key);
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return new docPropertiesClass();
		}
		//
		//====================================================================================================
		//
		private string encodeDocPropertyKey(string sourceKey)
		{
			string returnResult = "";
			try
			{
				if (!string.IsNullOrEmpty(sourceKey))
				{
					returnResult = sourceKey.ToLower();
					if (cpCore.webServer.requestSpaceAsUnderscore)
					{
						returnResult = genericController.vbReplace(returnResult, " ", "_");
					}
					if (cpCore.webServer.requestDotAsUnderscore)
					{
						returnResult = genericController.vbReplace(returnResult, ".", "_");
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnResult;
		}
		//
		//
		//
		//==========================================================================================
		/// <summary>
		/// add querystring to the doc properties
		/// </summary>
		/// <param name="QS"></param>
		public void addQueryString(string QS)
		{
			try
			{
				//
				string[] ampSplit = null;
				int ampSplitCount = 0;
				string[] ValuePair = null;
				string key = null;
				int Ptr = 0;
				//
				ampSplit = QS.Split('&');
				ampSplitCount = ampSplit.GetUpperBound(0) + 1;
				for (Ptr = 0; Ptr < ampSplitCount; Ptr++)
				{
					string nameValuePair = ampSplit[Ptr];
					docPropertiesClass docProperty = new docPropertiesClass();
					if (!string.IsNullOrEmpty(nameValuePair))
					{
						if (genericController.vbInstr(1, nameValuePair, "=") != 0)
						{
							ValuePair = nameValuePair.Split('=');
							key = DecodeResponseVariable(Convert.ToString(ValuePair[0]));
							if (!string.IsNullOrEmpty(key))
							{
								docProperty.Name = key;
								if (ValuePair.GetUpperBound(0) > 0)
								{
									docProperty.Value = DecodeResponseVariable(Convert.ToString(ValuePair[1]));
								}
								docProperty.IsForm = false;
								docProperty.IsFile = false;
								//cpCore.webServer.readStreamJSForm = cpCore.webServer.readStreamJSForm Or (UCase(.Name) = genericController.vbUCase(RequestNameJSForm))
								cpCore.docProperties.setProperty(key, docProperty);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// return the docProperties collection as the legacy optionString
		/// </summary>
		/// <returns></returns>
		public string getLegacyOptionStringFromVar()
		{
			string returnString = "";
			try
			{
				foreach (string key in getKeyList())
				{
					returnString += "" + "&" + genericController.encodeLegacyOptionStringArgument(key) + "=" + encodeLegacyOptionStringArgument(getProperty(key).Value);
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			return returnString;
		}
	}


}