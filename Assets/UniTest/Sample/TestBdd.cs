using UniTest;
using UnityEngine;
using System;
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
				Scenario("This coroutine #"+i+" for testing test == "+(i+1),test).Should.Be.EqualTo((i+1));
				yield return new WaitForSeconds(0.05f);
			}

		}

		[TestStory(2, IWant:"of Example Substory")]
		public class TestExample : TestFlow
		{
			[TestStory(1, IWant:"which is must be success")]
			public void SuccessTestSimpleScope() 
			{
				Exception ex = null;
				About(ex).Should.Not.Be.Thrown();
				object nullable = null;
				About(nullable).Should.Be.Null();
			}

			[TestStory(2, IWant:"which is must be failure")]
			public void FailureTestSimpleScope() 
			{
				Exception ex = new Exception();
				About(ex).Should.Not.Be.Thrown();
			}

			[TestStory(3, IWant:"which is must be ignored")]
			public void IgnoredScenarioOfThird() 
			{
				Scenario("This story",false).Should.Be.False();
			}
		}

		[TestStory(3, IWant: "which is must be failure with coroutine")]
		public IEnumerator FailureTestCoroutineScope1() 
		{
			Scenario("This coroutine story #3",true).Should.Be.True();
			yield return null;
			Scenario("This coroutine story #4, I know this story will be failure but, ",false).Must.Be.True();
		}

		[TestStory(4, IWant: "which is must be ignored")]
		public IEnumerator FailureTestCoroutineScope2() 
		{
			Scenario("This coroutine story #5",false).Should.Be.False();
			yield return null;
			Scenario("This coroutine story #6, I know this stroy will be failure but,",true).Should.Not.Be.True();
		}
	}
}