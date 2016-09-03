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
				AssertIf("This coroutine #"+i+" for testing test == "+(i+1),test).Should.Be.EqualTo((i+1));
				yield return new WaitForSeconds(0.05f);
			}

			WarnIf("list of 1 to 5",new int[] { 1,2,3,4,5 }).Should.Contains(1).And.Contains(5);
			CommentIf("list of 1 to 5",new int[] { 1,2,3,4,5 }).Should.Contains(1).And.Contains(5);
			AssertIf("list of 1 to 5",new int[] { 1,2,3,4,5 }).Should.Contains(1).And.Contains(5);

			AssertAbout("I'm an orange").Should.MatchesWith("orange");
			AssertAbout("I'm an orange").Should.Not.MatchesWith("apple");

			AssertAbout(1).Should.Be.GreaterThanOrEqualTo(1);
			AssertAbout(1).Should.Be.GreaterThanOrEqualTo(0);
			AssertAbout(1).Should.Be.EqualTo(1);
			AssertAbout(1).Should.Be.LesserThan(2);
			AssertAbout(1).Should.Be.LesserThanOrEqualTo(1);

			WarnAbout(true).Should.Be.True();
			WarnAbout(false).Should.Be.False();
			WarnAbout(0).Should.Be.ValueType();
			WarnAbout("some string").Should.Not.Be.ValueType();

			WarnAbout("WarnAbout will not assert").Should.MatchesWith("nothing");
			CommentAbout("CommentAbout will not assert").Should.MatchesWith("nothing");
			WarnIf("WarnIf will not assert",null).Should.Not.Null();
			CommentIf("CommentIf will not assert",(string)null).Should.MatchesWith("nothing");

			CommentAbout("boo string").Should.Be.TypeOf(typeof(string));
			CommentAbout("boo string").Should.Be.TypeOf<string>();

			CommentAbout(1f).Should.Be.Numeric();
			CommentAbout("zzz").Should.Be.Numeric();
		}

		[TestStory(2, IWant:"of Example Substory")]
		public class TestExample : TestFlow
		{
			[TestStory(1, IWant:"which is must be success")]
			public void SuccessTestSimpleScope() 
			{
				Exception ex = null;
				AssertAbout(ex).Should.Not.Be.Thrown();
				object nullable = null;
				AssertAbout(nullable).Should.Be.Null();
			}

			[TestStory(2, IWant:"which is must be failure")]
			public void FailureTestSimpleScope() 
			{
				Exception ex = new Exception();
				AssertAbout(ex).Should.Not.Be.Thrown();
			}

			[TestStory(3, IWant:"which is must be ignored")]
			public void IgnoredScenarioOfThird() 
			{
				AssertIf("This story",false).Should.Be.False();
			}
		}

		[TestStory(3, IWant: "which is must be ignored with coroutine")]
		public IEnumerator FailureTestCoroutineScope1() 
		{
			AssertIf("This coroutine story #3",true).Should.Be.True();
			yield return null;
			AssertIf("This coroutine story #4, I know this story will be failure but, ",false).Must.Be.True();
		}

		[TestStory(4, IWant: "which is must be ignored")]
		public IEnumerator FailureTestCoroutineScope2() 
		{
			AssertIf("This coroutine story #5",false).Should.Be.False();
			yield return null;
			AssertIf("This coroutine story #6, I know this stroy will be failure but,",true).Should.Not.Be.True();
		}
	}
}