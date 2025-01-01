namespace UAssetEditor.Classes;

public class UnrealField : Attribute
{
    
}

// Ik this is so scuffed but whatever - owen
[AttributeUsage(AttributeTargets.Method)]
public class UnrealValueGetter : Attribute
{
    
}

// So we can tell if this is a generic dotnet type or not
public interface IUnrealType
{
    
}