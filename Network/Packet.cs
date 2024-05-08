using System.Text;

namespace Network.data
{
    public class Packet : IDisposable
    {
        // True when fully formated for sending
        public bool Packaged = false;
        // A number to differenciate packet types
        public byte PacketType;
        public List<byte> Data;
        private byte[] _readingData = new byte[0];
        private int _readerPos = 0;
        public int PacketLength;
        #region  Constructors
        public Packet(byte[] _data, bool readLength = true)
        {
            PacketType = _data[0];
            List<byte> Readable = new List<byte>();
            Readable.AddRange(_data);
            if (readLength)
            {
                PacketLength = BitConverter.ToInt32(_data, 1);
                Data = Readable.GetRange(5, _data.Length - 5);
            }
            else
            {
                Data = Readable.GetRange(1, _data.Length - 1);
                Packaged = true;
            }
            _readingData = Data.ToArray();
        }
        public Packet(byte type)
        {
            Data = new List<byte>();
            PacketType = type;
        }
        #endregion
        public byte[] UnreadData()
        {
            return Data.GetRange(_readerPos, Data.Count - _readerPos).ToArray();
        }
        // Methods for converting variable to bytes and adding them to the array
        #region WriteingMethods
        public void Write(byte _data)
        {
            Data.Add(_data);
        }
        public void Write(byte[] _data)
        {
            Data.AddRange(_data);
        }
        public void Write(int value)
        {
            Data.AddRange(BitConverter.GetBytes(value));
        }
        public void Write(float value)
        {
            Data.AddRange(BitConverter.GetBytes(value));
        }
        public void Write(string value)
        {
            byte[] _data = Encoding.ASCII.GetBytes(value);
            Write((int)_data.Length);
            Data.AddRange(_data);
        }
        public void Write(bool value)
        {
            Data.AddRange(BitConverter.GetBytes(value));
        }
        #endregion
    // Methods for retunging values from the byte array
        #region ReadMethods
        public byte ReadByte()
        {
            _readerPos += 1;
            return Data[_readerPos - 1];
        }
        public int ReadInt()
        {
            _readerPos += 4;
            return BitConverter.ToInt32(_readingData, _readerPos - 4);
        }
        public string ReadString()
        {
            int length = ReadInt();
            byte[] value = Data.GetRange(_readerPos, length).ToArray();
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
            Data.InsertRange(index, _data);
        }
        // adds the PacketType to the front of the array
        public void PrepForSending()
        {
            Packaged = true;
            Data.Insert(0, PacketType);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
