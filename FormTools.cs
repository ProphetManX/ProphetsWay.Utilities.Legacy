using System.Linq;
using System.Windows.Forms;

namespace ProphetsWay.Utilities
{
	public static class FormTools
	{
		private delegate void SetControlValueCallback(Control oControl, string propName, object propValue);
		public static void SetControlPropertyValue(this Control oControl, string propName, object propValue)
		{
			if (oControl.InvokeRequired)
			{
				var d = new SetControlValueCallback(SetControlPropertyValue);
				oControl.Invoke(d, new[] { oControl, propName, propValue });
			}
			else
			{
				var t = oControl.GetType();
				var props = t.GetProperties();

				foreach (var p in props.Where(p => p.Name.ToUpper() == propName.ToUpper()))
					p.SetValue(oControl, propValue, null);

			}
		}
	}
}
