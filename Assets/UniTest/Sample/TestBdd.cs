using UniTest;
using System.Collections;

namespace UniTest.Sample 
{
	[TestStory(	1,
				AsA		: "Tester",
				IWant	: "The Test Story",
				SoThat	: "Passed"
				)]
	public class TestBdd
	{

		[TestStory(1, IWant:"which is must be success with coroutine")]
		public IEnumerator SuccessTestCoroutineScope() 
		{
			"This coroutine story #1".ShouldBe("success",()=>true);
			yield return null;
			"This coroutine story #2".ShouldBe("success",()=>true);
		}

		[TestStory(2, IWant:"of Example Substory")]
		public class TestExample 
		{
			[TestStory(1, IWant:"which is must be success")]
			public void SuccessTestSimpleScope() 
			{
				"This story".ShouldBe("success",()=>true);
			}

			[TestStory(2, IWant:"which is must be failure")]
			public void FailureTestSimpleScope() 
			{
				"This story".ShouldBe("failure",()=>false);
			}

			[TestStory(3, IWant:"which is must be ignored")]
			public void IgnoredScenarioOfThird() 
			{
				"This story".ShouldBe("failure",()=>false);
			}
		}

		[TestStory(3, IWant: "which is must be failure with coroutine")]
		public IEnumerator FailureTestCoroutineScope1() 
		{
			"This coroutine story #3".ShouldBe("success",()=>true);
			yield return null;
			"This coroutine story #4".ShouldBe("failure",()=>false);
		}

		[TestStory(4, IWant: "which is must be ignored")]
		public IEnumerator FailureTestCoroutineScope2() 
		{
			"This coroutine story #5".ShouldBe("failure",()=>false);
			yield return null;
			"This coroutine story #6".ShouldBe("failure",()=>false);
		}
	}
}