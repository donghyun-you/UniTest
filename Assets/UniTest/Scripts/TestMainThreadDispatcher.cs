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
				GameObject.DontDestroyOnLoad(go);
				return go.AddComponent<TestMainThreadDispatcher>();
			}
		}
	}
}