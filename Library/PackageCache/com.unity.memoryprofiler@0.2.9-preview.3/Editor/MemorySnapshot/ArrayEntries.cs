using System;
using System.IO;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.MemoryProfiler.Editor.Format
{
    public interface IArrayEntries<T>
    {
        uint GetNumEntries();
        void GetEntries(uint indexStart, uint numEntries, ref T[] dataOut);
    }

    public class ArrayEntries<T> : IArrayEntries<T>
    {
        internal MemorySnapshotFileReader m_Reader;
        internal EntryType m_EntryType;
        GetItem<T> m_GetItemFunc;

        internal ArrayEntries()
        {
            m_EntryType = unchecked((EntryType)0xffffffff);
        }

        internal ArrayEntries(MemorySnapshotFileReader reader, EntryType entryType,
                              GetItem<T> getItemFunc)
        {
            m_Reader = reader;
            m_EntryType = entryType;
            m_GetItemFunc = getItemFunc;
        }

        public uint GetNumEntries()
        {
            if (m_Reader == null)
                return 0;
            return m_Reader.GetNumEntries(m_EntryType);
        }

        public virtual void GetEntries(uint indexStart, uint numEntries, ref T[] dataOut)
        {
            if (GetNumEntries() == 0)
            {
                Debug.LogError("Unable to read, the array has 0 entries");
                return;
            }

            if (m_Reader == null)
                throw new Exception("No reader present for array entries");
            m_Reader.GetDataArray(m_EntryType, indexStart, numEntries, ref dataOut, m_GetItemFunc);
        }
    }

    internal class ByteArrayEntries : ArrayEntries<byte[]>
    {
        public ByteArrayEntries(MemorySnapshotFileReader reader, EntryType entryType)
            : base(reader, entryType, null)
        {
        }

        public override void GetEntries(uint indexStart, uint numEntries, ref byte[][] dataOut)
        {
            m_Reader.GetDataByteArray(m_EntryType, indexStart, numEntries, ref dataOut);
        }
    }

    public interface IConnectionEntries
    {
        ArrayEntries<int> from { get; }
        ArrayEntries<int> to { get; }
        uint GetNumEntries();
    }

    public class ConnectionEntries : IConnectionEntries
    {
        public ArrayEntries<int> from { get; }
        public ArrayEntries<int> to { get; }

        internal ConnectionEntries(MemorySnapshotFileReader reader)
        {
            from = new ArrayEntries<int>(reader, EntryType.Connections_From, ConversionFunctions.ToInt32);
            to = new ArrayEntries<int>(reader, EntryType.Connections_To, ConversionFunctions.ToInt32);
        }

        public uint GetNumEntries()
        {
            return from.GetNumEntries();
        }
    }

    public interface IGCHandleEntries
    {
        ArrayEntries<ulong> target { get; }
        uint GetNumEntries();
    }

    public class GCHandleEntries : IGCHandleEntries
    {
        public ArrayEntries<ulong> target { get; }

        internal GCHandleEntries(MemorySnapshotFileReader reader)
        {
            target = new ArrayEntries<ulong>(reader, EntryType.GCHandles_Target, ConversionFunctions.ToUInt64);
        }

        public uint GetNumEntries()
        {
            return target.GetNumEntries();
        }
    }

    public interface IManagedMemorySectionEntries
    {
        ArrayEntries<byte[]> bytes { get; }
        ArrayEntries<ulong> startAddress { get; }
        //raw data getter for testing compatibility of the old format vs the new one
        ArrayEntries<ulong> rawStartAddress { get; }
        uint GetNumEntries();
    }

    public class ManagedMemorySectionEntries : IManagedMemorySectionEntries
    {
        public ArrayEntries<byte[]> bytes { get; }
        public ArrayEntries<ulong> startAddress { get; }
        public ArrayEntries<ulong> rawStartAddress { get; }

        internal ManagedMemorySectionEntries(MemorySnapshotFileReader reader, EntryType entryTypeBase, bool hasHeapTypeEncodedInAddr)
        {
            //all addresses are shifted 1 bit to the left in the case of encoding the type inside the chunk (ver 12+)
            //only the new reader inside com.unity.memoryprofiler will read the encodings
            GetItem<ulong> func;
            if (hasHeapTypeEncodedInAddr)
                func = ConversionFunctions.ToUInt64WithMask;
            else
                func = ConversionFunctions.ToUInt64;

            startAddress = new ArrayEntries<ulong>(reader, (EntryType)(entryTypeBase + 0), func);
            rawStartAddress = new ArrayEntries<ulong>(reader, (EntryType)(entryTypeBase + 0), ConversionFunctions.ToUInt64);

            bytes = new ByteArrayEntries(reader, (EntryType)(entryTypeBase + 1));
        }

        public uint GetNumEntries()
        {
            return bytes.GetNumEntries();
        }
    }

    public interface INativeObjectEntries
    {
        IArrayEntries<string> objectName { get; }
        IArrayEntries<int> instanceId { get; }
        IArrayEntries<ulong> size { get; }
        IArrayEntries<int> nativeTypeArrayIndex { get; }
        IArrayEntries<HideFlags> hideFlags { get; }
        IArrayEntries<ObjectFlags> flags { get; }
        IArrayEntries<ulong> nativeObjectAddress { get; }
        IArrayEntries<long> rootReferenceId { get; }
        IArrayEntries<int> gcHandleIndex { get; }
        uint GetNumEntries();
    }

    public class NativeObjectEntries : INativeObjectEntries
    {
        public IArrayEntries<string> objectName { get; }
        public IArrayEntries<int> instanceId { get; }
        public IArrayEntries<ulong> size { get; }
        public IArrayEntries<int> nativeTypeArrayIndex { get; }
        public IArrayEntries<HideFlags> hideFlags { get; }
        public IArrayEntries<ObjectFlags> flags { get; }
        public IArrayEntries<ulong> nativeObjectAddress { get; }
        public IArrayEntries<long> rootReferenceId { get; }
        public IArrayEntries<int> gcHandleIndex { get; }

        internal NativeObjectEntries(MemorySnapshotFileReader reader, bool hasGcHandleIndex)
        {
            objectName = new ArrayEntries<string>(reader, EntryType.NativeObjects_Name, ConversionFunctions.ToString);
            instanceId = new ArrayEntries<int>(reader, EntryType.NativeObjects_InstanceId, ConversionFunctions.ToInt32);
            size = new ArrayEntries<ulong>(reader, EntryType.NativeObjects_Size, ConversionFunctions.ToUInt64);
            nativeTypeArrayIndex = new ArrayEntries<int>(reader, EntryType.NativeObjects_NativeTypeArrayIndex, ConversionFunctions.ToInt32);
            hideFlags = new ArrayEntries<HideFlags>(reader, EntryType.NativeObjects_HideFlags, ConversionFunctions.ToHideFlags);
            flags = new ArrayEntries<ObjectFlags>(reader, EntryType.NativeObjects_Flags, ConversionFunctions.ToObjectFlags);
            nativeObjectAddress = new ArrayEntries<ulong>(reader, EntryType.NativeObjects_NativeObjectAddress, ConversionFunctions.ToUInt64);
            rootReferenceId = new ArrayEntries<long>(reader, EntryType.NativeObjects_RootReferenceId, ConversionFunctions.ToInt64);
            if (hasGcHandleIndex)
                gcHandleIndex = new ArrayEntries<int>(reader, EntryType.NativeObjects_GCHandleIndex, ConversionFunctions.ToInt32);
            else
                gcHandleIndex = new ArrayEntries<int>();
        }

        public uint GetNumEntries()
        {
            return objectName.GetNumEntries();
        }
    }

    public interface INativeTypeEntries
    {
        ArrayEntries<string> typeName { get; }
        ArrayEntries<int> nativeBaseTypeArrayIndex { get; }
        uint GetNumEntries();
    }

    public class NativeTypeEntries : INativeTypeEntries
    {
        public ArrayEntries<string> typeName { get; }
        public ArrayEntries<int> nativeBaseTypeArrayIndex { get; }

        internal NativeTypeEntries(MemorySnapshotFileReader reader)
        {
            typeName = new ArrayEntries<string>(reader, EntryType.NativeTypes_Name, ConversionFunctions.ToString);
            nativeBaseTypeArrayIndex = new ArrayEntries<int>(reader, EntryType.NativeTypes_NativeBaseTypeArrayIndex, ConversionFunctions.ToInt32);
        }

        public uint GetNumEntries()
        {
            return typeName.GetNumEntries();
        }
    }

    public interface ITypeDescriptionEntries
    {
        ArrayEntries<TypeFlags> flags { get; }
        ArrayEntries<string> typeDescriptionName { get; }
        ArrayEntries<string> assembly { get; }
        ArrayEntries<int[]> fieldIndices { get; }
        ArrayEntries<byte[]> staticFieldBytes { get; }
        ArrayEntries<int> baseOrElementTypeIndex { get; }
        ArrayEntries<int> size { get; }
        ArrayEntries<ulong> typeInfoAddress { get; }
        ArrayEntries<int> typeIndex { get; }
        uint GetNumEntries();
    }

    public class TypeDescriptionEntries : ITypeDescriptionEntries
    {
        public ArrayEntries<TypeFlags> flags { get; }
        public ArrayEntries<string> typeDescriptionName { get; }
        public ArrayEntries<string> assembly { get; }
        public ArrayEntries<int[]> fieldIndices { get; }
        public ArrayEntries<byte[]> staticFieldBytes { get; }
        public ArrayEntries<int> baseOrElementTypeIndex { get; }
        public ArrayEntries<int> size { get; }
        public ArrayEntries<ulong> typeInfoAddress { get; }
        public ArrayEntries<int> typeIndex { get; }

        internal TypeDescriptionEntries(MemorySnapshotFileReader reader)
        {
            flags = new ArrayEntries<TypeFlags>(reader, EntryType.TypeDescriptions_Flags, ConversionFunctions.ToTypeFlags);
            typeDescriptionName = new ArrayEntries<string>(reader, EntryType.TypeDescriptions_Name, ConversionFunctions.ToString);
            assembly = new ArrayEntries<string>(reader, EntryType.TypeDescriptions_Assembly, ConversionFunctions.ToString);
            fieldIndices = new ArrayEntries<int[]>(reader, EntryType.TypeDescriptions_FieldIndices, ConversionFunctions.ToInt32Array);
            staticFieldBytes = new ByteArrayEntries(reader, EntryType.TypeDescriptions_StaticFieldBytes);
            baseOrElementTypeIndex = new ArrayEntries<int>(reader, EntryType.TypeDescriptions_BaseOrElementTypeIndex, ConversionFunctions.ToInt32);
            size = new ArrayEntries<int>(reader, EntryType.TypeDescriptions_Size, ConversionFunctions.ToInt32);
            typeInfoAddress = new ArrayEntries<ulong>(reader, EntryType.TypeDescriptions_TypeInfoAddress, ConversionFunctions.ToUInt64);
            typeIndex = new ArrayEntries<int>(reader, EntryType.TypeDescriptions_TypeIndex, ConversionFunctions.ToInt32);
        }

        public uint GetNumEntries()
        {
            return flags.GetNumEntries();
        }
    }

    public interface IFieldDescriptionEntries
    {
        ArrayEntries<string> fieldDescriptionName { get; }
        ArrayEntries<int> offset { get; }
        ArrayEntries<int> typeIndex { get; }
        ArrayEntries<bool> isStatic { get; }
        uint GetNumEntries();
    }

    public class FieldDescriptionEntries : IFieldDescriptionEntries
    {
        public ArrayEntries<string> fieldDescriptionName { get; }
        public ArrayEntries<int> offset { get; }
        public ArrayEntries<int> typeIndex { get; }
        public ArrayEntries<bool> isStatic { get; }

        internal FieldDescriptionEntries(MemorySnapshotFileReader reader)
        {
            fieldDescriptionName = new ArrayEntries<string>(reader, EntryType.FieldDescriptions_Name, ConversionFunctions.ToString);
            offset = new ArrayEntries<int>(reader, EntryType.FieldDescriptions_Offset, ConversionFunctions.ToInt32);
            typeIndex = new ArrayEntries<int>(reader, EntryType.FieldDescriptions_TypeIndex, ConversionFunctions.ToInt32);
            isStatic = new ArrayEntries<bool>(reader, EntryType.FieldDescriptions_IsStatic, ConversionFunctions.ToBoolean);
        }

        public uint GetNumEntries()
        {
            return fieldDescriptionName.GetNumEntries();
        }
    }

    public interface INativeMemoryLabelEntries
    {
        ArrayEntries<string> memoryLabelName { get; }
        uint GetNumEntries();
    }

    public class NativeMemoryLabelEntries : INativeMemoryLabelEntries
    {
        public ArrayEntries<string> memoryLabelName { get; }

        internal NativeMemoryLabelEntries(MemorySnapshotFileReader reader)
        {
            memoryLabelName = new ArrayEntries<string>(reader, EntryType.NativeMemoryLabels_Name, ConversionFunctions.ToString);
        }

        public uint GetNumEntries()
        {
            return memoryLabelName.GetNumEntries();
        }
    }

    public interface INativeRootReferenceEntries
    {
        ArrayEntries<long> id { get; }
        ArrayEntries<string> areaName { get; }
        ArrayEntries<string> objectName { get; }
        ArrayEntries<ulong> accumulatedSize { get; }
        uint GetNumEntries();
    }

    public class NativeRootReferenceEntries : INativeRootReferenceEntries
    {
        public ArrayEntries<long> id { get; }
        public ArrayEntries<string> areaName { get; }
        public ArrayEntries<string> objectName { get; }
        public ArrayEntries<ulong> accumulatedSize { get; }

        internal NativeRootReferenceEntries(MemorySnapshotFileReader reader)
        {
            id = new ArrayEntries<long>(reader, EntryType.NativeRootReferences_Id, ConversionFunctions.ToInt64);
            areaName = new ArrayEntries<string>(reader, EntryType.NativeRootReferences_AreaName, ConversionFunctions.ToString);
            objectName = new ArrayEntries<string>(reader, EntryType.NativeRootReferences_ObjectName, ConversionFunctions.ToString);
            accumulatedSize = new ArrayEntries<ulong>(reader, EntryType.NativeRootReferences_AccumulatedSize, ConversionFunctions.ToUInt64);
        }

        public uint GetNumEntries()
        {
            return id.GetNumEntries();
        }
    }

    public interface INativeAllocationEntries
    {
        ArrayEntries<int> memoryRegionIndex { get; }
        ArrayEntries<long> rootReferenceId { get; }
        ArrayEntries<long> allocationSiteId { get; }
        ArrayEntries<ulong> address { get; }
        ArrayEntries<ulong> size { get; }
        ArrayEntries<int> overheadSize { get; }
        ArrayEntries<int> paddingSize { get; }
        uint GetNumEntries();
    }

    public class NativeAllocationEntries : INativeAllocationEntries
    {
        public ArrayEntries<int> memoryRegionIndex { get; }
        public ArrayEntries<long> rootReferenceId { get; }
        public ArrayEntries<long> allocationSiteId { get; }
        public ArrayEntries<ulong> address { get; }
        public ArrayEntries<ulong> size { get; }
        public ArrayEntries<int> overheadSize { get; }
        public ArrayEntries<int> paddingSize { get; }

        internal NativeAllocationEntries(MemorySnapshotFileReader reader)
        {
            memoryRegionIndex = new ArrayEntries<int>(reader, EntryType.NativeAllocations_MemoryRegionIndex, ConversionFunctions.ToInt32);
            rootReferenceId = new ArrayEntries<long>(reader, EntryType.NativeAllocations_RootReferenceId, ConversionFunctions.ToInt64);
            allocationSiteId = new ArrayEntries<long>(reader, EntryType.NativeAllocations_AllocationSiteId, ConversionFunctions.ToInt64);
            address = new ArrayEntries<ulong>(reader, EntryType.NativeAllocations_Address, ConversionFunctions.ToUInt64);
            size = new ArrayEntries<ulong>(reader, EntryType.NativeAllocations_Size, ConversionFunctions.ToUInt64);
            overheadSize = new ArrayEntries<int>(reader, EntryType.NativeAllocations_OverheadSize, ConversionFunctions.ToInt32);
            paddingSize = new ArrayEntries<int>(reader, EntryType.NativeAllocations_PaddingSize, ConversionFunctions.ToInt32);
        }

        public uint GetNumEntries()
        {
            return memoryRegionIndex.GetNumEntries();
        }
    }

    public interface INativeMemoryRegionEntries
    {
        ArrayEntries<string> memoryRegionName { get; }
        ArrayEntries<int> parentIndex { get; }
        ArrayEntries<ulong> addressBase { get; }
        ArrayEntries<ulong> addressSize { get; }
        ArrayEntries<int> firstAllocationIndex { get; }
        ArrayEntries<int> numAllocations { get; }
        uint GetNumEntries();
    }

    public class NativeMemoryRegionEntries : INativeMemoryRegionEntries
    {
        public ArrayEntries<string> memoryRegionName { get; }
        public ArrayEntries<int> parentIndex { get; }
        public ArrayEntries<ulong> addressBase { get; }
        public ArrayEntries<ulong> addressSize { get; }
        public ArrayEntries<int> firstAllocationIndex { get; }
        public ArrayEntries<int> numAllocations { get; }

        internal NativeMemoryRegionEntries(MemorySnapshotFileReader reader)
        {
            memoryRegionName = new ArrayEntries<string>(reader, EntryType.NativeMemoryRegions_Name, ConversionFunctions.ToString);
            parentIndex = new ArrayEntries<int>(reader, EntryType.NativeMemoryRegions_ParentIndex, ConversionFunctions.ToInt32);
            addressBase = new ArrayEntries<ulong>(reader, EntryType.NativeMemoryRegions_AddressBase, ConversionFunctions.ToUInt64);
            addressSize = new ArrayEntries<ulong>(reader, EntryType.NativeMemoryRegions_AddressSize, ConversionFunctions.ToUInt64);
            firstAllocationIndex = new ArrayEntries<int>(reader, EntryType.NativeMemoryRegions_FirstAllocationIndex, ConversionFunctions.ToInt32);
            numAllocations = new ArrayEntries<int>(reader, EntryType.NativeMemoryRegions_NumAllocations, ConversionFunctions.ToInt32);
        }

        public uint GetNumEntries()
        {
            return memoryRegionName.GetNumEntries();
        }
    }

    public interface INativeAllocationSiteEntries
    {
        ArrayEntries<long> id { get; }
        ArrayEntries<int> memoryLabelIndex { get; }
        ArrayEntries<ulong[]> callstackSymbols { get; }
        uint GetNumEntries();
    }

    public class NativeAllocationSiteEntries : INativeAllocationSiteEntries
    {
        public ArrayEntries<long> id { get; }
        public ArrayEntries<int> memoryLabelIndex { get; }
        public ArrayEntries<ulong[]> callstackSymbols { get; }

        internal NativeAllocationSiteEntries(MemorySnapshotFileReader reader)
        {
            id = new ArrayEntries<long>(reader, EntryType.NativeAllocationSites_Id, ConversionFunctions.ToInt64);
            memoryLabelIndex = new ArrayEntries<int>(reader, EntryType.NativeAllocationSites_MemoryLabelIndex, ConversionFunctions.ToInt32);
            callstackSymbols = new ArrayEntries<ulong[]>(reader, EntryType.NativeAllocationSites_CallstackSymbols, ConversionFunctions.ToUInt64Array);
        }

        public uint GetNumEntries()
        {
            return id.GetNumEntries();
        }
    }

    public interface INativeCallstackSymbolEntries
    {
        ArrayEntries<ulong> symbol { get; }
        ArrayEntries<string> readableStackTrace { get; }
        uint GetNumEntries();
    }

    public class NativeCallstackSymbolEntries : INativeCallstackSymbolEntries
    {
        public ArrayEntries<ulong> symbol { get; }
        public ArrayEntries<string> readableStackTrace { get; }

        internal NativeCallstackSymbolEntries(MemorySnapshotFileReader reader)
        {
            symbol = new ArrayEntries<ulong>(reader, EntryType.NativeCallstackSymbol_Symbol, ConversionFunctions.ToUInt64);
            readableStackTrace = new ArrayEntries<string>(reader, EntryType.NativeCallstackSymbol_ReadableStackTrace, ConversionFunctions.ToString);
        }

        public uint GetNumEntries()
        {
            return symbol.GetNumEntries();
        }
    }

    internal class ConversionFunctions
    {
        public static VirtualMachineInformation ToVirtualMachineInformation(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(int) * 6)
            {
                throw new IOException("Invalid virtual machine information data");
            }
            VirtualMachineInformation result = new VirtualMachineInformation();
            result.pointerSize = BitConverter.ToInt32(data, sizeof(int) * 0);
            result.objectHeaderSize = BitConverter.ToInt32(data, sizeof(int) * 1);
            result.arrayHeaderSize = BitConverter.ToInt32(data, sizeof(int) * 2);
            result.arrayBoundsOffsetInHeader = BitConverter.ToInt32(data, sizeof(int) * 3);
            result.arraySizeOffsetInHeader = BitConverter.ToInt32(data, sizeof(int) * 4);
            result.allocationGranularity = BitConverter.ToInt32(data, sizeof(int) * 5);
            return result;
        }

        public static UnityEngine.HideFlags ToHideFlags(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(uint))
            {
                throw new IOException("Invalid data entry");
            }
            return (UnityEngine.HideFlags)BitConverter.ToUInt32(data, (int)startIndex);
        }

        public static ObjectFlags ToObjectFlags(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(uint))
            {
                throw new IOException("Invalid data entry");
            }
            return (ObjectFlags)BitConverter.ToUInt32(data, (int)startIndex);
        }

        public static TypeFlags ToTypeFlags(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(uint))
            {
                throw new IOException("Invalid data entry");
            }
            return (TypeFlags)BitConverter.ToUInt32(data, (int)startIndex);
        }

        public static short ToInt16(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(short))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToInt16(data, (int)startIndex);
        }

        public static int ToInt32(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(int))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToInt32(data, (int)startIndex);
        }

        public static long ToInt64(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(long))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToInt64(data, (int)startIndex);
        }

        public static ushort ToUInt16(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(ushort))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToUInt16(data, (int)startIndex);
        }

        public static uint ToUInt32(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(uint))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToUInt32(data, (int)startIndex);
        }
        
        public static ulong ToUInt64WithMask(byte[] data, uint startIndex, uint numBytes)
        {

            const ulong bitMask = ulong.MaxValue >> 1;
            if (numBytes != sizeof(ulong))
            {
                throw new IOException("Invalid data entry");
            }

            ulong res = BitConverter.ToUInt64(data, (int)startIndex);
            res = res & bitMask; //use bitmask to get real addr out
            return res;
        }

        public static ulong ToUInt64(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(ulong))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToUInt64(data, (int)startIndex);
        }

        public static bool ToBoolean(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes != sizeof(bool))
            {
                throw new IOException("Invalid data entry");
            }
            return BitConverter.ToBoolean(data, (int)startIndex);
        }

        public static byte[] ToByteArray(byte[] data, uint startIndex, uint numBytes)
        {
            byte[] result = new byte[numBytes];
            Array.Copy(data, startIndex, result, 0, numBytes);
            return result;
        }

        public static int[] ToInt32Array(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes % sizeof(int) != 0)
            {
                throw new IOException("Invalid data entry");
            }
            uint length = numBytes / sizeof(int);
            int[] result = new int[length];
            for (uint i = 0; i < length; i++)
            {
                result[i] = ToInt32(data, startIndex + sizeof(int) * i, sizeof(int));
            }
            return result;
        }

        public static long[] ToInt64Array(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes % sizeof(long) != 0)
            {
                throw new IOException("Invalid data entry");
            }
            uint length = numBytes / sizeof(long);
            long[] result = new long[length];
            for (uint i = 0; i < length; i++)
            {
                result[i] = ToInt64(data, startIndex + sizeof(long) * i, sizeof(long));
            }
            return result;
        }

        public static ulong[] ToUInt64Array(byte[] data, uint startIndex, uint numBytes)
        {
            if (numBytes % sizeof(ulong) != 0)
            {
                throw new IOException("Invalid data entry");
            }
            uint length = numBytes / sizeof(ulong);
            ulong[] result = new ulong[length];
            for (uint i = 0; i < length; i++)
            {
                result[i] = ToUInt64(data, startIndex + sizeof(ulong) * i, sizeof(ulong));
            }
            return result;
        }

        public static string ToString(byte[] data, uint startIndex, uint numBytes)
        {
            var result = new string('A', (int)numBytes);
            unsafe
            {
                fixed(char* resultPtr = result)
                {
                    fixed(byte* dataPtr = data)
                    {
                        UnsafeUtility.MemCpyStride(resultPtr, UnsafeUtility.SizeOf<char>(), dataPtr + startIndex, UnsafeUtility.SizeOf<byte>(), UnsafeUtility.SizeOf<byte>(), (int)numBytes);
                    }
                }
            }
            return result;
        }
    }
}