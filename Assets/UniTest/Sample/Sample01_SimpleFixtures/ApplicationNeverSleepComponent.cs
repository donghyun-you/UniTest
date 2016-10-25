using UnityEngine;
using System.Collections;

namespace UniTest 
{
	public class ApplicationNeverSleepComponent : MonoBehaviour 
	{
		void Start () 
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
	}
}