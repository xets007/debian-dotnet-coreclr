// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;

namespace System.Reflection
{
    [Serializable]
    internal sealed unsafe class MdFieldInfo : RuntimeFieldInfo, ISerializable
    {
        #region Private Data Members
        private int m_tkField;
        private string m_name;
        private RuntimeType m_fieldType;
        private FieldAttributes m_fieldAttributes;
        #endregion

        #region Constructor
        internal MdFieldInfo(
        int tkField, FieldAttributes fieldAttributes, RuntimeTypeHandle declaringTypeHandle, RuntimeTypeCache reflectedTypeCache, BindingFlags bindingFlags)
            : base(reflectedTypeCache, declaringTypeHandle.GetRuntimeType(), bindingFlags)
        {
            m_tkField = tkField;
            m_name = null;
            m_fieldAttributes = fieldAttributes;
        }
        #endregion

        #region Internal Members
        internal override bool CacheEquals(object o)
        {
            MdFieldInfo m = o as MdFieldInfo;

            if ((object)m == null)
                return false;

            return m.m_tkField == m_tkField &&
                m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals(
                    m.m_declaringType.GetTypeHandleInternal().GetModuleHandle());
        }
        #endregion

        #region MemberInfo Overrides
        public override String Name
        {
            get
            {
                if (m_name == null)
                    m_name = GetRuntimeModule().MetadataImport.GetName(m_tkField).ToString();

                return m_name;
            }
        }

        public override int MetadataToken { get { return m_tkField; } }
        internal override RuntimeModule GetRuntimeModule() { return m_declaringType.GetRuntimeModule(); }
        #endregion

        #region FieldInfo Overrides
        public override RuntimeFieldHandle FieldHandle { get { throw new NotSupportedException(); } }
        public override FieldAttributes Attributes { get { return m_fieldAttributes; } }

        public override bool IsSecurityCritical { get { return DeclaringType.IsSecurityCritical; } }
        public override bool IsSecuritySafeCritical { get { return DeclaringType.IsSecuritySafeCritical; } }
        public override bool IsSecurityTransparent { get { return DeclaringType.IsSecurityTransparent; } }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public override Object GetValueDirect(TypedReference obj)
        {
            return GetValue(null);
        }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public override void SetValueDirect(TypedReference obj, Object value)
        {
            throw new FieldAccessException(SR.Acc_ReadOnly);
        }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public unsafe override Object GetValue(Object obj)
        {
            return GetValue(false);
        }

        public unsafe override Object GetRawConstantValue() { return GetValue(true); }

        private unsafe Object GetValue(bool raw)
        {
            // Cannot cache these because they could be user defined non-agile enumerations

            Object value = MdConstant.GetValue(GetRuntimeModule().MetadataImport, m_tkField, FieldType.GetTypeHandleInternal(), raw);

            if (value == DBNull.Value)
                throw new NotSupportedException(SR.Arg_EnumLitValueNotFound);

            return value;
        }

        [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public override void SetValue(Object obj, Object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new FieldAccessException(SR.Acc_ReadOnly);
        }

        public override Type FieldType
        {
            get
            {
                if (m_fieldType == null)
                {
                    ConstArray fieldMarshal = GetRuntimeModule().MetadataImport.GetSigOfFieldDef(m_tkField);

                    m_fieldType = new Signature(fieldMarshal.Signature.ToPointer(),
                        (int)fieldMarshal.Length, m_declaringType).FieldType;
                }

                return m_fieldType;
            }
        }

        public override Type[] GetRequiredCustomModifiers()
        {
            return Array.Empty<Type>();
        }

        public override Type[] GetOptionalCustomModifiers()
        {
            return Array.Empty<Type>();
        }

        #endregion
    }
}
