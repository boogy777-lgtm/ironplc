using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0006
{
	// Token: 0x020001B5 RID: 437
	internal sealed class \u0005 : IOperatorExpressionVisitor
	{
		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x06001FF7 RID: 8183 RVA: 0x0006CD68 File Offset: 0x0006AF68
		// (set) Token: 0x06001FF8 RID: 8184 RVA: 0x0006CD70 File Offset: 0x0006AF70
		internal _IType PreferredType { get; set; }

		// Token: 0x06001FF9 RID: 8185 RVA: 0x0006CD7C File Offset: 0x0006AF7C
		public void \u0001(_IOperatorExpression \u0002)
		{
			this.PreferredType = TypeTable.AnyInt;
		}

		// Token: 0x06001FFA RID: 8186 RVA: 0x0006CD8C File Offset: 0x0006AF8C
		public void \u0002(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FFB RID: 8187 RVA: 0x0006CD90 File Offset: 0x0006AF90
		public void \u0003(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FFC RID: 8188 RVA: 0x0006CD94 File Offset: 0x0006AF94
		public void \u0004(_IOperatorExpression \u0002)
		{
			this.PreferredType = TypeTable.AnyInt;
		}

		// Token: 0x06001FFD RID: 8189 RVA: 0x0006CDA4 File Offset: 0x0006AFA4
		public void \u0005(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FFE RID: 8190 RVA: 0x0006CDA8 File Offset: 0x0006AFA8
		public void \u0006(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06001FFF RID: 8191 RVA: 0x0006CDAC File Offset: 0x0006AFAC
		public void \u0007(_IOperatorExpression \u0002)
		{
			this.PreferredType = TypeTable.Bool;
		}

		// Token: 0x06002000 RID: 8192 RVA: 0x0006CDBC File Offset: 0x0006AFBC
		public void \u0008(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002001 RID: 8193 RVA: 0x0006CDC0 File Offset: 0x0006AFC0
		public void \u000E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002002 RID: 8194 RVA: 0x0006CDC4 File Offset: 0x0006AFC4
		public void \u000F(_IOperatorExpression \u0002)
		{
			this.PreferredType = TypeTable.Bool;
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x0006CDD4 File Offset: 0x0006AFD4
		public void \u0010(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002004 RID: 8196 RVA: 0x0006CDD8 File Offset: 0x0006AFD8
		public void \u0011(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002005 RID: 8197 RVA: 0x0006CDDC File Offset: 0x0006AFDC
		public void \u0012(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002006 RID: 8198 RVA: 0x0006CDE0 File Offset: 0x0006AFE0
		public void \u0013(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002007 RID: 8199 RVA: 0x0006CDE4 File Offset: 0x0006AFE4
		public void \u0014(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002008 RID: 8200 RVA: 0x0006CDE8 File Offset: 0x0006AFE8
		public void \u0015(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002009 RID: 8201 RVA: 0x0006CDEC File Offset: 0x0006AFEC
		public void \u0016(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200A RID: 8202 RVA: 0x0006CDF0 File Offset: 0x0006AFF0
		public void \u0017(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200B RID: 8203 RVA: 0x0006CDF4 File Offset: 0x0006AFF4
		public void \u0018(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200C RID: 8204 RVA: 0x0006CDF8 File Offset: 0x0006AFF8
		public void \u0019(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200D RID: 8205 RVA: 0x0006CDFC File Offset: 0x0006AFFC
		public void \u001A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200E RID: 8206 RVA: 0x0006CE00 File Offset: 0x0006B000
		public void \u001B(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600200F RID: 8207 RVA: 0x0006CE04 File Offset: 0x0006B004
		public void \u001C(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002010 RID: 8208 RVA: 0x0006CE08 File Offset: 0x0006B008
		public void \u001D(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002011 RID: 8209 RVA: 0x0006CE0C File Offset: 0x0006B00C
		public void \u001E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002012 RID: 8210 RVA: 0x0006CE10 File Offset: 0x0006B010
		public void \u001F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002013 RID: 8211 RVA: 0x0006CE14 File Offset: 0x0006B014
		public void \u007F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002014 RID: 8212 RVA: 0x0006CE18 File Offset: 0x0006B018
		public void \u0080(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002015 RID: 8213 RVA: 0x0006CE1C File Offset: 0x0006B01C
		public void \u0081(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002016 RID: 8214 RVA: 0x0006CE20 File Offset: 0x0006B020
		public void \u0082(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002017 RID: 8215 RVA: 0x0006CE24 File Offset: 0x0006B024
		public void \u0083(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002018 RID: 8216 RVA: 0x0006CE28 File Offset: 0x0006B028
		public void \u0084(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002019 RID: 8217 RVA: 0x0006CE2C File Offset: 0x0006B02C
		public void \u0086(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201A RID: 8218 RVA: 0x0006CE30 File Offset: 0x0006B030
		public void \u0087(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201B RID: 8219 RVA: 0x0006CE34 File Offset: 0x0006B034
		public void \u0088(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x0006CE38 File Offset: 0x0006B038
		public void \u0089(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x0006CE3C File Offset: 0x0006B03C
		public void \u008A(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201E RID: 8222 RVA: 0x0006CE40 File Offset: 0x0006B040
		public void \u008B(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0600201F RID: 8223 RVA: 0x0006CE44 File Offset: 0x0006B044
		public void \u008C(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002020 RID: 8224 RVA: 0x0006CE48 File Offset: 0x0006B048
		public void \u008D(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002021 RID: 8225 RVA: 0x0006CE4C File Offset: 0x0006B04C
		public void \u008E(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06002022 RID: 8226 RVA: 0x0006CE50 File Offset: 0x0006B050
		public void \u008F(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x0400053D RID: 1341
		[CompilerGenerated]
		private _IType \u0001;
	}
}
