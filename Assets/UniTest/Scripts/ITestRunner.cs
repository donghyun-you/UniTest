using System;

namespace UniTest 
{
	public interface ITestRunner
	{
		TestNode Tester 
		{
			get; 
		}
		void Run(Action<bool> onComplete);
	}
}