namespace Studio.Daily.LangLink
{
    public interface IFileNameParser
    {
        (string localeName, string tableName) ParseFileName(string fileName);
    }
}