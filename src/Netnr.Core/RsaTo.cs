using System;
using System.Text;
using System.Security.Cryptography;

namespace Netnr.Core
{
    /// <summary> 
    /// RSA加密解密及RSA签名和验证
    /// </summary> 
    public class RsaTo
    {
        #region RSA 加密解密 

        /// <summary>
        /// RSA 的密钥产生 产生私钥 和公钥 
        /// </summary>
        /// <param name="xmlKeys"></param>
        /// <param name="xmlPublicKey"></param>
        public void RSAKey(out string xmlKeys, out string xmlPublicKey)
        {
            using RSACryptoServiceProvider rsa = new();
            xmlKeys = rsa.ToXmlString(true);
            xmlPublicKey = rsa.ToXmlString(false);
        }

        #region RSA的加密函数 

        /// <summary>
        /// RSA的加密函数  string
        /// </summary>
        /// <param name="xmlPublicKey"></param>
        /// <param name="m_strEncryptString"></param>
        /// <returns></returns>
        public string RSAEncrypt(string xmlPublicKey, string m_strEncryptString)
        {
            byte[] PlainTextBArray;
            byte[] CypherTextBArray;
            string Result;
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(xmlPublicKey);
            PlainTextBArray = (new UnicodeEncoding()).GetBytes(m_strEncryptString);
            CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
            Result = Convert.ToBase64String(CypherTextBArray);
            return Result;
        }

        /// <summary>
        /// RSA的加密函数 byte[]
        /// </summary>
        /// <param name="xmlPublicKey"></param>
        /// <param name="EncryptString"></param>
        /// <returns></returns>
        public string RSAEncrypt(string xmlPublicKey, byte[] EncryptString)
        {
            byte[] CypherTextBArray;
            string Result;
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(xmlPublicKey);
            CypherTextBArray = rsa.Encrypt(EncryptString, false);
            Result = Convert.ToBase64String(CypherTextBArray);
            return Result;
        }
        #endregion

        #region RSA的解密函数 

        /// <summary>
        /// RSA的解密函数  string
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="m_strDecryptString"></param>
        /// <returns></returns>
        public string RSADecrypt(string xmlPrivateKey, string m_strDecryptString)
        {
            byte[] PlainTextBArray;
            byte[] DypherTextBArray;
            string Result;
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(xmlPrivateKey);
            PlainTextBArray = Convert.FromBase64String(m_strDecryptString);
            DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
            Result = (new UnicodeEncoding()).GetString(DypherTextBArray);
            return Result;
        }

        /// <summary>
        /// RSA的解密函数  byte
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="DecryptString"></param>
        /// <returns></returns>
        public string RSADecrypt(string xmlPrivateKey, byte[] DecryptString)
        {
            byte[] DypherTextBArray;
            string Result;
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(xmlPrivateKey);
            DypherTextBArray = rsa.Decrypt(DecryptString, false);
            Result = (new UnicodeEncoding()).GetString(DypherTextBArray);
            return Result;
        }
        #endregion

        #endregion

        #region RSA数字签名 
        #region 获取Hash描述表 

        /// <summary>
        /// 获取Hash描述表 
        /// </summary>
        /// <param name="m_strSource"></param>
        /// <param name="HashData"></param>
        /// <returns></returns>
        public bool GetHash(string m_strSource, ref byte[] HashData)
        {
            //从字符串中取得Hash描述 
            byte[] Buffer;
            using HashAlgorithm MD5 = HashAlgorithm.Create("MD5");
            Buffer = Encoding.GetEncoding("GB2312").GetBytes(m_strSource);
            HashData = MD5.ComputeHash(Buffer);

            return true;
        }

        /// <summary>
        /// 获取Hash描述表 
        /// </summary>
        /// <param name="m_strSource"></param>
        /// <param name="strHashData"></param>
        /// <returns></returns>
        public bool GetHash(string m_strSource, ref string strHashData)
        {
            //从字符串中取得Hash描述 
            byte[] Buffer;
            byte[] HashData;
            using HashAlgorithm MD5 = HashAlgorithm.Create("MD5");
            Buffer = Encoding.GetEncoding("GB2312").GetBytes(m_strSource);
            HashData = MD5.ComputeHash(Buffer);

            strHashData = Convert.ToBase64String(HashData);
            return true;
        }

        /// <summary>
        /// 获取Hash描述表 
        /// </summary>
        /// <param name="objFile"></param>
        /// <param name="HashData"></param>
        /// <returns></returns>
        public bool GetHash(System.IO.FileStream objFile, ref byte[] HashData)
        {
            //从文件中取得Hash描述 
            using HashAlgorithm MD5 = HashAlgorithm.Create("MD5");
            HashData = MD5.ComputeHash(objFile);
            objFile.Close();

            return true;
        }

        /// <summary>
        /// 获取Hash描述表
        /// </summary>
        /// <param name="objFile"></param>
        /// <param name="strHashData"></param>
        /// <returns></returns>
        public bool GetHash(System.IO.FileStream objFile, ref string strHashData)
        {
            //从文件中取得Hash描述 
            byte[] HashData;
            using HashAlgorithm MD5 = HashAlgorithm.Create("MD5");
            HashData = MD5.ComputeHash(objFile);
            objFile.Close();

            strHashData = Convert.ToBase64String(HashData);

            return true;
        }
        #endregion

        #region RSA签名 
        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="p_strKeyPrivate"></param>
        /// <param name="HashbyteSignature"></param>
        /// <param name="EncryptedSignatureData"></param>
        /// <returns></returns>
        public bool SignatureFormatter(string p_strKeyPrivate, byte[] HashbyteSignature, ref byte[] EncryptedSignatureData)
        {
            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPrivate);
            RSAPKCS1SignatureFormatter RSAFormatter = new(RSA);
            //设置签名的算法为MD5 
            RSAFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

            return true;
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="p_strKeyPrivate"></param>
        /// <param name="HashbyteSignature"></param>
        /// <param name="m_strEncryptedSignatureData"></param>
        /// <returns></returns>
        public bool SignatureFormatter(string p_strKeyPrivate, byte[] HashbyteSignature, ref string m_strEncryptedSignatureData)
        {
            byte[] EncryptedSignatureData;

            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPrivate);
            RSAPKCS1SignatureFormatter RSAFormatter = new(RSA);
            //设置签名的算法为MD5 
            RSAFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

            m_strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);

            return true;
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="p_strKeyPrivate"></param>
        /// <param name="m_strHashbyteSignature"></param>
        /// <param name="EncryptedSignatureData"></param>
        /// <returns></returns>
        public bool SignatureFormatter(string p_strKeyPrivate, string m_strHashbyteSignature, ref byte[] EncryptedSignatureData)
        {
            byte[] HashbyteSignature;

            HashbyteSignature = Convert.FromBase64String(m_strHashbyteSignature);
            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPrivate);
            RSAPKCS1SignatureFormatter RSAFormatter = new(RSA);
            //设置签名的算法为MD5 
            RSAFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

            return true;
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="p_strKeyPrivate"></param>
        /// <param name="m_strHashbyteSignature"></param>
        /// <param name="m_strEncryptedSignatureData"></param>
        /// <returns></returns>
        public bool SignatureFormatter(string p_strKeyPrivate, string m_strHashbyteSignature, ref string m_strEncryptedSignatureData)
        {
            byte[] HashbyteSignature;
            byte[] EncryptedSignatureData;

            HashbyteSignature = Convert.FromBase64String(m_strHashbyteSignature);
            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPrivate);
            RSAPKCS1SignatureFormatter RSAFormatter = new(RSA);
            //设置签名的算法为MD5 
            RSAFormatter.SetHashAlgorithm("MD5");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

            m_strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);

            return true;
        }
        #endregion

        #region RSA 签名验证
        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="p_strKeyPublic"></param>
        /// <param name="HashbyteDeformatter"></param>
        /// <param name="DeformatterData"></param>
        /// <returns></returns>
        public bool SignatureDeformatter(string p_strKeyPublic, byte[] HashbyteDeformatter, byte[] DeformatterData)
        {
            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPublic);
            RSAPKCS1SignatureDeformatter RSADeformatter = new(RSA);
            //指定解密的时候HASH算法为MD5 
            RSADeformatter.SetHashAlgorithm("MD5");

            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="p_strKeyPublic"></param>
        /// <param name="p_strHashbyteDeformatter"></param>
        /// <param name="DeformatterData"></param>
        /// <returns></returns>
        public bool SignatureDeformatter(string p_strKeyPublic, string p_strHashbyteDeformatter, byte[] DeformatterData)
        {
            byte[] HashbyteDeformatter;

            HashbyteDeformatter = Convert.FromBase64String(p_strHashbyteDeformatter);

            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPublic);
            RSAPKCS1SignatureDeformatter RSADeformatter = new(RSA);
            //指定解密的时候HASH算法为MD5 
            RSADeformatter.SetHashAlgorithm("MD5");

            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="p_strKeyPublic"></param>
        /// <param name="HashbyteDeformatter"></param>
        /// <param name="p_strDeformatterData"></param>
        /// <returns></returns>
        public bool SignatureDeformatter(string p_strKeyPublic, byte[] HashbyteDeformatter, string p_strDeformatterData)
        {
            byte[] DeformatterData;

            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPublic);
            RSAPKCS1SignatureDeformatter RSADeformatter = new(RSA);
            //指定解密的时候HASH算法为MD5 
            RSADeformatter.SetHashAlgorithm("MD5");

            DeformatterData = Convert.FromBase64String(p_strDeformatterData);

            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// RSA 签名验证
        /// </summary>
        /// <param name="p_strKeyPublic"></param>
        /// <param name="p_strHashbyteDeformatter"></param>
        /// <param name="p_strDeformatterData"></param>
        /// <returns></returns>
        public bool SignatureDeformatter(string p_strKeyPublic, string p_strHashbyteDeformatter, string p_strDeformatterData)
        {
            byte[] DeformatterData;
            byte[] HashbyteDeformatter;

            HashbyteDeformatter = Convert.FromBase64String(p_strHashbyteDeformatter);
            RSACryptoServiceProvider RSA = new();

            RSA.FromXmlString(p_strKeyPublic);
            RSAPKCS1SignatureDeformatter RSADeformatter = new(RSA);
            //指定解密的时候HASH算法为MD5 
            RSADeformatter.SetHashAlgorithm("MD5");

            DeformatterData = Convert.FromBase64String(p_strDeformatterData);

            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #endregion
    }
}
