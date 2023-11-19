using dgmIS.DBconnect;
using dgmIS.Utilities;

namespace dmgIS
{
	public class Program
	{
		public static async Task Main()
		{
			var username = Util.InputMenuString("Iveskite prisijungimo varda");
			var password = Util.InputMenuString("Iveskite slaptazodi", true);


			var DB = new DBconnect(username, password);

			Console.WriteLine(string.Format("Sveiki {0}!", DB._access == DBconnect.Access.Admin ? "administratoriau" : username));

			start:

			var options = new List<string>();

			switch (DB._access)
			{
				case DBconnect.Access.Student:
					options.Add("Perziureti pazymius");                     //0
					break;
				case DBconnect.Access.Lecturer:
					options.Add("Ivesti nauja pazymi");                     //0
					options.Add("Pakeisti esama pazymi");                   //1
					break;
				case DBconnect.Access.Admin:
					options.Add("Studento sukurimas");                      //0
					options.Add("Studento salinimas");                      //1
					options.Add("Studentu grupiu sukurimas");               //2
					options.Add("Studentu grupiu salinimas");               //3
					options.Add("Studento priskirimas prie grupes");        //4
					options.Add("Destytojo sukurimas");                     //5
					options.Add("Destytojo salinimas");                     //6
					options.Add("Destytojo priskyrimas prie paskaitos");    //7
					options.Add("Paskaitos sukurimas");                     //8
					options.Add("Paskaitos salinimas");                     //9
					options.Add("Paskaitos priskyrimas prie grupes");       //10
					break;
			}

			options.Add("Atsijungti");

			var selection = Util.SelectionMenu("Pasirinkite norima operacija", options);

			switch (DB._access)
			{
				case DBconnect.Access.Student:
					switch (selection)
					{
						case 0:					//Pazymiu perziura
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
						case 0:					//Pazymio sukurimas
							var lecturerID = await DB.getLecturerID(username);

							var lectureID = (await DB.getLectureIDs())[Util.SelectionMenu("Pasirinkite paskaita", await DB.getLectures())];

							var groupID = (await DB.getGroupIDs(lectureID))[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups(lectureID))];

							var studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

							var grade = Util.InputMenu("Iveskite pazymi, kuri norite irasyti studentui");

							await DB.createGrade(studentID, lectureID, lecturerID, grade);

							Console.WriteLine(string.Format("Studentui pazymys {0} sekmingai ivestas.", grade));
							break;

						case 1:					//Pazymio pakeitimas
							lecturerID = await DB.getLecturerID(username);

							lectureID = (await DB.getLectureIDs())[Util.SelectionMenu("Pasirinkite paskaita", await DB.getLectures())];

							groupID = (await DB.getGroupIDs(lectureID))[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups(lectureID))];

							studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

							var gradeID = (await DB.getGradeIDsForStudent(studentID, lectureID, lecturerID))[Util.SelectionMenu("Pasirinkite pazymi, kuri norite pakeisti", await DB.getGradesForStudent(studentID, lectureID, lecturerID))];

							grade = Util.InputMenu("Iveskite nauja pazymi, kuri norite iteikti studentui");

							await DB.updateGrade(gradeID, grade);

							Console.WriteLine(string.Format("Studentui pazymys {0} sekmingai pakeistas.", grade));
							break;

						default:
							goto end;
					}

					break;
				case DBconnect.Access.Admin:
					switch (selection)
					{
						case 0:                 //Studento sukurimas
							var fName = Util.InputMenuString("Iveskite studento varda");
							var lName = Util.InputMenuString("Iveskite studento pavarde");
							var groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups())];
							await DB.createStudent(fName, lName, groupID);

							Console.WriteLine(string.Format("Studentas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
							break;

						case 1:                 //Studento salinimas
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups())];
							var studentID = (await DB.getStudentsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite studenta", await DB.getStudentFromGroupLogins(groupID))];

							DB.deleteStudent(studentID);

							Console.WriteLine("Operacija atlikta.");
							break;

						case 2:                 //Studentu grupiu sukurimas
							var sGroupName = Util.InputMenuString("Iveskite grupes pavadinima");
							await DB.createGroup(sGroupName);

							Console.WriteLine(string.Format("Studentu grupe {0} sekmingai sukurta", sGroupName));
							break;

						case 3:                 //Studentu grupiu salinimas
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite grupe", await DB.getGroups())];
							await DB.deleteGroup(groupID);

							Console.WriteLine("Grupe sekmingai pasalinta");
							break;

						case 4:                 //Studento priskyrimas prie grupes
							var sLogin = Util.InputMenuString("Iveskite studento koda");
							studentID = await DB.getStudentID(sLogin);
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite grupe i kuria norite perkelti studenta", await DB.getGroups())];

							await DB.assignStudentToGroup(studentID, groupID);

							Console.WriteLine("Operacija sekmingai atlikta");
							break;

						case 5:                 //Destytojo sukurimas
							fName = Util.InputMenuString("Iveskite destytojo varda");
							lName = Util.InputMenuString("Iveskite destytojo pavarde");
							await DB.createLecturer(fName, lName);

							Console.WriteLine(string.Format("Destytojas sekmingai sukurtas, jo paskyros prisijungimas yra: \nPrisijungimo vardas: {0}\nSlaptazodis: {1}", fName, lName));
							break;

						case 6:                 //Destytojo salinimas
							var lecturerID = (await DB.getLecturerIDs())[Util.SelectionMenu("Pasirinkite destytoja", await DB.getLecturerNames())];

							await DB.deleteLecturer(lecturerID);

							Console.WriteLine("Operacija atlikta.");
							break;

						case 7:                 //Destytojo priskyrimas prie paskaitos
							lecturerID = (await DB.getLecturerIDs())[Util.SelectionMenu("Pasirinkite destytoja", await DB.getLecturerNames())];
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite studentu grupe", await DB.getGroups())];
							var lectureID = (await DB.getLectureIDsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite norima paskaita", await DB.getLectureNamesFromGroup(groupID))];

							await DB.assignLecturer(lectureID, lecturerID);

							Console.WriteLine("Operacija atlikta.");
							break;

						case 8:                 //Paskaitos sukurimas
							lecturerID = (await DB.getLecturerIDs())[Util.SelectionMenu("Pasirinkite destytoja", await DB.getLecturerNames())];
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite studentu grupe", await DB.getGroups())];
							var lectureName = Util.InputMenuString("Iveskite paskaitos pavadinima");

							await DB.createLecture(lecturerID, lectureName, groupID);

							Console.WriteLine("Operacija atlikta.");
							break;

						case 9:                 //Paskaitos salinimas
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite studentu grupe", await DB.getGroups())];
							lectureID = (await DB.getLectureIDsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite norima paskaita", await DB.getLectureNamesFromGroup(groupID))];

							await DB.deleteLecture(lectureID);

							Console.WriteLine("Operacija atlikta.");
							break;

						case 10:                //Paskaitos priskyrimas prie grupes
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite studentu grupe", await DB.getGroups())];
							lectureID = (await DB.getLectureIDsFromGroup(groupID))[Util.SelectionMenu("Pasirinkite norima paskaita", await DB.getLectureNamesFromGroup(groupID))];
							groupID = (await DB.getGroupIDsAdmin())[Util.SelectionMenu("Pasirinkite studentu grupe", await DB.getGroups())];

							await DB.assignLecture(lectureID, groupID);
							break;

						default:
							goto end;
					}

					break;
			}

			if (Util.SelectionMenu("Ar norite testi?", new List<string> { "Ne", "Taip" }) == 1)
				goto start;

			end:

			Console.WriteLine("Programa sustabdyta");
		}
	}
}