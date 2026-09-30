using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200003C RID: 60
	internal sealed class LegacyGenericObjectService : IGenericObjectService
	{
		// Token: 0x060000F0 RID: 240 RVA: 0x00002EED File Offset: 0x000010ED
		internal LegacyGenericObjectService(Type type)
		{
			this._typeAccess = LegacyTypeAccess.Create(type);
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00002F01 File Offset: 0x00001101
		public object Clone(GenericObject go)
		{
			return LegacyGenericObjectService.CloneImpl(go, this._typeAccess);
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00002F10 File Offset: 0x00001110
		private static object CloneImpl(GenericObject go, LegacyTypeAccess typeAccess)
		{
			IGenericObject genericObject = (IGenericObject)ComponentManager.Singleton.InstanceFactory.CreateInstance(go.GetType());
			foreach (LegacyCloneableMemberAccess legacyCloneableMemberAccess in typeAccess.CloneableMemberAccesses)
			{
				object obj = legacyCloneableMemberAccess.Getter(go);
				object value = null;
				DuplicationMethod duplication = legacyCloneableMemberAccess.Duplication;
				if (duplication != DuplicationMethod.Shallow)
				{
					if (duplication == DuplicationMethod.Deep)
					{
						value = LegacyGenericObjectService.CloneDeeply(obj);
					}
				}
				else
				{
					value = obj;
				}
				legacyCloneableMemberAccess.Setter(genericObject, value);
			}
			((GenericObject)genericObject).AfterClone();
			return genericObject;
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00002FC0 File Offset: 0x000011C0
		public string[] GetSerializableValueNames(GenericObject go)
		{
			return LegacyGenericObjectService.GetSerializableValueNamesImpl(go, this._typeAccess);
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00002FD0 File Offset: 0x000011D0
		private static string[] GetSerializableValueNamesImpl(GenericObject go, LegacyTypeAccess typeAccess)
		{
			string[] array = new string[typeAccess.SerializableMemberAccesses.Count];
			int num = 0;
			foreach (string text in typeAccess.SerializableMemberAccesses.Keys)
			{
				array[num++] = text;
			}
			return array;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00003040 File Offset: 0x00001240
		public object GetSerializableValue(GenericObject go, string valueName)
		{
			return LegacyGenericObjectService.GetSerializableValueImpl(go, this._typeAccess, valueName);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000304F File Offset: 0x0000124F
		private static object GetSerializableValueImpl(GenericObject go, LegacyTypeAccess typeAccess, string valueName)
		{
			return typeAccess.SerializableMemberAccesses[valueName].Getter(go);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00003068 File Offset: 0x00001268
		public void SetSerializableValue(GenericObject go, string valueName, object value)
		{
			LegacyGenericObjectService.SetSerializableValueImpl(go, this._typeAccess, valueName, value);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00003078 File Offset: 0x00001278
		private static void SetSerializableValueImpl(GenericObject go, LegacyTypeAccess typeAccess, string valueName, object value)
		{
			LegacySerializableMemberAccess legacySerializableMemberAccess;
			typeAccess.SerializableMemberAccesses.TryGetValue(valueName, out legacySerializableMemberAccess);
			if (legacySerializableMemberAccess != null)
			{
				if (value == null && legacySerializableMemberAccess.Type.IsValueType)
				{
					value = Activator.CreateInstance(legacySerializableMemberAccess.Type);
				}
				if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, legacySerializableMemberAccess.Type, ref value, null, null))
				{
					return;
				}
				legacySerializableMemberAccess.Setter(go, value);
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x000030D6 File Offset: 0x000012D6
		public string[] GetSerializableValueNames(GenericObject2 go, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return LegacyGenericObjectService.GetSerializableValueNamesImpl(go, this._typeAccess, info, reporter);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x000030E8 File Offset: 0x000012E8
		private static string[] GetSerializableValueNamesImpl(GenericObject2 go, LegacyTypeAccess typeAccess, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			List<string> list = new List<string>();
			Version targetVersion = info.GetTargetVersion(go);
			if (targetVersion != null && typeAccess.SerializableMemberAccesses != null)
			{
				foreach (KeyValuePair<string, LegacySerializableMemberAccess> keyValuePair in typeAccess.SerializableMemberAccesses)
				{
					if (keyValuePair.Value.StorageVersion != null && keyValuePair.Value.StorageVersion.IsVersionInVersionRangeList(targetVersion))
					{
						list.Add(keyValuePair.Key);
					}
					else if (reporter is IArchiveReporter2)
					{
						bool flag = true;
						if (keyValuePair.Value.Ignorable)
						{
							flag = false;
						}
						if (keyValuePair.Value.DefaultValue != null && object.Equals(go.GetSerializableValue(keyValuePair.Key), keyValuePair.Value.DefaultValue.DefaultValue))
						{
							flag = false;
						}
						if (flag)
						{
							((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), keyValuePair.Key);
						}
					}
				}
			}
			return list.ToArray();
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00003204 File Offset: 0x00001404
		public object GetSerializableValue(GenericObject2 go, string valueName, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return LegacyGenericObjectService.GetSerializableValueImpl(go, valueName, info, reporter);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00003210 File Offset: 0x00001410
		private static object GetSerializableValueImpl(GenericObject2 go, string valueName, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return go.GetSerializableValue(valueName);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00003219 File Offset: 0x00001419
		public void SetSerializableValue(GenericObject2 go, string valueName, object value, IArchiveReporter reporter)
		{
			LegacyGenericObjectService.SetSerializableValueImpl(go, this._typeAccess, valueName, value, reporter);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000322C File Offset: 0x0000142C
		private static void SetSerializableValueImpl(GenericObject2 go, LegacyTypeAccess typeAccess, string valueName, object value, IArchiveReporter reporter)
		{
			if (typeAccess.SetSerializableValue2IsOverridden && !typeAccess.SetSerializableValue3IsOverridden)
			{
				go.SetSerializableValue(valueName, value);
				return;
			}
			LegacySerializableMemberAccess legacySerializableMemberAccess;
			typeAccess.SerializableMemberAccesses.TryGetValue(valueName, out legacySerializableMemberAccess);
			if (legacySerializableMemberAccess == null)
			{
				if (reporter is IArchiveReporter2)
				{
					((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), valueName);
				}
				return;
			}
			if (value == null && legacySerializableMemberAccess.Type.IsValueType)
			{
				value = Activator.CreateInstance(legacySerializableMemberAccess.Type);
			}
			if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, legacySerializableMemberAccess.Type, ref value, null, reporter))
			{
				if (reporter is IArchiveReporter2)
				{
					((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), valueName);
				}
				return;
			}
			legacySerializableMemberAccess.Setter(go, value);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x000032DD File Offset: 0x000014DD
		public void BeforeSerialize(GenericObject2 go, IArchiveVersionInfo info)
		{
			LegacyGenericObjectService.BeforeSerializeImpl(go, info);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x000032E6 File Offset: 0x000014E6
		private static void BeforeSerializeImpl(GenericObject2 go, IArchiveVersionInfo info)
		{
			go.BeforeSerialize();
		}

		// Token: 0x06000101 RID: 257 RVA: 0x000032EE File Offset: 0x000014EE
		public object CreateSerializableValue(GenericObject2 go, string valueName, byte[] nesting, IArchiveReporter reporter)
		{
			return LegacyGenericObjectService.CreateSerializableValueImpl(go, this._typeAccess, valueName, nesting, reporter);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00003300 File Offset: 0x00001500
		private static object CreateSerializableValueImpl(GenericObject go, LegacyTypeAccess typeAccess, string valueName, byte[] nesting, IArchiveReporter reporter)
		{
			LegacySerializableMemberAccess legacySerializableMemberAccess;
			typeAccess.SerializableMemberAccesses.TryGetValue(valueName, out legacySerializableMemberAccess);
			if (legacySerializableMemberAccess != null)
			{
				Type type = legacySerializableMemberAccess.Type;
				if (nesting != null)
				{
					foreach (byte b in nesting)
					{
						type = type.GetGenericArguments()[(int)b];
					}
				}
				return Activator.CreateInstance(type);
			}
			if (reporter is IArchiveReporter2)
			{
				((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), valueName);
			}
			return null;
		}

		// Token: 0x06000103 RID: 259 RVA: 0x0000336C File Offset: 0x0000156C
		public GenericCollectionSerializationMode? GetGenericSerializationMode(GenericObject2 go, string valueName, IArchiveVersionInfo info, bool forceLegacy, IArchiveReporter reporter)
		{
			return LegacyGenericObjectService.GetGenericSerializationModeImpl(go, this._typeAccess, valueName, info, forceLegacy, reporter);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00003380 File Offset: 0x00001580
		private static GenericCollectionSerializationMode? GetGenericSerializationModeImpl(GenericObject2 go, LegacyTypeAccess typeAccess, string valueName, IArchiveVersionInfo info, bool forceLegacy, IArchiveReporter reporter)
		{
			LegacySerializableMemberAccess legacySerializableMemberAccess;
			typeAccess.SerializableMemberAccesses.TryGetValue(valueName, out legacySerializableMemberAccess);
			if (legacySerializableMemberAccess == null)
			{
				return null;
			}
			if (forceLegacy && legacySerializableMemberAccess.Type.IsGenericType)
			{
				if (typeof(IList).IsAssignableFrom(legacySerializableMemberAccess.Type))
				{
					return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsNonGenericCollection);
				}
				if (typeof(IDictionary).IsAssignableFrom(legacySerializableMemberAccess.Type))
				{
					return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsNonGenericCollection);
				}
				return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsGenericCollection);
			}
			else
			{
				Version version = null;
				if (info != null)
				{
					version = info.GetTargetVersion(go);
				}
				if (legacySerializableMemberAccess.SaveAsArray != null && legacySerializableMemberAccess.SaveAsArray.IsVersionInVersionRangeList(version))
				{
					return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsArray);
				}
				if (legacySerializableMemberAccess.SaveAsNonGenericCollection != null && legacySerializableMemberAccess.SaveAsNonGenericCollection.IsVersionInVersionRangeList(version))
				{
					return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsNonGenericCollection);
				}
				if (typeof(IList).IsAssignableFrom(legacySerializableMemberAccess.Type) || typeof(IDictionary).IsAssignableFrom(legacySerializableMemberAccess.Type))
				{
					return new GenericCollectionSerializationMode?(GenericCollectionSerializationMode.AsGenericCollection);
				}
				return null;
			}
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00003484 File Offset: 0x00001684
		private static object CloneDeeply(object val)
		{
			if (val == null)
			{
				return null;
			}
			object obj;
			if (val is ICloneable)
			{
				obj = ((ICloneable)val).Clone();
			}
			else
			{
				if (!val.GetType().IsGenericType || (!(val is IList) && !(val is IDictionary)))
				{
					return val;
				}
				obj = Activator.CreateInstance(val.GetType());
			}
			if (!val.GetType().IsArray)
			{
				if (val is IList)
				{
					IList list = (IList)val;
					IList list2 = (IList)obj;
					if (list.Count == list2.Count)
					{
						for (int i = 0; i < list.Count; i++)
						{
							object value = LegacyGenericObjectService.CloneDeeply(list[i]);
							list2[i] = value;
						}
					}
					else
					{
						if (list2.Count > 0)
						{
							list2.Clear();
						}
						for (int j = 0; j < list.Count; j++)
						{
							object value2 = LegacyGenericObjectService.CloneDeeply(list[j]);
							list2.Add(value2);
						}
					}
				}
				else if (val is IDictionary)
				{
					IDictionary dictionary = (IDictionary)val;
					IDictionary dictionary2 = (IDictionary)obj;
					foreach (object obj2 in dictionary)
					{
						DictionaryEntry dictionaryEntry = (DictionaryEntry)obj2;
						object key = dictionaryEntry.Key;
						object value3 = dictionaryEntry.Value;
						object key2 = LegacyGenericObjectService.CloneDeeply(key);
						object value4 = LegacyGenericObjectService.CloneDeeply(value3);
						dictionary2[key2] = value4;
					}
				}
				return obj;
			}
			if (val.GetType().GetArrayRank() > 1)
			{
				throw new MultiDimensionalArraysNotSupportedException();
			}
			Array array = (Array)val;
			Array array2 = (Array)obj;
			if (array.GetLowerBound(0) != 0)
			{
				throw new NonZeroLowerBoundNotSupportedException();
			}
			int length = array.GetLength(0);
			if (val.GetType().GetElementType().IsPrimitive)
			{
				int count = Marshal.SizeOf(val.GetType().GetElementType()) * length;
				Buffer.BlockCopy(array, 0, array2, 0, count);
			}
			else
			{
				for (int k = 0; k < length; k++)
				{
					object value5 = LegacyGenericObjectService.CloneDeeply(array.GetValue(k));
					array2.SetValue(value5, k);
				}
			}
			return array2;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x000036B0 File Offset: 0x000018B0
		private static bool MaybePerformGenericConversion(GenericObject go, LegacyTypeAccess typeAccess, string valueName, Type targetType, ref object value, byte[] nesting, IArchiveReporter reporter)
		{
			if (targetType.IsGenericType && value != null && value.GetType() != targetType)
			{
				if (!(value is Array))
				{
					if (value is IList)
					{
						IList list = LegacyGenericObjectService.CreateSerializableValueImpl(go, typeAccess, valueName, nesting, reporter) as IList;
						if (list == null)
						{
							if (reporter is IArchiveReporter2)
							{
								((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), valueName);
							}
							return false;
						}
						foreach (object value2 in ((IList)value))
						{
							if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, targetType, ref value2, LegacyGenericObjectService.Add(nesting, 0), reporter))
							{
								return false;
							}
							list.Add(value2);
						}
						value = list;
						return true;
					}
					else
					{
						if (!(value is IDictionary))
						{
							return true;
						}
						IDictionary dictionary = LegacyGenericObjectService.CreateSerializableValueImpl(go, typeAccess, valueName, nesting, reporter) as IDictionary;
						if (dictionary == null)
						{
							if (reporter is IArchiveReporter2)
							{
								((IArchiveReporter2)reporter).ReportDataSkipped(go.GetType(), valueName);
							}
							return false;
						}
						foreach (object obj in ((IDictionary)value).Keys)
						{
							object key = obj;
							if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, targetType, ref key, LegacyGenericObjectService.Add(nesting, 0), reporter))
							{
								return false;
							}
							object value3 = ((IDictionary)value)[obj];
							if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, targetType, ref value3, LegacyGenericObjectService.Add(nesting, 1), reporter))
							{
								return false;
							}
							dictionary[key] = value3;
						}
						value = dictionary;
						return true;
					}
					bool result;
					return result;
				}
				Array array = Array.CreateInstance(value.GetType().GetElementType(), ((Array)value).Length);
				for (int i = 0; i < ((Array)value).Length; i++)
				{
					object value4 = ((Array)value).GetValue(i);
					if (!LegacyGenericObjectService.MaybePerformGenericConversion(go, typeAccess, valueName, targetType, ref value4, LegacyGenericObjectService.Add(nesting, 0), reporter))
					{
						return false;
					}
					array.SetValue(value4, i);
				}
				value = array;
			}
			return true;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00003904 File Offset: 0x00001B04
		private static byte[] Add(byte[] arr, byte val)
		{
			if (arr == null)
			{
				return new byte[]
				{
					val
				};
			}
			byte[] array = new byte[arr.Length + 1];
			Array.Copy(arr, array, arr.Length);
			array[arr.Length] = val;
			return array;
		}

		// Token: 0x06000108 RID: 264 RVA: 0x0000393B File Offset: 0x00001B3B
		public bool IsSerializableForVersion(GenericObject2 go, IArchiveVersionInfo info)
		{
			return this.IsSerializableForVersionImpl(go, this._typeAccess, info);
		}

		// Token: 0x06000109 RID: 265 RVA: 0x0000394C File Offset: 0x00001B4C
		private bool IsSerializableForVersionImpl(GenericObject2 go, LegacyTypeAccess typeAccess, IArchiveVersionInfo info)
		{
			StorageVersionAttribute storageVersionAttribute = typeAccess.StorageVersionAttribute;
			if (storageVersionAttribute == null)
			{
				return false;
			}
			Version targetVersion = info.GetTargetVersion(go);
			return !(targetVersion == null) && storageVersionAttribute.IsVersionInVersionRangeList(targetVersion);
		}

		// Token: 0x04000033 RID: 51
		private LegacyTypeAccess _typeAccess;
	}
}
