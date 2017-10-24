using System;
using System.Text;

/* ==============================================================================
* Description：CstEncoding ,此用于包装系统默认的Encoding类，并且提供有限的另类编码方式，比如：把内容以HEX方式显示
 * ，并且提供了按名字生成编码后的String的方法
* Author     ：litao
* Create Date：2017/10/20 11:42:59
* ==============================================================================*/

namespace FileViewer.UserControls.LargeFileTextBox
{
    /// <summary>
    /// CstEncoding
    /// </summary>
    class CstEncoding
    {
        private readonly IdentifyEncoding _identifyEncoding = new IdentifyEncoding();

        public string GetString(string encodingName, byte[] bytes, int count)
        {
            if (string.IsNullOrWhiteSpace(encodingName) )
            {
                try
                {
                    string autoCheckName = _identifyEncoding.GetEncodingName(IdentifyEncoding.ToSByteArray(bytes));
                    string text = Encoding.GetEncoding(autoCheckName).GetString(bytes, 0, count);
                    return text;
                }
                catch (Exception)
                {
                    encodingName = "Hex";
                }
            }

            if (encodingName == "Hex")
            {
                string text = BytesToHexString(bytes, count);
                return text;
            }
            return string.Empty;
        }

        /// <summary>
        /// Byte数组转化为16进制字符串
        /// </summary>
        /// <returns></returns>
        private string BytesToHexString(byte[] bytes, int count)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (i != 0
                    && i % 4 == 0)
                {
                    stringBuilder.Append(" ");
                }
                stringBuilder.Append(Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }
    }
}
