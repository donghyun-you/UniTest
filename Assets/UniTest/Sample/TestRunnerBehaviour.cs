using UnityEngine;
using System.Collections;
using UniTest;

namespace UniTest.Sample 
{
	public class TestRunnerBehaviour 
		: MonoBehaviour 
	{

		// Use this for initialization
		void Start () 
		{
			var runner = new TestRunner(typeof(TestBdd));
			runner.Run(result=>{
				
			});
		}
	}
}