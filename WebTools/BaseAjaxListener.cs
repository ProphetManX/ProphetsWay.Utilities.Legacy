using System.Collections.Generic;

namespace ProphetsWay.Utilities.AjaxTools
{
	public abstract class BaseAjaxListener
	{
		protected BaseAjaxListener(Dictionary<string, string> arguments)
		{
			_args = new AjaxArgumentsHandler(arguments);
		}

		protected AjaxArgumentsHandler _args;


	}

	public class AjaxArgumentsHandler
	{
		private readonly Dictionary<string, string> arguments;

		public AjaxArgumentsHandler(Dictionary<string, string> args)
		{
			arguments = args;
		}

		/// <summary>
		/// Returns the original 'arguments' that were passed in upon construction.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> GetOriginalArguments()
		{
			return arguments;
		}

		/// <summary>
		/// Will return a Typed value from the 'arguments' input dictionary by a known given key.
		/// </summary>
		/// <typeparam name="T">The Type of the requested return type.</typeparam>
		/// <param name="key">The key of the parameter requested.</param>
		public T GetValueByKey<T>(string key)
		{
			return arguments.GetValueFromKey<T>(key);
		}

		//a nice way to list alot of the commonly used 
		public string UserId { get { return arguments.GetValueFromKey<string>(AjaxConstants.UserId); } }
	
	
	}
}
