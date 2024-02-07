// This solution comes from:
// https://gist.github.khoury.northeastern.edu/canivit/d9abf10f7c220ece540ff59bba16fdf6
// Thank you Can.
namespace Q.Common;
public class CharacterReader : TextReader
  {
    private readonly TextReader _reader;

    public CharacterReader(TextReader reader)
    {
      _reader = reader;
    }

    public override void Close()
    {
      _reader.Close();
    }

    protected override void Dispose(bool disposing)
    {
      _reader.Dispose();
    }

    public override int Read(char[] buffer, int index, int count)
    {
      return _reader.Read(buffer, index, 1);
    }
  }