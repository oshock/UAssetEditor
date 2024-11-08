﻿namespace UnrealExtractor.Unreal.IoStore;

public enum EIoChunkType5 : byte
{
    Invalid = 0,
    ExportBundleData = 1,
    BulkData = 2,
    OptionalBulkData = 3,
    MemoryMappedBulkData = 4,
    ScriptObjects = 5,
    ContainerHeader = 6,
    ExternalFile = 7,
    ShaderCodeLibrary = 8,
    ShaderCode = 9,
    PackageStoreEntry = 10,
    DerivedData = 11,
    EditorDerivedData = 12
}

public class FIoChunkId
{
    public ulong ChunkId;
    public ushort ChunkIndex;
    private byte _padding;
    public EIoChunkType5 ChunkType;
}