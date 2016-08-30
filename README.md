#UniTest

**Simple Unit Test Framework** for Unity3D.

Coroutine support for asynchronous test.

Support Unity Legacy GUI for Runtime/Editor

##Usage(BDD)

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
			Scenario("This coroutine #"+i+" for testing test == "+(i+1)).ShouldBe("success",test == (i+1)).Done();
			yield return new WaitForSeconds(0.05f);
		}
	}
}
```

##Usage(TDD)

implement the class like this.

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

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without r/Users/ruel/git/donghyun-you/UniTest/README.mdestriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.