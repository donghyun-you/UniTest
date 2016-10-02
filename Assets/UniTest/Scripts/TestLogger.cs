using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;

namespace UniTest 
{
	public class TestLogger
	{
		public enum LogType 
		{
			kVerbose,
			kInfo,
			kWarning,
			kError,
		}

		public delegate void LogEvent(LogType type,object invoker,string text);
		public delegate void LogExceptionEvent(object invoker,Exception ex);

		public static event LogEvent OnLogged = delegate(LogType type, object invoker, string text) {};
		public static event LogExceptionEvent OnExceptionLogged = delegate(object invoker, System.Exception ex) {};

		private static string GetTimeSummary() 
		{
			return DateTime.Now.ToString("hh:mm:ss");
		}

		[Conditional("VERBOSE_LOG")]
		public static void Verbose(object invoker,string text) 
		{
			UnityEngine.Debug.Log("[<color=silver>Verbose</color>/<color=gray>"+invoker.GetType().Name+"</color>/"+GetTimeSummary()+"] "+text);
			OnLogged.Invoke(LogType.kVerbose,invoker,text);
		}

		public static void Info(object invoker,string text)
		{
			UnityEngine.Debug.Log("[<color=green>Info</color>/<color=gray>"+invoker.GetType().Name+"</color>/"+GetTimeSummary()+"] "+text);
			OnLogged.Invoke(LogType.kInfo,invoker,text);
		}

		public static void Warning(object invoker,string text)
		{
			UnityEngine.Debug.LogWarning("[Warning/"+invoker.GetType().Name+"/"+GetTimeSummary()+"] "+text);
			OnLogged.Invoke(LogType.kWarning,invoker,text);
		}

		public static void Error(object invoker,string text)
		{
			UnityEngine.Debug.LogError("[Error/"+invoker.GetType().Name+"/"+GetTimeSummary()+"] "+text);
			OnLogged.Invoke(LogType.kError,invoker,text);
		}

		public static void Exception(object invoker,Exception ex) 
		{
			UnityEngine.Debug.LogException(ex);
			OnExceptionLogged.Invoke(invoker,ex);
		}
	}
}
