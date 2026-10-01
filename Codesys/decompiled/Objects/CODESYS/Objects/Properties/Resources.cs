using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CODESYS.Objects.Properties
{
	// Token: 0x02000002 RID: 2
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal Resources()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("CODESYS.Objects.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x00002084 File Offset: 0x00000284
		// (set) Token: 0x06000004 RID: 4 RVA: 0x0000208B File Offset: 0x0000028B
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002093 File Offset: 0x00000293
		internal static string AddObjectConstraintException
		{
			get
			{
				return Resources.ResourceManager.GetString("AddObjectConstraintException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000006 RID: 6 RVA: 0x000020A9 File Offset: 0x000002A9
		internal static string ChildObjectIsAncestorException
		{
			get
			{
				return Resources.ResourceManager.GetString("ChildObjectIsAncestorException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000020BF File Offset: 0x000002BF
		internal static string DeleteObjectConstraintException
		{
			get
			{
				return Resources.ResourceManager.GetString("DeleteObjectConstraintException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000020D5 File Offset: 0x000002D5
		internal static string DongleRequiredException
		{
			get
			{
				return Resources.ResourceManager.GetString("DongleRequiredException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000009 RID: 9 RVA: 0x000020EB File Offset: 0x000002EB
		internal static string InvalidObjectGuidException
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidObjectGuidException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000A RID: 10 RVA: 0x00002101 File Offset: 0x00000301
		internal static string InvalidProjectHandleException
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidProjectHandleException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600000B RID: 11 RVA: 0x00002117 File Offset: 0x00000317
		internal static string MissingTypeGuidException
		{
			get
			{
				return Resources.ResourceManager.GetString("MissingTypeGuidException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600000C RID: 12 RVA: 0x0000212D File Offset: 0x0000032D
		internal static string MoveObjectConstraintException
		{
			get
			{
				return Resources.ResourceManager.GetString("MoveObjectConstraintException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002143 File Offset: 0x00000343
		internal static string ObjectAlreadyHasParentException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectAlreadyHasParentException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600000E RID: 14 RVA: 0x00002159 File Offset: 0x00000359
		internal static string ObjectAlreadyInUseException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectAlreadyInUseException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600000F RID: 15 RVA: 0x0000216F File Offset: 0x0000036F
		internal static string ObjectLoadException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectLoadException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002185 File Offset: 0x00000385
		internal static string ObjectManagerAlreadyExistsException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectManagerAlreadyExistsException", Resources.resourceCulture);
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000011 RID: 17 RVA: 0x0000219B File Offset: 0x0000039B
		internal static string ObjectNameInvalidException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectNameInvalidException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000021B1 File Offset: 0x000003B1
		internal static string ObjectNameNotUniqueException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectNameNotUniqueException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000021C7 File Offset: 0x000003C7
		internal static string ObjectNotSerializableException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectNotSerializableException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000021DD File Offset: 0x000003DD
		internal static string ObjectReadOnlyException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectReadOnlyException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000015 RID: 21 RVA: 0x000021F3 File Offset: 0x000003F3
		internal static string ObjectSaveException
		{
			get
			{
				return Resources.ResourceManager.GetString("ObjectSaveException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000016 RID: 22 RVA: 0x00002209 File Offset: 0x00000409
		internal static string ProjectIntegrityException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProjectIntegrityException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000017 RID: 23 RVA: 0x0000221F File Offset: 0x0000041F
		internal static string ProjectLoadException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProjectLoadException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00002235 File Offset: 0x00000435
		internal static string ProjectSaveException
		{
			get
			{
				return Resources.ResourceManager.GetString("ProjectSaveException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000019 RID: 25 RVA: 0x0000224B File Offset: 0x0000044B
		internal static string WrongEncryptionPasswordException
		{
			get
			{
				return Resources.ResourceManager.GetString("WrongEncryptionPasswordException", Resources.resourceCulture);
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002261 File Offset: 0x00000461
		internal static string WrongPasswordException
		{
			get
			{
				return Resources.ResourceManager.GetString("WrongPasswordException", Resources.resourceCulture);
			}
		}

		// Token: 0x04000001 RID: 1
		private static ResourceManager resourceMan;

		// Token: 0x04000002 RID: 2
		private static CultureInfo resourceCulture;
	}
}
