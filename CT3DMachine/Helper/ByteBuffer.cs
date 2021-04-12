using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace CT3DMachine.Helper
{
    class ByteBuffer
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private byte[] mBuffer;
        private int mLength = 0;
        private int mReadPos = 0;
        private int mWritePos = 0;

        public ByteBuffer()
        {
            mBuffer = new byte[4096];
            mLength = 4096;
        }

        public int getTotalLength()
        {
            return mWritePos - mReadPos;
        }
        // Write
        public void putByte(byte value)
        {
            if(availableLength() >= 1)
            {
                mBuffer[mWritePos] = value;
                mWritePos++;
            } else
            {
                shrink();
                if(availableLength() >= 1)
                {
                    mBuffer[mWritePos] = value;
                    mWritePos++;
                } else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putInt16(Int16 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(Int16))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int16));
                mWritePos += sizeof(Int16);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(Int16))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int16));
                    mWritePos += sizeof(Int16);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putUInt16(UInt16 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(UInt16))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt16));
                mWritePos += sizeof(UInt16);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(UInt16))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt16));
                    mWritePos += sizeof(UInt16);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putInt32(Int32 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(Int32))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int32));
                mWritePos += sizeof(Int32);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(Int32))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int32));
                    mWritePos += sizeof(Int32);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putUInt32(UInt32 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(UInt32))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt32));
                mWritePos += sizeof(UInt32);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(UInt32))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt32));
                    mWritePos += sizeof(UInt32);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putInt64(Int64 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(Int64))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int64));
                mWritePos += sizeof(Int64);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(Int64))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(Int64));
                    mWritePos += sizeof(Int64);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putUInt64(UInt64 value, bool bigEndian = true)
        {
            byte[] v = BitConverter.GetBytes(value);
            if (bigEndian)
            {
                Array.Reverse(v);
            }

            if (availableLength() >= sizeof(UInt64))
            {
                Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt64));
                mWritePos += sizeof(UInt64);
            }
            else
            {
                shrink();
                if (availableLength() >= sizeof(UInt64))
                {
                    Buffer.BlockCopy(v, 0, mBuffer, mWritePos, sizeof(UInt64));
                    mWritePos += sizeof(UInt64);
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        public void putBytes(byte[] value, int length)
        {
            
            if (availableLength() >= length)
            {
                Buffer.BlockCopy(value, 0, mBuffer, mWritePos, length);
                mWritePos += length;
            }
            else
            {
                shrink();
                if (availableLength() >= length)
                {
                    Buffer.BlockCopy(value, 0, mBuffer, mWritePos, length);
                    mWritePos += length;
                }
                else
                {
                    //TODO increase size of mBuffer
                }
            }
        }

        //Read
        public bool getByte(ref byte value)
        {
            if (mReadPos < mWritePos)
            {
                value = mBuffer[mReadPos];
                mReadPos++;
                return true;
            }
            return false;
        }

        public bool getInt16(ref Int16 value, bool bigEndian = true)
        {
            if(mReadPos <= (mWritePos - sizeof(Int16)))
            {
                byte[] v = new byte[sizeof(Int16)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(Int16));
                if(bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToInt16(v, 0);
                mReadPos += sizeof(Int16);
                return true;
            }
            return false;
        }

        public bool getUInt16(ref UInt16 value, bool bigEndian = true)
        {
            if (mReadPos <= (mWritePos - sizeof(UInt16)))
            {
                byte[] v = new byte[sizeof(UInt16)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(UInt16));
                if (bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToUInt16(v, 0);
                mReadPos += sizeof(UInt16);
                return true;
            }
            return false;
        }

        public bool getInt32(ref Int32 value, bool bigEndian = true)
        {
            if (mReadPos <= (mWritePos - sizeof(Int32)))
            {
                byte[] v = new byte[sizeof(Int32)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(Int32));
                if (bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToInt32(v, 0);
                mReadPos += sizeof(Int32);
                return true;
            }
            return false;
        }

        public bool getUInt32(ref UInt32 value, bool bigEndian = true)
        {
            if (mReadPos <= (mWritePos - sizeof(UInt32)))
            {
                byte[] v = new byte[sizeof(UInt32)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(UInt32));
                if (bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToUInt32(v, 0);
                mReadPos += sizeof(UInt32);
                return true;
            }
            return false;
        }

        public bool getInt64(ref Int64 value, bool bigEndian = true)
        {
            if (mReadPos <= (mWritePos - sizeof(Int64)))
            {
                byte[] v = new byte[sizeof(Int64)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(Int64));
                if (bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToInt64(v, 0);
                mReadPos += sizeof(Int64);
                return true;
            }
            return false;
        }

        public bool getUInt64(ref UInt64 value, bool bigEndian = true)
        {
            if (mReadPos <= (mWritePos - sizeof(UInt64)))
            {
                byte[] v = new byte[sizeof(UInt64)];
                Buffer.BlockCopy(mBuffer, mReadPos, v, 0, sizeof(UInt64));
                if (bigEndian)
                {
                    Array.Reverse(v);
                }
                value = BitConverter.ToUInt64(v, 0);
                mReadPos += sizeof(UInt64);
                return true;
            }
            return false;
        }

        public bool getBytes(ref Byte[] value, int length)
        {
            if (mReadPos <= (mWritePos - length))
            {
                Buffer.BlockCopy(mBuffer, mReadPos, value, 0, length);
                mReadPos += length;
                return true;
            }
            return false;
        }

        public void rewindRead(int numberByte)
        {
            if(numberByte <= 0)
            {
                return;
            }

            if(numberByte >= mReadPos)
            {
                mReadPos = 0;
            } else
            {
                mReadPos -= numberByte;
            }
        }

        public void clear() {
            mReadPos = 0;
            mWritePos = 0;
            Array.Clear(mBuffer, 0, mBuffer.Length);
        }
         
        private void setReadPosition(int readPos)
        {
            if(readPos > mLength)
            {

            } else
            {
                mReadPos = readPos;
            }
        }

        private int availableLength()
        {
            return mLength - mWritePos;
        }

        private void shrink() {
            for(int i = 0; i < (mWritePos - mReadPos); i++)
            {
                mBuffer[i] = mBuffer[mReadPos + i];
            }
            mWritePos -= mReadPos;
            mReadPos = 0;            
        }
        
    }
}
