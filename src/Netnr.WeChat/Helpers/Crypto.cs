using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace Netnr.WeChat.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class Crypto
    {
        /// <summary>
        /// 
        /// </summary>
        public class Cryptography
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="inval"></param>
            /// <returns></returns>
            public static uint HostToNetworkOrder(uint inval)
            {
                uint outval = 0;
                for (int i = 0; i < 4; i++)
                    outval = (outval << 8) + ((inval >> (i * 8)) & 255);
                return outval;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inval"></param>
            /// <returns></returns>
            public static int HostToNetworkOrder(int inval)
            {
                int outval = 0;
                for (int i = 0; i < 4; i++)
                    outval = (outval << 8) + ((inval >> (i * 8)) & 255);
                return outval;
            }
            /// <summary>
            /// 解密方法
            /// </summary>
            /// <param name="Input">密文</param>
            /// <param name="EncodingAESKey"></param>
            /// <param name="appid"></param>
            /// <returns></returns>
            public static string AES_decrypt(string Input, string EncodingAESKey, ref string appid)
            {
                byte[] Key;
                Key = Convert.FromBase64String(EncodingAESKey + "=");
                byte[] Iv = new byte[16];
                Array.Copy(Key, Iv, 16);
                byte[] btmpMsg = AES_decrypt(Input, Iv, Key);

                int len = BitConverter.ToInt32(btmpMsg, 16);
                len = IPAddress.NetworkToHostOrder(len);

                byte[] bMsg = new byte[len];
                byte[] bAppid = new byte[btmpMsg.Length - 20 - len];
                Array.Copy(btmpMsg, 20, bMsg, 0, len);
                Array.Copy(btmpMsg, 20 + len, bAppid, 0, btmpMsg.Length - 20 - len);
                string oriMsg = Encoding.UTF8.GetString(bMsg);
                appid = Encoding.UTF8.GetString(bAppid);

                return oriMsg;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Input"></param>
            /// <param name="EncodingAESKey"></param>
            /// <param name="appid"></param>
            /// <returns></returns>
            public static string AES_encrypt(string Input, string EncodingAESKey, string appid)
            {
                byte[] Key;
                Key = Convert.FromBase64String(EncodingAESKey + "=");
                byte[] Iv = new byte[16];
                Array.Copy(Key, Iv, 16);
                string Randcode = CreateRandCode(16);
                byte[] bRand = Encoding.UTF8.GetBytes(Randcode);
                byte[] bAppid = Encoding.UTF8.GetBytes(appid);
                byte[] btmpMsg = Encoding.UTF8.GetBytes(Input);
                byte[] bMsgLen = BitConverter.GetBytes(HostToNetworkOrder(btmpMsg.Length));
                byte[] bMsg = new byte[bRand.Length + bMsgLen.Length + bAppid.Length + btmpMsg.Length];

                Array.Copy(bRand, bMsg, bRand.Length);
                Array.Copy(bMsgLen, 0, bMsg, bRand.Length, bMsgLen.Length);
                Array.Copy(btmpMsg, 0, bMsg, bRand.Length + bMsgLen.Length, btmpMsg.Length);
                Array.Copy(bAppid, 0, bMsg, bRand.Length + bMsgLen.Length + btmpMsg.Length, bAppid.Length);

                return AES_encrypt(bMsg, Iv, Key);

            }
            private static string CreateRandCode(int codeLen)
            {
                string codeSerial = "2,3,4,5,6,7,a,c,d,e,f,h,i,j,k,m,n,p,r,s,t,A,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,U,V,W,X,Y,Z";
                if (codeLen == 0)
                {
                    codeLen = 16;
                }
                string[] arr = codeSerial.Split(',');
                string code = "";
                Random rand = new(unchecked((int)DateTime.Now.Ticks));
                for (int i = 0; i < codeLen; i++)
                {
                    int randValue = rand.Next(0, arr.Length - 1);
                    code += arr[randValue];
                }
                return code;
            }

            private static string AES_encrypt(string Input, byte[] Iv, byte[] Key)
            {
                var aes = new RijndaelManaged
                {
                    //秘钥的大小，以位为单位
                    KeySize = 256,
                    //支持的块大小
                    BlockSize = 128,
                    //填充模式
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC,
                    Key = Key,
                    IV = Iv
                };
                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(Input);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    xBuff = ms.ToArray();
                }
                string Output = Convert.ToBase64String(xBuff);
                return Output;
            }

            private static string AES_encrypt(byte[] Input, byte[] Iv, byte[] Key)
            {
                var aes = new RijndaelManaged
                {
                    //秘钥的大小，以位为单位
                    KeySize = 256,
                    //支持的块大小
                    BlockSize = 128,
                    //填充模式
                    //aes.Padding = PaddingMode.PKCS7;
                    Padding = PaddingMode.None,
                    Mode = CipherMode.CBC,
                    Key = Key,
                    IV = Iv
                };
                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] xBuff = null;

                #region 自己进行PKCS7补位，用系统自己带的不行
                byte[] msg = new byte[Input.Length + 32 - Input.Length % 32];
                Array.Copy(Input, msg, Input.Length);
                byte[] pad = KCS7Encoder(Input.Length);
                Array.Copy(pad, 0, msg, Input.Length, pad.Length);
                #endregion

                #region 注释的也是一种方法，效果一样
                //ICryptoTransform transform = aes.CreateEncryptor();
                //byte[] xBuff = transform.TransformFinalBlock(msg, 0, msg.Length);
                #endregion

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        cs.Write(msg, 0, msg.Length);
                    }
                    xBuff = ms.ToArray();
                }

                string Output = Convert.ToBase64String(xBuff);
                return Output;
            }

            private static byte[] KCS7Encoder(int text_length)
            {
                int block_size = 32;
                // 计算需要填充的位数
                int amount_to_pad = block_size - (text_length % block_size);
                if (amount_to_pad == 0)
                {
                    amount_to_pad = block_size;
                }
                // 获得补位所用的字符
                char pad_chr = chr(amount_to_pad);
                string tmp = "";
                for (int index = 0; index < amount_to_pad; index++)
                {
                    tmp += pad_chr;
                }
                return Encoding.UTF8.GetBytes(tmp);
            }

            /**
             * 将数字转化成ASCII码对应的字符，用于对明文进行补码
             * 
             * @param a 需要转化的数字
             * @return 转化得到的字符
             */
            static char chr(int a)
            {
                byte target = (byte)(a & 0xFF);
                return (char)target;
            }

            private static byte[] AES_decrypt(string Input, byte[] Iv, byte[] Key)
            {
                RijndaelManaged aes = new()
                {
                    KeySize = 256,
                    BlockSize = 128,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None,
                    Key = Key,
                    IV = Iv
                };
                var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] xBuff = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Convert.FromBase64String(Input);
                        byte[] msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                        Array.Copy(xXml, msg, xXml.Length);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    xBuff = decode2(ms.ToArray());
                }
                return xBuff;
            }

            private static byte[] decode2(byte[] decrypted)
            {
                int pad = (int)decrypted[decrypted.Length - 1];
                if (pad < 1 || pad > 32)
                {
                    pad = 0;
                }
                byte[] res = new byte[decrypted.Length - pad];
                Array.Copy(decrypted, 0, res, 0, decrypted.Length - pad);
                return res;
            }
        }


        /// <summary>
        /// -40001 ： 签名验证错误
        /// -40002 :  xml解析失败
        /// -40003 :  sha加密生成签名失败
        /// -40004 :  AESKey 非法
        /// -40005 :  appid 校验错误
        /// -40006 :  AES 加密失败
        /// -40007 ： AES 解密失败
        /// -40008 ： 解密后得到的buffer非法
        /// -40009 :  base64加密异常
        /// -40010 :  base64解密异常
        /// </summary>
        public class WXBizMsgCrypt
        {
            private readonly string m_sToken;
            private string m_sEncodingAESKey;
            private readonly string m_sAppID;

            private enum WXBizMsgCryptErrorCode
            {
                WXBizMsgCrypt_OK = 0,
                WXBizMsgCrypt_ValidateSignature_Error = -40001,
                WXBizMsgCrypt_ParseXml_Error = -40002,
                WXBizMsgCrypt_ComputeSignature_Error = -40003,
                WXBizMsgCrypt_IllegalAesKey = -40004,
                WXBizMsgCrypt_ValidateAppid_Error = -40005,
                WXBizMsgCrypt_EncryptAES_Error = -40006,
                WXBizMsgCrypt_DecryptAES_Error = -40007,
                WXBizMsgCrypt_IllegalBuffer = -40008,
                WXBizMsgCrypt_EncodeBase64_Error = -40009,
                WXBizMsgCrypt_DecodeBase64_Error = -40010
            };

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="sToken">公众平台上，开发者设置的Token</param>
            /// <param name="sEncodingAESKey">公众平台上，开发者设置的EncodingAESKey</param>
            /// <param name="sAppID">公众帐号的appid</param>
            public WXBizMsgCrypt(string sToken, string sEncodingAESKey, string sAppID)
            {
                m_sToken = sToken;
                m_sAppID = sAppID;
                m_sEncodingAESKey = sEncodingAESKey;
            }

            /// <summary>
            /// 检验消息的真实性，并且获取解密后的明文
            /// </summary>
            /// <param name="sMsgSignature">签名串，对应URL参数的msg_signature</param>
            /// <param name="sTimeStamp">时间戳，对应URL参数的timestamp</param>
            /// <param name="sNonce">随机串，对应URL参数的nonce</param>
            /// <param name="sPostData">密文，对应POST请求的数据</param>
            /// <param name="sMsg">解密后的原文，当return返回0时有效</param>
            /// <returns>成功0，失败返回对应的错误码</returns>
            public int DecryptMsg(string sMsgSignature, string sTimeStamp, string sNonce, string sPostData, ref string sMsg)
            {
                if (m_sEncodingAESKey.Length != 43)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_IllegalAesKey;
                }
                XmlDocument doc = new();
                XmlNode root;
                string sEncryptMsg;
                try
                {
                    doc.LoadXml(sPostData);
                    root = doc.FirstChild;
                    sEncryptMsg = root["Encrypt"].InnerText;
                }
                catch (Exception)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_ParseXml_Error;
                }
                //verify signature
                int ret = VerifySignature(m_sToken, sTimeStamp, sNonce, sEncryptMsg, sMsgSignature);
                if (ret != 0)
                    return ret;
                //decrypt
                string cpid = "";
                try
                {
                    sMsg = Cryptography.AES_decrypt(sEncryptMsg, m_sEncodingAESKey, ref cpid);
                }
                catch (FormatException)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_DecodeBase64_Error;
                }
                catch (Exception)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_DecryptAES_Error;
                }
                if (cpid != m_sAppID)
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_ValidateAppid_Error;
                return 0;
            }

            /// <summary>
            /// 将企业号回复用户的消息加密打包
            /// </summary>
            /// <param name="sReplyMsg">企业号待回复用户的消息，xml格式的字符串</param>
            /// <param name="sTimeStamp">时间戳，可以自己生成，也可以用URL参数的timestamp</param>
            /// <param name="sNonce">随机串，可以自己生成，也可以用URL参数的nonce</param>
            /// <param name="sEncryptMsg">加密后的可以直接回复用户的密文，包括msg_signature, timestamp, nonce, encrypt的xml格式的字符串,当return返回0时有效</param>
            /// <returns>成功0，失败返回对应的错误码</returns>
            public int EncryptMsg(string sReplyMsg, string sTimeStamp, string sNonce, ref string sEncryptMsg)
            {
                if (m_sEncodingAESKey.Length != 43)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_IllegalAesKey;
                }
                string raw;
                try
                {
                    raw = Cryptography.AES_encrypt(sReplyMsg, m_sEncodingAESKey, m_sAppID);
                }
                catch (Exception)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_EncryptAES_Error;
                }
                string MsgSigature = "";
                int ret = GenarateSinature(m_sToken, sTimeStamp, sNonce, raw, ref MsgSigature);
                if (0 != ret)
                    return ret;
                sEncryptMsg = "";

                string EncryptLabelHead = "<Encrypt><![CDATA[";
                string EncryptLabelTail = "]]></Encrypt>";
                string MsgSigLabelHead = "<MsgSignature><![CDATA[";
                string MsgSigLabelTail = "]]></MsgSignature>";
                string TimeStampLabelHead = "<TimeStamp><![CDATA[";
                string TimeStampLabelTail = "]]></TimeStamp>";
                string NonceLabelHead = "<Nonce><![CDATA[";
                string NonceLabelTail = "]]></Nonce>";
                sEncryptMsg = sEncryptMsg + "<xml>" + EncryptLabelHead + raw + EncryptLabelTail;
                sEncryptMsg = sEncryptMsg + MsgSigLabelHead + MsgSigature + MsgSigLabelTail;
                sEncryptMsg = sEncryptMsg + TimeStampLabelHead + sTimeStamp + TimeStampLabelTail;
                sEncryptMsg = sEncryptMsg + NonceLabelHead + sNonce + NonceLabelTail;
                sEncryptMsg += "</xml>";
                return 0;
            }

            /// <summary>
            /// 
            /// </summary>
            public class DictionarySort : IComparer
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="oLeft"></param>
                /// <param name="oRight"></param>
                /// <returns></returns>
                public int Compare(object oLeft, object oRight)
                {
                    string sLeft = oLeft as string;
                    string sRight = oRight as string;
                    int iLeftLength = sLeft.Length;
                    int iRightLength = sRight.Length;
                    int index = 0;
                    while (index < iLeftLength && index < iRightLength)
                    {
                        if (sLeft[index] < sRight[index])
                            return -1;
                        else if (sLeft[index] > sRight[index])
                            return 1;
                        else
                            index++;
                    }
                    return iLeftLength - iRightLength;

                }
            }

            //Verify Signature
            private static int VerifySignature(string sToken, string sTimeStamp, string sNonce, string sMsgEncrypt, string sSigture)
            {
                string hash = "";
                int ret = GenarateSinature(sToken, sTimeStamp, sNonce, sMsgEncrypt, ref hash);
                if (ret != 0)
                    return ret;
                //System.Console.WriteLine(hash);
                if (hash == sSigture)
                    return 0;
                else
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_ValidateSignature_Error;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sToken"></param>
            /// <param name="sTimeStamp"></param>
            /// <param name="sNonce"></param>
            /// <param name="sMsgEncrypt"></param>
            /// <param name="sMsgSignature"></param>
            /// <returns></returns>
            public static int GenarateSinature(string sToken, string sTimeStamp, string sNonce, string sMsgEncrypt, ref string sMsgSignature)
            {
                ArrayList AL = new()
                {
                sToken,
                sTimeStamp,
                sNonce,
                sMsgEncrypt
            };
                AL.Sort(new DictionarySort());
                string raw = "";
                for (int i = 0; i < AL.Count; ++i)
                {
                    raw += AL[i];
                }

                SHA1 sha;
                ASCIIEncoding enc;
                string hash;
                try
                {
                    sha = new SHA1CryptoServiceProvider();
                    enc = new ASCIIEncoding();
                    byte[] dataToHash = enc.GetBytes(raw);
                    byte[] dataHashed = sha.ComputeHash(dataToHash);
                    hash = BitConverter.ToString(dataHashed).Replace("-", "");
                    hash = hash.ToLower();
                }
                catch (Exception)
                {
                    return (int)WXBizMsgCryptErrorCode.WXBizMsgCrypt_ComputeSignature_Error;
                }
                sMsgSignature = hash;
                return 0;
            }
        }
    }
}
