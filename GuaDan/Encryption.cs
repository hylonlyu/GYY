using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GuaDan
{
    public class Encryption
    {
        private ICryptoTransform decryptor;
        private static string DEFAULT_HASH_ALGORITHM = "SHA1";
        private static int DEFAULT_KEY_SIZE = 0x100;
        private static int DEFAULT_MAX_SALT_LEN = 8;
        private static int DEFAULT_MIN_SALT_LEN = MIN_ALLOWED_SALT_LEN;
        private ICryptoTransform encryptor;
        private static int MAX_ALLOWED_SALT_LEN = 0xff;
        private int maxSaltLen;
        private static int MIN_ALLOWED_SALT_LEN = 4;
        private int minSaltLen;

        public Encryption(string passPhrase)
            : this(passPhrase, null)
        {
        }

        public Encryption(string passPhrase, string initVector)
            : this(passPhrase, initVector, -1)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen)
            : this(passPhrase, initVector, minSaltLen, -1)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen, int maxSaltLen)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue)
            : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1)
        {
        }

        public Encryption(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue, int passwordIterations)
        {
            this.minSaltLen = -1;
            this.maxSaltLen = -1;
            this.encryptor = null;
            this.decryptor = null;
            if (minSaltLen < MIN_ALLOWED_SALT_LEN)
            {
                this.minSaltLen = DEFAULT_MIN_SALT_LEN;
            }
            else
            {
                this.minSaltLen = minSaltLen;
            }
            if ((maxSaltLen < 0) || (maxSaltLen > MAX_ALLOWED_SALT_LEN))
            {
                this.maxSaltLen = DEFAULT_MAX_SALT_LEN;
            }
            else
            {
                this.maxSaltLen = maxSaltLen;
            }
            if (keySize <= 0)
            {
                keySize = DEFAULT_KEY_SIZE;
            }
            if (hashAlgorithm == null)
            {
                hashAlgorithm = DEFAULT_HASH_ALGORITHM;
            }
            else
            {
                hashAlgorithm = hashAlgorithm.ToUpper().Replace("-", "");
            }
            byte[] rgbIV = null;
            byte[] rgbSalt = null;
            if (initVector == null)
            {
                rgbIV = new byte[0];
            }
            else
            {
                rgbIV = Encoding.ASCII.GetBytes(initVector);
            }
            if (saltValue == null)
            {
                rgbSalt = new byte[0];
            }
            else
            {
                rgbSalt = Encoding.ASCII.GetBytes(saltValue);
            }
            byte[] bytes = new PasswordDeriveBytes(passPhrase, rgbSalt, hashAlgorithm, passwordIterations).GetBytes(keySize / 8);
            RijndaelManaged managed = new RijndaelManaged();
            if (rgbIV.Length == 0)
            {
                managed.Mode = CipherMode.ECB;
            }
            else
            {
                managed.Mode = CipherMode.CBC;
            }
            this.encryptor = managed.CreateEncryptor(bytes, rgbIV);
            this.decryptor = managed.CreateDecryptor(bytes, rgbIV);
        }

        private byte[] AddSalt(byte[] plainTextBytes)
        {
            if ((this.maxSaltLen == 0) || (this.maxSaltLen < this.minSaltLen))
            {
                return plainTextBytes;
            }
            byte[] sourceArray = this.GenerateSalt();
            byte[] destinationArray = new byte[plainTextBytes.Length + sourceArray.Length];
            Array.Copy(sourceArray, destinationArray, sourceArray.Length);
            Array.Copy(plainTextBytes, 0, destinationArray, sourceArray.Length, plainTextBytes.Length);
            return destinationArray;
        }

        public static string AESDecrypt(string EncryptStr, string sKey, string sIV)
        {
            try
            {
                string str = new Encryption(sKey, sIV).Decrypt(EncryptStr);
                return str;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }

        public static string AESEncrypt(string srcStr, string sKey, string sIV)
        {
            try
            {
                string str = new Encryption(sKey, sIV).Encrypt(srcStr);
                return str;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }

        public string Decrypt(byte[] cipherTextBytes)
        {
            return Encoding.UTF8.GetString(this.DecryptToBytes(cipherTextBytes));
        }

        public string Decrypt(string cipherText)
        {
            return this.Decrypt(Convert.FromBase64String(cipherText));
        }

        public byte[] DecryptToBytes(string cipherText)
        {
            return this.DecryptToBytes(Convert.FromBase64String(cipherText));
        }

        public byte[] DecryptToBytes(byte[] cipherTextBytes)
        {
            byte[] buffer = null;
            byte[] destinationArray = null;
            int num = 0;
            int sourceIndex = 0;
            MemoryStream stream = new MemoryStream(cipherTextBytes);
            buffer = new byte[cipherTextBytes.Length];
            lock (this)
            {
                CryptoStream stream2 = new CryptoStream(stream, this.decryptor, CryptoStreamMode.Read);
                num = stream2.Read(buffer, 0, buffer.Length);
                stream.Close();
                stream2.Close();
            }
            if ((this.maxSaltLen > 0) && (this.maxSaltLen >= this.minSaltLen))
            {
                sourceIndex = (((buffer[0] & 3) | (buffer[1] & 12)) | (buffer[2] & 0x30)) | (buffer[3] & 0xc0);
            }
            destinationArray = new byte[num - sourceIndex];
            Array.Copy(buffer, sourceIndex, destinationArray, 0, num - sourceIndex);
            return destinationArray;
        }

        public string Encrypt(byte[] plainTextBytes)
        {
            return Convert.ToBase64String(this.EncryptToBytes(plainTextBytes));
        }

        public string Encrypt(string plainText)
        {
            return this.Encrypt(Encoding.UTF8.GetBytes(plainText));
        }

        public byte[] EncryptToBytes(string plainText)
        {
            return this.EncryptToBytes(Encoding.UTF8.GetBytes(plainText));
        }

        public byte[] EncryptToBytes(byte[] plainTextBytes)
        {
            byte[] buffer = this.AddSalt(plainTextBytes);
            MemoryStream stream = new MemoryStream();
            lock (this)
            {
                CryptoStream stream2 = new CryptoStream(stream, this.encryptor, CryptoStreamMode.Write);
                stream2.Write(buffer, 0, buffer.Length);
                stream2.FlushFinalBlock();
                byte[] buffer2 = stream.ToArray();
                stream.Close();
                stream2.Close();
                return buffer2;
            }
        }

        private int GenerateRandomNumber(int minValue, int maxValue)
        {
            byte[] data = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(data);
            int seed = ((((data[0] & 0x7f) << 0x18) | (data[1] << 0x10)) | (data[2] << 8)) | data[3];
            Random random = new Random(seed);
            return random.Next(minValue, maxValue + 1);
        }

        private byte[] GenerateSalt()
        {
            int minSaltLen = 0;
            if (this.minSaltLen == this.maxSaltLen)
            {
                minSaltLen = this.minSaltLen;
            }
            else
            {
                minSaltLen = this.GenerateRandomNumber(this.minSaltLen, this.maxSaltLen);
            }
            byte[] data = new byte[minSaltLen];
            new RNGCryptoServiceProvider().GetNonZeroBytes(data);
            data[0] = (byte)((data[0] & 0xfc) | (minSaltLen & 3));
            data[1] = (byte)((data[1] & 0xf3) | (minSaltLen & 12));
            data[2] = (byte)((data[2] & 0xcf) | (minSaltLen & 0x30));
            data[3] = (byte)((data[3] & 0x3f) | (minSaltLen & 0xc0));
            return data;
        }

        public static string MD5(string src)
        {
            System.Security.Cryptography.MD5 md = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.Default.GetBytes(src);
            return BitConverter.ToString(md.ComputeHash(bytes)).Replace("-", "");
        }

        public static string SHA1(string src)
        {
            System.Security.Cryptography.SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] bytes = Encoding.Default.GetBytes(src);
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        /// <summary>
        /// DES 加密(数据加密标准，速度较快，适用于加密大量数据的场合)
        /// </summary>
        /// <param name="EncryptString">待加密的密文</param>
        /// <param name="EncryptKey">加密的密钥</param>
        /// <returns>returns</returns>
        public static string DESEncrypt(string EncryptString, string EncryptKey)
        {
            if (string.IsNullOrEmpty(EncryptString)) { throw (new Exception("密文不得为空")); }

            if (string.IsNullOrEmpty(EncryptKey)) { throw (new Exception("密钥不得为空")); }

            if (EncryptKey.Length != 8) { throw (new Exception("密钥必须为8位")); }

            byte[] m_btIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            string m_strEncrypt = "";

            DESCryptoServiceProvider m_DESProvider = new DESCryptoServiceProvider();

            try
            {
                byte[] m_btEncryptString = Encoding.Default.GetBytes(EncryptString);

                MemoryStream m_stream = new MemoryStream();

                CryptoStream m_cstream = new CryptoStream(m_stream, m_DESProvider.CreateEncryptor(Encoding.Default.GetBytes(EncryptKey), m_btIV), CryptoStreamMode.Write);

                m_cstream.Write(m_btEncryptString, 0, m_btEncryptString.Length);

                m_cstream.FlushFinalBlock();

                m_strEncrypt = Convert.ToBase64String(m_stream.ToArray());

                m_stream.Close(); m_stream.Dispose();

                m_cstream.Close(); m_cstream.Dispose();
            }
            catch (IOException ex) { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (ArgumentException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { m_DESProvider.Clear(); }

            return m_strEncrypt;
        }


        /// <summary>
        /// DES 解密(数据加密标准，速度较快，适用于加密大量数据的场合)
        /// </summary>
        /// <param name="DecryptString">待解密的密文</param>
        /// <param name="DecryptKey">解密的密钥</param>
        /// <returns>returns</returns>
        public static string DESDecrypt(string DecryptString, string DecryptKey)
        {
            if (string.IsNullOrEmpty(DecryptString)) { throw (new Exception("密文不得为空")); }

            if (string.IsNullOrEmpty(DecryptKey)) { throw (new Exception("密钥不得为空")); }

            if (DecryptKey.Length != 8) { throw (new Exception("密钥必须为8位")); }

            byte[] m_btIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

            string m_strDecrypt = "";

            DESCryptoServiceProvider m_DESProvider = new DESCryptoServiceProvider();

            try
            {
                byte[] m_btDecryptString = Convert.FromBase64String(DecryptString);

                MemoryStream m_stream = new MemoryStream();

                CryptoStream m_cstream = new CryptoStream(m_stream, m_DESProvider.CreateDecryptor(Encoding.Default.GetBytes(DecryptKey), m_btIV), CryptoStreamMode.Write);

                m_cstream.Write(m_btDecryptString, 0, m_btDecryptString.Length);

                m_cstream.FlushFinalBlock();

                m_strDecrypt = Encoding.Default.GetString(m_stream.ToArray());

                m_stream.Close(); m_stream.Dispose();

                m_cstream.Close(); m_cstream.Dispose();
            }
            catch (IOException ex) { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (ArgumentException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { m_DESProvider.Clear(); }

            return m_strDecrypt;
        }
    }
}
