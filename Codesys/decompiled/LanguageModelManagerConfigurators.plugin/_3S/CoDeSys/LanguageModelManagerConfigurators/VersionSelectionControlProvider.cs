using System;
using System.Windows.Forms;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.VersionCompatibilityManager;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{EFE8711E-64CA-49f5-A5C6-E1D63279FB43}")]
	public class VersionSelectionControlProvider : IVersionSelectionControlProvider
	{
		internal static readonly Guid GUID = new Guid("{EFE8711E-64CA-49f5-A5C6-E1D63279FB43}");

		public string ProviderName => Strings.VersionSelection_ProviderName;

		public UserControl GetControl()
		{
			return new VersionSelectionControl();
		}
	}
}
