using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.ProjectCompare;

namespace _3S.CoDeSys.LanguageModelManagerConfigurators
{
	[TypeGuid("{051F2750-DB46-4A8E-99F5-20ADF57EBD70}")]
	public class BuildPropertyComparer : IObjectPropertyComparer
	{
		public bool AcceptsObjectProperty(Guid propertyTypeGuid)
		{
			return propertyTypeGuid == BuildPropertiesControl.BuildPropertyGuid;
		}

		public bool CheckEquality(IObjectProperty leftProperty, IObjectProperty rightProperty, bool ignoreWhitespace, bool ignoreComments, bool ignoreProperties)
		{
			IBuildProperty5 buildProperty = leftProperty as IBuildProperty5;
			IBuildProperty5 buildProperty2 = rightProperty as IBuildProperty5;
			IBuildProperty5 buildProperty3 = APEnvironment.CreateBuildProperty();
			bool num = buildProperty?.ExcludeFromBuild ?? buildProperty3.ExcludeFromBuild;
			bool flag = buildProperty2?.ExcludeFromBuild ?? buildProperty3.ExcludeFromBuild;
			if (num != flag)
			{
				return false;
			}
			bool num2 = buildProperty?.External ?? buildProperty3.External;
			bool flag2 = buildProperty2?.External ?? buildProperty3.External;
			if (num2 != flag2)
			{
				return false;
			}
			bool num3 = buildProperty?.EnableSystemCall ?? buildProperty3.EnableSystemCall;
			bool flag3 = buildProperty2?.EnableSystemCall ?? buildProperty3.EnableSystemCall;
			if (num3 != flag3)
			{
				return false;
			}
			string obj = ((buildProperty != null) ? buildProperty.CompilerDefines : buildProperty3.CompilerDefines);
			string text = ((buildProperty2 != null) ? buildProperty2.CompilerDefines : buildProperty3.CompilerDefines);
			if (obj != text)
			{
				return false;
			}
			bool num4 = buildProperty?.LinkAlways ?? buildProperty3.LinkAlways;
			bool flag4 = buildProperty2?.LinkAlways ?? buildProperty3.LinkAlways;
			if (num4 != flag4)
			{
				return false;
			}
			IList<string> coll = ((buildProperty != null) ? buildProperty.Undefines : buildProperty3.Undefines);
			IList<string> coll2 = ((buildProperty2 != null) ? buildProperty2.Undefines : buildProperty3.Undefines);
			if (!BuildPropertyHelper.OrderInsensitiveSequenceEqual(coll, coll2))
			{
				return false;
			}
			int num5 = buildProperty?.MemoryReserveForOnlineChange ?? buildProperty3.MemoryReserveForOnlineChange;
			int num6 = buildProperty2?.MemoryReserveForOnlineChange ?? buildProperty3.MemoryReserveForOnlineChange;
			if (num5 != num6)
			{
				return false;
			}
			return true;
		}
	}
}
