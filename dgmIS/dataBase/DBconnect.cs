using dgmIS.Utilities;
using MySql.Data.MySqlClient;

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

		#region Connection functions

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

		private void closeConnection()
		{
			conn.Close();
		}

		#endregion

		#region Querries

		private async Task createQuerry(string querry) //querry - uzklausa
		{
			createConnection(_u, _p);

			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;
			await myCommand.ExecuteNonQueryAsync();

			closeConnection();
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

		#region Get singular elements

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

		#endregion

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

		#endregion

		#region Lecturer commands

		public async Task createLecturer(string fName, string lName)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("DROP USER IF EXISTS '{0}'", fName));
			await createQuerry(string.Format("CREATE USER '{0}' IDENTIFIED BY '{1}';", fName, lName));
			await createQuerry(string.Format("INSERT INTO sys.lecturers(fName, lName) VALUES('{0}', '{1}')", new object[2] { fName, lName }));
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

		public async Task assignLecturer(int lectureID, int lecturerID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("UPDATE sys.lectures SET lecturerID = '{0}' WHERE lectureID = '{1}'", lecturerID, lectureID));
			await createQuerry(string.Format("UPDATE sys.grades SET lecturerID = '{0}' WHERE lectureID = '{1}'", lecturerID, lectureID));
		}

		public async Task<string> getLecturer(string lecturerID)
		{
			string fName = await getString(string.Format("SELECT fName FROM sys.lecturers WHERE lecturerID = '{0}'", lecturerID));
			string lName = await getString(string.Format("SELECT lName FROM sys.lecturers WHERE lecturerID = '{0}'", lecturerID));
			return fName + " " + lName;
		}

		public async Task<int> getLecturerID(string fName)
		{
			return await getInt(string.Format("SELECT lecturerID FROM sys.lecturers WHERE fName = '{0}'", fName));
		}

		public async Task<List<int>> getLecturerIDs()
		{
			var lecturerIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll("SELECT lecturerID FROM sys.lecturers")))
			{
				lecturerIDs.Add(id[0]);
			}
			return lecturerIDs;
		}

		public async Task<List<string>> getLecturerNames()
		{
			var lecturerIDs = await getLecturerIDs();

			var lecturerNames = new List<string>();

			foreach (var lecturerID in lecturerIDs)
			{
				var fName = await getString(string.Format("SELECT fName FROM sys.lecturers WHERE lecturerID = {0}", lecturerID));
				var lName = await getString(string.Format("SELECT lName FROM sys.lecturers WHERE lecturerID = {0}", lecturerID));
				lecturerNames.Add(fName + " " + lName);
			}

			return lecturerNames;
		}

		#endregion

		#region Student commands
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
			await createQuerry(string.Format("DELETE FROM sys.grades WHERE studentID = '{0}'", studentID));

			await createQuerry("DROP USER '" + sName+ "'");
		}

		private string createSlogin()
		{
			int number = new Random().Next(1000000);

			createQuerry("SELECT * FROM sys.students WHERE sLogin = '" + "s" + number.ToString("######") + "'", out object result);

			if ((bool)result == true)
				return createSlogin();

			return "s" + number.ToString("######");
		}

		public async Task<List<List<object>>> listGradesForStudent()
		{
			var sID = await getInt(string.Format("SELECT studentID AS sID FROM sys.students WHERE fName = '{0}'", _u));

			return await getAll(string.Format("SELECT studentID, lectureID, lecturerID, gradeID FROM sys.grades WHERE studentID = '{0}'", sID));
		}

		public async Task<string> getStudent(string studentID)
		{
			return await getString(string.Format("SELECT sLogin FROM sys.students WHERE studentID = '{0}'", studentID));
		}

		public async Task<List<int>> getStudentsFromGroup(int groupID)
		{
			var studentIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT studentID FROM sys.students WHERE sGroupID = '{0}'", groupID))))
			{
				studentIDs.Add(id[0]);
			}
			return studentIDs;
		}

		public async Task assignStudentToGroup(int studentID, int sGroupID)
		{
			await createQuerry(string.Format("UPDATE sys.students SET sGroupID = '{0}' WHERE studentID = '{1}'", sGroupID, studentID));
		}

		public async Task<int> getStudentID(string sLogin)
		{
			return await getInt(string.Format("SELECT studentID FROM sys.students WHERE sLogin = '{0}'", sLogin));
		}

		public async Task<List<string>> getStudentFromGroupLogins(int groupID)
		{
			var studentIDs = await getStudentsFromGroup(groupID);

			var students = new List<string>();

			foreach (var studentID in studentIDs)
			{
				students.Add(await getString(string.Format("SELECT sLogin FROM sys.students WHERE studentID = {0}", studentID)));
			}

			return students;
		}

		#endregion

		#region Gruop commands

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

		public async Task<int> getGroupID(string username)
		{
			var id = await getInt(string.Format("SELECT sGroupID from sys.students WHERE fName = '{0}'", username));

			return id;
		}

		public async Task<List<int>> getGroupIDs()
		{
			var lecturerID = await getLecturerID(_u);

			var groupIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT sGroupID FROM sys.lectures WHERE lecturerID = '{0}'", lecturerID))))
			{
				groupIDs.Add(id[0]);
			}
			return groupIDs;
		}

		public async Task<List<int>> getGroupIDs(int lectureID)
		{
			var groupIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT sGroupID FROM sys.lectures WHERE lectureID = '{0}'", lectureID))))
			{
				groupIDs.Add(id[0]);
			}
			return groupIDs;
		}

		public async Task<List<int>> getGroupIDsAdmin()
		{
			var groupIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll("SELECT sGroupID FROM sys.sgroups")))
			{
				groupIDs.Add(id[0]);
			}
			return groupIDs;
		}

		public async Task<List<string>> getGroups()
		{
			var groupIDs = await getGroupIDsAdmin();

			var groups = new List<string>();

			foreach (var groupID in groupIDs)
			{
				groups.Add(await getString(string.Format("SELECT sGroupName FROM sys.sgroups WHERE sGroupID = {0}", groupID)));
			}

			return groups;
		}

		public async Task<List<string>> getGroups(int lectureID)
		{
			var groupIDs = await getGroupIDs(lectureID);

			var groups = new List<string>();

			foreach (var groupID in groupIDs)
			{
				groups.Add(await getString(string.Format("SELECT sGroupName FROM sys.sgroups WHERE sGroupID = {0}", groupID)));
			}

			return groups;
		}

		#endregion

		#region Lecture commands

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
			await createQuerry(string.Format("DELETE FROM sys.grades WHERE lectureID = '{0}'", lectureID));
		}

		public async Task assignLecture(int lectureID, int sGroupID)
		{
			if (_access != Access.Admin)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return;
			}

			await createQuerry(string.Format("UPDATE sys.lectures SET sGroupID = '{0}' WHERE lectureID = '{1}'", sGroupID, lectureID));
			await createQuerry(string.Format("DELETE FROM sys.grades WHERE lectureID = '{0}'", lectureID));
		}

		public async Task<string> getLecture(string lectureID)
		{
			return await getString(string.Format("SELECT lectureName FROM sys.lectures WHERE lectureID = '{0}'", lectureID));
		}

		public async Task<List<int>> getLectureIDs()
		{
			var lecturerID = await getLecturerID(_u);

			var lectureIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT lectureID FROM sys.lectures WHERE lecturerID = '{0}'", lecturerID))))
			{
				lectureIDs.Add(id[0]);
			}
			return lectureIDs;
		}

		public async Task<List<string>> getLectures()
		{
			var lectureIDs = await getLectureIDs();

			var lectures = new List<string>();

			foreach (var lectureID in lectureIDs)
			{
				lectures.Add(await getString(string.Format("SELECT lectureName FROM sys.lectures WHERE lectureID = {0}", lectureID)));
			}

			return lectures;
		}

		public async Task<List<int>> getLectureIDsFromGroup(int groupID)
		{
			var lectureIDs = new List<int>();
			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT lectureID FROM sys.lectures WHERE sGroupID = '{0}'", groupID))))
			{
				lectureIDs.Add(id[0]);
			}
			return lectureIDs;
		}

		public async Task<List<string>> getLectureNamesFromGroup(int groupID)
		{
			var lectureIDs = await getLectureIDsFromGroup(groupID);

			var lectures = new List<string>();

			foreach (var lectureID in lectureIDs)
			{
				lectures.Add(await getString(string.Format("SELECT lectureName FROM sys.lectures WHERE lectureID = {0}", lectureID)));
			}

			return lectures;
		}

		#endregion

		#region Grade commands

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

		public async Task<string> getGrade(string gradeID)
		{
			return (await getInt(string.Format("SELECT grade FROM sys.grades WHERE gradeID = '{0}'", gradeID))).ToString();
		}

		public async Task<List<int>> getGradeIDsForStudent(int studentID, int lectureID, int lecturerID)
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return new List<int>();
			}
			var gradeIDs = new List<int>();

			foreach (var id in Util.convertToIntList(await getAll(string.Format("SELECT gradeID FROM sys.grades WHERE (studentID = '{0}' AND lectureID = '{1}' AND lecturerID = '{2}')", studentID, lectureID, lecturerID))))
			{
				gradeIDs.Add(id[0]);
			}
			return gradeIDs;
		}

		public async Task<List<string>> getGradesForStudent(int studentID, int lectureID, int lecturerID)
		{
			if (_access < Access.Lecturer)
			{
				Console.WriteLine("Neturite teisiu panaudoti sia operacija");
				return new List<string>();
			}
			var gradeIDs = await getGradeIDsForStudent(studentID, lectureID, lecturerID);

			var grades = new List<string>();

			foreach (var gradeID in gradeIDs)
			{
				grades.Add((await getInt(string.Format("SELECT grade FROM sys.grades WHERE gradeID = {0}", gradeID))).ToString());
			}
			return grades;
		}

		#endregion

	}
}
