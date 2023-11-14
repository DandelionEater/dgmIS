using MySql.Data.MySqlClient;

namespace dgmIS.DBconnect
{
	public class DBconnect
	{
		MySqlConnection conn;

		public DBconnect createConnection(string username, string password)
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
			return this;
		}
		public DBconnect createQuerry(string querry) //querry - uzklausa
		{
			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;
			myCommand.ExecuteNonQuery();
			return this;
		}
		public DBconnect createQuerry(string querry, out object result) //querry - uzklausa
		{
			MySqlCommand myCommand = new MySqlCommand();

			myCommand.Connection = conn;
			myCommand.CommandText = querry;

			result = myCommand.ExecuteReader().Read();
			return this;
		}
		public void closeConnection()
		{
			conn.Close();
		}
		public DBconnect createLecturer(string fName, string lName, string lGroup = "")
		{
			createQuerry(string.Format("CREATE USER '{0}' IDENTIFIED BY '{1}';", fName, lName));
			createQuerry(string.Format("INSERT INTO sys.lecturers(fName, lName, lGroup) VALUES('{0}', '{1}', '{2}')", new object[3] { fName, lName, lGroup }));
			createQuerry(string.Format("GRANT INSERT, UPDATE, SELECT ON sys.grades TO '{0}'", fName));
			createQuerry(string.Format("GRANT SELECT ON sys.lectures TO '{0}'", fName));
			return this;
		}
		public DBconnect createStudent(string fName, string lName, string sGroup = "")
		{
			createQuerry(string.Format("CREATE USER '{0}'@* IDENTIFIED BY '{1}';", fName, lName), out object result);
			createQuerry(string
				.Format("INSERT INTO sys" +
				".students(sLogin, fName, lName, sGroup) VALUES('{0}', '{1}', '{2}', '{3}')", new object[4] { createSlogin(), fName, lName, sGroup }), out result);
			createQuerry(string.Format("GRANT SELECT on sys.grades, sys.lectures TO '{0}'@*", fName), out result);
			return this;
		}
		public string createSlogin()
		{
			int number = new Random().Next(1000000);

			createQuerry("SELECT * FROM sys.students WHERE sLogin = '" + "s" + number.ToString("######") + "'", out object result);

			if ((bool)result == true)
				return createSlogin();

			return "s" + number.ToString("######");
		}
	}
}
