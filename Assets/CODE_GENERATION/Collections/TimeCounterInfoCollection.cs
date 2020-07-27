using ScriptableObjectArchitecture;
using UnityEngine;


namespace Pok
{
	[CreateAssetMenu(
	    fileName = "TimeCounterInfoCollection.asset",
	    menuName = SOArchitecture_Utility.COLLECTION_SUBMENU + "TimeCounterInfo",
	    order = 120)]
	public class TimeCounterInfoCollection : Collection<TimeCounterInfo>
	{
	}
}