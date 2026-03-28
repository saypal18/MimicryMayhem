using System.Reflection;

public static class ReflectionUtils
{
    public static T GetFieldValue<T>(object obj, string fieldName)
    {
        if (obj == null) return default;
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field == null) return default;
        return (T)field.GetValue(obj);
    }
}
