using System.Collections.Generic;
using System.IO;

namespace FlvMerge
{
    /// <summary>
    /// Class <c>FlvUtil</c> is a utility set for Flv file.
    /// Author: Xuan525
    /// Date: 25/07/2019
    /// </summary>
    public static class FlvUtil
    {
        private const string creator = "Coded by Bili-dl";
        private const string matadatacreator = "Bili-dl Internal Metadata creator";

        public static void FlvMerge(string[] inputs, string output)
        {
            MetadataTagCreater.Metadata metadata = new MetadataTagCreater.Metadata(creator, matadatacreator);
            uint timestampOffset = 0;

            foreach(string input in inputs)
            {
                using (FlvFile flv = new FlvFile(input))
                {
                    while (true)
                    {
                        FlvFile.Tag tag = flv.ReadTag();
                        if (tag == null)
                            break;
                        if (tag.Type == FlvFile.Tag.TagType.Script)
                        {
                            FlvFile.Tag.ScriptTag scriptTag = (FlvFile.Tag.ScriptTag)tag;
                            FlvFile.Tag.ScriptTag.String tagName = (FlvFile.Tag.ScriptTag.String)scriptTag.Name;
                            if (tagName.Value == "onMetaData")
                            {
                                FlvFile.Tag.ScriptTag.EcmaArray tagValue = (FlvFile.Tag.ScriptTag.EcmaArray)scriptTag.Value;
                                metadata.hasKeyframes |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["hasKeyframes"]).Value;
                                metadata.hasVideo |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["hasVideo"]).Value;
                                metadata.hasAudio |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["hasAudio"]).Value;
                                metadata.hasMetadata |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["hasMetadata"]).Value;
                                metadata.canSeekToEnd |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["canSeekToEnd"]).Value;
                                metadata.duration += ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["duration"]).Value;
                                metadata.videocodecid = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["videocodecid"]).Value;
                                metadata.width = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["width"]).Value;
                                metadata.height = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["height"]).Value;
                                metadata.audiocodecid = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["audiocodecid"]).Value;
                                metadata.audiosamplerate = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["audiosamplerate"]).Value;
                                metadata.audiosamplesize = ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["audiosamplesize"]).Value;
                                metadata.stereo |= ((FlvFile.Tag.ScriptTag.Boolean)tagValue.Items["stereo"]).Value;
                            }
                        }
                        else
                        {
                            metadata.datasize += tag.TagLengthWithPts;
                            metadata.lasttimestamp = tag.Header.Timestamp + timestampOffset;
                            if (tag.Type == FlvFile.Tag.TagType.Video)
                            {
                                FlvFile.Tag.VideoTag videoTag = (FlvFile.Tag.VideoTag)tag;
                                metadata.videosize += videoTag.TagLength;
                                metadata.framerate++;
                                metadata.videodatarate += videoTag.BodyLength;
                                if (videoTag.FrameType == FlvFile.Tag.VideoTag.FrameTypes.KeyFrame)
                                {
                                    metadata.lastkeyframetimestamp = videoTag.Header.Timestamp + timestampOffset;
                                    metadata.lastkeyframelocation = (uint)metadata.filesize;
                                    metadata.keyframesFilepositions.Add((uint)metadata.filesize);
                                    metadata.keyframesTimes.Add((double)(videoTag.Header.Timestamp + timestampOffset) / 1000);
                                }
                            }
                            else if (tag.Type == FlvFile.Tag.TagType.Audio)
                            {
                                FlvFile.Tag.AudioTag audioTag = (FlvFile.Tag.AudioTag)tag;
                                metadata.audiosize += audioTag.TagLength;
                                metadata.audiodatarate += audioTag.BodyLength;
                            }
                            metadata.filesize += tag.TagLengthWithPts;
                        }
                        
                    }
                }
                timestampOffset = (uint)(metadata.duration * 1000);
            }
            metadata.framerate /= metadata.duration;
            metadata.videodatarate /= metadata.duration * 1024 / 8;
            metadata.audiodatarate /= metadata.duration * 1024 / 8;
            metadata.lasttimestamp /= 1000;
            metadata.lastkeyframetimestamp /= 1000;

            FlvFile.Tag.ScriptTag metadataTag = MetadataTagCreater.CreatMetadataTag(metadata);

            FlvFile.FlvHeader flvHeader = new FlvFile.FlvHeader(FlvFile.FlvHeader.StreamFlag.VideoAndAudio);

            timestampOffset = 0;
            using (FileStream fileStream = new FileStream(output, FileMode.Create))
            {
                fileStream.Write(flvHeader.HeaderBytes, 0, flvHeader.HeaderBytes.Length);
                byte[] metadataBytes = metadataTag.TagBytesWithPts;
                fileStream.Write(metadataBytes, 0, metadataBytes.Length);
                double duration = 0;
                foreach (string input in inputs)
                {
                    using (FlvFile flv = new FlvFile(input))
                    {
                        
                        while (true)
                        {
                            FlvFile.Tag tag = flv.ReadTag();
                            if (tag == null)
                                break;
                            if (tag.Type == FlvFile.Tag.TagType.Script)
                            {
                                FlvFile.Tag.ScriptTag scriptTag = (FlvFile.Tag.ScriptTag)tag;
                                FlvFile.Tag.ScriptTag.String tagName = (FlvFile.Tag.ScriptTag.String)scriptTag.Name;
                                if (tagName.Value == "onMetaData")
                                {
                                    FlvFile.Tag.ScriptTag.EcmaArray tagValue = (FlvFile.Tag.ScriptTag.EcmaArray)scriptTag.Value;
                                    duration += ((FlvFile.Tag.ScriptTag.Number)tagValue.Items["duration"]).Value;
                                }
                            }
                            else
                            {
                                tag.Header.Timestamp += timestampOffset;
                                byte[] buffer = tag.TagBytesWithPts;
                                fileStream.Write(buffer, 0, buffer.Length);
                            }

                        }
                    }
                    timestampOffset = (uint)(duration * 1000);
                }
            }
        }

        private static class MetadataTagCreater
        {
            public struct Metadata
            {
                public string creator;
                public string metadatacreator;
                public bool hasKeyframes;
                public bool hasVideo;
                public bool hasAudio;
                public bool hasMetadata;
                public bool canSeekToEnd;
                public double duration;
                public uint datasize;
                public uint videosize;
                public double framerate;
                public double videodatarate;
                public double videocodecid;
                public double width;
                public double height;
                public uint audiosize;
                public double audiodatarate;
                public double audiocodecid;
                public double audiosamplerate;
                public double audiosamplesize;
                public bool stereo;
                public double filesize;
                public double lasttimestamp;
                public double lastkeyframetimestamp;
                public uint lastkeyframelocation;
                public List<uint> keyframesFilepositions;
                public List<double> keyframesTimes;

                public Metadata(string creator, string metadatacreator)
                {
                    this.creator = creator;
                    this.metadatacreator = metadatacreator;
                    hasKeyframes = false;
                    hasVideo = false;
                    hasAudio = false;
                    hasMetadata = false;
                    canSeekToEnd = false;
                    duration = 0;
                    datasize = 0;
                    videosize = 0;
                    framerate = 0;
                    videodatarate = 0;
                    videocodecid = 0;
                    width = 0;
                    height = 0;
                    audiosize = 0;
                    audiodatarate = 0;
                    audiocodecid = 0;
                    audiosamplerate = 0;
                    audiosamplesize = 0;
                    stereo = false;
                    filesize = 0;
                    lasttimestamp = 0;
                    lastkeyframetimestamp = 0;
                    lastkeyframelocation = 0;
                    keyframesFilepositions = new List<uint>();
                    keyframesTimes = new List<double>();
                }
            }

            public static FlvFile.Tag.ScriptTag CreatMetadataTag(Metadata metadata)
            {
                FlvFile.Tag.ScriptTag.EcmaArray ecmaArray = new FlvFile.Tag.ScriptTag.EcmaArray();

                ecmaArray.Items.Add("creator", new FlvFile.Tag.ScriptTag.String(metadata.creator));
                ecmaArray.Items.Add("metadatacreator", new FlvFile.Tag.ScriptTag.String(metadata.metadatacreator));
                ecmaArray.Items.Add("hasKeyframes", new FlvFile.Tag.ScriptTag.Boolean(metadata.hasKeyframes));
                ecmaArray.Items.Add("hasVideo", new FlvFile.Tag.ScriptTag.Boolean(metadata.hasVideo));
                ecmaArray.Items.Add("hasAudio", new FlvFile.Tag.ScriptTag.Boolean(metadata.hasAudio));
                ecmaArray.Items.Add("hasMetadata", new FlvFile.Tag.ScriptTag.Boolean(metadata.hasMetadata));
                ecmaArray.Items.Add("canSeekToEnd", new FlvFile.Tag.ScriptTag.Boolean(metadata.canSeekToEnd));
                ecmaArray.Items.Add("duration", new FlvFile.Tag.ScriptTag.Number(metadata.duration));
                ecmaArray.Items.Add("datasize", new FlvFile.Tag.ScriptTag.Number(metadata.datasize));
                ecmaArray.Items.Add("videosize", new FlvFile.Tag.ScriptTag.Number(metadata.videosize));
                ecmaArray.Items.Add("framerate", new FlvFile.Tag.ScriptTag.Number(metadata.framerate));
                ecmaArray.Items.Add("videodatarate", new FlvFile.Tag.ScriptTag.Number(metadata.videodatarate));
                ecmaArray.Items.Add("videocodecid", new FlvFile.Tag.ScriptTag.Number(metadata.videocodecid));
                ecmaArray.Items.Add("width", new FlvFile.Tag.ScriptTag.Number(metadata.width));
                ecmaArray.Items.Add("height", new FlvFile.Tag.ScriptTag.Number(metadata.height));
                ecmaArray.Items.Add("audiosize", new FlvFile.Tag.ScriptTag.Number(metadata.audiosize));
                ecmaArray.Items.Add("audiodatarate", new FlvFile.Tag.ScriptTag.Number(metadata.audiodatarate));
                ecmaArray.Items.Add("audiocodecid", new FlvFile.Tag.ScriptTag.Number(metadata.audiocodecid));
                ecmaArray.Items.Add("audiosamplerate", new FlvFile.Tag.ScriptTag.Number(metadata.audiosamplerate));
                ecmaArray.Items.Add("audiosamplesize", new FlvFile.Tag.ScriptTag.Number(metadata.audiosamplesize));
                ecmaArray.Items.Add("stereo", new FlvFile.Tag.ScriptTag.Boolean(metadata.stereo));
                ecmaArray.Items.Add("filesize", new FlvFile.Tag.ScriptTag.Number(metadata.filesize));
                ecmaArray.Items.Add("lasttimestamp", new FlvFile.Tag.ScriptTag.Number(metadata.lasttimestamp));
                ecmaArray.Items.Add("lastkeyframetimestamp", new FlvFile.Tag.ScriptTag.Number(metadata.lastkeyframetimestamp));
                ecmaArray.Items.Add("lastkeyframelocation", new FlvFile.Tag.ScriptTag.Number(metadata.lastkeyframelocation));

                FlvFile.Tag.ScriptTag.Object keyframes = new FlvFile.Tag.ScriptTag.Object();

                FlvFile.Tag.ScriptTag.StrictArray filepositions = new FlvFile.Tag.ScriptTag.StrictArray();
                foreach (uint fileposition in metadata.keyframesFilepositions)
                    filepositions.Items.Add(new FlvFile.Tag.ScriptTag.Number(fileposition));
                keyframes.Items.Add("filepositions", filepositions);

                FlvFile.Tag.ScriptTag.StrictArray times = new FlvFile.Tag.ScriptTag.StrictArray();
                foreach (double time in metadata.keyframesTimes)
                    times.Items.Add(new FlvFile.Tag.ScriptTag.Number(time));
                keyframes.Items.Add("times", times);

                ecmaArray.Items.Add("keyframes", keyframes);


                FlvFile.Tag.ScriptTag scriptTag = new FlvFile.Tag.ScriptTag("onMetaData", ecmaArray);


                scriptTag.Header.DataSize = scriptTag.BodyLength;
                uint offset = scriptTag.TagLengthWithPts + FlvFile.FlvHeader.HeaderLengthWithPts;
                ((FlvFile.Tag.ScriptTag.Number)ecmaArray.Items["filesize"]).Value += offset;
                ((FlvFile.Tag.ScriptTag.Number)ecmaArray.Items["lastkeyframelocation"]).Value += offset;
                foreach (FlvFile.Tag.ScriptTag.ScriptData scriptData in filepositions.Items)
                    ((FlvFile.Tag.ScriptTag.Number)scriptData).Value += offset;

                return scriptTag;
            }
        }
    }
}
