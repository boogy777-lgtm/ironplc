using System.Xml;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	[ReleasedInterface]
	public interface IGenericInterfaceExtensionProvider
	{
		void RaiseEvent(string stEvent, XmlDocument eventData);

		void AttachToEvent(string stEvent, GenericEventDelegate callback);

		void DetachFromEvent(string stEvent, GenericEventDelegate callback);

		bool IsFunctionAvailable(string stFunction);

		XmlDocument CallFunction(string stFunction, XmlDocument functionData);
	}
}
