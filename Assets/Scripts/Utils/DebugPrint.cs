public static class DebugString
{
    public static string EnumToString(CellType type)
    {
        return type switch
        {
            (CellType.Basic) => "Basic",
            _ => "CellTypeMissingDebugString",
        };
    }
}
