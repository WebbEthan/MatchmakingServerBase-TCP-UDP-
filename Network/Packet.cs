using System.Text;
public class Packet : IDisposable
{
    // True when fully formated for sending
    public bool Packaged = false;
    // A number to differenciate packet types
    public byte PacketType;
    public List<byte> data;
    private byte[] _readingData;
    private int _readerPos = 0;
    #region  Constructors
    public Packet(byte[] _data)
    {
        PacketType = _data[0];
        Packaged = true;
        List<byte> Readable = new List<byte>();
        Readable.AddRange(_data);
        data = Readable.GetRange(1, _data.Length - 1);
        _readingData = data.ToArray();
    }
    public Packet(byte type)
    {
        data = new List<byte>();
        PacketType = type;
    }
    #endregion
    public byte[] UnreadData()
    {
        return data.GetRange(_readerPos, data.Count - _readerPos).ToArray();
    }
    // Methods for converting variable to bytes and adding them to the array
    #region WriteingMethods
    public void Write(byte _data)
    {
        data.Add(_data);
    }
    public void Write(byte[] _data)
    {
        data.AddRange(_data);
    }
    public void Write(int value)
    {
        data.AddRange(BitConverter.GetBytes(value));
    }
    public void Write(float value)
    {
        data.AddRange(BitConverter.GetBytes(value));
    }
    public void Write(string value)
    {
        byte[] _data = Encoding.ASCII.GetBytes(value);
        Write(_data.Length);
        data.AddRange(_data);
    }
    public void Write(bool value)
    {
        data.AddRange(BitConverter.GetBytes(value));
    }
    #endregion
   // Methods for retunging values from the byte array
    #region ReadMethods
    public byte ReadByte()
    {
        _readerPos += 1;
        return data[_readerPos - 1];
    }
    public int ReadInt()
    {
        _readerPos += 4;
        return BitConverter.ToInt32(_readingData, _readerPos - 4);
    }
    public string ReadString()
    {
        int length = ReadInt();
        byte[] value = data.GetRange(_readerPos, length).ToArray();
        _readerPos += length;
        return Encoding.ASCII.GetString(value);
    }
    public bool ReadBool()
    {
        _readerPos += 1;
        return _readingData[_readerPos - 1] == 1;
    }
    #endregion
    public void Insert(int index, byte[] _data)
    {
        data.InsertRange(index, _data);
    }
    // adds the PacketType to the front of the array
    public void PrepForSending()
    {
        Packaged = true;
        data.Insert(0, PacketType);
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}