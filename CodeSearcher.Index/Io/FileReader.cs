using CodeSearcher.BusinessLogic.InternalInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.Io
{
    internal class FileReader : IFileReader
    {
        private IList<String> m_FileExtensions;
        //private byte[] m_Buffer;
        //private int m_BufferLength;
        //private StringBuilder m_StringBuilder;

        public  FileReader(IList<String> fileExtensions)
        {
            m_FileExtensions = fileExtensions;
            //m_Buffer = new byte[66560];
            //m_BufferLength = m_Buffer.Length;
            //m_StringBuilder = new StringBuilder(5 * 1024 * 1024);
        }

        //public Task ReadFilesAsync(String srcPath, Action<IList<FileStructure>> action)
        //{
        //    return Task.Run(() =>
        //    {
        //        var currentDirectory = new DirectoryInfo(srcPath);
        //        if (!currentDirectory.Exists) throw new ArgumentException("Source Folder dosn't exist!");

        //        var data = ReadFolderConent(currentDirectory);

        //        var tasks = new List<Task>();
        //        foreach (var innerDirectory in currentDirectory.EnumerateDirectories())
        //        {
        //            var innerTask = ReadFilesAsync(innerDirectory.FullName, action);
        //            tasks.Add(innerTask);
        //        }

        //        //task.Wait();
        //        action(data);

        //        Task.WaitAll(tasks.ToArray());
        //    });
        //}
        public Task ReadFilesAsync(String srcPath, Action<IList<FileStructure>> action)
        {
            return ReadFilesAsync(srcPath, action, 0);
        }

        private Task ReadFilesAsync(String srcPath, Action<IList<FileStructure>> action, int depth)
        {
            return Task.Run(() =>
            {
                if (depth >= 1)
                {
                    ReadFilesInSync(srcPath, action);
                }
                {
                    var currentDirectory = new DirectoryInfo(srcPath);
                    if (!currentDirectory.Exists) throw new ArgumentException("Source Folder dosn't exist!");

                    var data = ReadFolderContent(currentDirectory);
                    action(data);

                    var tasks = new List<Task>();
                    foreach (var innerDirectory in currentDirectory.EnumerateDirectories())
                    {
                        var innerTask = ReadFilesAsync(innerDirectory.FullName, action, ++depth);
                        tasks.Add(innerTask);
                    }

                    //task.Wait();
                    //action(data);

                    Task.WaitAll(tasks.ToArray());
                }
            });
        }

        public void ReadFilesInSync(String srcPath, Action<IList<FileStructure>> action)
        {
            var currentDirectory = new DirectoryInfo(srcPath);
            if (!currentDirectory.Exists) throw new ArgumentException("Source Folder dosn't exist!");

            var data = ReadFolderContent(currentDirectory);
            action(data);


            foreach (var innerDirectory in currentDirectory.EnumerateDirectories())
            {
                ReadFilesInSync(innerDirectory.FullName, action);

            }      
        }

        //private async Task<IList<FileStructure>> ReadFolderConentAsync(DirectoryInfo di)
        //{
        //    var tasks = new List<Task<FileStructure>>();
        //    var resultTexts = new List<FileStructure>();

        //    foreach(var file in di.GetFiles())
        //    {
        //        if(m_FileExtensions.Contains(file.Extension))
        //        {
        //            tasks.Add(ReadTextAsync(file.FullName));
        //        }
        //    }

        //    await Task.WhenAll(tasks);

        //    foreach(var task in tasks)
        //    {
        //        resultTexts.Add(task.Result);
        //    }

        //    return resultTexts;
        //}

        private IList<FileStructure> ReadFolderContent(DirectoryInfo di)
        {
            var files = new List<FileStructure>();
            var resultTexts = new List<FileStructure>();

            foreach (var file in di.GetFiles())
            {
                if (m_FileExtensions.Contains(file.Extension))
                {
                    files.Add(ReadText(file.FullName));
                }
            }

            //await Task.WhenAll(tasks);

            foreach (var file in files)
            {
                resultTexts.Add(file);
            }

            return resultTexts;
        }

        private FileStructure ReadText(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: false))
            {
                //var sb = new StringBuilder(5 * 1024 * 1024);
                var sb = new StringBuilder();
                //m_StringBuilder.Clear();
                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.Default.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                //while ((numRead = sourceStream.Read(m_Buffer, 0, m_BufferLength)) != 0)
                //{
                //    string text = Encoding.Default.GetString(m_Buffer, 0, numRead);
                //    sb.Append(text);
                //}

                return new FileStructure
                {
                    FilePath = filePath,
                    Text = sb.ToString()
                };
            }
        }

        //private async Task<FileStructure> ReadTextAsync(string filePath)
        //{
        //    using (FileStream sourceStream = new FileStream(filePath, 
        //        FileMode.Open, FileAccess.Read, FileShare.Read,
        //        bufferSize: 4096, useAsync: true))
        //    {
        //        StringBuilder sb = new StringBuilder();

        //        byte[] buffer = new byte[0x1000];
        //        int numRead;
        //        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        //        {
        //            string text = Encoding.Default.GetString(buffer, 0, numRead);
        //            sb.Append(text);
        //        }

        //        return new FileStructure
        //        {
        //            FilePath = filePath,
        //            Text = sb.ToString()
        //        };
        //    }
        //}
    }
}
