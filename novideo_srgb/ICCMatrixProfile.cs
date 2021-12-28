using System;
using System.IO;

namespace novideo_srgb
{
    public class ICCMatrixProfile
    {
        public Matrix matrix = Matrix.Zero3x3();
        public ToneCurve[] trcs = new ToneCurve[3];
        public ToneCurve[] vcgt;

        private ICCMatrixProfile()
        {
        }

        public static ICCMatrixProfile FromFile(string path)
        {
            var result = new ICCMatrixProfile();

            using (var reader = new ICCBinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var stream = reader.BaseStream;

                {
                    stream.Seek(0x24, SeekOrigin.Begin);
                    var magic = new string(reader.ReadChars(4));
                    if (magic != "acsp")
                    {
                        throw new ICCProfileException("Not an ICC profile");
                    }
                }

                {
                    stream.Seek(0xC, SeekOrigin.Begin);
                    var type = new string(reader.ReadChars(4));
                    if (type != "mntr")
                    {
                        throw new ICCProfileException("Not a display device profile");
                    }
                }

                {
                    stream.Seek(0x10, SeekOrigin.Begin);
                    var spaces = new string(reader.ReadChars(8));
                    if (spaces != "RGB XYZ ")
                    {
                        throw new ICCProfileException("Not an RGB profile with XYZ PCS");
                    }
                }

                stream.Seek(0x80, SeekOrigin.Begin);

                var tagCount = reader.ReadUInt32();

                var seenTags = 0;

                for (uint i = 0; i < tagCount; i++)
                {
                    stream.Seek(0x80 + 4 + 12 * i, SeekOrigin.Begin);
                    var tagSig = new string(reader.ReadChars(4));

                    var offset = reader.ReadUInt32();
                    var size = reader.ReadUInt32();

                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                    var index = Array.IndexOf(new[] { 'r', 'g', 'b' }, tagSig[0]);
                    if (tagSig.EndsWith("TRC"))
                    {
                        var typeSig = new string(reader.ReadChars(4));
                        if (typeSig != "curv")
                        {
                            throw new ICCProfileException(tagSig + " is not of curveType");
                        }

                        reader.ReadUInt32();

                        var numEntries = reader.ReadUInt32();

                        ToneCurve curve;
                        if (numEntries == 1)
                        {
                            var gamma = reader.ReadU8Fixed8();
                            curve = new GammaToneCurve(gamma);
                        }
                        else
                        {
                            var entries = new ushort[numEntries];
                            for (uint j = 0; j < numEntries; j++)
                            {
                                entries[j] = reader.ReadUInt16();
                            }

                            curve = new LutToneCurve(entries);
                        }

                        result.trcs[index] = curve;

                        seenTags++;
                    }
                    else if (tagSig.EndsWith("XYZ"))
                    {
                        reader.ReadUInt32();
                        reader.ReadUInt32();

                        for (var j = 0; j < 3; j++)
                        {
                            result.matrix[j, index] = reader.ReadS15Fixed16();
                        }

                        seenTags++;
                    }
                    else if (tagSig == "vcgt")
                    {
                        reader.ReadChars(4);
                        reader.ReadUInt32();
                        var type = reader.ReadUInt32();
                        if (type != 0) throw new ICCProfileException("Only VCGT type 0 is supported");

                        var numChannels = reader.ReadUInt16();
                        var numEntries = reader.ReadUInt16();
                        var entrySize = reader.ReadUInt16();

                        if (numChannels != 3) throw new ICCProfileException("Only VCGT with 3 channels is supported");

                        result.vcgt = new ToneCurve[3];
                        for (var j = 0; j < 3; j++)
                        {
                            var values = new ushort[numEntries];
                            for (var k = 0; k < numEntries; k++)
                            {
                                switch (entrySize)
                                {
                                    case 1:
                                        values[k] = (ushort)(reader.ReadByte() * ushort.MaxValue / byte.MaxValue);
                                        break;
                                    case 2:
                                        values[k] = reader.ReadUInt16();
                                        break;
                                    default:
                                        throw new ICCProfileException("Only 8 and 16 bit VCGT is supported");
                                }
                            }

                            result.vcgt[j] = new LutToneCurve(values);
                        }
                    }
                }

                if (seenTags != 6)
                {
                    throw new ICCProfileException("Missing required tags for curves + matrix profile");
                }
            }

            return result;
        }
    }
}