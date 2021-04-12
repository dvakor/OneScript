/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    public static class ValueAdoptionExtensions
    {
        public static bool AsBoolean(this BslValue val) => (bool) val;
        public static DateTime AsDate(this BslValue val) => (DateTime) val;
        public static decimal AsNumber(this BslValue val) => (decimal) val;
        public static string AsString(this BslValue val) => (string) val;
        
        public static IRuntimeContextInstance AsObject(this BslValue val) 
            => val is IRuntimeContextInstance ctx? ctx : throw RuntimeException.ValueIsNotObjectException();
        
        public static bool AsBoolean(this IValue val) => (bool) (BslValue)val;
        public static DateTime AsDate(this IValue val) => (DateTime) (BslValue)val;
        public static decimal AsNumber(this IValue val) => (decimal) (BslValue)val;
        public static string AsString(this IValue val) => (string) (BslValue)val;
        
        public static IRuntimeContextInstance AsObject(this IValue val) 
            => val is IRuntimeContextInstance ctx? ctx : throw RuntimeException.ValueIsNotObjectException();

        public static object CastToClrObject(this IValue value)
        {
            return ContextValuesMarshaller.ConvertToCLRObject(value);
        }

        public static bool IsSkippedArgument(this IValue val)
        {
            return ReferenceEquals(val, BslSkippedParameterValue.Instance);
        }
    }
}