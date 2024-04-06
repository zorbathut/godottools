
public static class Assert
{
    public static bool IsNull(object val)
    {
        if (val != null)
        {
            Dbg.Err($"Value is not null: {val}");
            return false;
        }

        return true;
    }

    public static bool IsNotNull(object val)
    {
        if (val == null)
        {
            Dbg.Err("Value is null");
            return false;
        }

        return true;
    }

    public static bool IsTrue(bool val, string msg = null)
    {
        if (!val)
        {
            Dbg.Err(msg ?? "Value is false");
            return false;
        }

        return true;
    }

    public static bool IsEmpty<T>(System.Collections.Generic.ICollection<T> collection)
    {
        if (collection.Count != 0)
        {
            Dbg.Err($"Collection is not empty: {collection}");
            return false;
        }

        return true;
    }

    public static bool AreSame(object expected, object actual)
    {
        if (expected != actual)
        {
            Dbg.Err($"Values do not match: expected {expected}, actual {actual}");
            return false;
        }

        return true;
    }

    public static bool AreEqual<T>(T expected, T actual)
    {
        if (!expected.Equals(actual))
        {
            Dbg.Err($"Values do not match: expected {expected}, actual {actual}");
            return false;
        }

        return true;
    }
}
