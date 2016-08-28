using System;

namespace UniTest 
{
	public static class ShouldBeScenario 
	{
		public static string ShouldBe(this string self,string what,Func<bool> condition)
		{
			condition = condition ?? delegate 
			{
				return true;
			};
				
			if(condition() == false) throw new ScenarioFailureException(self.Trim()+", and should not be "+what); 
			return self.Trim() + " should be "+what;
		}
	}
}