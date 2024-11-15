namespace UnrealExtractor.Utils;

public static class CastUtils
{
    public static T As<T>(this object obj) => (T)obj;
}