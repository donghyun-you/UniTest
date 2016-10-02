using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UniTest.Server 
{
	public class TestServerHandlers 
		: IDisposable
	{
		#region internal
		private TestServer _server = null;
		private bool _isDisposed = false;
		public IDisposable disposable = null;

		public TestServerHandlers(TestServer server) 
		{
			_server = server;
			disposable = BindPackets(server);	
		}

		public void Dispose() 
		{
			if(_isDisposed == false) 
			{
				_isDisposed = true;
				disposable.Dispose();
			}
		}

		private IDisposable BindPackets(TestServer server) 
		{
			List<IDisposable> disposables = new List<IDisposable>();

			disposables.Add(server.RegisterHandler(TestServer.MessageType.STDIN,OnStdin));
				
			return DisposableCreator.Create(()=>
			{
				foreach(var disposable in disposables) 
				{
					disposable.Dispose();
				}
				disposables.Clear();
			});
		}

		private Protocol _receiver = new Protocol();
		private void OnStdin(TestServer.ClientConnection sender,TestServer.MessageType message_type,string json) 
		{
			TestLogger.Verbose(this,"<<< "+json);
			JsonUtility.FromJsonOverwrite(json,_receiver);

			Type containerType = Type.GetType("UniTest.Server.TestServerHandlers+Protocol+"+_receiver.func);

			if(containerType == null) 
			{
				TestLogger.Warning(this,"dropping stdin message. unexpected protocol: "+json);
			}
			else 
			{
				Protocol received = JsonUtility.FromJson(json,containerType) as Protocol;
				this.GetType().InvokeMember("OnMessageReceived",BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,null,this,new object[] { sender, received });
			}
		}

		#endregion

		#region business logics
		public class Protocol 
		{
			public string func;
			public string args;

			public class RunAllTest : Protocol 
			{
			}

			public class RunTestOfType : Protocol 
			{
				public Type GetTestType() 
				{
					return Type.GetType(args.Trim());
				}
			}
		}

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunAllTest message) 
		{
			TestLogger.Verbose(this,"RunAllTest");
			sender.Send(TestServer.MessageType.STDOUT,"\"RunAllTest\"");
			_server.CloseClient(sender);
		}

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunTestOfType message) 
		{
			var testType = message.GetTestType();
			TestLogger.Verbose(this,"RunTestOfType: "+(testType == null ? "?":testType.ToString()));
			sender.Send(TestServer.MessageType.STDOUT,"\"RunTestOfType\"");
			_server.CloseClient(sender);
		}
		#endregion
		

	}
}