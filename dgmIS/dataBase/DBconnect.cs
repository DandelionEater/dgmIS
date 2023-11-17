using MySql.Data.MySqlClient;
using System.Data;

namespace dgmIS.DBconnect
{
	public class DBconnect
	{
		string _u, _p;

		public Access _access;

		public enum Access { Student, Lecturer, Admin };

		public DBconnect(string username, string password)
		{
			_u = username;
			_p = password;

			if (Exists(string.Format("SELECT * FROM sys.students WHERE fName = '{0}'", username)))
				_access = Access.Student;
			else if (Exists(string.Format("SELECT * FROM sys.lecturers WHERE fName = '{0}'", username)))
				_access = Access.Lecturer;
			else
				_access = Access.Admin;
		}

		MySqlConnection conn;

		private void createConnection(string username, string password)
		{
			string myConnectionString = string.Format("server=127.0.0.1;uid={0};pwd={1};database=sys", username, password);

			try
			{
				conn = new MySqlConnection();
				conn.ConnectionString = myConnectionString;
				conn.Open();
			}
			catch (MySqlException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		private async Task createQuerry(string querry) //querry - uzklausa
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;
			await myCommand.ExecuteNonQueryAsync();

			closeConnection();
		}
		private async Task<string> getString(string querry)
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			var result = myCommand.ExecuteReader();
			await result.ReadAsync();

			var output = result.GetString(0);

			closeConnection();

			return output;
		}
		private async Task<int> getInt(string querry)
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			var result = await myCommand.ExecuteReaderAsync();

			var output = 0;

			if (await result.ReadAsync())
				output = result.GetInt32(0);

			closeConnection();

			return output;
		}
		private async Task<List<List<object>>> getAll(string querry)
		{
			createConnection(_u, _p);

			var list = new List<List<object>>();

			MySqlCommand myCommand = new();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			var results = await myCommand.ExecuteReaderAsync();

			while (await results.ReadAsync())
			{
				var temp = new List<object>();

				for (int i = 0; i < results.FieldCount; i++)
					temp.Add(results[i]);

				list.Add(temp);
			}

			closeConnection();

			return list;
		}
		private void createQuerry(string querry, out object result) //querry - uzklausa
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			result = myCommand.ExecuteReader().Read();

			closeConnection();
		}
		private bool Exists(string querry)
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			var result = myCommand.ExecuteReader().Read();

			closeConnection();

			return result;


		}
		private void closeConnection()
		{
			conn.Close();
		}
		public async Task createLecturer(string fName, string lName, string lGroup = "")
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("DROP USER IF EXISTS '{0}'", fName));
			await createQuerry(string.Format("CREATE USER '{0}' IDENTIFIED BY '{1}';", fName, lName));
			await createQuerry(string.Format("INSERT INTO sys.lecturers(fName, lName, lGroup) VALUES('{0}', '{1}', '{2}')", new object[3] { fName, lName, lGroup }));
			await createQuerry(string.Format("GRANT INSERT, UPDATE, SELECT ON sys.grades TO '{0}'", fName));
			await createQuerry(string.Format("GRANT SELECT ON * TO '{0}'", fName));
		}
		public async Task deleteLecturer(int lecturerID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			var lName = await getString(string.Format("SELECT fName FROM sys.lecturers WHERE lecturerID = '{0}'", lecturerID));
			await createQuerry(string.Format("DELETE FROM sys.lecturers WHERE lecturerID = '{0}'", lecturerID));

			await createQuerry("DROP USER " + lName);
		}
		public async Task createStudent(string fName, string lName, int sGroupID = 0)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("DROP USER IF EXISTS '{0}'", fName));
			await createQuerry(string.Format("DELETE FROM sys.students WHERE fName = '{0}'", fName));

			await createQuerry(string.Format("CREATE USER '{0}' IDENTIFIED BY '{1}';", fName, lName));
			await createQuerry(string
				.Format("INSERT INTO sys" +
				".students(sLogin, fName, lName, sGroupID) VALUES('{0}', '{1}', '{2}', '{3}')", new object[4] { createSlogin(), fName, lName, sGroupID }));
			await createQuerry(string.Format("GRANT SELECT ON * TO '{0}'", fName));
		}
		public async void deleteStudent(int studentID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			var sName = await getString(string.Format("SELECT fName FROM sys.students WHERE studentID = '{0}'", studentID));
			await createQuerry(string.Format("DELETE FROM sys.students WHERE studentID = '{0}'", studentID));

			await createQuerry("DROP USER " + sName);
		}
		private string createSlogin()
		{
			int number = new Random().Next(1000000);

			createQuerry("SELECT * FROM sys.students WHERE sLogin = '" + "s" + number.ToString("######") + "'", out object result);

			if ((bool)result == true)
				return createSlogin();

			return "s" + number.ToString("######");
		}
		public async Task createGroup(string sGroupName)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("INSERT INTO sys.sgroups(sGroupName) VALUES ('{0}')", sGroupName));
		}
		public async Task deleteGroup(int sGroupID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("DELETE FROM sys.sgroups WHERE sGroupID = '{0}'", sGroupID));
		}
		public async Task createLecture(int lecturerID, string lectureName, int sGroupID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("INSERT INTO sys.lectures(lecturerID, lectureName, sGroupID) VALUES ('{0}', '{1}', '{2}')", lecturerID, lectureName, sGroupID));
		}
		public async Task deleteLecture(int lectureID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("DELETE FROM sys.lectures WHERE lectureID = '{0}'", lectureID));
		}
		public async Task assignLecturer(int lectureID, int lecturerID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("UPDATE sys.lectures SET lecturerID = '{0}' WHERE lectureID = '{1}'", lecturerID, lectureID));
		}
		public async Task assignLecture(int lectureID, int sGroupID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("UPDATE sys.lectures SET sGroupID = '{0}' WHERE lectureID = '{1}'", sGroupID, lectureID));
		}
		public async Task createGrade(int studentID, int lectureID, int lecturerID, int grade)
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("INSERT INTO sys.grades(studentID, lectureID, lecturerID, grade) " +
				"VALUES ('{0}', '{1}', '{2}', '{3}')", new object[4] { studentID, lectureID, lecturerID, grade }));
		}
		public async Task updateGrade(int gradeID, int grade)
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("UPDATE sys.grades SET grade = '{0}' WHERE gradeID = '{1}'", grade, gradeID));
		}
		public async Task<List<List<object>>> listStudents()
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return new List<List<object>>();
			}

			var studentsList = await getAll("SELECT fName AS Vardas, lName AS Pavarde, sGroupID AS Grupe FROM sys.students");

			return studentsList;
		}
		public async Task<List<List<object>>> listLecturers()
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return new List<List<object>>();
			}

			var lecturersList = await getAll("SELECT fName AS Vardas, lName AS Pavarde, lGroup AS Grupe FROM sys.lecturers");

			return lecturersList;
		}
		public async Task<List<List<object>>> listGradesForSudent(string fName, string lName)
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return new List<List<object>>();
			}

			return await getAll(string.Format("SELECT * FROM sys.grades WHERE fName = '{0}' and lName = '{1}'", fName, lName));
		}

		public async Task<List<List<object>>> listGradesForStudent()
		{
			var sID = await getInt(string.Format("SELECT studentID AS sID FROM sys.students WHERE fName = '{0}'", _u));

			return await getAll(string.Format("SELECT studentID, lectureID, lecturerID, grade FROM sys.grades WHERE studentID = '{0}'", sID));
		}

		public async Task<List<List<object>>> listLectures()
		{
			return await getAll(string.Format("SELECT * FROM sys.lectures WHERE sGroupID = '{0}'", getGroupID(_u)));
		}

		public async Task<int> getGroupID(string username)
		{
			var id = await getInt(string.Format("SELECT sGroupID from sys.students WHERE fName = '{0}'", username));

			return id;
		}
	}
}
