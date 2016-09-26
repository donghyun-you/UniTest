using System;

namespace UniTest.Server 
{
	public class TestPacketBind : Attribute
	{
		public TestServer.MessageType Message 
		{
			get;
			private set;
		}

		public TestPacketBind(TestServer.MessageType message) 
		{
			Message = message;
		}
	}
}