using System;

public class CastTypeExtension {
    public static T CastTo<T>(object input) {
        return (T) input;
    }

    public static T ConvertTo<T>(object input) {
        return (T) Convert.ChangeType(input, typeof(T));
    }
}