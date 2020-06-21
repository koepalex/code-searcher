using CodeSearcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeSearcher.BusinessLogic.Management
{
    internal class CodeSearcherIndex : ICodeSearcherIndex
    {
        public CodeSearcherIndex()
        {

        }
        public CodeSearcherIndex(string sourcePath, string indexPath, IList<string> fileExtensions)
        {
            SourcePath = sourcePath;
            IndexPath = indexPath;
            FileExtensions = fileExtensions;
            CreatedTime = DateTime.UtcNow;
            ID = BuildIdForIndex();
        }
        
        /// <inheritdoc/>
        public int ID { get; set; }

        /// <inheritdoc/>
        public string SourcePath { get; set; }
        
        /// <inheritdoc/>
        public string IndexPath { get; set; }
        
        /// <inheritdoc/>
        public DateTime CreatedTime { get; set; }

        /// <inheritdoc/>
        public IList<string> FileExtensions { get; set; }

        public bool Equals(ICodeSearcherIndex other)
        {
            if (other == null)
            {
                return false;
            }

            return ID == other.ID;
        }

        public bool Equals(ICodeSearcherIndex x, ICodeSearcherIndex y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public int GetHashCode(ICodeSearcherIndex obj)
        {
            return obj.GetHashCode();
        }

        public static bool operator ==(CodeSearcherIndex a, ICodeSearcherIndex b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CodeSearcherIndex a, ICodeSearcherIndex b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is ICodeSearcherIndex index)
            {
                return Equals(index);
            }
            return base.Equals(obj);
        }

        private int BuildIdForIndex()
        {
            var sb = new StringBuilder();
            sb.Append(SourcePath);
            sb.Append(IndexPath);
            foreach (var extension in FileExtensions)
            {
                sb.Append(extension);
            }
            return sb.ToString().GetHashCode();
        }
    }
}
