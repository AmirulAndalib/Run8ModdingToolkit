using System.Text;
using System.Security.Cryptography;

class Program
{
	public static int Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Specify a mode: t8d, dt8, verify");
			return 1;
		}

		string mode = args[0];

		string inputFilePath, inputFileName, outputFileName, outputFilePath;
		string? inputFileDirectory;
		if (mode.ToLower() == "t8d")
		{
			if (args.Length < 2)
			{
				Console.WriteLine("no input file specified!");
				return 1;
			}
			inputFilePath = args[1];
			if (!File.Exists(inputFilePath))
			{
				Console.WriteLine("File does not exist: " + inputFilePath);
				return 1;
			}

			inputFileName = Path.GetFileName(inputFilePath);
			inputFileDirectory = Path.GetDirectoryName(inputFilePath);

			if (inputFileDirectory == null)
			{
				inputFileDirectory = Directory.GetCurrentDirectory();
			}

			outputFileName = Path.GetFileNameWithoutExtension(inputFilePath) + ".dds";
			outputFilePath = Path.Join(inputFileDirectory, outputFileName);

			try
			{
				ConvertTX8ToDDS(inputFilePath, outputFilePath);
				Console.WriteLine("Conversion of " + inputFileName + " to " + outputFileName + ": OK");
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Conversion of " + inputFileName + " to " + outputFileName + ": FAILED");
				Console.WriteLine(ex.Message);
				return 1;
			}
		}
		else if (mode.ToLower() == "dt8")
		{
			if (args.Length < 2)
			{
				Console.WriteLine("no input file specified!");
				return 1;
			}
			inputFilePath = args[1];
			if (!File.Exists(inputFilePath))
			{
				Console.WriteLine("File does not exist: " + inputFilePath);
				return 1;
			}

			inputFileName = Path.GetFileName(inputFilePath);
			inputFileDirectory = Path.GetDirectoryName(inputFilePath);

			if (inputFileDirectory == null)
			{
				inputFileDirectory = Directory.GetCurrentDirectory();
			}

			outputFileName = Path.GetFileNameWithoutExtension(inputFilePath) + ".tx8";
			outputFilePath = Path.Join(inputFileDirectory, outputFileName);

			try
			{
				ConvertDDStoTX8(inputFilePath, outputFilePath);
				Console.WriteLine("Conversion of " + inputFileName + " to " + outputFileName + ": OK");
				try
				{
					bool verified = VerifyTX8(outputFilePath);
					if (verified)
					{
						Console.WriteLine("Verification of " + outputFileName + ": OK");
						return 0;
					}
					else
					{
						Console.WriteLine("Verification of " + outputFileName + ": FAILED");
						return 1;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Verification of " + outputFileName + ": FAILED");
					Console.WriteLine(ex.Message);
					return 1;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Conversion of " + inputFileName + " to " + outputFileName + ": FAILED");
				Console.WriteLine(ex.Message);
				return 1;
			}
		}
		else if (mode.ToLower() == "verify")
		{
			if (args.Length < 2)
			{
				Console.WriteLine("no input file specified!");
				return 1;
			}
			inputFilePath = args[1];
			if (!File.Exists(inputFilePath))
			{
				Console.WriteLine("File does not exist: " + inputFilePath);
				return 1;
			}

			try
			{
				bool verified = VerifyTX8(inputFilePath);
				if (verified)
				{
					Console.WriteLine("Verification of " + inputFilePath + ": OK");
					return 0;
				}
				else
				{
					Console.WriteLine("Verification of " + inputFilePath + ": FAILED");
					return 1;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Verification of " + inputFilePath + ": FAILED");
				Console.WriteLine(ex.Message);
				return 1;
			}
		}
		else
		{
			Console.WriteLine("Invalid mode: " + mode);
			Console.WriteLine("Valid modes are: t8d, dt8, verify");
			return 1;
		}
	}

	static byte[] GetMD5HashFromBytes(byte[] byte_0)
	{
		byte[] bytes;
		using (MD5 md = MD5.Create())
		{
			bytes = Encoding.ASCII.GetBytes(BitConverter.ToString(md.ComputeHash(byte_0)).Replace("-", string.Empty));
		}
		return bytes;
	}

	static byte smethod_33(int byteAsInt)
	{
		// we are checking if the byte is greater than 255
		if (byteAsInt > 255)
		{
			// if the byte is greater than 255, subtract 256 from the byte
			return (byte)(byteAsInt - 256);
		}
		// check if the byte is less than 0
		if (byteAsInt < 0)
		{
			// if the byte is less than 0, add 256 to the byte
			return (byte)(byteAsInt + 256);
		}
		// return the byte without modification
		return (byte)byteAsInt;
	}

	static byte smethod_33_r(int byteAsInt)
	{
		// we are checking if the byte is greater than 255
		if (byteAsInt > 255)
		{
			// if the byte is greater than 255, subtract 256 from the byte
			return (byte)(byteAsInt + 256);
		}
		// check if the byte is less than 0
		if (byteAsInt < 0)
		{
			// if the byte is less than 0, add 256 to the byte
			return (byte)(byteAsInt - 256);
		}
		// return the byte without modification
		return (byte)byteAsInt;
	}

	static void AddHexOperation(byte[] theBytes)
	{
		// ensure the bytes are actually something
		if (theBytes != null && theBytes.Length != 0)
		{
			// loop the bytes
			for (int i = 0; i < theBytes.Length; i++)
			{
				// for each byte, we run a method with the byte + 96
				theBytes[i] = smethod_33_r((int)(theBytes[i] - 96));
			}
			return;
		}
	}

	static void SubstractHexOperation(byte[] theBytes)
	{
		// ensure the bytes are actually something
		if (theBytes != null && theBytes.Length != 0)
		{
			// loop the bytes
			for (int i = 0; i < theBytes.Length; i++)
			{
				// for each byte, we run a method with the byte + 96
				theBytes[i] = smethod_33((int)(theBytes[i] + 96));
			}
			return;
		}
	}

	static bool CompareMD5Hash(byte[] byte_1, byte[] byte_2)
	{
		if (byte_1 == null || byte_2 == null)
		{
			return false;
		}
		if (byte_1.Length == byte_2.Length)
		{
			for (int i = 0; i < byte_1.Length; i++)
			{
				if (byte_1[i] != byte_2[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	static void ConvertDDStoTX8(string inputPath, string outputPath)
	{
		using (FileStream fileStream = File.OpenRead(inputPath))
		{
			byte[] byte_0 = new byte[32];
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);

			byte_0 = GetMD5HashFromBytes(array);

			byte[] newData = new byte[array.Length + byte_0.Length];
			Buffer.BlockCopy(array, 0, newData, 0, array.Length);
			Buffer.BlockCopy(byte_0, 0, newData, array.Length, byte_0.Length);

			AddHexOperation(newData);

			File.WriteAllBytes(outputPath, newData);
			fileStream.Close();
		}
	}

	static void ConvertTX8ToDDS(string inputPath, string outputPath)
	{
		using (FileStream fileStream = File.OpenRead(inputPath))
		{
			byte[] byte_0 = new byte[32];
			byte[] array = new byte[fileStream.Length - 32L];
			fileStream.Read(array, 0, (int)fileStream.Length - 32);
			fileStream.Read(byte_0, 0, 32);
			SubstractHexOperation(array);
			SubstractHexOperation(byte_0);

			File.WriteAllBytes(outputPath, array);
			fileStream.Close();
		}

	}

	static bool VerifyTX8(string path)
	{
		using (FileStream fileStream = File.OpenRead(path))
		{
			byte[] byte_0 = new byte[32];
			byte[] array = new byte[fileStream.Length - 32L];
			fileStream.Read(array, 0, (int)fileStream.Length - 32);
			fileStream.Read(byte_0, 0, 32);

			SubstractHexOperation(array);
			SubstractHexOperation(byte_0);

			if (CompareMD5Hash(byte_0, GetMD5HashFromBytes(array)))
			{
				fileStream.Close();
				return true;
			}


			fileStream.Close();
			return false;
		}
	}
}
