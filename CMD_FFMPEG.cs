/* Code Written and created by Austin Tyler 2024.
 * 
 * This class is a very basic set of common instructions for the popular framework 'FFMPEG'. Initially, I was using XABE's C# FFMPEG wrapper, however 
 * After many headaches and issues with running FFMPEG instructions I decided to create my own. Full disclosure, I am in no way a professional programmer
 * and to consider me anything close to a novice would be a complement, so keep that in mind when reading through this class. Feel free to edit, add or 
 * remove methods to your heart's content and hopefully you find this as useful as I have.
 * 
 * That being said, you will need FFMPEG executable installed on your machine. (https://ffmpeg.org/download.html)
 * 
 * This is a wrapper that will periodically be updated depending on what features I need to add, however, I am in favor of letting this be open source so
 * others can add additional functionality. 
 * 
 * 
 */


using System;
using System.Diagnostics;
using System.IO;

namespace MIDI_to_video
{
    public class CMD_FFMPEG
    {
        string EXPORTLOCATION = "";
        public CMD_FFMPEG(string ExportLocation) 
        {
            EXPORTLOCATION = ExportLocation;
        }
        public void setExportLocation(string Loc)
        {
            EXPORTLOCATION = Loc;
        }
        public void FFMPEG_SetVolume(string InputFile,double Volume, string Output )
        {
            Console.WriteLine($@"| CMD_FFMPEG || FFMPEG_SetVolume || setting the volume of {InputFile} to {Volume}");

            string command = $"ffmpeg -i {InputFile} -filter:a \"volume={Volume}\" \"{EXPORTLOCATION}\\{Output}\"";
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_ScaleVideo || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        }
        public void FFMPEG_Concat(string VideoOne, string VideoTwo,string Output)
        {
            Console.WriteLine($@"| CMD_FFMPEG || FFMPEG_Concat || Concatonating  {VideoOne} onto {VideoTwo}");

            string command = $"ffmpeg -i {VideoOne} -i {VideoTwo} -filter_complex \"[0:a][1:a]concat=n=2:v=0:a=1[outa]; [v0][1:v]concat=n=2:v=1:a=0[outv]\" -map \"[outv]\" -map \"[outa]\" \"{Output}\"";
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_Concat || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        }
        public void FFMPEG_ScaleVideo(string Video, int XResolution, int YResolution,string Output)
        {
            //ffmpeg WILL NOT overwrite a video, so if youre trying to replace a video, save its name, rescale as temp, the, rename temp with its old name.
            Console.WriteLine($@"CMD_FFMPEG || FFMPEG_ScaleVideo || Scaling {Video} to {XResolution}x{YResolution} and naming sending it here: {Output}");

            string command = $"ffmpeg -i {Video} -vf \"scale={XResolution}:{YResolution},setsar=2:1\" -c:v libx264 -crf 23 -preset fast \"{EXPORTLOCATION}\\{Output}.mp4\"";
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_ScaleVideo || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        } 
        public void FFMPEG_CreateBaseVideo(string color, int XResolution, int YResolution,double Duration, string Name, string FileType)
        {
            Console.WriteLine($@"CMD_FFMPEG || FFMPEG_CreateBaseVideo || Creating a(n) {color} video called {Name}.{FileType}, its resolution is {XResolution}x{YResolution}, it's {Duration} seconds and it will be exported here: {EXPORTLOCATION}");
            //color can be hexadecimal where #xxxxxxxx is available, additionally so is RGB RRR:GGG:BBB 0-255 is the format, additionally RGBA is also allowed for transparancy RRR:GGG:BBB:TTT
            string command = $@"ffmpeg -f lavfi -i color=c={color}:s={XResolution}x{YResolution}:d={Duration} -f lavfi -t {Duration} -i anullsrc=r=48000:cl=stereo -vf scale={XResolution}:{YResolution},setdar=16:9,setsar=256:81,format=yuvj420p -c:v libx264 -c:a aac -strict experimental -shortest ""{EXPORTLOCATION}\\{Name}.{FileType}""";
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_CreateBaseVideo || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        }
        public void FFMPEG_Overlay(string VideoBase, string VideoOverlay, double OverlayDuration, double OverlayTime, string Output,bool FirstLoop )
        {

            Console.WriteLine($@"| CMD_FFMPEG || FFMPEG_Overlay || overlaying  {VideoOverlay} onto {VideoBase}");

            int x = 1;
            string command;
            if (FirstLoop == true)
            {
                x = 0;
                command = $@"ffmpeg -i ""{VideoBase}"" -i ""{VideoOverlay}"" -filter_complex ""[0:v][1:v] overlay=0:0:enable='between(t,{OverlayDuration * x},{OverlayTime + OverlayDuration})'[v]"" -map ""[v]"" -map ""1:a"" -c:v libx264 -crf 23 -preset fast ""{EXPORTLOCATION}\{Output}.mp4""";
            }
            else
            {
                command = $@"ffmpeg -i ""{VideoBase}"" -i ""{VideoOverlay}"" -filter_complex ""[0:v][1:v] overlay=0:0:enable='between(t,{OverlayDuration},{OverlayTime + OverlayDuration})'[v]; [0:a][1:a] amix=inputs=2:duration=shortest[a]"" -map ""[v]"" -map ""[a]"" -c:v libx264 -crf 23 -preset fast ""{EXPORTLOCATION}\{Output}.mp4""";
            }
                try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_CreateBaseVideo || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        }

        public void FFMPEG_Vstack(string VideoMain, string VideoStack)
        {

            Console.WriteLine($@"CMD_FFMPEG || FFMPEG_Vstack || Stacking {VideoStack} ontop of {VideoMain}");

            string command = $"ffmpeg -i {VideoMain} -i {VideoStack} -filter_complex \"[0:v][1:v]vstack=inputs=2[outv];[0:a][1:a]amix=inputs=2[outa]\" -map \"[outv]\" -map \"[outa]\" {EXPORTLOCATION}\\temp.mp4\"";
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_Vstack || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
            
        }
        public void FFMPEG_Trim(string VideoToTrim, double VideoTrimLength, string Savelocation)
        {
            Console.WriteLine($@"CMD_FFMPEG || FFMPEG_Trim || Trimming video: {VideoToTrim} to the length of {VideoTrimLength}");

            string command = $"ffmpeg -i \"{VideoToTrim}\" " +
                   $"-filter_complex \"[0:v]trim=start=0:end={VideoTrimLength},setpts=PTS-STARTPTS[v];" +
                   $" [0:a]atrim=start=0:end={VideoTrimLength},asetpts=PTS-STARTPTS[a]\" " +
                   $"-map [v] -map [a] " +
                   $"-t {VideoTrimLength} " +
                   "-loglevel verbose " +
                   "-c:a aac " +
                   $@" {Savelocation}";
            Console.WriteLine($@"CMD_FFMPEG || FFMPEG_VTrim || Trimming {VideoToTrim}");
            try
            {
                CMD_Call(command);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($@"CMD_FFMPEG || FFMPEG_Trim || CMD_Call --FAILED-- With message: {e.Message}");
                Console.WriteLine();
            }
        }
        public void CMD_Call(string command)
        {
            Console.WriteLine($@"CMD_FFMPEG || CMD_Call || ( {command} ) Being Executed");
            ProcessStartInfo StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = new Process())
            {
                process.StartInfo = StartInfo;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.WriteLine("FFmpeg command failed with exit code: " + process.ExitCode);
                }
                else
                {
                    Console.WriteLine("FFmpeg command executed successfully.");
                }
            }
        }
    }
}
