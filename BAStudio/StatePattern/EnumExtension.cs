using System;
using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;

// https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104840

namespace BAStudio.StatePattern
{
    public static class EnumExtension
    {
        // size-specific version
        public static TInt AsInteger<TEnum, TInt>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
            where TInt : unmanaged
        {
            if (UnsafeUtility.SizeOf<TEnum>() != UnsafeUtility.SizeOf<TInt>()) throw new Exception("Underlying type size mismatch");
            TInt value = UnsafeUtility.As<TEnum, TInt>(ref enumValue);
            return value;
        }

        // long version
        public static long AsInt64<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            long value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<long>()) value = UnsafeUtility.As<TEnum, long>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<int>()) value = UnsafeUtility.As<TEnum, int>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<short>()) value = UnsafeUtility.As<TEnum, short>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<sbyte>()) value = UnsafeUtility.As<TEnum, sbyte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // int version
        public static int AsInt32<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            int value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<int>()) value = UnsafeUtility.As<TEnum, int>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<short>()) value = UnsafeUtility.As<TEnum, short>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<sbyte>()) value = UnsafeUtility.As<TEnum, sbyte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // short version
        public static short AsInt16<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            short value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<short>()) value = UnsafeUtility.As<TEnum, short>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<sbyte>()) value = UnsafeUtility.As<TEnum, sbyte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // sbyte version
        public static sbyte AsInt8<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            sbyte value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<sbyte>()) value = UnsafeUtility.As<TEnum, sbyte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // ulong version
        public static ulong AsUInt64<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            ulong value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<ulong>()) value = UnsafeUtility.As<TEnum, ulong>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<uint>()) value = UnsafeUtility.As<TEnum, uint>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<ushort>()) value = UnsafeUtility.As<TEnum, ushort>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<byte>()) value = UnsafeUtility.As<TEnum, byte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // uint version
        public static uint AsUInt32<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            uint value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<uint>()) value = UnsafeUtility.As<TEnum, uint>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<ushort>()) value = UnsafeUtility.As<TEnum, ushort>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<byte>()) value = UnsafeUtility.As<TEnum, byte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // ushort version
        public static ushort AsUInt16<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            ushort value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<ushort>()) value = UnsafeUtility.As<TEnum, ushort>(ref enumValue);
            else if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<byte>()) value = UnsafeUtility.As<TEnum, byte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        // byte version
        public static byte AsUInt8<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            byte value;
            if (UnsafeUtility.SizeOf<TEnum>() == UnsafeUtility.SizeOf<byte>()) value = UnsafeUtility.As<TEnum, byte>(ref enumValue);
            else throw new Exception("Underlying type size mismatch");
            return value;
        }

        /// <summary>
        /// 
        /// **Warning: The method explicitly cast the values to TInt.**
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TInt"></typeparam>
        /// <returns></returns>
        public static (TInt min, TInt max) MinMax<TEnum, TInt>() where TEnum : unmanaged, Enum where TInt : unmanaged
        {
            var valueMap = System.Enum.GetValues(typeof(TEnum));
            if (valueMap.Length == 0) throw new InvalidEnumArgumentException("Nothing is defined! At least one item must be defined for the enum.");
            return ((TInt) valueMap.GetValue(0), (TInt) valueMap.GetValue(valueMap.Length - 1));
        }

        public static (int min, int max) MinMaxInt<TEnum>() where TEnum : unmanaged, Enum
        => MinMax<TEnum, int>();
    }
}