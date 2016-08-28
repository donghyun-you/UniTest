using UnityEngine;
using System.Collections;

namespace UniTest.Sample 
{
	[TestStory(	1,
		AsA		: "Tester",
		IWant	: "The TDD Test",
		SoThat	: "Passed"
	)]
	public class TestTdd
	{
		[TestStory(1, IWant:"which is must be success with coroutine")]
		public IEnumerator SuccessTestCoroutineScope() 
		{
			"This coroutine story #1".ShouldBe("success",()=>true);
			yield return null;
			"This coroutine story #2".ShouldBe("success",()=>true);
		}

	}
}
