// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Dom
{
	/// <summary>
	/// Class capable of loading xml documentation files. XmlDoc automatically creates a
	/// binary cache for big xml files to reduce memory usage.
	/// </summary>
	public sealed class XmlDoc : IDisposable
	{
		struct IndexEntry : IComparable<IndexEntry>
		{
			public int HashCode;
			public int FileLocation;
			
			public int CompareTo(IndexEntry other)
			{
				return HashCode.CompareTo(other.HashCode);
			}
			
			public IndexEntry(int HashCode, int FileLocation)
			{
				this.HashCode = HashCode;
				this.FileLocation = FileLocation;
			}
		}
		
		Dictionary<string, string> xmlDescription = new Dictionary<string, string>();
		IndexEntry[] index; // SORTED array of index entries
		Queue<string> keyCacheQueue;
		
		const int cacheLength = 150; // number of strings to cache when working in file-mode
		
		void ReadMembersSection(XmlReader reader)
		{
			while (reader.Read()) {
				switch (reader.NodeType) {
					case XmlNodeType.EndElement:
						if (reader.LocalName == "members") {
							return;
						}
						break;
					case XmlNodeType.Element:
						if (reader.LocalName == "member") {
							string memberAttr = reader.GetAttribute(0);
							string innerXml   = reader.ReadInnerXml();
							xmlDescription[memberAttr] = innerXml;
						}
						break;
				}
			}
		}
		
		public string GetDocumentation(string key)
		{
			if (xmlDescription == null)
				throw new ObjectDisposedException("XmlDoc");
			lock (xmlDescription) {
				string result;
				if (xmlDescription.TryGetValue(key, out result))
					return result;
				if (index == null)
					return null;
				return LoadDocumentation(key);
			}
		}
		
		#region Save binary files
		// FILE FORMAT FOR BINARY DOCUMENTATION
		// long  magic = 0x4244636f446c6d58 (identifies file type = 'XmlDocDB')
		const long magic = 0x4244636f446c6d58;
		// short version = 2              (file version)
		const short version = 2;
		// long  fileDate                 (last change date of xml file in DateTime ticks)
		// int   testHashCode = magicTestString.GetHashCode() // (check if hash-code implementation is compatible)
		// int   entryCount               (count of entries)
		// int   indexPointer             (points to location where index starts in the file)
		// {
		//   string key                   (documentation key as length-prefixed string)
		//   string docu                  (xml documentation as length-prefixed string)
		// }
		// indexPointer points to the start of the following section:
		// {
		//   int hashcode
		//   int    index           (index where the docu string starts in the file)
		// }
		
		void Save(string fileName, DateTime fileDate)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
				using (BinaryWriter w = new BinaryWriter(fs)) {
					w.Write(magic);
					w.Write(version);
					w.Write(fileDate.Ticks);
					
					IndexEntry[] index = new IndexEntry[xmlDescription.Count];
					w.Write(index.Length);
					
					int indexPointerPos = (int)fs.Position;
					w.Write(0); // skip 4 bytes
					
					int i = 0;
					foreach (KeyValuePair<string, string> p in xmlDescription) {
						index[i] = new IndexEntry(p.Key.GetHashCode(), (int)fs.Position);
						w.Write(p.Key);
						w.Write(p.Value.Trim());
						i += 1;
					}
					
					Array.Sort(index);
					
					int indexStart = (int)fs.Position;
					foreach (IndexEntry entry in index) {
						w.Write(entry.HashCode);
						w.Write(entry.FileLocation);
					}
					w.Seek(indexPointerPos, SeekOrigin.Begin);
					w.Write(indexStart);
				}
			}
		}
		#endregion
		
		#region Load binary files
		BinaryReader loader;
		FileStream fs;
		
		bool LoadFromBinary(string fileName, DateTime fileDate)
		{
			keyCacheQueue   = new Queue<string>(cacheLength);
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			int len = (int)fs.Length;
			loader = new BinaryReader(fs);
			try {
				if (loader.ReadInt64() != magic) {
					LoggingService.Warn("Cannot load XmlDoc: wrong magic");
					return false;
				}
				if (loader.ReadInt16() != version) {
					LoggingService.Warn("Cannot load XmlDoc: wrong version");
					return false;
				}
				if (loader.ReadInt64() != fileDate.Ticks) {
					LoggingService.Info("Not loading XmlDoc: file changed since cache was created");
					return false;
				}
				int count = loader.ReadInt32();
				int indexStartPosition = loader.ReadInt32(); // go to start of index
				if (indexStartPosition >= len) {
					LoggingService.Error("XmlDoc: Cannot find index, cache invalid!");
					return false;
				}
				fs.Position = indexStartPosition;
				IndexEntry[] index = new IndexEntry[count];
				for (int i = 0; i < index.Length; i++) {
					index[i] = new IndexEntry(loader.ReadInt32(), loader.ReadInt32());
				}
				this.index = index;
				return true;
			} catch (Exception ex) {
				LoggingService.Error("Cannot load from cache", ex);
				return false;
			}
		}
		
		string LoadDocumentation(string key)
		{
			if (keyCacheQueue.Count > cacheLength - 1) {
				xmlDescription.Remove(keyCacheQueue.Dequeue());
			}
			
			int hashcode = key.GetHashCode();
			
			// use interpolation search to find the item
			string resultDocu = null;
			
			int m = Array.BinarySearch(index, new IndexEntry(hashcode, 0));
			if (m >= 0) {
				// correct hash code found.
				// possibly there are multiple items with the same hash, so go to the first.
				while (--m >= 0 && index[m].HashCode == hashcode);
				// go through all items that have the correct hash
				while (++m < index.Length && index[m].HashCode == hashcode) {
					fs.Position = index[m].FileLocation;
					string keyInFile = loader.ReadString();
					if (keyInFile == key) {
						resultDocu = loader.ReadString();
						break;
					} else {
						LoggingService.Warn("Found " + keyInFile + " instead of " + key);
					}
				}
			}
			
			keyCacheQueue.Enqueue(key);
			xmlDescription.Add(key, resultDocu);
			
			return resultDocu;
		}
		
		public void Dispose()
		{
			if (loader != null) {
				loader.Close();
				fs.Close();
			}
			xmlDescription = null;
			index = null;
			keyCacheQueue = null;
			loader = null;
			fs = null;
		}
		#endregion
		
		public static XmlDoc Load(XmlReader reader)
		{
			XmlDoc newXmlDoc = new XmlDoc();
			while (reader.Read()) {
				if (reader.IsStartElement()) {
					switch (reader.LocalName) {
						case "members":
							newXmlDoc.ReadMembersSection(reader);
							break;
					}
				}
			}
			return newXmlDoc;
		}
		
		public static XmlDoc Load(string fileName, string cachePath)
		{
			LoggingService.Debug("Loading XmlDoc for " + fileName);
			Directory.CreateDirectory(cachePath);
			string cacheName = cachePath + "/" + Path.GetFileNameWithoutExtension(fileName)
				+ "." + fileName.GetHashCode().ToString("x") + ".dat";
			XmlDoc doc;
			if (File.Exists(cacheName)) {
				doc = new XmlDoc();
				if (doc.LoadFromBinary(cacheName, File.GetLastWriteTimeUtc(fileName))) {
					LoggingService.Debug("XmlDoc: Load from cache successful");
					return doc;
				} else {
					doc.Dispose();
					try {
						File.Delete(cacheName);
					} catch {}
				}
			}
			
			try {
				using (XmlTextReader xmlReader = new XmlTextReader(fileName)) {
					doc = Load(xmlReader);
				}
			} catch (XmlException ex) {
				LoggingService.Warn("Error loading XmlDoc", ex);
				return new XmlDoc();
			}
			
			if (doc.xmlDescription.Count > cacheLength * 2) {
				LoggingService.Debug("XmlDoc: Creating cache");
				DateTime date = File.GetLastWriteTimeUtc(fileName);
				try {
					doc.Save(cacheName, date);
				} catch (Exception ex) {
					LoggingService.Error("Cannot write to cache file", ex);
					return doc;
				}
				doc.Dispose();
				doc = new XmlDoc();
				doc.LoadFromBinary(cacheName, date);
			}
			return doc;
		}
	}
}
