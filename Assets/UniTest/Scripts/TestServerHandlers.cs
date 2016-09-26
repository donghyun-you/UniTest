using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UniTest.Server 
{
	public class TestServerHandlers 
		: IDisposable
	{
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

			disposables.Add(server.RegisterHandler(TestServer.MessageType.CLOSE,OnClosed));
			disposables.Add(server.RegisterHandler(TestServer.MessageType.ECHO,OnEcho));
				
			return DisposableCreator.Create(()=>
			{
				foreach(var disposable in disposables) 
				{
					disposable.Dispose();
				}
				disposables.Clear();
			});
		}

		private void OnClosed(TestServer.ClientConnection sender,TestServer.MessageType message_type,string json) 
		{
			TestLogger.Info(this,"on close invoked");
			_server.CloseClient(sender);
		}

		private void OnEcho(TestServer.ClientConnection sender,TestServer.MessageType message_type,string json) 
		{
			TestLogger.Info(this,"echoing");
			sender.Send(TestServer.MessageType.ECHO,"Replying: "+json);
		}
	}
}