namespace UAssetEditor.Unreal.Versioning;

public enum EUnrealEngineObjectLicenseeUEVersion
{
    VER_LIC_NONE = 0,

    // - this needs to be the last line (see note below)
    VER_LIC_AUTOMATIC_VERSION_PLUS_ONE,
    VER_LIC_AUTOMATIC_VERSION = VER_LIC_AUTOMATIC_VERSION_PLUS_ONE - 1
}