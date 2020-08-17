using Pok;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public class UnitDefineLevelIntEvent : UnityEvent<UnitDefineLevelInt> { }

	[CreateAssetMenu(
	    fileName = "UnitDefineLevelIntVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "UnitDefineLevelInt",
	    order = 120)]
	public class UnitDefineLevelIntVariable : BaseVariable<UnitDefineLevelInt, UnitDefineLevelIntEvent>
	{
	}
}