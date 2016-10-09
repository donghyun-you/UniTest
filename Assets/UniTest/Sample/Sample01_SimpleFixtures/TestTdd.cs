using UnityEngine;
using System.Collections;

namespace UniTest.Sample 
{
	[TestScenario(3,Summary:"Tdd example")]
	public class TestTdd : TestFlow
	{
		[TestScenario(1, Summary:"test with coroutine for success test")]
		public IEnumerator SuccessTestCoroutineScope() 
		{
			int test = 1;
			Assert("test == 1",test == 1);
			Comment("simple comment");

			for(int i=0;i<5;i++) 
			{
				test++;
				Assert("test == "+(i+2),test == (i+2));
				yield return new WaitForSeconds(0.05f);
			}
		}

		[TestScenario(2, Summary:"test with coroutine for failure test")]
		public IEnumerator FailureTestCoroutineScope() 
		{
			int test = 1;
			Assert("test == 1",test == 1);
			yield return new WaitForSeconds(0.5f);
			test++;
			Assert("test == 1 (it would be failure)",test == 1);
		}
	}
}
