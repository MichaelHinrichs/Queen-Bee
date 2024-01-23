namespace Nanook.QueenBee.Parser
{
    /// <summary>
    /// A simple class to store strings found within scripts
    /// </summary>
    public class ScriptString
    {
        public ScriptString(string text, int pos, int length, bool isUnicode)
        {
            Text = text;
            Pos = pos;
            Length = length;
            IsUnicode = isUnicode;
        }

        public string Text { get; set; }
        public int Pos { get; set; }
        public int Length { get; set; }
        public bool IsUnicode { get; set; }
    }
}
