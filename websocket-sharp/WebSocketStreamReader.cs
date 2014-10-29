namespace WebSocketSharp
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;

	internal class WebSocketStreamReader
	{
		private readonly Stream _innerStream;
		private readonly ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);
		private bool _isReading = false;

		public WebSocketStreamReader(Stream innerStream)
		{
			_innerStream = innerStream;
		}

		public IEnumerable<WebSocketMessage> Read()
		{
			lock (_innerStream)
			{
				if (_isReading)
				{
					yield break;
				}

				_isReading = true;
			}

			var closed = false;
			while (!closed)
			{
				_waitHandle.Reset();
				var header = ReadHeader();
				if (header == null)
				{
					yield break;
				}

				var readInfo = GetStreamReadInfo(header);

				var msg = CreateMessage(header, readInfo, _waitHandle);
				if (msg.Code == Opcode.Close)
				{
					closed = true;
				}

				yield return msg;

				_waitHandle.Wait();
			}
		}

		private WebSocketMessage CreateMessage(WebSocketFrameHeader header, StreamReadInfo readInfo, ManualResetEventSlim waitHandle)
		{
			switch (header.Opcode)
			{
				case Opcode.Cont:
					throw new WebSocketException(CloseStatusCode.InconsistentData, "Did not expect continuation frame.");
				case Opcode.Binary:
					return new FragmentedMessage(header.Opcode, _innerStream, readInfo, GetStreamReadInfo, waitHandle);
				case Opcode.Close:
					var msg = new SimpleMessage(Opcode.Close, waitHandle);
					msg.Consume();
					return msg;
				default:
					msg = new SimpleMessage(header.Opcode, waitHandle);
					msg.Consume();
					return msg;
			}
		}

		private StreamReadInfo GetStreamReadInfo()
		{
			var h = ReadHeader();
			return GetStreamReadInfo(h);
		}

		private StreamReadInfo GetStreamReadInfo(WebSocketFrameHeader header)
		{
			/* Extended Payload Length */

			var size = header.PayloadLength < 126 ? 0 : header.PayloadLength == 126 ? 2 : 8;

			var extPayloadLen = size > 0 ? _innerStream.ReadBytes(size) : new byte[0];
			if (size > 0 && extPayloadLen.Length != size)
			{
				throw new WebSocketException("The 'Extended Payload Length' of a frame cannot be read from the data source.");
			}

			//frame._extPayloadLength = extPayloadLen;

			/* Masking Key */

			var masked = header.Mask == Mask.Mask;
			var maskingKey = masked ? _innerStream.ReadBytes(4) : new byte[0];
			if (masked && maskingKey.Length != 4)
			{
				throw new WebSocketException("The 'Masking Key' of a frame cannot be read from the data source.");
			}

			//frame._maskingKey = maskingKey;

			/* Payload Data */

			ulong len = header.PayloadLength < 126
							? header.PayloadLength
							: header.PayloadLength == 126
								  ? extPayloadLen.ToUInt16(ByteOrder.Big)
								  : extPayloadLen.ToUInt64(ByteOrder.Big);

			return new StreamReadInfo(header.Fin == Fin.Final, len, maskingKey);
		}

		private WebSocketFrameHeader ReadHeader()
		{
			var header = new byte[2];
			var headerLength = _innerStream.Read(header, 0, 2);
			if (headerLength == 0)
			{
				return null;
			}

			if (headerLength != 2)
			{
				throw new WebSocketException("The header part of a frame cannot be read from the data source.");
			}

			var frameHeader = new WebSocketFrameHeader(header);
			var validation = WebSocketFrameHeader.Validate(frameHeader);

			if (validation != null)
			{
				throw new WebSocketException(CloseStatusCode.ProtocolError, validation);
			}

			return frameHeader;
		}
	}
}