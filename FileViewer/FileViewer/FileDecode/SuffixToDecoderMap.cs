using System.Collections.Generic;
using FileViewer.Decoders;

namespace FileViewer.FileDecode
{
   /// <summary>
   /// 后缀和文件解码器映射类
   /// 功能：建立后缀与特定文件解码器的映射关系
   /// </summary>
    class SuffixToFileDecoderMap
    {
        /// <summary>
        /// 后缀与解码器的映射关系
        /// </summary>
        private readonly Dictionary<string, IFileDecoder> _suffixToDecoderDic = new Dictionary<string, IFileDecoder>();

        public SuffixToFileDecoderMap(FileDecoderCollection manager)
        {       
            MapPathToFile(".mp3|.wma|.ape|.flac|.aac|.ac3|.mmf|.amr|.m4a|.m4r|.ogg|.wav|.mp2", manager.AudioFile);
            MapPathToFile(".bin", manager.BinaryFile);
            MapPathToFile(".html|.Xml", manager.HtmlFile);
            MapPathToFile(".jpg|.png|.ico|.bmp|.tif|.tga|.gif", manager.PictureFile);
            MapPathToFile(".txt|.ini", manager.TextFile);
            MapPathToFile(".avi|.rmvb|.rm|.mp4|.mkv|.webM|.3gp|.WMV|.MPG|.vob|.mov|.flv|.swf", manager.VideoFile);
        }

        private void MapPathToFile(string suffixExp,IFileDecoder file)
        {
            string[] suffixes = suffixExp.Split('|');
            foreach (var suffix in suffixes)
            {
                string lowerSuffix = suffix.ToLower();
                _suffixToDecoderDic.Add(lowerSuffix, file);
            }
        }

        /// <summary>
        /// 根据后缀返回一个文件解码器
        /// </summary>
        /// <returns></returns>
        public IFileDecoder GetDecoder(string suffix)
        {
            string lowerSuffix = suffix.ToLower();
            if (_suffixToDecoderDic.ContainsKey(lowerSuffix))
            {
                return _suffixToDecoderDic[suffix];
            }
            return null;
        }
    }    
}
