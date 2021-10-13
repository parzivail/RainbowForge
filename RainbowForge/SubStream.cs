using System;
using System.IO;

namespace RainbowForge
{
	public class SubStream : Stream, IDisposable
	{
		private readonly Stream _source;
		private readonly int _length;
		private readonly long _minPos;
		private readonly long _maxPos;

		public SubStream(Stream source, long offset, int length)
		{
			_source = source;
			_minPos = offset;
			_length = length;
			_maxPos = offset + length;

			if (offset + length > source.Length)
				throw new EndOfStreamException();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_source?.Dispose();
			}

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		public override void Flush()
		{
			_source.Flush();
		}

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			return _source.Read(buffer, offset, Math.Max(Math.Min((int)(_maxPos - _source.Position), count), 0));
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin)
		{
			return _source.Seek(offset + _minPos, origin);
		}

		/// <inheritdoc />
		public override void SetLength(long value) => throw new NotSupportedException();

		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		/// <inheritdoc />
		public override bool CanRead => _source.CanRead;

		/// <inheritdoc />
		public override bool CanSeek => _source.CanSeek;

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override long Length => _length;

		/// <inheritdoc />
		public override long Position
		{
			get => _source.Position - _minPos;
			set => _source.Position = _minPos + value;
		}
	}
}