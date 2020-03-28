namespace Avalon.Common.Interfaces
{
    public interface IAlias
    {
        string AliasExpression { get; set; }
        string Character { get; set; }
        string Command { get; set; }
        int Count { get; set; }
        bool Enabled { get; set; }
        string Group { get; set; }
        bool IsLua { get; set; }
        bool Lock { get; set; }
    }
}