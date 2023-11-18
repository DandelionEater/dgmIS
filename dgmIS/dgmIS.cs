using dgmIS.DBconnect;
using dgmIS.Utilities;
using System.Text.RegularExpressions;

var username = Util.InputMenuString("Iveskite prisijungimo varda");
var password = Util.InputMenuString("Iveskite slaptazodi", true);


var DB = new DBconnect(username, password);

Console.WriteLine(string.Format("Sveiki {0}!", username == "root" ? "administratoriau" : username));

start:

var options = new List<string>();

switch (DB._access)
{
	case DBconnect.Access.Student:
		options.Add("Perziureti pazymius");
		break;
	case DBconnect.Access.Lecturer:
		options.Add("Ivesti nauja pazymi");
		options.Add("Pakeisti esama pazymi");
		break;
	case DBconnect.Access.Admin:
		options.Add("Studento sukurimas");
		options.Add("Studento salinimas");
		options.Add("Studentu grupiu sukurimas");
		options.Add("Studentu grupiu salinimas");
		options.Add("Studento priskirimas prie grupes");
		options.Add("Destytojo sukurimas");
		options.Add("Destytojo salinimas");
		options.Add("Destytojo priskyrimas prie paskaitos");
		options.Add("Paskaitos sukurimas");
		options.Add("Paskaitos salinimas");
		options.Add("Paskaitos priskyrimas prie grupes");
		break;
}

options.Add("Atsijungti");

var selection = Util.SelectionMenu("Pasirinkite norima operacija", options);

switch (DB._access)
{
	case DBconnect.Access.Student:
		switch (selection)
		{
			case 0:
				await Util.displayData(Util.convertToStrList(await DB.listGradesForStudent()), new Dictionary<string, Func<string, Task<string>>> { { "Studentas", DB.getStudent },
					{ "Paskaita", DB.getLecture }, { "Destytojas", DB.getLecturer }, { "Pazymys", DB.getGrade } });
				break;



			default:
				goto end;
		}

		break;
	case DBconnect.Access.Lecturer:
		switch (selection)
		{
			case 0:
				var lecturerID = await DB.getLecturerID(username);

				var lectureID = (await DB.getLectureIDs())[Util.SelectionMenu("Pasirinkite grupe", await DB.getLectures())];

				var groupID = (await DB.getGroupIDs(lectureID))[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups(lectureID))];

				var studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

				var grade = Util.InputMenu("Iveskite pazymi, kuri norite iteikti studentui");

				await DB.createGrade(studentID, lectureID, lecturerID, grade);
				break;
			case 1:
				lecturerID = await DB.getLecturerID(username);

				lectureID = (await DB.getLectureIDs())[Util.SelectionMenu("Pasirinkite grupe", await DB.getLectures())];

				groupID = (await DB.getGroupIDs(lectureID))[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups(lectureID))];

				studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

				var gradeID = (await DB.getGradeIDsForStudent(studentID, lectureID, lecturerID))[Util.SelectionMenu("Pasirinkite pazymi, kuri norite pakeisti", await DB.getGradesForStudent(studentID, lectureID, lecturerID))];

				grade = Util.InputMenu("Iveskite nauja pazymi, kuri norite iteikti studentui");

				await DB.updateGrade(gradeID, grade);
				break;



			default:
				goto end;
		}

		break;
	case DBconnect.Access.Admin:
		switch (selection)
		{
			case 0:
				var fName = Util.InputMenuString("Iveskite studento varda");
				var lName = Util.InputMenuString("Iveskite studento pavarde");
				await DB.createStudent(fName, lName);

				Console.WriteLine(string.Format("Studentas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
				break;
			case 1:
				var groupID = (await DB.getGroupIDs())[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups())];
				var studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

				DB.deleteStudent(studentID);
				break;
			case 5:
				fName = Util.InputMenuString("Iveskite destytojo varda");
				lName = Util.InputMenuString("Iveskite destytojo pavarde");
				await DB.createLecturer(fName, lName);

				Console.WriteLine(string.Format("Destytojas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
				break;
			case 6:

				break;

			default:
				goto end;
		}

		break;
}

if (Util.SelectionMenu("Ar norite testi?", new List<string> { "Ne", "Taip" }) == 1)
	goto start;

end:

Console.WriteLine("Program has ended");