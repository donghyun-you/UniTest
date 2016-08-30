using UniTest;
using UnityEngine;
using System.Collections;

namespace UniTest.Sample 
{
	[TestStory(	1,
				AsA		: "Tester",
				IWant	: "The Test Story",
				SoThat	: "Passed"
				)]
	public class TestBdd : TestFlow
	{

		[TestStory(1, IWant:"which is must be success with coroutine")]
		public IEnumerator SuccessTestCoroutineScope() 
		{
			
			int test = 0;
			for(int i=0;i<5;i++) 
			{
				test++;
				Scenario("This coroutine #"+i+" for testing test == "+(i+1)).ShouldBe("success",test == (i+1)).Done();
				yield return new WaitForSeconds(0.05f);
			}

		}

		[TestStory(2, IWant:"of Example Substory")]
		public class TestExample : TestFlow
		{
			[TestStory(1, IWant:"which is must be success")]
			public void SuccessTestSimpleScope() 
			{
				Scenario("This story").ShouldBe("success",()=>true).Done();
			}

			[TestStory(2, IWant:"which is must be failure")]
			public void FailureTestSimpleScope() 
			{
				Scenario("This story").ShouldBe("failure",()=>false).Done();
			}

			[TestStory(3, IWant:"which is must be ignored")]
			public void IgnoredScenarioOfThird() 
			{
				Scenario("This story").ShouldBe("failure",()=>false).Done();
			}
		}

		[TestStory(3, IWant: "which is must be failure with coroutine")]
		public IEnumerator FailureTestCoroutineScope1() 
		{
			Scenario("This coroutine story #3").ShouldBe("success",()=>true).Done();
			yield return null;
			Scenario("This coroutine story #4").ShouldBe("failure",()=>false).Done();
		}

		[TestStory(4, IWant: "which is must be ignored")]
		public IEnumerator FailureTestCoroutineScope2() 
		{
			Scenario("This coroutine story #5").ShouldBe("failure",()=>false).Done();
			yield return null;
			Scenario("This coroutine story #6").ShouldBe("failure",()=>false).Done();
		}
	}
}