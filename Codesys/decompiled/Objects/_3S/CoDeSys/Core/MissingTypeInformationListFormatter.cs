using System;
using CODESYS.ComponentModel.Serialization;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000029 RID: 41
	[ReleasedClass]
	public class MissingTypeInformationListFormatter : SecureBinaryFormatterBase
	{
		// Token: 0x060000B3 RID: 179 RVA: 0x0000276C File Offset: 0x0000096C
		public MissingTypeInformationListFormatter() : base(MissingTypeInformationListFormatter.WellKnownTypes)
		{
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00002779 File Offset: 0x00000979
		protected override object Coalesce(DeserializedObject deo)
		{
			return base.CoalesceList<MissingTypeInformation>(deo, new SecureBinaryFormatterBase.CoalesceFunc<MissingTypeInformation>(this.CoalesceMissingTypeInformation2));
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00002790 File Offset: 0x00000990
		private MissingTypeInformation CoalesceMissingTypeInformation2(DeserializedObject deo)
		{
			DeserializedObject deo2 = deo.Object["MissingTypeGuid"];
			Guid missingTypeGuid = (Guid)base.CoalesceGuid(deo2);
			DeserializedObject deo3 = deo.Object["OwningPackageId"];
			Guid owningPackageId = (Guid)base.CoalesceGuid(deo3);
			string stOwningPackageName = (string)deo.Object["OwningPackageName"].Value;
			DeserializedObject deo4 = deo.Object["OwningPackageVersion"];
			Version owningPackageVersion = (!deo4.IsNull) ? base.CoalesceVersion(deo4) : null;
			return new MissingTypeInformation(missingTypeGuid, owningPackageId, stOwningPackageName, owningPackageVersion);
		}

		// Token: 0x04000021 RID: 33
		private static readonly WellKnownType[] WellKnownTypes = new WellKnownType[]
		{
			new WellKnownType("System.Collections.Generic.List`1[[_3S.CoDeSys.Core.Objects.MissingTypeInformation*]]", false, new WellKnownMember[]
			{
				new WellKnownMember("_items", "_3S.CoDeSys.Core.Objects.MissingTypeInformation[]"),
				new WellKnownMember("_size", "System.Int32"),
				new WellKnownMember("_version", "System.Int32")
			}),
			new WellKnownType("_3S.CoDeSys.Core.Objects.MissingTypeInformation", false, new WellKnownMember[]
			{
				new WellKnownMember("MissingTypeGuid", "System.Guid"),
				new WellKnownMember("OwningPackageId", "System.Guid"),
				new WellKnownMember("OwningPackageName", "System.String"),
				new WellKnownMember("OwningPackageVersion", "System.Version")
			}),
			SecureBinaryFormatterBase.WktGuid,
			SecureBinaryFormatterBase.WktVersion
		};
	}
}
