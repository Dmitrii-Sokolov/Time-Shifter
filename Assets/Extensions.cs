using System.Text;

public static class Extensions
{
    public static void AppendSubline(this StringBuilder stringBuilder, string line)
    {
        if (stringBuilder.Length > 0)
            stringBuilder.AppendLine();

        stringBuilder.Append(line);
    }
}