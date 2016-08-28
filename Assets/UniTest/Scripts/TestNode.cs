using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using System.Text;
using UnityEngine;

namespace UniTest 
{
	public class TestNode
		: TestElement
	{

		public TestElement[] Children
		{
			get;
			private set;
		}

		public Type NodeType 
		{
			get; 
			private set; 
		}

		public override string Summarize() 
		{
			StringBuilder builder = new StringBuilder();

			var instanceType = this.Instance == null ? "<unknown type>" : this.Instance.GetType().ToString();
			builder.AppendFormat("{0}: {1} ({2})",this.TestState,instanceType,this.Story);

			return builder.ToString();
		}

		public static class Factory 
		{
			public static TestNode Create(Type node_type, TestNode parent) 
			{
				var testStoryAttributes = node_type.GetCustomAttributes(typeof(TestStoryAttribute),false).Select(entry => entry as TestStoryAttribute);

				if(!testStoryAttributes.Any()) 
				{
					throw new InvalidProgramException("TestStoryNodeAttribute required for "+node_type);	
				}

				var testStoryAttribute = testStoryAttributes.First();

				var node = new TestNode() 
				{
					Parent			= parent,
					NodeType 		= node_type,
					TestState 		= TestResultType.kNotTested,
					Instance 		= Activator.CreateInstance(node_type),
					Order 			= testStoryAttributes.First().Order,
					SelfStory		= testStoryAttribute.Summary,
				};

				if(parent == null) node.buildHierarchy();

				return node;
			}

			public static TestNode CreateCompositionNode(string self_story) 
			{
				return new TestNode() 
				{
					TestState 		= TestResultType.kNotTested,
					SelfStory 		= self_story,
				};
			}
		}

		private void buildHierarchy() 
		{
			// NOTE(ruel): nestedClasses which is contains TestStoryNodeAttribute
			var nestedClasses 	= NodeType	// Reterieve all nested classes of this class
											.GetNestedTypes()
											// filter all nested classes that contains TestStoryNodeAttribute 
											.Where(type=>type.GetCustomAttributes(typeof(TestStoryAttribute),false).Any())
											// create BddNode for normalize interface
											.Select(type=>Factory.Create(type,this) as TestElement);
											
			var testMethods		= NodeType	// Reterieve all methods of this class
											.GetMethods(BindingFlags.Instance | BindingFlags.Public)
											// filter all methods that contains TestStoryFixtureAttribute
											.Where(method=>method.GetCustomAttributes(typeof(TestStoryAttribute),false).Any())
											// create TestElement for create interface to control
											.Select(method=>TestMethod.Factory.Create(this,method.Name) as TestElement);
			
			this.Children 		= (nestedClasses.Concat(testMethods)).OrderBy(element=>element.Order).ToArray();

			TestLogger.Verbose(this,"TestCases: count: "+this.Children.Length+"/ \nList/ \n"+string.Join("\n",this.Children.Select(cases=>"["+cases.Order+"] "+cases.Summarize()).ToArray()));

			if(this.Children.Any()) 
			{
				// NOTE(ruel): build children recursively.
				foreach(var child in this.Children.Where(cases=>cases is TestNode).Select(cases=>cases as TestNode)) 
				{
					child.buildHierarchy();
				}
			}
		}

		public override void Execute(Action<bool> on_finished = null) 
		{
			execute(on_finished).ToObservable().Subscribe();
		}

		private IEnumerator execute(Action<bool> on_finished) 
		{
			on_finished = on_finished ?? delegate(bool is_succeeded) {
				TestLogger.Info(this,"succeeded: "+is_succeeded);
			};

			if(on_finished == null) 
			{
				throw new ArgumentNullException("onFinished");
			}

			foreach(TestElement testCase in Children) 
			{
				Exception caughtEx = null;

				if(this.TestState == TestResultType.kFailed) 
				{
					testCase.MarkAsIgnored();
					if(testCase is TestMethod)
					{
						TestLogger.Info(this,"<color=gray>- "+testCase.Summarize()+"</color>");
					}
				} 
				else 
				{
					yield return Observable.Create<Unit>(ob=>
					{
						testCase.Execute(result=> 
						{ 
							if(result) 
							{
								ob.OnNext(Unit.Default);
								ob.OnCompleted();
							} else {
								ob.OnError(testCase.FailedException);
							} 
						});
						return Disposable.Empty;
					})
					.StartAsCoroutine(_=>{

					},ex=>{
						caughtEx = ex;
					});

					if(caughtEx != null) 
					{
						this.TestState = TestResultType.kFailed;
						if(testCase is TestMethod)
						{
							TestLogger.Info(this,"<color=red>\u2716 "+testCase.Summarize()+"</color>");
						}
					}
					else 
					{
						if(testCase is TestMethod)
						{
							TestLogger.Info(this,"<color=green>\u2714 "+testCase.Summarize()+"</color>");
						}
					}
				}
			}

			if(this.TestState == TestResultType.kNotTested) 
			{
				this.TestState = TestResultType.kPassed;
				on_finished(true);
			}
			else
			{
				on_finished(false);
			}
		}

		public void Add(TestElement element) 
		{
			if(element != null) 
			{
				if(this.Children == null) 
				{
					this.Children = new TestElement[] { element };
				}
				else 
				{
					this.Children = this.Children.Concat(Enumerable.Repeat(element,1)).ToArray();	
				}
			}
		}

		public void AddRange(IEnumerable<TestElement> elements) 
		{
			if(elements != null && elements.Any()) 
			{
				if(this.Children == null) 
				{
					this.Children = elements.ToArray();
				}
				else 
				{
					this.Children = this.Children.Concat(elements).ToArray();
				}
			}
		}
	}
}