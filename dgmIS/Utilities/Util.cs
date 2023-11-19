using MySqlX.XDevAPI.Relational;
using System.Security.Policy;

namespace dgmIS.Utilities
{
	public static class Util
	{
		public static int InputMenu(string prompt)
		{
			Console.Write(prompt + ": ");

			int input = 0;

			while (!int.TryParse(Console.ReadLine(), out input))
			{
				Console.WriteLine("Blogas ivedimas, pabandykite dar karta.");

				Console.Write(prompt + ": ");
			}

			return input;
		}

		public static string InputMenuString(string prompt)
		{
			Console.Write(prompt + ": ");

			string input = Console.ReadLine();

			return input;
		}

		public static string InputMenuString(string prompt, bool password)
		{
			Console.Write(prompt + ": ");

			bool stop = false;
			string finalText = "";

			while (!stop)
			{
				ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
				char passwordChar = consoleKeyInfo.KeyChar;

				stop = passwordChar == '\n' || passwordChar == '\r';

				if (!stop)
				{
					Console.Write('*');
				}

				finalText += passwordChar;
			}

			Console.WriteLine();

			return finalText;
		}

		public static int SelectionMenu(string prompt, List<string> options)
		{
			Console.WriteLine(prompt);

			foreach (var option in options)
			{
				Console.WriteLine(string.Format("[{0}] {1}", options.IndexOf(option), option));
			}

			Console.Write("Jusu pasirinkimas: ");

			int selection = -1;

			if (!int.TryParse(Console.ReadLine(), out selection))
			{
				Console.WriteLine("Blogas ivedimas, pabandykite dar karta.");
				Console.Write("Jusu pasirinkimas: ");
			}

			return selection;
		}

		public static string getKeyName(Dictionary<string, Func<string, Task<string>>> data, int index)
		{
			return data.Keys.ElementAt(index);
		}

		public static List<List<string>> convertToStrList(List<List<object>> data)
		{
			var list = new List<List<string>>();

			foreach (List<object> item in data)
			{
				var temp = new List<string>();

				foreach (object str in item)
				{
					temp.Add(str.ToString());
				}

				list.Add(temp);
			}

			return list;
		}

		public static List<List<int>> convertToIntList(List<List<object>> data)
		{
			var list = new List<List<int>>();

			foreach (List<object> item in data)
			{
				var temp = new List<int>();

				foreach (object str in item)
				{
					temp.Add(int.Parse(str.ToString()));
				}

				list.Add(temp);
			}

			return list;
		}

		public static List<List<string>> flipRowsAndColumns(List<List<string>> data)
		{
			var rows = new List<List<string>>();
			if (data.Count > 0)
			{
				for (int i = 0; i < data[0].Count; i++)
				{
					var columns = new List<string>();
					for (int j = 0; j < data.Count; j++)
					{
						columns.Add(data[j][i]);
					}
					rows.Add(columns);
				}
			}

			return rows;
		}

		public static async Task<int> maxCharInColumn(List<string> data, Func<string, Task<string>> convert)
		{
			int max = 0;
			foreach (string row in data)
			{
				max = Math.Max(max, convert != default ? (await convert.Invoke(row)).Length : row.Length);
			}
			return max;
		}

		public static async Task displayData(List<List<string>> data, Dictionary<string, Func<string, Task<string>>> columns)
		{
			List<int> columnSizes = new();

			var flippedData = flipRowsAndColumns(data);

			foreach (var row in flippedData)
			{
				columnSizes.Add(await maxCharInColumn(row, columns.Values.ElementAt(flippedData.IndexOf(row))));
			}

			for (int i = -1; i < data.Count; i++)
			{
				for (int j = 0; j < (data.Count > 0 ? data[Math.Max(0, i)].Count : columns.Count); j++)
				{
					var key = getKeyName(columns, j);

					var size = Math.Max(columnSizes.Count > 0 ? columnSizes[j] + 3 : 0, key.Length + 3);

					if (i == -1)
					{
						Console.Write(key);
						for (int k = key.Length; k < size; k++)
						{
							Console.Write(" ");
						}
						continue;
					}
					Console.Write(columns[key] != default ? await columns[key].Invoke(data[i][j]) : data[i][j]);
					for (int k = columns[key] != default ? (await columns[key].Invoke(data[i][j])).Length : data[i][j].Length; k < size; k++)
					{
						Console.Write(" ");
					}

				}
				Console.WriteLine();
			}
		}
	}
}
