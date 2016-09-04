using UnityEngine;
using System.Collections;

namespace UniTest 
{
	public class TestMainThreadDispatcher 
		: MonoBehaviour 
	{
		public static TestMainThreadDispatcher Instance 
		{
			get 
			{
				var go = new GameObject(typeof(TestMainThreadDispatcher).Name);
				return go.AddComponent<TestMainThreadDispatcher>();
			}
		}
	}
}