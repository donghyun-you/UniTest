using System;
using System.Diagnostics;
using System.Reflection;

namespace UniTest 
{
	public class TestFlow
	{
		public delegate void TestedScenarioEvent(TestFlow flow, string flow_method, string summary);
		public event TestedScenarioEvent OnTestSucceed = delegate(TestFlow flow, string flow_method, string summary) {};

		public string message = "";

		public TestFlow Parent 
		{
			get; private set;
		}

		public string ParentMethod 
		{
			get; private set;
		}

		public TestFlow Scenario(string message) 
		{
			return new TestFlow(this,getMethodName(),message);
		}

		private string getMethodName() 
		{
			StackTrace calledInfo = new StackTrace(2);
			string methodName = calledInfo.GetFrame(0).GetMethod().Name;

			// NOTE(ruel): estimate this coroutine.
			if(methodName == "MoveNext") 
			{
				string reflectedTypeName = calledInfo.GetFrame(0).GetMethod().ReflectedType.Name;
				methodName = reflectedTypeName.Substring(reflectedTypeName.IndexOf('<') + 1, reflectedTypeName.IndexOf('>') - 1);
			}

			return methodName;
		}

		protected TestFlow() 
		{
		}

		protected TestFlow(TestFlow parent,string parent_method,string message) 
		{
			this.Parent = parent;
			this.ParentMethod = parent_method;
			this.message = message;
		}

		public TestFlow ShouldBe(string what,Func<bool> condition)
		{
			condition = condition ?? delegate 
			{
				return true;
			};

			if(condition() == false) throw new ScenarioFailureException(message.Trim()+", and should not be "+what); 

			message += " should be "+what;

			return this;
		}

		public TestFlow ShouldBe(string what,bool condition)
		{
			return ShouldBe(what,()=>condition);
		}

		public void Done() 
		{
			OnTestSucceed.Invoke(this,null,this.message);

			if(Parent != null) 
			{
				Parent.OnTestSucceed(this.Parent,this.ParentMethod,this.message);
			}
		}

		public void Assert(bool condition) 
		{
			if(condition == false) 
			{
				throw new Exception(getAssertedFilename());
			}
			else 
			{
				OnTestSucceed.Invoke(this,getMethodName(),message);
			}
		}

		public void Assert(string message,bool condition) 
		{
			if(message == null) 
			{
				message = getAssertedFilename();	
			}

			if(condition == false) 
			{
				throw new Exception(message);
			} 
			else 
			{
				OnTestSucceed.Invoke(this,getMethodName(),message);
			}
		}

		private string getAssertedFilename() 
		{
			return (new StackTrace(2)).GetFrames()[0].GetFileName();
		}
	}
}