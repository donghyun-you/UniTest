using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

using System.Runtime.InteropServices;

namespace UniTest.Server 
{
	public class TestServer
	{
		#region config
		private const string IP = "0.0.0.0";
		#endregion

		#region threadsafe events
		public event Action onStarted = delegate() {};
		public event Action<Exception> onErrored = delegate(Exception obj) {};
		#endregion

		public delegate void LineReceive(ClientConnection sender, string Data);

		public enum MessageType 
		{
			UNDEFINED,

			STDIN,
			STDOUT,
			STDERR,
		}

		public class ClientConnection : IDisposable
		{
			public static int s_increment = 0;
			public int InstanceId 
			{
				get; 
				private set;
			}

			private const int BUFFER_SIZE = 0xff;
			private TcpClient _client;
			private TestServer _server;
			private byte[] _buffer = new byte[BUFFER_SIZE];
			private bool _isDisposed = false;
			public bool IsDisposed 
			{
				get 
				{
					return _isDisposed; 
				}
			}

			public event LineReceive onLineReceived;

			public ClientConnection(TestServer server,TcpClient client) 
			{
				_client = client;	
				_server = server;
				_client.GetStream().BeginRead(_buffer, 0, BUFFER_SIZE, new AsyncCallback(receiver), null);
				InstanceId = s_increment++;
			}

			public void Send(MessageType message_type,string data) 
			{
				//TestLogger.Info(this,"Sending("+message_type+"): "+data);
				if(data == null) 
				{
					throw new ArgumentNullException("data");
				}

				lock(this._client.GetStream()) 
				{
					var writer = new StreamWriter(this._client.GetStream());
					writer.Write("Length: "+data.Length+"\n");
					writer.Write("MessageType: "+message_type.ToString()+"\n");
					writer.Write("\n");
					writer.Write(data + (char)13 + (char) + 10);
					writer.Flush();
				}
			}

			public void SendOut(string message) 
			{
				this.Send(TestServer.MessageType.STDOUT,message);
			}

			public void SendError(string message) 
			{
				this.Send(TestServer.MessageType.STDERR,message);
			}

			public void Dispose() 
			{
				if(_isDisposed == false) 
				{
					_isDisposed = true;
					_client.Close();
					TestLogger.Info(this,"closing");
				}
			}

			private void receiver(IAsyncResult result) 
			{
				int read;
				string message;

				try 
				{
					if(_isDisposed) 
					{
						return;
					}

					lock(_client.GetStream()) 
					{
						read = _client.GetStream().EndRead(result);
					}

					if(read > 0) 
					{
						message = Encoding.UTF8.GetString(_buffer, 0, read);
						var splitMessage = message.Split('\n');
						for(int i=0,d=splitMessage.Length;i<d;i++) 
						{
							onLineReceived(this,splitMessage[i]);
						}

						lock (_client.GetStream())
						{
							_client.GetStream().BeginRead(_buffer, 0, BUFFER_SIZE, new AsyncCallback(receiver), null);
						}
					}
					else 
					{
						// message receiving failed. close the connection
						_server.CloseClient(this);
					}
				} 
				catch(Exception ex) 
				{
					Debug.LogException(ex);
				}
			}
		}

		private TcpListener _listener = null;

		private IPAddress _ip = null;
		private short _port = 7701;

		private Thread _thread = null;

		private List<ClientConnection> _connections = new List<ClientConnection>();

		public static class Factory 
		{
			public static TestServer Create(short port=7701) 
			{
				return new TestServer() 
				{
					_port = port,
					_ip = IPAddress.Parse(IP),
				};
			}
		}

		public void Start() 
		{

			TestLogger.Info(this,"start: "+_ip+":"+_port);

			if(_thread != null) 
			{
				TestLogger.Warning(this,"skipping start the network. it has already started");
			}
			else 
			{
				// NOTE(donghyun-you): make sure test main thread dispatcher existing.
				TestMainThreadDispatcher.Instance.Run(()=>
				{
					_thread = new Thread(new ThreadStart(main));
					_thread.Start();
				},Debug.LogException);
			}
		}

		public void CloseClient(ClientConnection connection) 
		{
			connection.Dispose();
			_connections.Remove(connection);
		}

		public void CloseAllClient() 
		{
			foreach(var connection in _connections) 
			{
				connection.Dispose();
			}
			_connections.Clear();
		}

		public void Close() 
		{
			CloseAllClient();

			if(_thread != null && _thread.IsAlive) 
			{
				_thread.Abort();
				_thread = null;
			} 
		}

		void main() 
		{
			try 
			{
				_listener = new TcpListener(_ip, _port);
				_listener.Start();

				TestMainThreadDispatcher.Instance.Run(()=>
				{
					onStarted();
				},Debug.LogException);

				TestLogger.Info(this,"start server looping");

				for(;;) 
				{
					var connection = new ClientConnection(this,_listener.AcceptTcpClient());
					connection.onLineReceived += OnLineReceived;
					_connections.Add(connection);
					TestLogger.Info(this,"new connection found: ");
				}

			} 
			catch(Exception ex) 
			{
				if(ex is ThreadAbortException == false) 
				{
					Debug.LogWarning(ex);

					TestMainThreadDispatcher.Instance.Run(()=>
					{
						onErrored(ex);
					},Debug.LogException);
				}
				else 
				{
					TestLogger.Info(this,"test server is shutting down");
				}
			}
			finally
			{
				if(_listener != null) 
				{
					_listener.Stop();
					_listener = null;
				}
			}
		}


		Dictionary<ClientConnection,ProcessingMessage> messages = new Dictionary<ClientConnection, ProcessingMessage>();

		private class ProcessingMessage
		{
			public long length = -1;
			public MessageType messageType = MessageType.UNDEFINED;
			public StringBuilder builder = null;

			public bool IsValid 
			{
				get
				{
					return length >= 0 && messageType != MessageType.UNDEFINED;
				}
			}
		}

		private void OnLineReceived(ClientConnection sender, string message) 
		{
			// NOTE(donghyun-you) : message processing like HTTP header and body. receive headers first, and after empty line, the body will be come.
			// 						header and message type must be defined. or the server drop the message.
			//
			// Expecting message example. (third line is expected empty)
			//
			// MessageType: ECHO
			// Length: 13
			//
			// {"foo":"boo"}

			TestLogger.Verbose(this,"receiving message("+message.Length+"): "+message);

			ProcessingMessage processor;
			if(messages.TryGetValue(sender,out processor) == false) 
			{
				processor = messages[sender] = new ProcessingMessage();
			}

			if(processor.builder != null) 
			{
				processor.builder.Append(message);

				// NOTE(donghyun-you): if message received complete. dispatch the message
				if(processor.builder.Length >= processor.length) 
				{
					OnMessageReceived(sender,processor.messageType,processor.builder.ToString());
					messages.Remove(sender);
				}
			}
			else if(message.StartsWith("Length:")) 
			{
				var content = message.Substring(message.IndexOf(":")+1).Trim();

				TestLogger.Verbose(this,"Length("+content.Length+"): \""+content+"\"");

				long parsedLength;
				if(long.TryParse(content,out parsedLength)) 
				{
					processor.length = parsedLength;
				}
				else 
				{
					throw new System.FormatException("Unable to parse as long on length content");
				}
			}
			else if(message.StartsWith("MessageType:")) 
			{
				var content = message.Substring(message.IndexOf(":")+1).Trim();

				TestLogger.Verbose(this,"MessageType("+content.Length+"): \""+content+"\"");

				processor.messageType = (MessageType)Enum.Parse(typeof(MessageType),content);
			}
			else if(message.Trim().Length == 0) 
			{
				if(processor.IsValid == false) 
				{
					// NOTE(donghyun-you): invalid message. drop it
					messages.Remove(sender);
					TestLogger.Warning(this,"invalid message has income. droppping.");
				}
				else
				{
					// NOTE(donghyun-you): its meaning start the body on next line
					processor.builder = new StringBuilder();
				}
			}
		}

		public delegate void MessageHandleEvent(ClientConnection sender,MessageType type,string body);

		private Dictionary<MessageType,MessageHandleEvent> messageHandlers = new Dictionary<MessageType,MessageHandleEvent>();

		private void OnMessageReceived(ClientConnection sender, MessageType message_type, string message) 
		{
			TestMainThreadDispatcher.Instance.Run(()=>{

				TestLogger.Verbose(this,"message receiving: "+message_type+": "+message);

				MessageHandleEvent registeredHandler;
				if(messageHandlers.TryGetValue(message_type,out registeredHandler)) 
				{
					registeredHandler.Invoke(sender,message_type,message);
				}

			},Debug.LogException);
		}

		public IDisposable RegisterHandler(MessageType message_type,MessageHandleEvent handler) 
		{
			TestLogger.Verbose(this,"registerating handler: "+message_type);

			MessageHandleEvent registeredHandler;
			if(messageHandlers.TryGetValue(message_type,out registeredHandler) == false) 
			{
				messageHandlers[message_type] = delegate(ClientConnection sender,MessageType type,string obj) {};
			}

			messageHandlers[message_type] += handler;

			return DisposableCreator.Create(()=>
			{
				TestLogger.Verbose(this,"unregisterating handler: "+message_type);
				messageHandlers[message_type] -= handler;
			});
		}
	}
}