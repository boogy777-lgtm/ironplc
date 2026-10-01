using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.OnlineExpressionInterpreter;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E9 RID: 233
	internal class OnlineExpressionProxy : IOnlineVarRef5, IOnlineVarRef4, IOnlineVarRef3, IOnlineVarRef2, IOnlineVarRef
	{
		// Token: 0x06001160 RID: 4448 RVA: 0x000323D0 File Offset: 0x000313D0
		internal OnlineExpressionProxy(IOnlineExpression oexp)
		{
			this._oexp = oexp;
			this._oexp.Changed += this._oexp_Changed;
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x00032401 File Offset: 0x00031401
		private void _oexp_Changed(IOnlineExpression onlineExpression)
		{
			if (this.Changed != null)
			{
				this.Changed(this);
			}
		}

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x06001162 RID: 4450 RVA: 0x00032417 File Offset: 0x00031417
		public IExpression Expression
		{
			get
			{
				return this._oexp.Expression;
			}
		}

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x06001163 RID: 4451 RVA: 0x00032424 File Offset: 0x00031424
		public DateTime Timestamp
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x06001164 RID: 4452 RVA: 0x0003242B File Offset: 0x0003142B
		public VarRefState State
		{
			get
			{
				return this._oexp.State;
			}
		}

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x06001165 RID: 4453 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool Forced
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x06001166 RID: 4454 RVA: 0x00032438 File Offset: 0x00031438
		public object Value
		{
			get
			{
				return this._oexp.Value;
			}
		}

		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x06001167 RID: 4455 RVA: 0x00005F0F File Offset: 0x00004F0F
		public byte[] RawValue
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x06001168 RID: 4456 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x06001169 RID: 4457 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public object PreparedValue
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x0600116A RID: 4458 RVA: 0x00005F0F File Offset: 0x00004F0F
		public byte[] PreparedRawValue
		{
			get
			{
				return null;
			}
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x00032445 File Offset: 0x00031445
		public void SuspendMonitoring()
		{
			this._oexp.SuspendMonitoring();
		}

		// Token: 0x0600116C RID: 4460 RVA: 0x00032452 File Offset: 0x00031452
		public void ResumeMonitoring()
		{
			this._oexp.ResumeMonitoring();
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x0003245F File Offset: 0x0003145F
		public string GetStateMessage()
		{
			return "";
		}

		// Token: 0x0600116E RID: 4462 RVA: 0x00032466 File Offset: 0x00031466
		public void Release()
		{
			this._oexp.Release();
		}

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x0600116F RID: 4463 RVA: 0x00032474 File Offset: 0x00031474
		// (remove) Token: 0x06001170 RID: 4464 RVA: 0x000324AC File Offset: 0x000314AC
		public event OnlineVarRefEventHandler Changed;

		// Token: 0x06001171 RID: 4465 RVA: 0x00032401 File Offset: 0x00031401
		public void SetPreparedRawValue(byte[] byRaw)
		{
			if (this.Changed != null)
			{
				this.Changed(this);
			}
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x06001172 RID: 4466 RVA: 0x000324E1 File Offset: 0x000314E1
		public bool Reached
		{
			get
			{
				return this._oexp is IOnlineExpression2 && (this._oexp as IOnlineExpression2).Reached;
			}
		}

		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x06001173 RID: 4467 RVA: 0x00032502 File Offset: 0x00031502
		public bool FlowControl
		{
			get
			{
				return this._oexp is IOnlineExpression2 && (this._oexp as IOnlineExpression2).FlowControl;
			}
		}

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x06001174 RID: 4468 RVA: 0x00032523 File Offset: 0x00031523
		// (set) Token: 0x06001175 RID: 4469 RVA: 0x0003252B File Offset: 0x0003152B
		public string WatchListExpression
		{
			get
			{
				return this._stWatchExpression;
			}
			set
			{
				this._stWatchExpression = value;
			}
		}

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x06001176 RID: 4470 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool Writeable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x06001177 RID: 4471 RVA: 0x00032534 File Offset: 0x00031534
		// (remove) Token: 0x06001178 RID: 4472 RVA: 0x0003256C File Offset: 0x0003156C
		public event OnChangingPreparedValuesEventHandler OnChangingPreparedValues;

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x06001179 RID: 4473 RVA: 0x000325A1 File Offset: 0x000315A1
		// (set) Token: 0x0600117A RID: 4474 RVA: 0x000325A9 File Offset: 0x000315A9
		public object Tag
		{
			get
			{
				return this._tag;
			}
			set
			{
				this._tag = value;
			}
		}

		// Token: 0x0400040D RID: 1037
		private readonly IOnlineExpression _oexp;

		// Token: 0x0400040E RID: 1038
		private object _tag;

		// Token: 0x0400040F RID: 1039
		private string _stWatchExpression = string.Empty;
	}
}
