#UniTest

**This project is no longer maintaned. Unity3D implemented "Unity Test Runner" as same purpose. and it works enough**
https://docs.unity3d.com/Manual/testing-editortestsrunner.html

**Simple Unit Test Framework** for Unity3D.

Coroutine support for asynchronous test.

Support Unity Legacy GUI for Runtime/Editor

##Executing

![sample](images/sample.png?raw=true "running sample(runtime)")

##Usage (Meaningful chaning)

implement a class like this. this implementation inspired from should.js

```cs
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

		[TestStory(3, IWant: "which is must be failure with coroutine")]
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
```


##Usage (Simple Assert)

implement a class like this.

```cs
[TestScenario(2,Summary:"Tdd example")]
public class TestTdd : TestFlow
{
	[TestScenario(1, Summary:"test with coroutine for success test")]
	public IEnumerator SuccessTestCoroutineScope() 
	{
		int test = 1;
		Assert("test == 1",test == 1);

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
```

##License

The MIT License (MIT)

Copyright (c) 2016 Donghyun You

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
