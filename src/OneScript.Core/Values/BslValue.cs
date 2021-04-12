/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using OneScript.Commons;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public abstract class BslValue : DynamicObject, IComparable<BslValue>, IEquatable<BslValue>, IValue
    {
        protected virtual string ConvertToString() => ToString();

        public abstract int CompareTo(BslValue other);

        public abstract bool Equals(BslValue other);

        public static explicit operator string(BslValue value) => value.ConvertToString();
        
        public static explicit operator bool(BslValue target) => 
            target is BslBooleanValue v ? (bool)v : throw BslExceptions.ConvertToBooleanException();

        public static explicit operator decimal(BslValue target) =>
            target is BslNumericValue v ? (decimal) v : throw BslExceptions.ConvertToNumberException();
        
        public static explicit operator int(BslValue target) => 
            target is BslNumericValue v ? (int)(decimal) v : throw BslExceptions.ConvertToNumberException();
        
        public static explicit operator DateTime(BslValue target) => 
            target is BslDateValue v ? (DateTime) v : throw BslExceptions.ConvertToDateException();


        #region Stack Runtime Bridge

        public virtual TypeDescriptor SystemType => BasicTypes.UnknownType;

        public virtual int CompareTo(IValue other) => CompareTo((BslValue) other);

        public virtual bool Equals(IValue other) => Equals((BslValue) other);

        public virtual IValue GetRawValue() => this;
        
        #endregion
    }
}
