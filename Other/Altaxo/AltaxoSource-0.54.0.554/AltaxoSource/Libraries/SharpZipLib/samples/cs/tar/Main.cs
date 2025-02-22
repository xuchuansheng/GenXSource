using System;
using System.IO;

using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;

/// <summary>
/// The tar class implements a simplistic version of the
/// traditional UNIX tar command. It currently supports
/// creating, listing, and extracting from archives. 
/// It supports GZIP, unix compress and bzip2 compression
/// GNU long filename extensions are supported, POSIX extensions are not yet supported...
/// See the help (-? or --help) for option details.
/// </summary>
public class Tar
{
	/// <summary>
	/// Flag that determines if verbose feedback is to be provided.
	/// </summary>
	bool verbose;
	
	/// <summary>
	/// The compresion to use when creating archives.
	/// </summary>
	enum Compression
	{
		None,
		Compress,
		Gzip,
		Bzip2
	}

	Compression compression = Compression.None;

	/// <summary>
	/// Operation to perform on archive
	/// </summary>
	enum Operation
	{
		List,
		Create,
		Extract
	}

	Operation operation = Operation.List;

	/// <summary>
	/// True if we are not to overwrite existing files.  (Unix noKlobber option)
	/// </summary>
	bool keepOldFiles;
	
	/// <summary>
	/// True if we are to convert ASCII text files from local line endings
	/// to the UNIX standard '\n'.
	/// </summary>
	bool asciiTranslate;
	
	/// <summary>
	/// The archive name provided on the command line, '-' if stdio.
	/// </summary>
	string archiveName;
	
	/// <summary>
	/// The blocking factor to use for the tar archive IO. Set by the '-b' option.
	/// </summary>
	int blockingFactor;
	
	/// <summary>
	/// The userId to use for files written to archives. Set by '-U' option.
	/// </summary>
	int userId;
	
	/// <summary>
	/// The userName to use for files written to archives. Set by '-u' option.
	/// </summary>
	string userName;
	
	/// <summary>
	/// The groupId to use for files written to archives. Set by '-G' option.
	/// </summary>
	int groupId;
	
	/// <summary>
	/// The groupName to use for files written to archives. Set by '-g' option.
	/// </summary>
	string groupName;
	
	/// <summary>
	/// The main entry point of the tar class.
	/// </summary>
	public static void Main(string[] argv)
	{
		Tar app = new Tar();
		app.InstanceMain(argv);
	}
	
	/// <summary>
	/// Establishes the default userName with the 'user.name' property.
	/// </summary>
	public Tar()
	{
		this.verbose        = false;
		this.archiveName    = null;
		this.keepOldFiles   = false;
		this.asciiTranslate = false;
		
		this.blockingFactor = TarBuffer.DefaultBlockFactor;
		this.userId   = 0;
		
		string sysUserName = Environment.UserName;
		this.userName = ((sysUserName == null) ? "" : sysUserName);
		
		this.groupId   = 0;
		this.groupName = "None";
	}

	string[] GetFilesForSpec(string spec)
	{
		string dir = Path.GetDirectoryName(spec);
		if (dir == null || dir.Length == 0)
			dir = Directory.GetCurrentDirectory();

		return System.IO.Directory.GetFiles(dir, Path.GetFileName(spec));
	}

	/// <summary>
	/// This is the "real" main. The class main() instantiates a tar object
	/// for the application and then calls this method. Process the arguments
	/// and perform the requested operation.
	/// </summary>
	public void InstanceMain(string[] argv)
	{
		TarArchive archive = null;
		
		int argIdx = this.ProcessArguments(argv);

		if (this.archiveName != null && ! this.archiveName.Equals("-")) {
			if (operation == Operation.Create) {
				if (!Directory.Exists(Path.GetDirectoryName(archiveName))) {
					Console.Error.WriteLine("Directory for archive doesnt exist");
					return;
				}
			}
			else {
				if (File.Exists(this.archiveName) == false) {
					Console.Error.WriteLine("File does not exist " + this.archiveName);
					return;
				}
			}
		}
		
		if (operation == Operation.Create) { 		               // WRITING
			Stream outStream = Console.OpenStandardOutput();
			
			if (this.archiveName != null && ! this.archiveName.Equals("-")) {
				outStream = File.Create(archiveName);
			}
			
			if (outStream != null) {
				switch (this.compression) {
					case Compression.Compress:
						outStream = new DeflaterOutputStream(outStream);
						break;

					case Compression.Gzip:
						outStream = new GZipOutputStream(outStream);
						break;

					case Compression.Bzip2:
						outStream = new BZip2OutputStream(outStream, 9);
					break;
				}
				archive = TarArchive.CreateOutputTarArchive(outStream, this.blockingFactor);
			}
		} else {								// EXTRACTING OR LISTING
			Stream inStream = Console.OpenStandardInput();
			
			if (this.archiveName != null && ! this.archiveName.Equals( "-" )) {
				inStream = File.OpenRead(archiveName);
			}
			
			if (inStream != null) {
				switch (this.compression) {
					case Compression.Compress:
						inStream = new InflaterInputStream(inStream);
						break;

					case Compression.Gzip:
						inStream = new GZipInputStream(inStream);
						break;
					
					case Compression.Bzip2:
						inStream = new BZip2InputStream(inStream);
						break;
				}
				archive = TarArchive.CreateInputTarArchive(inStream, this.blockingFactor);
			}
		}
		
		if (archive != null) {						// SET ARCHIVE OPTIONS
			archive.SetKeepOldFiles(this.keepOldFiles);
			archive.SetAsciiTranslation(this.asciiTranslate);
			
			archive.SetUserInfo(this.userId, this.userName, this.groupId, this.groupName);
		}
		
		if (archive == null) {
			Console.Error.WriteLine( "no processing due to errors" );
		} else if (operation == Operation.Create) {                        // WRITING
			if (verbose) {
				archive.ProgressMessageEvent += new ProgressMessageHandler(ShowTarProgressMessage);
			}

			for ( ; argIdx < argv.Length ; ++argIdx ) {
				string[] fileNames = GetFilesForSpec(argv[argIdx]);
				if (fileNames.Length > 0) {
					foreach (string name in fileNames) {
						TarEntry entry = TarEntry.CreateEntryFromFile(name);
						archive.WriteEntry(entry, true);
					}
				} else {
					Console.Error.Write("No files for " + argv[argIdx]);
				}
			}
		} else if (operation == Operation.List) {                   // LISTING
			archive.ProgressMessageEvent += new ProgressMessageHandler(ShowTarProgressMessage);
			archive.ListContents();
		} else {                                                    // EXTRACTING
			string userDir = Environment.CurrentDirectory;
			if (verbose) {
				archive.ProgressMessageEvent += new ProgressMessageHandler(ShowTarProgressMessage);
			}
			
			if (userDir != null) {
				archive.ExtractContents(userDir);
			}
		}

		if (archive != null) {                                   // CLOSE ARCHIVE
			archive.CloseArchive();
		}
	}
	
	///
	/// <summary>
	/// Process arguments, handling options, and return the index of the
	/// first non-option argument.
	/// </summary>
	/// <returns>
	/// The index of the first non-option argument.
	/// </returns>
	int ProcessArguments(string[] args)
	{
		int idx = 0;
		bool bailOut = false;
		bool gotOP = false;
		
		for ( ; idx < args.Length ; ++idx ) {
			string arg = args[ idx ];
			
			if (!arg.StartsWith("-")) {
				break;
			}
			
			if (arg.StartsWith("--" )) {
				int valuePos = arg.IndexOf('=');
				string argValue = null;
				
				if (valuePos >= 0) {
					argValue = arg.Substring(valuePos + 1);
					arg = arg.Substring(0, valuePos);
				}

				if (arg.Equals( "--help")) {
					this.ShowHelp();
					Environment.Exit(1);
				} else if (arg.Equals( "--version")) {
					this.Version();
					Environment.Exit(1);
				} else if (arg.Equals("--extract")) {
					gotOP = true;
					operation = Operation.Extract;
				} else if (arg.Equals("--list")) {
					gotOP = true;
					operation = Operation.List;
				} else if (arg.Equals("--create")) {
					gotOP = true;
					operation = Operation.Create;
				} else if (arg.Equals("--gzip")) {
					compression = Compression.Gzip;
				} else if (arg.Equals("--bzip2")) {
					compression = Compression.Bzip2;
				} else if (arg.Equals("--compress")) {
					compression = Compression.Compress;
				} else if (arg.Equals("--blocking-factor")) {
					if (argValue == null || argValue.Length == 0)
						Console.Error.WriteLine("expected numeric blocking factor");
					else {
						try {
							this.blockingFactor = Int32.Parse(argValue);
						} catch {
							Console.Error.WriteLine("invalid blocking factor");
						}
					}
				} else if (arg.Equals("--verbose")) {
					this.verbose = true;
				} else if (arg.Equals("--keep-old-files")) {
					this.keepOldFiles = true;
				} else if (arg.Equals("--record-size")) {
					if (argValue == null || argValue.Length == 0) {
						Console.Error.WriteLine("expected numeric record size");
						bailOut = true;
					} else {
						int size;
						try
						{
							size = Int32.Parse(argValue);
							if (size % TarBuffer.BlockSize != 0) {
								Console.Error.WriteLine("Record size must be a multiple of " + TarBuffer.BlockSize.ToString());
								bailOut = true;
							} else 
								this.blockingFactor = size / TarBuffer.BlockSize;
						} catch {
							Console.Error.WriteLine("non-numeric record size");
							bailOut = true;
						}
					}
				} else {
					Console.Error.WriteLine("unknown option: " + arg);
					this.ShowHelp();
					Environment.Exit(1);
				}
			} else {
				for (int cIdx = 1; cIdx < arg.Length; ++cIdx) {
					switch (arg[cIdx]) 
					{
						case '?':
							this.ShowHelp();
							Environment.Exit(1);
							break;

						case 'f':
							this.archiveName = args[++idx];
							break;

						case 'j':
							this.compression = Compression.Bzip2;
							break;

						case 'z':
							this.compression = Compression.Gzip;
							break;

						case 'Z':
							this.compression = Compression.Compress;
							break;

						case 'e':
							this.asciiTranslate = true;
							break;

						case 'c':
							gotOP = true;
							operation = Operation.Create;
							break;

						case 'x':
							gotOP = true;
							operation = Operation.Extract;
							break;

						case 't':
							gotOP = true;
							operation = Operation.List;
							break;

						case 'k':
							this.keepOldFiles = true;
							break;

						case 'b':
							this.blockingFactor = Int32.Parse(args[++idx]);
							break;

						case 'u':
							this.userName = args[++idx];
							break;

						case 'U':
							this.userId = Int32.Parse(args[ ++idx ]);
							break;

						case 'g':
							this.groupName = args[++idx];
							break;

						case 'G':
							this.groupId = Int32.Parse(args[ ++idx ]);
							break;

						case 'v':
							this.verbose = true;
							break;

						default:
							Console.Error.WriteLine("unknown option: " + arg[cIdx]);
							this.ShowHelp();
							Environment.Exit(1);
							break;
					}
				}
			}
		}

		if (!gotOP) {
			Console.Error.WriteLine("you must specify an operation option (c, x, or t)");
			Console.Error.WriteLine("Try tar --help");
			bailOut = true;
		}

		if (bailOut == true) {
			Environment.Exit(1);
		}
		return idx;
	}

	string decodeType(int type, bool slashTerminated)
	{
		string result = "?";
		switch (type)
		{
			case TarHeader.LF_OLDNORM:       // -jr- TODO this decoding is incomplete, not all possible known values are decoded...
			case TarHeader.LF_NORMAL:
			case TarHeader.LF_LINK:
				if (slashTerminated)
					result = "d";
				else
					result = "-";
				break;

			case TarHeader.LF_DIR:
				result = "d";
				break;

			case TarHeader.LF_GNU_VOLHDR:
				result = "V";
				break;

			case TarHeader.LF_GNU_MULTIVOL:
				result = "M";
				break;

			case TarHeader.LF_CONTIG:
				result = "C";
				break;

			case TarHeader.LF_FIFO:
				result = "p";
				break;

			case TarHeader.LF_SYMLINK:
				result = "l";
				break;

			case TarHeader.LF_CHR:
				result = "c";
				break;

			case TarHeader.LF_BLK:
				result = "b";
				break;
		}

		return result;
	}

	string decodeMode(int mode)
	{	

		const int S_ISUID = 0x0800;
		const int S_ISGID = 0x0400;
		const int S_ISVTX = 0x0200;

		const int S_IRUSR = 0x0100;
		const int S_IWUSR = 0x0080;
		const int S_IXUSR = 0x0040;

		const int S_IRGRP = 0x0020;
		const int S_IWGRP = 0x0010;
		const int S_IXGRP = 0x0008;

		const int S_IROTH = 0x0004;
		const int S_IWOTH = 0x0002;
		const int S_IXOTH = 0x0001;


		System.Text.StringBuilder result = new System.Text.StringBuilder();
		result.Append((mode & S_IRUSR) != 0 ? 'r' : '-');
		result.Append((mode & S_IWUSR) != 0 ? 'w' : '-');
		result.Append((mode & S_ISUID) != 0
				? ((mode & S_IXUSR) != 0 ? 's' : 'S')
				: ((mode & S_IXUSR) != 0 ? 'x' : '-'));
		result.Append((mode & S_IRGRP) != 0 ? 'r' : '-');
		result.Append((mode & S_IWGRP) != 0 ? 'w' : '-');
		result.Append((mode & S_ISGID) != 0
				? ((mode & S_IXGRP) != 0 ? 's' : 'S')
				: ((mode & S_IXGRP) != 0 ? 'x' : '-'));
		result.Append((mode & S_IROTH) != 0 ? 'r' : '-');
		result.Append((mode & S_IWOTH) != 0 ? 'w' : '-');
		result.Append( (mode & S_ISVTX) != 0
				? ((mode & S_IXOTH) != 0 ? 't' : 'T')
				: ((mode & S_IXOTH) != 0 ? 'x' : '-'));

		return result.ToString();
	}

	/// <summary>
	/// Display progress information on console
	/// </summary>
	public void ShowTarProgressMessage(TarArchive archive, TarEntry entry, string message)
	{
		if (entry.TarHeader.TypeFlag != TarHeader.LF_NORMAL && entry.TarHeader.TypeFlag != TarHeader.LF_OLDNORM) {
			Console.WriteLine("Entry type " + (char)entry.TarHeader.TypeFlag + " found!");
		}

		if (message != null)
			Console.Write(entry.Name + " " + message);
		else {
			if (this.verbose) {
				string modeString = decodeType(entry.TarHeader.TypeFlag, entry.Name.EndsWith("/")) + decodeMode(entry.TarHeader.Mode);
				string userString = (entry.UserName == null || entry.UserName.Length == 0) ? entry.UserId.ToString() : entry.UserName;
				string groupString = (entry.GroupName == null || entry.GroupName.Length == 0) ? entry.GroupId.ToString() : entry.GroupName;
				
				Console.WriteLine(string.Format("{0} {1}/{2} {3,8} {4:yyyy-MM-dd HH:mm:ss} {5}", modeString, userString, groupString, entry.Size, entry.ModTime.ToLocalTime(), entry.Name));
			} else {
				Console.WriteLine(entry.Name);
			}
		}
	}
	
	string SharpZipVersion()
	{
		System.Reflection.Assembly zipAssembly = System.Reflection.Assembly.GetAssembly(new TarHeader().GetType());
		Version v = zipAssembly.GetName().Version;
		return "#ZipLib v" + v.Major + "." + v.Minor + "." + v.Build + "." + v.Revision;
	}
	
	/// <summary>
	/// Print version information.
	/// </summary>
	void Version()
	{
		Console.Error.WriteLine( "tar 2.0.6.2" );
		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "{0}", SharpZipVersion() );
		Console.Error.WriteLine( "Copyright (c) 2002 by Mike Krueger" );
		Console.Error.WriteLine( "Copyright (c) 1998,1999 by Tim Endres (Java version)" );
		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "This program is free software licensed to you under the" );
		Console.Error.WriteLine( "GNU General Public License. See the accompanying LICENSE" );
		Console.Error.WriteLine( "file, or the webpage <http://www.gjt.org/doc/gpl> or," );
		Console.Error.WriteLine( "visit www.gnu.org for more details." );
		Console.Error.WriteLine( "" );
	}
	
	/// <summary>
	/// Print help information.
	/// </summary>
	private void ShowHelp()
	{
		Console.Error.WriteLine( "Usage: tar [option]...   [file]..." );
		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "Examples:" );
		Console.Error.WriteLine( "  tar -cf archive.tar foo bar    # create archive.tar from files foo and bar" );
		Console.Error.WriteLine( "  tar -tvf archive.tar           # List all files in archive tar verbosely" );
		Console.Error.WriteLine( "  tar -xvf archive.tar           # Extract all files from archive.tar" );
		Console.Error.WriteLine( "" );

		Console.Error.WriteLine( "Main operation mode:" );
		Console.Error.WriteLine( "  -t, --list                 list the contents of an archive" );
		Console.Error.WriteLine( "  -x, --extract              extract files from an archive" );
		Console.Error.WriteLine( "  -c, --create               create a new archive" );
		Console.Error.WriteLine( "" );

		Console.Error.WriteLine( "Options:" );
		Console.Error.WriteLine( "  -f file,                   use 'file' as the tar archive" );
		Console.Error.WriteLine( "  -e,                        Turn on ascii translation" );
		Console.Error.WriteLine( "  -z, --gzip                 use gzip compression" );
		Console.Error.WriteLine( "  -Z, --compress             use unix compress" );
		Console.Error.WriteLine( "  -j, --bzip2                use bzip2 compression" );
		Console.Error.WriteLine( "  -k, --keep-old-files       dont overwrite existing files when extracting" );
		Console.Error.WriteLine( "  -b blks,                   set blocking factor (blks * 512 bytes per record)" );
		Console.Error.WriteLine( "      --record-size=SIZE     SIZE bytes per record, multiple of 512");
		Console.Error.WriteLine( "  -u name,                   set user name to 'name'" );
		Console.Error.WriteLine( "  -U id,                     set user id to 'id'" );
		Console.Error.WriteLine( "  -g name,                   set group name to 'name'" );
		Console.Error.WriteLine( "  -G id,                     set group id to 'id'" );

		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "Informative output:" );
		Console.Error.WriteLine( "  -?, --help                 print this help then exit" );
		Console.Error.WriteLine( "      --version,             print tar program version information" );
		Console.Error.WriteLine( "  -v, --verbose              verbosely list files processed" );
		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "The translation option -e will translate from local line" );
		Console.Error.WriteLine( "endings to UNIX line endings of '\\n' when writing tar" );
		Console.Error.WriteLine( "archives, and from UNIX line endings into local line endings" );
		Console.Error.WriteLine( "when extracting archives." );
		Console.Error.WriteLine( "" );
		Console.Error.WriteLine( "This tar defaults to -b " + TarBuffer.DefaultBlockFactor.ToString());
		Environment.Exit(1);
	}
}
			
/*
** Authored by Timothy Gerard Endres
** <mailto:time@gjt.org>  <http://www.trustice.com>
**
** This work has been placed into the public domain.
** You may use this work in any way and for any purpose you wish.
**
** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
** REDISTRIBUTION OF THIS SOFTWARE.
**
*/
