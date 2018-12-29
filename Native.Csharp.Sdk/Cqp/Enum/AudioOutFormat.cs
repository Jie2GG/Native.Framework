using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Native.Csharp.Sdk.Cqp.Enum
{
	/// <summary>
	/// 指示酷Q接收语音时的转换格式
	/// </summary>
	public enum AudioOutFormat
	{
		/// <summary>
		/// mp3音频格式
		/// </summary>
		MPEG_Layer3,
		/// <summary>
		/// arm音频格式
		/// </summary>
		AMR_NB,
		/// <summary>
		/// wma音频格式
		/// </summary>
		Windows_Media_Audio,
		/// <summary>
		/// m4a音频格式
		/// </summary>
		MPEG4,
		/// <summary>
		/// spx音频格式
		/// </summary>
		Speex,
		/// <summary>
		/// ogg音频格式
		/// </summary>
		OggVorbis,
		/// <summary>
		/// wav音频格式
		/// </summary>
		WAVE,
		/// <summary>
		/// flac音频格式
		/// </summary>
		FLAC
	}
}
