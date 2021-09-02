using System;
using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Components
{
	public class CompressedLocalizationData
	{
		public ushort Pivot { get; }
		public LocalizationDictionaryEntry[] Dictionary { get; }
		public LocalizationPointer[] LocalizationPointers { get; }
		public ushort[] StringMetas { get; }
		public ushort[] StringLengths { get; }
		public List<byte[]> DictPointers { get; }
		public string[] Strings { get; }

		private CompressedLocalizationData(ushort pivot, LocalizationDictionaryEntry[] dictionary, LocalizationPointer[] localizationPointers, ushort[] stringMetas, ushort[] stringLengths,
			List<byte[]> dictPointers, string[] strings)
		{
			Pivot = pivot;
			Dictionary = dictionary;
			LocalizationPointers = localizationPointers;
			StringMetas = stringMetas;
			StringLengths = stringLengths;
			DictPointers = dictPointers;
			Strings = strings;
		}

		private static ushort ByteswapUint16(ushort value)
		{
			var b0 = value & 0x00FF;
			var b1 = value & 0xFF00;
			return (ushort)((b0 << 8) | (b1 >> 8));
		}

		private static uint ByteswapUint32(uint value)
		{
			var b0 = value & 0x000000FF;
			var b1 = value & 0x0000FF00;
			var b2 = value & 0x00FF0000;
			var b3 = value & 0xFF000000;
			return (b0 << 24) | (b1 << 8) | (b2 >> 8) | (b3 >> 24);
		}

		public static CompressedLocalizationData Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.CompressedLocalizationData, magic);

			var dataLength = r.ReadInt32();

			var pivot = ByteswapUint16(r.ReadUInt16());
			var numDictionaryEntries = ByteswapUint16(r.ReadUInt16());

			var dictionary = new LocalizationDictionaryEntry[numDictionaryEntries];
			for (var i = 0; i < numDictionaryEntries; i++)
			{
				var entryChar = (r.ReadUInt16());
				var entryNextIdx = (r.ReadUInt16());

				dictionary[i] = new LocalizationDictionaryEntry(entryChar, entryNextIdx);
			}

			var numLocalizations = ByteswapUint16(r.ReadUInt16());

			var localizationPointers = new LocalizationPointer[numLocalizations];
			for (var i = 0; i < numLocalizations; i++)
			{
				var localizationId = ByteswapUint32(r.ReadUInt32());
				var stringDataPos = ByteswapUint32(r.ReadUInt32());
				var lengthDataPos = ByteswapUint32(r.ReadUInt32());

				localizationPointers[i] = new LocalizationPointer(localizationId, stringDataPos, lengthDataPos);
			}

			var numStrings = (localizationPointers[0].StringDataPos - localizationPointers[0].LengthDataPos) / 4;

			var stringMetas = new ushort[numStrings];
			var stringLengths = new ushort[numStrings];

			var lastStringEndPos = 0;
			for (var i = 0; i < numStrings; i++)
			{
				stringMetas[i] = ByteswapUint16(r.ReadUInt16());

				var stringEndPos = ByteswapUint16(r.ReadUInt16());

				if (stringEndPos < lastStringEndPos)
					lastStringEndPos = 0;

				stringLengths[i] = (ushort)(stringEndPos - lastStringEndPos);

				lastStringEndPos = stringEndPos;
			}

			var dictPointers = new List<byte[]>();

			for (var i = 0; i < numStrings; i++)
				dictPointers.Add(r.ReadBytes(stringLengths[i]));

			var strings = new string[numStrings];

			for (var i = 0; i < numStrings; i++)
			{
				strings[i] = Decompress(pivot, dictionary, stringLengths[i], dictPointers[i]);
			}

			return new CompressedLocalizationData(pivot, dictionary, localizationPointers, stringMetas, stringLengths, dictPointers, strings);
		}

		private static string Decompress(ushort pivot, LocalizationDictionaryEntry[] dictionary, ushort stringLength, byte[] dictPointers)
		{
			var str = "";

			var dictIndex = 0;

			var dictExtendedIndexOffset = 0xFFFFFF01 * pivot;

			var charRunStack = new ushort[32];
			var charRunIdx = 0;

			while (dictIndex < stringLength)
			{
				ushort dictPointer = dictPointers[dictIndex];
				int nextPos;

				if (pivot <= dictPointer)
				{
					if ((byte)dictPointer == 0xFF)
					{
						// 3 byte extended dict pointer format
						var nextDictPointer = BitConverter.ToUInt16(dictPointers, dictIndex + 1);
						nextPos = dictIndex + 3;
						dictPointer = ByteswapUint16(nextDictPointer);
					}
					else
					{
						// 2 byte extended dict pointer format
						var nextDictPointer = dictPointers[dictIndex + 1];
						nextPos = dictIndex + 2;
						dictPointer = (ushort)(dictExtendedIndexOffset + ((dictPointer << 8) | nextDictPointer));
					}
				}
				else
				{
					nextPos = dictIndex + 1;
				}

				var dictionaryIndex = (ushort)(dictPointer + 1);
				while (true)
				{
					ushort outputChar;

					while (true)
					{
						var (dictEntryChar, nextDictEntry) = dictionary[dictionaryIndex];
						outputChar = ByteswapUint16(dictEntryChar);

						// if the next dictionary entry is zero, this entry contained the output character
						if (nextDictEntry == 0)
							break;

						// if not, it starts a sequence of characters: push a value onto the character run stack
						charRunStack[charRunIdx] = outputChar;
						charRunIdx++;

						dictionaryIndex = ByteswapUint16(nextDictEntry);
					}

					str += (char)outputChar;

					// if there's no next override position, the string is done
					if (charRunIdx == 0)
						break;

					// pop one value out of the character run stack
					dictionaryIndex = charRunStack[charRunIdx - 1];
					charRunIdx--;
				}

				dictIndex = nextPos;
			}

			return str;
		}
	}
}