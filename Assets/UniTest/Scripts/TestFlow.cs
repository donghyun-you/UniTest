using System;
using System.Diagnostics;
using System.Reflection;

namespace UniTest 
{
	public class TestFlow
	{
		public delegate void TestedScenarioEvent(TestFlow flow, string flow_method, string summary);
		public event TestedScenarioEvent OnTestSucceed = delegate(TestFlow flow, string flow_method, string summary) {};
		public event TestedScenarioEvent OnTestCritical = delegate(TestFlow flow, string flow_method, string summary) {};
		public event TestedScenarioEvent OnTestComment = delegate(TestFlow flow, string flow_method, string summary) {};
		public event TestedScenarioEvent OnTestWarning = delegate(TestFlow flow, string flow_method, string summary) {};

		public string message = "";

		public TestFlow Parent 
		{
			get; private set;
		}

		public string ParentMethod 
		{
			get; private set;
		}

		public object Subject
		{
			get; private set;
		}

		private bool _negation = true;

		#region subject

		public TestFlow Scenario(string message,object subject) 
		{
			return new TestFlow(this,getMethodName(),message,subject);
		}

		public TestFlow About(object subject) 
		{
			return new TestFlow(this,getMethodName(),subject == null ? "?" : subject.ToString(),subject);
		}

		protected TestFlow() 
		{
		}

		protected TestFlow(TestFlow parent,string parent_method,string message,object subject) 
		{
			this.Parent 		= parent;
			this.ParentMethod 	= parent_method;
			this.message 		= message;
			this.Subject 		= subject;
		}

		#endregion

		#region conclusion assertion

		public TestFlow Type(Type type) 
		{
			message += " type";
			if((Subject.GetType() == type) != _negation) 
			{
				throw new Exception(message.Trim());
			} 
			return Done();
		}

		public TestFlow Thrown() 
		{
			message += " exception";
			if((Subject != null && Subject is Exception) != _negation) 
			{
				if(Subject is Exception) 
				{
					throw new ScenarioFailureException(message.Trim(),Subject as Exception);
				} 
				else 
				{
					throw new ScenarioFailureException(message.Trim());
				}
			} 
			return Done();
		}

		public TestFlow EqualTo(IComparable target) 
		{
			message += " equal to "+target.ToString();
			if((Subject is IComparable && (Subject as IComparable).CompareTo(target) == 0) != _negation) 
			{
				throw new ScenarioFailureException(message.Trim());
			} 
			return Done();
		}

		public TestFlow OK() 
		{
			message += " OK";
			if((Subject is bool && Convert.ToBoolean(Subject)) != _negation) 
			{
				throw new ScenarioFailureException(message.Trim());
			}
			return Done();
		}

		public TestFlow Null() 
		{
			message += " null";
			if((Subject == null) != _negation)
			{
				throw new ScenarioFailureException(message.Trim());
			}
			return Done();
		}

		public TestFlow True() 
		{
			message += " true";
			if((Subject is bool && Convert.ToBoolean(Subject) == true) != _negation)
			{
				throw new ScenarioFailureException(message.Trim());
			}
			return Done();
		}

		public TestFlow False() 
		{
			message += " false";
			if((Subject is bool && Convert.ToBoolean(Subject) == false) != _negation)
			{
				throw new ScenarioFailureException(message.Trim());
			}
			return Done();
		}

		public TestFlow Done()
		{
			OnTestSucceed.Invoke(this,null,this.message);

			if(Parent != null) 
			{
				Parent.OnTestSucceed(this.Parent,this.ParentMethod,this.message);
			}

			return this;
		}

		public TestFlow Comment(string comment=null) 
		{
			if(string.IsNullOrEmpty(comment)) 
			{
				OnTestComment.Invoke(this,null,comment);

				if(Parent != null) 
				{
					Parent.OnTestComment(this.Parent,this.ParentMethod,comment);
				}	
			}

			return this;
		}

		public TestFlow Warn(string message=null) 
		{
			if(string.IsNullOrEmpty(message)) 
			{
				OnTestComment.Invoke(this,null,message);

				if(Parent != null) 
				{
					Parent.OnTestWarning(this.Parent,this.ParentMethod,message);
				}	
			}

			return this;
		}

		public TestFlow Critical(string message=null) 
		{
			if(string.IsNullOrEmpty(message)) 
			{
				OnTestComment.Invoke(this,null,message);

				if(Parent != null) 
				{
					Parent.OnTestCritical(this.Parent,this.ParentMethod,message);
				}	
			}

			return this;
		}

		#endregion

		#region negation chaining

		public TestFlow Not		{ get { this.message += " not"; this._negation = !this._negation; return this; } }

		#endregion

		#region chaining

		public TestFlow Be 		{ get { this.message += " be"; 		return this; } }
		public TestFlow An 		{ get { this.message += " an"; 		return this; } }
		public TestFlow Of 		{ get { this.message += " of"; 		return this; } }
		public TestFlow A 		{ get { this.message += " a"; 		return this; } }
		public TestFlow And 	{ get { this.message += " and"; 	return this; } }
		public TestFlow Have 	{ get { this.message += " have"; 	return this; } }
		public TestFlow Has 	{ get { this.message += " has"; 	return this; } }
		public TestFlow With 	{ get { this.message += " with"; 	return this; } }
		public TestFlow Is 		{ get { this.message += " is"; 		return this; } }
		public TestFlow Which 	{ get { this.message += " which"; 	return this; } }
		public TestFlow The 	{ get { this.message += " the"; 	return this; } }
		public TestFlow It 		{ get { this.message += " it"; 		return this; } }
		public TestFlow Should	{ get { this.message += " should";	return this; } }
		public TestFlow Must	{ get { this.message += " must";	return this; } }

		#endregion

		#region assert

		public void Assert(Exception ex) 
		{
			if(ex != null) 
			{
				throw ex;
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

		#endregion

		private string getAssertedFilename() 
		{
			return (new StackTrace(2)).GetFrames()[0].GetFileName();
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
	}
}