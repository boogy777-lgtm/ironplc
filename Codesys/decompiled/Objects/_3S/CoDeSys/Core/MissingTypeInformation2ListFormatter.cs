using System;
using CODESYS.ComponentModel.Serialization;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200002A RID: 42
	[ReleasedClass]
	public class MissingTypeInformation2ListFormatter : SecureBinaryFormatterBase
	{
		// Token: 0x060000B7 RID: 183 RVA: 0x000028FE File Offset: 0x00000AFE
		public MissingTypeInformation2ListFormatter() : base(MissingTypeInformation2ListFormatter.WellKnownTypes)
		{
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x0000290B File Offset: 0x00000B0B
		protected override object Coalesce(DeserializedObject deo)
		{
			return base.CoalesceList<MissingTypeInformation2>(deo, new SecureBinaryFormatterBase.CoalesceFunc<MissingTypeInformation2>(this.CoalesceMissingTypeInformation2));
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00002920 File Offset: 0x00000B20
		private MissingTypeInformation2 CoalesceMissingTypeInformation2(DeserializedObject deo)
		{
			DeserializedObject deo2 = deo.Object["MissingTypeGuid"];
			Guid missingTypeGuid = (Guid)base.CoalesceGuid(deo2);
			string stPlugInName = (string)deo.Object["PlugInName"].Value;
			DeserializedObject deo3 = deo.Object["PlugInVersion"];
			Version plugInVersion = (!deo3.IsNull) ? base.CoalesceVersion(deo3) : null;
			DeserializedObject deo4 = deo.Object["OwningPackageId"];
			Guid owningPackageId = (Guid)base.CoalesceGuid(deo4);
			string stOwningPackageName = (string)deo.Object["OwningPackageName"].Value;
			DeserializedObject deo5 = deo.Object["OwningPackageVersion"];
			Version owningPackageVersion = (!deo5.IsNull) ? base.CoalesceVersion(deo5) : null;
			return new MissingTypeInformation2(missingTypeGuid, stPlugInName, plugInVersion, owningPackageId, stOwningPackageName, owningPackageVersion);
		}

		// Token: 0x04000022 RID: 34
		private static readonly WellKnownType[] WellKnownTypes = new WellKnownType[]
		{
			new WellKnownType("System.Collections.Generic.List`1[[_3S.CoDeSys.Core.Objects.MissingTypeInformation2*]]", false, new WellKnownMember[]
			{
				new WellKnownMember("_items", "_3S.CoDeSys.Core.Objects.MissingTypeInformation2[]"),
				new WellKnownMember("_size", "System.Int32"),
				new WellKnownMember("_version", "System.Int32")
			}),
			new WellKnownType("_3S.CoDeSys.Core.Objects.MissingTypeInformation2", false, new WellKnownMember[]
			{
				new WellKnownMember("MissingTypeGuid", "System.Guid"),
				new WellKnownMember("PlugInName", "System.String"),
				new WellKnownMember("PlugInVersion", "System.Version"),
				new WellKnownMember("OwningPackageId", "System.Guid"),
				new WellKnownMember("OwningPackageName", "System.String"),
				new WellKnownMember("OwningPackageVersion", "System.Version")
			}),
			SecureBinaryFormatterBase.WktGuid,
			SecureBinaryFormatterBase.WktVersion
		};
	}
}
